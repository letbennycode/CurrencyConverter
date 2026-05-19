namespace WexCurrencyConverter.Domain.Exceptions;

public sealed class InvalidTransactionDescriptionException : PurchaseException
{
    public string Description { get; }
    
    public InvalidTransactionDescriptionException(string description, string reason)
        : base($"Description is invalid: {reason}.")
    {
        Description = description;
    }
}