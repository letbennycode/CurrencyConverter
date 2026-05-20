using WexCurrencyConverter.Application.Abstractions.Interfaces;
using WexCurrencyConverter.Application.Models;
using WexCurrencyConverter.Domain.Exceptions;

namespace WexCurrencyConverter.Application.Services;

public sealed class CurrencyConversionService(
    IPurchaseRepository repository,
    ITreasuryRatesClient treasuryClient) : ICurrencyConversionService
{
    public async Task<ConvertedPurchaseResponse> ConvertAsync(
        Guid purchaseId,
        string currency,
        CancellationToken ct)
    {
        var purchase = await repository.GetByIdAsync(purchaseId, ct)
            ?? throw new PurchaseNotFoundException(purchaseId);

        var rate = await treasuryClient.GetRateAsync(
            currency,
            purchase.TransactionDate,
            purchase.TransactionDate.AddMonths(-6),
            ct)
            ?? throw new ExchangeRateUnavailableException(currency, purchase.TransactionDate);

        var convertedAmount = Math.Round(
            purchase.AmountUsd * rate.Rate,
            2,
            MidpointRounding.ToEven);

        return new ConvertedPurchaseResponse(
            purchase.Id,
            purchase.Description,
            purchase.TransactionDate,
            purchase.AmountUsd,
            rate.Currency,
            rate.Rate,
            rate.EffectiveDate,
            convertedAmount);
    }
}