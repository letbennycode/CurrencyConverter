// TreasuryRatesClient.cs
using System.Globalization;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using WexCurrencyConverter.Application.Abstractions.Interfaces;
using WexCurrencyConverter.Application.Models;

namespace WexCurrencyConverter.Infrastructure.ExternalServices;

public sealed class TreasuryRatesClient(
    HttpClient httpClient, 
    IMemoryCache cache) : ITreasuryRatesClient 
{
    private const string Endpoint =
        "services/api/fiscal_service/v1/accounting/od/rates_of_exchange";

    public async Task<ExchangeRate?> GetRateAsync(
        string currency,
        DateOnly from,
        DateOnly to,
        CancellationToken ct)
    {
        var cacheKey = $"treasury:{currency}:{from:yyyy-MM-dd}:{to:yyyy-MM-dd}";

        // If cached value exists for the exchange rate, return that
        if (cache.TryGetValue(cacheKey, out ExchangeRate? cached))
            return cached;

        var encodedCurrency = Uri.EscapeDataString(currency);

        var filter =
            $"country_currency_desc:eq:{encodedCurrency}," +
            $"record_date:lte:{from:yyyy-MM-dd}," +
            $"record_date:gte:{to:yyyy-MM-dd}";

        var url =
            $"{Endpoint}" +
            $"?fields=country_currency_desc,exchange_rate,record_date" +
            $"&filter={filter}" +
            $"&sort=-record_date" +
            $"&page[size]=1";

        // If there is no value in cache, call the treasury API
        using var response = await httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content
            .ReadFromJsonAsync<TreasuryResponse>(cancellationToken: ct);

        var record = payload?.Data?.FirstOrDefault();
        if (record is null) return null;

        if (!decimal.TryParse(
                record.ExchangeRate,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var rate) || rate <= 0m)
        {
            throw new InvalidOperationException(
                $"Treasury returned an invalid exchange_rate '{record.ExchangeRate}' " +
                $"for {currency} on {record.RecordDate}.");
        }

        if (!DateOnly.TryParseExact(
                record.RecordDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var effectiveDate))
        {
            throw new InvalidOperationException(
                $"Treasury returned an unparseable record_date '{record.RecordDate}' " +
                $"for {currency}.");
        }

        var result = new ExchangeRate(record.CountryCurrencyDesc, rate, effectiveDate);
        
        // Store result before returning so next request is a hit
        cache.Set(cacheKey, result, TimeSpan.FromHours(1));

        return result;
    }
}