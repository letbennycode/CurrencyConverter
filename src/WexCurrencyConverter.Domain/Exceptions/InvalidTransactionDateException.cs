namespace WexCurrencyConverter.Domain.Exceptions;

public sealed class InvalidTransactionDateException : PurchaseException
{
    public DateOnly AttemptedDate { get; }

    public InvalidTransactionDateException(DateOnly attemptedDate)
        : base($"Transaction date {attemptedDate:yyyy-MM-dd} cannot be in the future.")
    {
        AttemptedDate = attemptedDate;
    }
}