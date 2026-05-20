using System.ComponentModel.DataAnnotations;

namespace WexCurrencyConverter.Application.Models;

public record CreatePurchaseRequest
{
    [Required]
    public string Description { get; init; } = string.Empty;

    [Required]
    public DateOnly TransactionDate { get; init; }

    [Required]
    public decimal AmountUsd { get; init; }
}