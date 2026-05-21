using WexCurrencyConverter.Application.Models;

namespace WexCurrencyConverter.Application.Abstractions.Interfaces;

public interface ITreasuryRatesClient
{
    Task<ExchangeRate?> GetRateAsync(string currency, DateOnly from, DateOnly to, CancellationToken ct);
}