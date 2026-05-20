namespace WexCurrencyConverter.Domain.Exceptions;

public sealed class PurchaseNotFoundException(Guid id)
    : PurchaseException($"Purchase '{id}' was not found.");