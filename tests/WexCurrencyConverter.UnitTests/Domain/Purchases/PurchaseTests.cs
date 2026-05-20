using WexCurrencyConverter.Domain.Purchases;
using WexCurrencyConverter.Domain.Exceptions;
using FluentAssertions;

namespace WexCurrencyConverter.UnitTests.Domain.Purchases;

public class PurchaseTests
{
    [Fact]
    public void CreateTransaction_WithEmptyId_ReturnsArgumentException()
    {
        var date = new DateOnly(2026, 5, 19);

        Action act = () => Purchase.CreateTransaction(Guid.Empty, "Test Empty Guid", date, 6.75m);

        act.Should().Throw<ArgumentException>();
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

        purchase.Description.Should().Be("Overpriced Artisan Coffee (Oat Milk)");
        purchase.AmountUsd.Should().Be(6.75m);
        purchase.TransactionDate.Should().Be(date);
    }

    [Fact]
    public void CreateTransaction_WithInvalidCharacterCount_ReturnsArgumentException()
    {
        var date = new DateOnly(2026, 5, 19);
        var longDescription = new string('a', 51);

        Action act = () => Purchase.CreateTransaction(Guid.NewGuid(), longDescription, date, 6.75m);

        act.Should().Throw<InvalidTransactionDescriptionException>();
    }

    [Fact]
    public void CreateTransaction_WithDescriptionAtMaxLength_Succeeds()
    {
        var date = new DateOnly(2026, 5, 19);
        var atLimit = new string('a', 50);

        var purchase = Purchase.CreateTransaction(Guid.NewGuid(), atLimit, date, 4.75m);

        purchase.Description.Should().Be(atLimit);
    }

    [Fact]
    public void CreateTransaction_TrimsDescriptionWhitespace()
    {
        var date = new DateOnly(2026, 5, 19);

        var purchase = Purchase.CreateTransaction(Guid.NewGuid(), "  Coffee  ", date, 4.75m);

        purchase.Description.Should().Be("Coffee");
    }

    [Fact]
    public void CreateTransaction_WithNullDescription_ThrowsArgumentOutOfRangeException()
    {
        var date = new DateOnly(2026, 5, 19);

        Action act = () => Purchase.CreateTransaction(Guid.NewGuid(), "", date, 6.75m);

        act.Should().Throw<InvalidTransactionDescriptionException>();
    }

    [Fact]
    public void CreateTransaction_WithFutureDate_ThrowsInvalidTransactionDateException()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var tomorrow = today.AddDays(1);

        Action act = () => Purchase.CreateTransaction(Guid.NewGuid(), "More Coffee", tomorrow, 4.75m);

        act.Should().Throw<InvalidTransactionDateException>();
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

        purchase.Description.Should().Be("Overpriced Artisan Coffee (Oat Milk)");
        purchase.AmountUsd.Should().Be(6.75m);
        purchase.TransactionDate.Should().Be(today);
    }

    [Fact]
    public void CreateTransaction_WithZeroAmount_InvalidPurchaseAmountException()
    {
        var date = new DateOnly(2026, 5, 19);

        Action act = () => Purchase.CreateTransaction(Guid.NewGuid(), "Matcha Oat Milk", date, 0m);

        act.Should().Throw<InvalidPurchaseAmountException>();
    }

    [Fact]
    public void CreateTransaction_WithNegativeAmount_ThrowsInvalidPurchaseAmountException()
    {
        var date = new DateOnly(2026, 5, 19);

        Action act = () => Purchase.CreateTransaction(Guid.NewGuid(), "Coffee", date, -4.75m);

        act.Should().Throw<InvalidPurchaseAmountException>();
    }

    [Fact]
    public void CreateTransaction_WithMoreThanTwoDecimalPlaces_ThrowsInvalidPurchaseAmountException()
    {
        var date = new DateOnly(2026, 5, 19);

        Action act = () => Purchase.CreateTransaction(Guid.NewGuid(), "Coffee", date, 4.755m);

        act.Should().Throw<InvalidPurchaseAmountException>();
    }
}