using WexCurrencyConverter.Domain.Purchases;

namespace WexCurrencyConverter.Application.Purchases;

public interface IPurchaseRepository
{
    Task<Purchase> AddAsync(Purchase purchase, CancellationToken ct);
    Task<Purchase?> GetByIdAsync(Guid id, CancellationToken ct);
}