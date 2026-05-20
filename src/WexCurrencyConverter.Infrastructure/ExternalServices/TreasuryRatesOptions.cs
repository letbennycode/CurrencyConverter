using System.ComponentModel.DataAnnotations;

public sealed class TreasuryRatesOptions
{
    public const string SectionName = "TreasuryRatesClient";

    [Required]
    public string BaseAddress { get; init; } = "https://api.fiscaldata.treasury.gov/";

    [Required]
    public int TimeoutSeconds { get; init; } = 30;

    [Required]
    public int RetryCount { get; init; } = 3;
}