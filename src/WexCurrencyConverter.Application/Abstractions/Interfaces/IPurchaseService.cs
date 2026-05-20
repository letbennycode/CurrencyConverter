using WexCurrencyConverter.Application.Models;

namespace WexCurrencyConverter.Application.Abstractions.Interfaces;

public interface IPurchaseService
{
    Task<PurchaseResponse> CreateAsync(CreatePurchaseRequest request, CancellationToken ct);
    Task<PurchaseResponse?> GetByIdAsync(Guid id, CancellationToken ct);
}