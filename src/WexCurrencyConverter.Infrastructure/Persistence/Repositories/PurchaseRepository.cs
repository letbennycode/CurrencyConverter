using Microsoft.EntityFrameworkCore;
using WexCurrencyConverter.Application.Abstractions.Interfaces;
using WexCurrencyConverter.Domain.Purchases;

namespace WexCurrencyConverter.Infrastructure.Persistence.Repositories;

public class PurchaseRepository : IPurchaseRepository
{
    private readonly PurchaseDbContext _db;

    public PurchaseRepository(PurchaseDbContext db) => _db = db;

    public async Task<Purchase> AddAsync(Purchase purchase, CancellationToken ct)
    {
        _db.Purchases.Add(purchase);
        await _db.SaveChangesAsync(ct);
        return purchase;
    }

    public Task<Purchase?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Purchases.FirstOrDefaultAsync(p => p.Id == id, ct);
}