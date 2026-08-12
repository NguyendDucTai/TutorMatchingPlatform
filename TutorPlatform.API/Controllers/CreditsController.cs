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

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, string> _depositReturnUrls = new();

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

            // Append cancelUrl to VNPay payment URL if provided
            var paymentUrl = paymentUrlDetail.Url;
            if (!string.IsNullOrWhiteSpace(command.CancelUrl))
            {
                var separator = paymentUrl.Contains('?') ? "&" : "?";
                paymentUrl = $"{paymentUrl}{separator}vnp_CancelUrl={Uri.EscapeDataString(command.CancelUrl)}";
            }

            long paymentId = paymentUrlDetail.PaymentId;
            Guid depositReqId = LongToGuid(paymentId);

            // Dynamically register Frontend client return origin
            string clientOrigin = command.ReturnUrl;
            if (string.IsNullOrWhiteSpace(clientOrigin))
            {
                clientOrigin = Request.Headers["Origin"].ToString();
            }
            if (string.IsNullOrWhiteSpace(clientOrigin))
            {
                clientOrigin = Request.Headers["Referer"].ToString();
            }
            if (!string.IsNullOrWhiteSpace(clientOrigin))
            {
                try
                {
                    var uri = new Uri(clientOrigin);
                    clientOrigin = $"{uri.Scheme}://{uri.Authority}";
                    _depositReturnUrls[depositReqId] = clientOrigin;
                }
                catch {}
            }

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

            return Ok(ApiResponse<string>.Ok(paymentUrl));
        }

        [HttpGet("vnpay-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> VnpayCallback()
        {
            string targetFrontendUrl = null;

            try
            {
                var paymentResult = _vnpayClient.GetPaymentResult(Request.Query);
                long paymentId = paymentResult.PaymentId;

                if (paymentId <= 0 && Request.Query.TryGetValue("vnp_TxnRef", out var txnRefStr) && long.TryParse(txnRefStr, out long parsedTxnRef))
                {
                    paymentId = parsedTxnRef;
                }

                if (paymentId > 0)
                {
                    var reqGuid = LongToGuid(paymentId);
                    if (_depositReturnUrls.TryRemove(reqGuid, out var savedOrigin) && !string.IsNullOrWhiteSpace(savedOrigin))
                    {
                        targetFrontendUrl = savedOrigin.TrimEnd('/');
                    }

                    var depositRequest = await _dbContext.DepositRequests.FindAsync(reqGuid);
                    string respCode = Request.Query["vnp_ResponseCode"].ToString();

                    if (depositRequest != null)
                    {
                        if (respCode == "00")
                        {
                            if (depositRequest.Status == 0)
                            {
                                depositRequest.Status = 1; // Success / Approved
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
                            }

                            if (string.IsNullOrWhiteSpace(targetFrontendUrl))
                            {
                                targetFrontendUrl = _configuration["FrontendUrl"] ?? _configuration["Vnpay:FrontendUrl"] ?? "http://localhost:5173";
                            }
                            targetFrontendUrl = targetFrontendUrl.TrimEnd('/');
                            return Redirect($"{targetFrontendUrl}/wallet?payment=success");
                        }
                        else
                        {
                            // Payment cancelled or failed by user on VNPAY (ResponseCode 24 or other error code)
                            if (depositRequest.Status == 0)
                            {
                                depositRequest.Status = 2; // Cancelled / Rejected (Thất bại / Đã hủy)
                                depositRequest.UpdatedAt = DateTime.UtcNow;
                                await _dbContext.SaveChangesAsync();
                            }

                            if (string.IsNullOrWhiteSpace(targetFrontendUrl))
                            {
                                targetFrontendUrl = _configuration["FrontendUrl"] ?? _configuration["Vnpay:FrontendUrl"] ?? "http://localhost:5173";
                            }
                            targetFrontendUrl = targetFrontendUrl.TrimEnd('/');
                            return Redirect($"{targetFrontendUrl}/wallet?payment=failed");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VNPAY Callback Exception: {ex.Message}");
            }

            if (string.IsNullOrWhiteSpace(targetFrontendUrl))
            {
                targetFrontendUrl = _configuration["FrontendUrl"] ?? _configuration["Vnpay:FrontendUrl"] ?? "http://localhost:5173";
            }
            targetFrontendUrl = targetFrontendUrl.TrimEnd('/');
            return Redirect($"{targetFrontendUrl}/wallet?payment=failed");
        }

        [HttpGet("vnpay-cancel")]
        [AllowAnonymous]
        public async Task<IActionResult> VnpayCancel()
        {
            string targetFrontendUrl = _configuration["FrontendUrl"] ?? _configuration["Vnpay:FrontendUrl"] ?? "http://localhost:5173";
            targetFrontendUrl = targetFrontendUrl.TrimEnd('/');

            try
            {
                var txnRef = Request.Query["vnp_TxnRef"].ToString();
                if (!string.IsNullOrEmpty(txnRef) && long.TryParse(txnRef, out var paymentId))
                {
                    var reqGuid = LongToGuid(paymentId);
                    if (_depositReturnUrls.TryRemove(reqGuid, out var savedOrigin) && !string.IsNullOrWhiteSpace(savedOrigin))
                    {
                        targetFrontendUrl = savedOrigin.TrimEnd('/');
                    }

                    var depositRequest = await _dbContext.DepositRequests.FindAsync(reqGuid);
                    if (depositRequest != null && depositRequest.Status == 0)
                    {
                        depositRequest.Status = 2; // Da huy
                        depositRequest.UpdatedAt = DateTime.UtcNow;
                        await _dbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VnpayCancel error: {ex.Message}");
            }

            return Redirect($"{targetFrontendUrl}/wallet?payment=cancelled");
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
