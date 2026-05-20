// TreasuryRatesClient.cs
using System.Globalization;
using System.Net.Http.Json;
using WexCurrencyConverter.Application.Abstractions.Interfaces;
using WexCurrencyConverter.Application.Models;

namespace WexCurrencyConverter.Infrastructure.ExternalServices;

public sealed class TreasuryRatesClient(HttpClient httpClient) : ITreasuryRatesClient
{
    private const string Endpoint =
        "services/api/fiscal_service/v1/accounting/od/rates_of_exchange";

    public async Task<ExchangeRate?> GetRateAsync(
        string currency,
        DateOnly from,
        DateOnly to,
        CancellationToken ct)
    {
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

        return new ExchangeRate(record.CountryCurrencyDesc, rate, effectiveDate);
    }
}