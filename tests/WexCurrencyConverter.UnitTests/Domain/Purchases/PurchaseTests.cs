
using WexCurrencyConverter.Domain.Purchases;
using WexCurrencyConverter.Domain.Exceptions;

namespace WexCurrencyConverter.UnitTests.Domain.Purchases;

public class PurchaseTests
{
    [Fact]
    public void CreateTransaction_WithEmptyId_ReturnsArgumentException()
    {
        var date = new DateOnly(2026, 5, 19);

        Assert.Throws<ArgumentException>(() =>
            Purchase.CreateTransaction(Guid.Empty, "Test Empty Guid", date, 6.75m));
    }

    [Fact]
    public void CreateTransaction_WithValidInputs_ReturnsExpectedPurchase()
    {
        var date = new DateOnly(2026, 5, 19);

        var purchase = Purchase.CreateTransaction(
            Guid.NewGuid(),
            "Overpriced Artisan Coffee (Oat Milk)",
            date,
            6.75m);

        Assert.Equal("Overpriced Artisan Coffee (Oat Milk)", purchase.Description);
        Assert.Equal(6.75m, purchase.AmountUsd);
        Assert.Equal(date, purchase.TransactionDate);
    }

    [Fact]
    public void CreateTransaction_WithInvalidCharacterCount_ReturnsArgumentException()
    {
        var date = new DateOnly(2026, 5, 19);
        var longDescription = new string ('a', 51);

        Assert.Throws<InvalidTransactionDescriptionException>(() =>
            Purchase.CreateTransaction(Guid.NewGuid(), longDescription, date, 6.75m));
    }

    [Fact]
    public void CreateTransaction_WithDescriptionAtMaxLength_Succeeds()
    {
        var date = new DateOnly(2026, 5, 19);
        var atLimit = new string('a', 50);

        var purchase = Purchase.CreateTransaction(Guid.NewGuid(), atLimit, date, 4.75m);

        Assert.Equal(atLimit, purchase.Description);
    }

    [Fact]
    public void CreateTransaction_TrimsDescriptionWhitespace()
    {
        var date = new DateOnly(2026, 5, 19);

        var purchase = Purchase.CreateTransaction(Guid.NewGuid(), "  Coffee  ", date, 4.75m);

        Assert.Equal("Coffee", purchase.Description);
    }

    [Fact]
    public void CreateTransaction_WithNullDescription_ThrowsArgumentOutOfRangeException()
    {
        var date = new DateOnly(2026, 5, 19);

        Assert.Throws<InvalidTransactionDescriptionException>(() =>
            Purchase.CreateTransaction(Guid.NewGuid(), "", date, 6.75m));
    }

    [Fact]
    public void CreateTransaction_WithFutureDate_ThrowsInvalidTransactionDateException()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var tomorrow = today.AddDays(1);

        Assert.Throws<InvalidTransactionDateException>(() =>
            Purchase.CreateTransaction(Guid.NewGuid(), "More Coffee", tomorrow, 4.75m));
    }

    [Fact]
    public void CreateTransaction_WithCurrentDate_ReturnsExpectedPurchase()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        var purchase = Purchase.CreateTransaction(
            Guid.NewGuid(),
            "Overpriced Artisan Coffee (Oat Milk)",
            today,
            6.75m);

        Assert.Equal("Overpriced Artisan Coffee (Oat Milk)", purchase.Description);
        Assert.Equal(6.75m, purchase.AmountUsd);
        Assert.Equal(today, purchase.TransactionDate);
    }

    [Fact]
    public void CreateTransaction_WithZeroAmount_InvalidPurchaseAmountException()
    {
        var date = new DateOnly(2026, 5, 19);

        Assert.Throws<InvalidPurchaseAmountException>(() =>
            Purchase.CreateTransaction(Guid.NewGuid(), "Matcha Oat Milk", date, 0m));
    }

    [Fact]
    public void CreateTransaction_WithNegativeAmount_ThrowsInvalidPurchaseAmountException()
    {
        var date = new DateOnly(2026, 5, 19);

        Assert.Throws<InvalidPurchaseAmountException>(() =>
            Purchase.CreateTransaction(Guid.NewGuid(), "Coffee", date, -4.75m));
    }

    [Fact]
    public void CreateTransaction_WithMoreThanTwoDecimalPlaces_ThrowsInvalidPurchaseAmountException()
    {
        var date = new DateOnly(2026, 5, 19);

        Assert.Throws<InvalidPurchaseAmountException>(() =>
            Purchase.CreateTransaction(Guid.NewGuid(), "Coffee", date, 4.755m));
    }
    
}