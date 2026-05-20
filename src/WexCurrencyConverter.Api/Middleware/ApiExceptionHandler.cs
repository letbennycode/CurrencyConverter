using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;
using WexCurrencyConverter.Domain.Exceptions;

namespace WexCurrencyConverter.Api.Middleware;

public class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public ApiExceptionHandler(
        ILogger<ApiExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct)
    {
        _logger.LogError(exception, "Exception caught: {Message}", exception.Message);

        var (status, title) = exception switch
        {
            InvalidPurchaseAmountException => (StatusCodes.Status422UnprocessableEntity, "Transaction amount is invalid."),
            InvalidTransactionDateException => (StatusCodes.Status422UnprocessableEntity, "Transaction date is invalid."),
            InvalidTransactionDescriptionException => (StatusCodes.Status422UnprocessableEntity, "Description is invalid."),
            PurchaseNotFoundException => (StatusCodes.Status404NotFound, "Purchase was not found."),
            ExchangeRateUnavailableException => (StatusCodes.Status422UnprocessableEntity, "Exchange Rate Unavailable."),
            PurchaseException => (StatusCodes.Status422UnprocessableEntity, "A domain rule was violated."),
            HttpRequestException => (StatusCodes.Status502BadGateway, "External service error."),
            BrokenCircuitException => (StatusCodes.Status503ServiceUnavailable, "Service temporarily unavailable."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == StatusCodes.Status500InternalServerError && !_environment.IsDevelopment()
                ? "An internal error occurred. Please try again later."
                : exception.Message
        };

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, ct);

        return true;
    }
}