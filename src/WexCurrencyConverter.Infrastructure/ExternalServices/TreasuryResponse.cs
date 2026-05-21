using System.Text.Json.Serialization;

namespace WexCurrencyConverter.Infrastructure.ExternalServices;

internal sealed class TreasuryResponse
{
    [JsonPropertyName("data")]
    public List<TreasuryRateRecord>? Data { get; init; }
}

internal sealed class TreasuryRateRecord
{
    [JsonPropertyName("country_currency_desc")]
    public string CountryCurrencyDesc { get; init; } = string.Empty;

    [JsonPropertyName("exchange_rate")]
    public string ExchangeRate { get; init; } = string.Empty;

    [JsonPropertyName("record_date")]
    public string RecordDate { get; init; } = string.Empty;
}