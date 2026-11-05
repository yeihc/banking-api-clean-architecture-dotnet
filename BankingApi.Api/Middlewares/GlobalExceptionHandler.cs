using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BankingApi.Domain.Exceptions;

namespace BankingApi.Api.Middlewares
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is DomainException domainEx)
            {
                _logger.LogWarning("Domain exception caught: [{Code}] {Message}", domainEx.Code, domainEx.Message);

                httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                await httpContext.Response.WriteAsJsonAsync(new
                {
                    Code = domainEx.Code,
                    Message = domainEx.Message
                }, cancellationToken);

                return true;
            }

            if (exception is ArgumentException argEx)
            {
                _logger.LogWarning("Validation argument exception caught: {Message}", argEx.Message);

                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(new
                {
                    Message = argEx.Message
                }, cancellationToken);

                return true;
            }

            if (exception is DbUpdateConcurrencyException concurrencyEx)
            {
                _logger.LogWarning("Concurrency conflict caught: {Message}", concurrencyEx.Message);

                httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                await httpContext.Response.WriteAsJsonAsync(new
                {
                    Code = "CONCURRENCY_CONFLICT",
                    Message = "A concurrency conflict occurred. Please retry the operation."
                }, cancellationToken);

                return true;
            }

            if (exception is DbUpdateException dbUpdateEx &&
                dbUpdateEx.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true)
            {
                _logger.LogWarning("Unique constraint conflict caught: {Message}", dbUpdateEx.Message);

                httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                await httpContext.Response.WriteAsJsonAsync(new
                {
                    Code = "CONCURRENCY_CONFLICT",
                    Message = "A concurrent request with the same idempotency key was detected."
                }, cancellationToken);

                return true;
            }

            _logger.LogError(exception, "Unhandled system exception occurred: {Message}", exception.Message);

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new
            {
                Message = "An unexpected error occurred."
            }, cancellationToken);

            return true;
        }
    }
}
