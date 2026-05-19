namespace WexCurrencyConverter.Domain.Exceptions;

public abstract class PurchaseException : Exception
{
    protected PurchaseException(string message) : base(message) { }
    protected PurchaseException(string message, Exception inner) : base(message, inner) { }
}