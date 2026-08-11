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

        private string GetFrontendRedirectUrl(long paymentId, string status)
        {
            string targetOrigin;
            if (paymentId > 0 && _paymentOriginCache.TryRemove(paymentId, out var cachedOrigin) && !string.IsNullOrWhiteSpace(cachedOrigin))
            {
                targetOrigin = cachedOrigin;
            }
            else
            {
                targetOrigin = _configuration["FrontendUrl"] ?? "https://tutormatching-platform.vercel.app";
                if (Request.Host.Host.Contains("localhost", StringComparison.OrdinalIgnoreCase))
                {
                    targetOrigin = "http://localhost:5173";
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

            // Create VNPAY request (PaymentId and CreatedTime are read-only and automatically generated)
            var vnpayRequest = new VNPAY.Models.VnpayPaymentRequest
            {
                Money = (double)(command.Amount * 1000), // 1 credit = 1,000 VND
                Description = $"Nap {command.Amount} tin chi vao tai khoan TutorPlatform",
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

            // Dynamically determine and cache caller origin for zero-hardcode redirect
            string clientOrigin = !string.IsNullOrWhiteSpace(command.ReturnUrl)
                ? command.ReturnUrl
                : (Request.Headers.TryGetValue("Origin", out var originHeader) && !string.IsNullOrWhiteSpace(originHeader)
                    ? originHeader.ToString()
                    : (Request.Headers.TryGetValue("Referer", out var refererHeader) && !string.IsNullOrWhiteSpace(refererHeader)
                        ? new Uri(refererHeader.ToString()).GetLeftPart(UriPartial.Authority)
                        : (_configuration["FrontendUrl"] ?? "https://tutormatching-platform.vercel.app")));

            clientOrigin = clientOrigin.TrimEnd('/');
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
            try
            {
                var paymentResult = _vnpayClient.GetPaymentResult(Request.Query);
                currentPaymentId = paymentResult.PaymentId;
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

                        return Redirect(GetFrontendRedirectUrl(currentPaymentId, "success"));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VNPAY Callback Exception: {ex.Message}");
            }

            return Redirect(GetFrontendRedirectUrl(currentPaymentId, "failed"));
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
