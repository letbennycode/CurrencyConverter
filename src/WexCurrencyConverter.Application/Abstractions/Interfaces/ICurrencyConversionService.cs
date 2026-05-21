using WexCurrencyConverter.Application.Models;

namespace WexCurrencyConverter.Application.Abstractions.Interfaces;

public interface ICurrencyConversionService
{
    Task<ConvertedPurchaseResponse> ConvertAsync(
        Guid purchaseId,
        string currency,
        CancellationToken ct);
}