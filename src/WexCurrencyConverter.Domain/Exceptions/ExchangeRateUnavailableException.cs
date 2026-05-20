// Domain/Exceptions/ExchangeRateUnavailableException.cs
namespace WexCurrencyConverter.Domain.Exceptions;

public sealed class ExchangeRateUnavailableException(string currency, DateOnly transactionDate)
    : Exception($"No exchange rate available for '{currency}' within 6 months of {transactionDate:yyyy-MM-dd}.");