using WexCurrencyConverter.Application.Models;

namespace WexCurrencyConverter.Application.Abstractions.Interfaces;

public interface IPurchaseService
{
    Task<PurchaseResponse> CreateAsync(CreatePurchaseRequest request, CancellationToken ct);
    Task<decimal> GetMonthlyTotal(DateOnly from, DateOnly to, CancellationToken ct);
}