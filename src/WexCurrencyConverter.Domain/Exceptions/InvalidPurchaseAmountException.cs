namespace WexCurrencyConverter.Domain.Exceptions;

public sealed class InvalidPurchaseAmountException : PurchaseException
{
    public decimal AttemptedAmount { get; }

    public InvalidPurchaseAmountException(decimal attemptedAmount, string reason)
        : base($"Transaction amount {attemptedAmount} is invalid: {reason}.")
    {
        AttemptedAmount = attemptedAmount;
    }
}