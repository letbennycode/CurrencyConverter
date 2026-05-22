using WexCurrencyConverter.Domain.Purchases;

namespace WexCurrencyConverter.Application.Abstractions.Interfaces;

public interface IPurchaseRepository
{
    Task<Purchase> AddAsync(Purchase purchase, CancellationToken ct);
    Task<Purchase?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<Purchase>> GetAllTransactionsAsync(CancellationToken ct);
}