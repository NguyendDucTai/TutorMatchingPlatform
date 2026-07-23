using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorMatchingPlatform.Application.Credits.Commands.DepositCredit;
using TutorMatchingPlatform.Application.Credits.Queries.GetCreditBalance;
using TutorMatchingPlatform.Application.Credits.Queries.GetCreditTransactions;

namespace TutorMatchingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CreditsController : ControllerBase
    {
        private readonly ISender _sender;

        public CreditsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositCreditRequestDto request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var command = new DepositCreditCommand
            {
                UserId = userId,
                Amount = request.Amount,
                Note = request.Note
            };

            var result = await _sender.Send(command);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var result = await _sender.Send(new GetCreditBalanceQuery { UserId = userId });
            return Ok(new { Balance = result });
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var result = await _sender.Send(new GetCreditTransactionsQuery { UserId = userId });
            return Ok(result);
        }
    }

    public class DepositCreditRequestDto
    {
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }
}
