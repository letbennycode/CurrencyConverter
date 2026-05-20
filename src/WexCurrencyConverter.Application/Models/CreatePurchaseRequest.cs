using System.ComponentModel.DataAnnotations;

namespace WexCurrencyConverter.Application.Models;

public record CreatePurchaseRequest
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Description { get; init; } = string.Empty;

    [Required]
    public DateOnly TransactionDate { get; init; }

    [Required]
    [Range(0.01, 9999999999999999.99)]
    public decimal AmountUsd { get; init; }
}