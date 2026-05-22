using WexCurrencyConverter.Domain.Exceptions;

namespace WexCurrencyConverter.Domain.Purchases;

public sealed class Purchase
{
    public Guid Id { get; private set; }
    public string Description { get; private set; }
    public DateOnly TransactionDate { get; private set; }
    public decimal AmountUsd { get; private set; }

    private const int MaxDescriptionLength = 50;

    private Purchase(Guid id, string description, DateOnly transactionDate, decimal amountUsd)
    {
        Id = id;
        Description = description;
        TransactionDate = transactionDate;
        AmountUsd = amountUsd;
    }

    public static Purchase Create(
        Guid id,
        string description,
        DateOnly transactionDate,
        decimal amountUsd)
    {
        if (Guid.Empty == id)
        {
            throw new ArgumentException("Transaction id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new InvalidTransactionDescriptionException(description, "Description cannot be empty. Please provide a valid description.");
        }

        var descriptionTrimmed = description.Trim();

        if (descriptionTrimmed.Length > MaxDescriptionLength)
        {
            throw new InvalidTransactionDescriptionException(description, $"Description cannot exceed {MaxDescriptionLength} characters.");
        }

        if (transactionDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new InvalidTransactionDateException(transactionDate);
        }

        if (amountUsd <= 0)
        {
            throw new InvalidPurchaseAmountException(amountUsd, "Purchase amount must be positive.");
        }

        if (decimal.Round(amountUsd, 2) != amountUsd)
        {
            throw new InvalidPurchaseAmountException(amountUsd, "Cannot have more than 2 decimal places.");
        }

        return new Purchase(id, descriptionTrimmed, transactionDate, amountUsd);
    }
}
