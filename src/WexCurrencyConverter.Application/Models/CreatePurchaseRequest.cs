using System.ComponentModel.DataAnnotations;

namespace WexCurrencyConverter.Application.Models;

/// <summary>
/// Request body for creating a new purchase transaction.
/// </summary>
public record CreatePurchaseRequest
{
    /// <summary>A brief description of the purchase (max 50 characters).</summary>
    [Required]
    public string Description { get; init; } = string.Empty;

    /// <summary>The date the transaction occurred.</summary>
    [Required]
    public DateOnly TransactionDate { get; init; }

    /// <summary>The purchase amount in US dollars, rounded to two decimal places.</summary>
    [Required]
    public decimal AmountUsd { get; init; }
}
