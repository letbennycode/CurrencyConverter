using WexCurrencyConverter.Domain.Purchases;

namespace WexCurrencyConverter.Application.Models;

/// <summary>
/// Represents a purchase as it was stored (no currency conversion applied.)
/// </summary>
/// <param name="Id">Unique identifier assigned to the purchase.</param>
/// <param name="Description">Description provided at the time of creation.</param>
/// <param name="TransactionDate">Date the transaction took place.</param>
/// <param name="AmountUsd">Original purchase amount in US dollars.</param>
public record PurchaseResponse(
    Guid Id,
    string Description,
    DateOnly TransactionDate,
    decimal AmountUsd
)
{
    public static PurchaseResponse FromEntity(Purchase purchase) => new(
        purchase.Id,
        purchase.Description,
        purchase.TransactionDate,
        purchase.AmountUsd
    );
}
