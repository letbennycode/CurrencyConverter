using WexCurrencyConverter.Domain.Purchases;

namespace WexCurrencyConverter.Application.Models;

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