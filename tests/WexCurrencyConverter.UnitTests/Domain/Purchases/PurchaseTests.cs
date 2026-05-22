using WexCurrencyConverter.Domain.Purchases;
using WexCurrencyConverter.Domain.Exceptions;
using FluentAssertions;

namespace WexCurrencyConverter.UnitTests.Domain.Purchases;

public class PurchaseTests
{

    [Fact]
    public void Create_WithValidInputs_ReturnsExpectedPurchase()
    {
        var date = new DateOnly(2026, 5, 19);

        var purchase = Purchase.Create(
            Guid.NewGuid(),
            "Overpriced Artisan Coffee (Oat Milk)",
            date,
            6.75m);

        purchase.Description.Should().Be("Overpriced Artisan Coffee (Oat Milk)");
        purchase.AmountUsd.Should().Be(6.75m);
        purchase.TransactionDate.Should().Be(date);
    }

    [Fact]
    public void Create_WithEmptyId_ThrowsArgumentException()
    {
        var date = new DateOnly(2026, 5, 19);

        Action act = () => Purchase.Create(Guid.Empty, "Test Empty Guid", date, 6.75m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithInvalidCharacterCount_ThrowsArgumentException()
    {
        var date = new DateOnly(2026, 5, 19);
        var longDescription = new string('a', 51);

        Action act = () => Purchase.Create(Guid.NewGuid(), longDescription, date, 6.75m);

        act.Should().Throw<InvalidTransactionDescriptionException>();
    }

    [Fact]
    public void Create_WithDescriptionAtMaxLength_Succeeds()
    {
        var date = new DateOnly(2026, 5, 19);
        var atLimit = new string('a', 50);

        var purchase = Purchase.Create(Guid.NewGuid(), atLimit, date, 4.75m);

        purchase.Description.Should().Be(atLimit);
    }

    [Fact]
    public void Create_TrimsDescriptionWhitespace()
    {
        var date = new DateOnly(2026, 5, 19);

        var purchase = Purchase.Create(Guid.NewGuid(), "  Coffee  ", date, 4.75m);

        purchase.Description.Should().Be("Coffee");
    }

    [Fact]
    public void Create_WithEmptyDescription_ThrowsInvalidTransactionDescriptionException()
    {
        var date = new DateOnly(2026, 5, 19);

        Action act = () => Purchase.Create(Guid.NewGuid(), "", date, 6.75m);

        act.Should().Throw<InvalidTransactionDescriptionException>();
    }

    [Fact]
    public void Create_WithFutureDate_ThrowsInvalidTransactionDateException()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tomorrow = today.AddDays(1);

        Action act = () => Purchase.Create(Guid.NewGuid(), "More Coffee", tomorrow, 4.75m);

        act.Should().Throw<InvalidTransactionDateException>();
    }

    [Fact]
    public void Create_WithCurrentDate_ReturnsExpectedPurchase()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var purchase = Purchase.Create(
            Guid.NewGuid(),
            "Overpriced Artisan Coffee (Oat Milk)",
            today,
            6.75m);

        purchase.Description.Should().Be("Overpriced Artisan Coffee (Oat Milk)");
        purchase.AmountUsd.Should().Be(6.75m);
        purchase.TransactionDate.Should().Be(today);
    }

    [Fact]
    public void Create_WithZeroAmount_ThrowsInvalidPurchaseAmountException()
    {
        var date = new DateOnly(2026, 5, 19);

        Action act = () => Purchase.Create(Guid.NewGuid(), "Matcha Oat Milk", date, 0m);

        act.Should().Throw<InvalidPurchaseAmountException>();
    }

    [Fact]
    public void Create_WithNegativeAmount_ThrowsInvalidPurchaseAmountException()
    {
        var date = new DateOnly(2026, 5, 19);

        Action act = () => Purchase.Create(Guid.NewGuid(), "Coffee", date, -4.75m);

        act.Should().Throw<InvalidPurchaseAmountException>();
    }

    [Fact]
    public void Create_WithMoreThanTwoDecimalPlaces_ThrowsInvalidPurchaseAmountException()
    {
        var date = new DateOnly(2026, 5, 19);

        Action act = () => Purchase.Create(Guid.NewGuid(), "Coffee", date, 4.755m);

        act.Should().Throw<InvalidPurchaseAmountException>();
    }
}