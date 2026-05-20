namespace WexCurrencyConverter.Application.Models;

public sealed record ExchangeRate(
    string Currency,
    decimal Rate,
    DateOnly EffectiveDate);