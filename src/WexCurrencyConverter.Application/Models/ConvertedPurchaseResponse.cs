namespace WexCurrencyConverter.Application.Models;

/// <summary>
/// A purchase with its amount converted to a requested foreign currency.
/// </summary>
/// <param name="Id">Unique identifier of the purchase.</param>
/// <param name="Description">Description of the purchase.</param>
/// <param name="TransactionDate">Date the transaction took place.</param>
/// <param name="AmountUsd">Original amount in US dollars.</param>
/// <param name="Currency">The currency the amount was converted to.</param>
/// <param name="ExchangeRate">The Treasury exchange rate applied during conversion.</param>
/// <param name="ExchangeRateDate">The date the exchange rate was published.</param>
/// <param name="ConvertedAmount">The purchase amount in the target currency, rounded to two decimal places.</param>
public sealed record ConvertedPurchaseResponse(
    Guid Id,
    string Description,
    DateOnly TransactionDate,
    decimal AmountUsd,
    string Currency,
    decimal ExchangeRate,
    DateOnly ExchangeRateDate,
    decimal ConvertedAmount);
