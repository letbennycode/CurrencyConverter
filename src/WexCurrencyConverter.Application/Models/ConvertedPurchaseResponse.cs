namespace WexCurrencyConverter.Application.Models;

public sealed record ConvertedPurchaseResponse(
    Guid Id,
    string Description,
    DateOnly TransactionDate,
    decimal AmountUsd,
    string Currency,
    decimal ExchangeRate,
    DateOnly ExchangeRateDate,
    decimal ConvertedAmount);