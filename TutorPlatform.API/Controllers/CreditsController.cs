using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TutorPlatform.API.Common;
using TutorPlatform.Application.Contracts.Credits;
using TutorPlatform.Application.Features.Credits.Commands.DepositCredits;
using TutorPlatform.Application.Features.Credits.Queries.GetBalance;
using TutorPlatform.Application.Features.Credits.Queries.GetCreditTransactions;
using TutorPlatform.Domain.Common;

namespace TutorPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreditsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly TutorPlatform.Infrastructure.Persistence.ApplicationDbContext _dbContext;
        private readonly VNPAY.IVnpayClient _vnpayClient;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<long, string> _paymentOriginCache = new();

        public CreditsController(
            IMediator mediator, 
            TutorPlatform.Infrastructure.Persistence.ApplicationDbContext dbContext,
            VNPAY.IVnpayClient vnpayClient,
            Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _mediator = mediator;
            _dbContext = dbContext;
            _vnpayClient = vnpayClient;
            _configuration = configuration;
        }

        private string GetFrontendRedirectUrl(string? description, long paymentId, string status)
        {
            string? targetOrigin = null;

            // 1. Try to extract from VNPay description tag [ORIGIN:...]
            if (!string.IsNullOrWhiteSpace(description) && description.Contains("[ORIGIN:", StringComparison.OrdinalIgnoreCase))
            {
                int start = description.IndexOf("[ORIGIN:", StringComparison.OrdinalIgnoreCase) + 8;
                int end = description.IndexOf("]", start);
                if (end > start)
                {
                    var extracted = description.Substring(start, end - start).Trim();
                    if (!string.IsNullOrWhiteSpace(extracted) && !extracted.Contains("vnpayment.vn", StringComparison.OrdinalIgnoreCase))
                    {
                        targetOrigin = extracted;
                    }
                }
            }

            // 2. Try to extract from in-memory cache
            if (string.IsNullOrWhiteSpace(targetOrigin) && paymentId > 0 && _paymentOriginCache.TryRemove(paymentId, out var cachedOrigin) && !string.IsNullOrWhiteSpace(cachedOrigin))
            {
                if (!cachedOrigin.Contains("vnpayment.vn", StringComparison.OrdinalIgnoreCase))
                {
                    targetOrigin = cachedOrigin;
                }
            }

            // 3. Fallback safely (NEVER redirect to vnpayment.vn)
            if (string.IsNullOrWhiteSpace(targetOrigin) || targetOrigin.Contains("vnpayment.vn", StringComparison.OrdinalIgnoreCase))
            {
                targetOrigin = _configuration["FrontendUrl"];
                if (string.IsNullOrWhiteSpace(targetOrigin))
                {
                    targetOrigin = Request.Host.Host.Contains("localhost", StringComparison.OrdinalIgnoreCase)
                        ? "http://localhost:5173"
                        : "https://tutormatching-platform.vercel.app";
                }
            }

            return $"{targetOrigin.TrimEnd('/')}/?tab=wallet&payment={status}";
        }

        private static Guid LongToGuid(long value)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        private static long GuidToLong(Guid guid)
        {
            byte[] bytes = guid.ToByteArray();
            return BitConverter.ToInt64(bytes, 0);
        }

        [HttpPost("deposit")]
        [Authorize(Roles = "Student,Tutor")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Deposit([FromBody] DepositCreditsCommand command)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            if (command.Amount <= 0)
            {
                return BadRequest(ApiResponse<object>.Error(400, "Số tiền nạp phải lớn hơn 0."));
            }

            // Dynamically determine caller origin
            string clientOrigin = !string.IsNullOrWhiteSpace(command.ReturnUrl)
                ? command.ReturnUrl
                : (Request.Headers.TryGetValue("Origin", out var originHeader) && !string.IsNullOrWhiteSpace(originHeader)
                    ? originHeader.ToString()
                    : (Request.Headers.TryGetValue("Referer", out var refererHeader) && !string.IsNullOrWhiteSpace(refererHeader) && !refererHeader.ToString().Contains("vnpayment.vn", StringComparison.OrdinalIgnoreCase)
                        ? new Uri(refererHeader.ToString()).GetLeftPart(UriPartial.Authority)
                        : (_configuration["FrontendUrl"] ?? $"{Request.Scheme}://{Request.Host}")));

            clientOrigin = clientOrigin.TrimEnd('/');

            // Create VNPAY request with origin embedded in Description (vnp_OrderInfo)
            var vnpayRequest = new VNPAY.Models.VnpayPaymentRequest
            {
                Money = (double)(command.Amount * 1000), // 1 credit = 1,000 VND
                Description = $"Nap {command.Amount} tc [ORIGIN:{clientOrigin}]",
                BankCode = VNPAY.Models.Enums.BankCode.ANY
            };

            VNPAY.Models.PaymentUrlDetail paymentUrlDetail;
            try
            {
                paymentUrlDetail = _vnpayClient.CreatePaymentUrl(vnpayRequest);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Error(400, $"Lỗi khởi tạo thanh toán VNPAY: {ex.Message}"));
            }

            long paymentId = paymentUrlDetail.PaymentId;
            Guid depositReqId = LongToGuid(paymentId);

            _paymentOriginCache[paymentId] = clientOrigin;

            // Create a pending deposit request
            var request = new TutorPlatform.Infrastructure.Models.DepositRequestDataModel
            {
                Id = depositReqId,
                UserId = userId,
                Amount = command.Amount,
                Status = 0, // Pending payment
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.DepositRequests.AddAsync(request);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok(paymentUrlDetail.Url));
        }

        [HttpGet("vnpay-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> VnpayCallback()
        {
            long currentPaymentId = 0;
            string? paymentDescription = null;
            try
            {
                var paymentResult = _vnpayClient.GetPaymentResult(Request.Query);
                currentPaymentId = paymentResult.PaymentId;
                paymentDescription = paymentResult.Description;
                if (paymentResult.PaymentId > 0)
                {
                    var reqGuid = LongToGuid(paymentResult.PaymentId);
                    var depositRequest = await _dbContext.DepositRequests.FindAsync(reqGuid);
                    if (depositRequest != null && depositRequest.Status == 0)
                    {
                        depositRequest.Status = 1; // Success
                        depositRequest.UpdatedAt = DateTime.UtcNow;

                        var user = await _dbContext.Users.FindAsync(depositRequest.UserId);
                        if (user != null)
                        {
                            user.CreditBalance += depositRequest.Amount;

                            var tx = new TutorPlatform.Infrastructure.Models.CreditTransactionDataModel
                            {
                                Id = Guid.NewGuid(),
                                UserId = depositRequest.UserId,
                                Amount = depositRequest.Amount,
                                Type = 0, // Credit
                                Description = $"Nạp tiền thành công qua cổng thanh toán VNPAY. Mã GD: {paymentResult.VnpayTransactionId}",
                                BalanceAfter = user.CreditBalance,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            await _dbContext.CreditTransactions.AddAsync(tx);
                            await _dbContext.SaveChangesAsync();

                            await _mediator.Publish(new TutorPlatform.Application.Features.Notifications.Events.DepositRequestApprovedEvent
                            {
                                UserId = depositRequest.UserId,
                                Amount = depositRequest.Amount,
                                NewBalance = user.CreditBalance
                            });
                        }
                        else
                        {
                            await _dbContext.SaveChangesAsync();
                        }

                        return Redirect(GetFrontendRedirectUrl(paymentDescription, currentPaymentId, "success"));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VNPAY Callback Exception: {ex.Message}");
            }

            return Redirect(GetFrontendRedirectUrl(paymentDescription, currentPaymentId, "failed"));
        }

        [HttpGet("balance")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<WalletBalanceDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBalance()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var query = new GetBalanceQuery(userId);
            var response = await _mediator.Send(query);
            return Ok(ApiResponse<WalletBalanceDto>.Ok(response));
        }

        [HttpGet("transactions")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CreditTransactionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransactions([FromQuery] Guid? targetUserId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var query = new GetCreditTransactionsQuery
            {
                UserId = userId,
                TargetUserId = targetUserId,
                RequestorRole = roleClaim ?? string.Empty,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var response = await _mediator.Send(query);
            return Ok(ApiResponse<PagedResult<CreditTransactionDto>>.Ok(response));
        }
    }
}
