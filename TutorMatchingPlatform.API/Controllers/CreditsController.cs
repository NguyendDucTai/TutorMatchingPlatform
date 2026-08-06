using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.API.Common;
using TutorMatchingPlatform.Application.Credits.Commands.DepositCredit;
using TutorMatchingPlatform.Application.Credits.Queries.GetCreditBalance;
using TutorMatchingPlatform.Application.Credits.Queries.GetCreditTransactions;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Infrastructure.Data;

namespace TutorMatchingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreditsController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly TutorMatchingPlatformDbContext _context;
        private readonly VNPAY.IVnpayClient _vnpayClient;

        public CreditsController(
            ISender sender,
            TutorMatchingPlatformDbContext context,
            VNPAY.IVnpayClient vnpayClient)
        {
            _sender = sender;
            _context = context;
            _vnpayClient = vnpayClient;
        }

        [HttpPost("deposit")]
        [Authorize]
        public async Task<IActionResult> Deposit([FromBody] DepositCreditRequestDto request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            if (request.Amount <= 0)
            {
                return BadRequest(ApiResponse<object>.Error(400, "Amount must be greater than 0."));
            }

            // Create VNPAY request
            var vnpayRequest = new VNPAY.Models.VnpayPaymentRequest
            {
                Money = (double)(request.Amount * 1000), // 1 credit = 1,000 VND
                Description = $"Nap {request.Amount} credit vao tai khoan TutorMatchingPlatform",
                BankCode = VNPAY.Models.Enums.BankCode.ANY
            };

            VNPAY.Models.PaymentUrlDetail paymentUrlDetail;
            try
            {
                paymentUrlDetail = _vnpayClient.CreatePaymentUrl(vnpayRequest);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Error(400, $"VNPAY Initialization Error: {ex.Message}"));
            }

            // Save pending credit request
            var creditRequest = new CreditRequest
            {
                UserId = userId,
                Amount = request.Amount,
                Status = Domain.Enums.CreditRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Note = $"VNPAY Payment ID: {paymentUrlDetail.PaymentId}"
            };

            await _context.CreditRequests.AddAsync(creditRequest);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok(paymentUrlDetail.Url));
        }

        [HttpGet("vnpay-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> VnpayCallback()
        {
            try
            {
                var paymentResult = _vnpayClient.GetPaymentResult(Request.Query);
                if (paymentResult.PaymentId > 0)
                {
                    var paymentIdStr = $"VNPAY Payment ID: {paymentResult.PaymentId}";
                    var creditRequest = await _context.CreditRequests
                        .FirstOrDefaultAsync(r => r.Note == paymentIdStr && r.Status == Domain.Enums.CreditRequestStatus.Pending);

                    if (creditRequest != null)
                    {
                        creditRequest.Status = Domain.Enums.CreditRequestStatus.Approved;
                        creditRequest.ProcessedAt = DateTime.UtcNow;

                        var user = await _context.Users.FindAsync(creditRequest.UserId);
                        if (user != null)
                        {
                            user.CreditBalance += creditRequest.Amount;

                            var transaction = new CreditTransaction
                            {
                                UserId = creditRequest.UserId,
                                Amount = creditRequest.Amount,
                                Type = Domain.Enums.CreditTransactionType.Deposit,
                                ReferenceId = paymentResult.VnpayTransactionId.ToString(),
                                Description = $"Nạp tiền thành công qua VNPAY. Mã GD: {paymentResult.VnpayTransactionId}",
                                CreatedAt = DateTime.UtcNow
                            };

                            await _context.CreditTransactions.AddAsync(transaction);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            await _context.SaveChangesAsync();
                        }

                        return Redirect("http://localhost:5173/?tab=wallet&payment=success");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VNPAY Callback Exception: {ex.Message}");
            }

            return Redirect("http://localhost:5173/?tab=wallet&payment=failed");
        }

        [HttpGet("balance")]
        [Authorize]
        public async Task<IActionResult> GetBalance()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var result = await _sender.Send(new GetCreditBalanceQuery { UserId = userId });
            return Ok(ApiResponse<object>.Ok(new { Balance = result }));
        }

        [HttpGet("transactions")]
        [Authorize]
        public async Task<IActionResult> GetTransactions()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var result = await _sender.Send(new GetCreditTransactionsQuery { UserId = userId });
            return Ok(ApiResponse<object>.Ok(result));
        }
    }

    public class DepositCreditRequestDto
    {
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }
}
