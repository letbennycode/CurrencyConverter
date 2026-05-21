using WexCurrencyConverter.Application.Abstractions.Interfaces;
using WexCurrencyConverter.Application.Models;
using WexCurrencyConverter.Domain.Purchases;

namespace WexCurrencyConverter.Application.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IPurchaseRepository _repository;

    public PurchaseService(IPurchaseRepository repository)
    {
        _repository = repository;
    }

    public async Task<PurchaseResponse> CreateAsync(
        CreatePurchaseRequest request,
        CancellationToken ct)
    {
        var purchase = Purchase.CreateTransaction(
            Guid.NewGuid(),
            request.Description,
            request.TransactionDate,
            request.AmountUsd
        );

        var saved = await _repository.AddAsync(purchase, ct);

        return PurchaseResponse.FromEntity(saved);
    }
}