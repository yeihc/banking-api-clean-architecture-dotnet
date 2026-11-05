using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BankingApi.Application.Commands;
using BankingApi.Application.UseCases;
using BankingApi.Domain.Models;

namespace BankingApi.Api.Controllers
{
    [ApiController]
    [Route("accounts")]
    public sealed class AccountController : ControllerBase
    {
        private readonly OpenAccountUseCase _openAccountUseCase;

        public AccountController(OpenAccountUseCase openAccountUseCase)
        {
            _openAccountUseCase = openAccountUseCase;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> OpenAccount([FromBody] OpenAccountRequest request)
        {
            var command = new OpenAccountCommand(
                request.CustomerId,
                Money.Of(request.InitialDeposit)
            );

            var account = await _openAccountUseCase.ExecuteAsync(command);

            var response = new AccountResponse(
                account.Id,
                account.AccountNumber,
                account.Status.ToString().ToUpperInvariant()
            );

            return StatusCode(StatusCodes.Status201Created, response);
        }
    }

    public record OpenAccountRequest(Guid CustomerId, decimal InitialDeposit);

    public record AccountResponse(
        Guid AccountId,
        string AccountNumber,
        string Status
    );
}
