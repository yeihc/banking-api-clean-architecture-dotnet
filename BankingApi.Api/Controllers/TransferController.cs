using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BankingApi.Application.Commands;
using BankingApi.Application.UseCases;
using BankingApi.Domain.Models;
using BankingApi.Domain.Repositories;

namespace BankingApi.Api.Controllers
{
    [ApiController]
    [Route("transfers")]
    public sealed class TransferController : ControllerBase
    {
        private readonly TransferFundsUseCase _transferFundsUseCase;
        private readonly ITransferRepository _transferRepository;

        public TransferController(
            TransferFundsUseCase transferFundsUseCase,
            ITransferRepository transferRepository
        )
        {
            _transferFundsUseCase = transferFundsUseCase;
            _transferRepository = transferRepository;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> TransferFunds([FromBody] TransferFundsRequest request)
        {
            var command = new TransferFundsCommand(
                request.SourceAccountId,
                request.TargetAccountId,
                Money.Of(request.Amount),
                request.IdempotencyKey
            );

            await _transferFundsUseCase.ExecuteAsync(command);

            var response = new TransferFundsResponse(
                request.IdempotencyKey,
                "ACCEPTED"
            );

            return Ok(response);
        }

        [HttpGet("{idempotencyKey}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTransfer(string idempotencyKey)
        {
            var transfer = await _transferRepository.FindByIdempotencyKeyAsync(idempotencyKey);
            if (transfer == null)
            {
                return NotFound(new { Message = $"Transfer with idempotency key '{idempotencyKey}' not found." });
            }

            var response = new TransferFundsResponse(
                transfer.IdempotencyKey,
                transfer.Status.ToString()
            );

            return Ok(response);
        }
    }

    public record TransferFundsRequest(
        Guid SourceAccountId,
        Guid TargetAccountId,
        decimal Amount,
        string IdempotencyKey
    );

    public record TransferFundsResponse(
        string IdempotencyKey,
        string Status
    );
}
