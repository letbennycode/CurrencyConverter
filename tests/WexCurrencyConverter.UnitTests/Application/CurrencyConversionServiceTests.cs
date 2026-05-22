using FluentAssertions;
using NSubstitute;
using WexCurrencyConverter.Application.Abstractions.Interfaces;
using WexCurrencyConverter.Application.Models;
using WexCurrencyConverter.Application.Services;
using WexCurrencyConverter.Domain.Exceptions;
using WexCurrencyConverter.Domain.Purchases;

namespace WexCurrencyConverter.UnitTests.Application;

public sealed class CurrencyConversionServiceTests
{
    private readonly IPurchaseRepository _repository = Substitute.For<IPurchaseRepository>();
    private readonly ITreasuryRatesClient _client = Substitute.For<ITreasuryRatesClient>();
    private readonly CurrencyConversionService _sut;

    public CurrencyConversionServiceTests()
    {
        _sut = new CurrencyConversionService(_repository, _client);
    }

    [Fact]
    public async Task ConvertAsync_ValidRequest_ReturnsCorrectConvertedAmount()
    {
        // Arrange
        var purchase = Purchase.Create(Guid.NewGuid(), "Test purchase", new DateOnly(2024, 1, 15), 100m);
        var rate = new ExchangeRate("Canada-Dollar", 1.35m, new DateOnly(2024, 1, 1));

        _repository.GetByIdAsync(purchase.Id, Arg.Any<CancellationToken>())
            .Returns(purchase);

        _client.GetRateAsync(
                Arg.Any<string>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(rate);

        // Act
        var result = await _sut.ConvertAsync(purchase.Id, "Canada-Dollar", CancellationToken.None);

        // Assert
        result.ConvertedAmount.Should().Be(135.00m);
        result.ExchangeRate.Should().Be(1.35m);
        result.Currency.Should().Be("Canada-Dollar");
        result.AmountUsd.Should().Be(100m);
    }

    [Fact]
    public async Task ConvertAsync_PurchaseNotFound_ThrowsPurchaseNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Purchase?)null);

        // Act
        var act = () => _sut.ConvertAsync(id, "Canada-Dollar", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<PurchaseNotFoundException>();
    }

    [Fact]
    public async Task ConvertAsync_RateUnavailable_ThrowsExchangeRateUnavailableException()
    {
        // Arrange
        var purchase = Purchase.Create(Guid.NewGuid(), "Test purchase", new DateOnly(2024, 1, 15), 100m);
        _repository.GetByIdAsync(purchase.Id, Arg.Any<CancellationToken>())
            .Returns(purchase);

        _client.GetRateAsync(
                Arg.Any<string>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns((ExchangeRate?)null);

        // Act
        var act = () => _sut.ConvertAsync(purchase.Id, "Canada-Dollar", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ExchangeRateUnavailableException>();
    }

    [Fact]
    public async Task ConvertAsync_ValidRequest_PassesCorrectDateWindowToClient()
    {
        // Arrange
        var transactionDate = new DateOnly(2024, 1, 15);
        var purchase = Purchase.Create(Guid.NewGuid(), "Test purchase", transactionDate, 100m);
        var rate = new ExchangeRate("Canada-Dollar", 1.35m, transactionDate);

        _repository.GetByIdAsync(purchase.Id, Arg.Any<CancellationToken>())
            .Returns(purchase);

        _client.GetRateAsync(
                Arg.Any<string>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(rate);

        // Act
        await _sut.ConvertAsync(purchase.Id, "Canada-Dollar", CancellationToken.None);

        // Assert
        await _client.Received(1).GetRateAsync(
            "Canada-Dollar",
            transactionDate,
            transactionDate.AddMonths(-6),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConvertAsync_MidpointAmount_RoundsProperlyToTwoDecimals()
    {
        var purchase = Purchase.Create(Guid.NewGuid(), "Test purchase", new DateOnly(2024, 1, 15), 100m);
        var rate = new ExchangeRate("Canada-Dollar", 1.005m, new DateOnly(2024, 1, 1));

        _repository.GetByIdAsync(purchase.Id, Arg.Any<CancellationToken>())
            .Returns(purchase);

        _client.GetRateAsync(
                Arg.Any<string>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(rate);

        // Act
        var result = await _sut.ConvertAsync(purchase.Id, "Canada-Dollar", CancellationToken.None);

        // Assert
        result.ConvertedAmount.Should().Be(100.50m);
    }

    [Fact]
    public async Task ConvertAsync_ValidRequest_ResponseContainsAllPurchaseFields()
    {
        // Arrange
        var transactionDate = new DateOnly(2024, 1, 15);
        var purchase = Purchase.Create(Guid.NewGuid(), "Coffee Run", transactionDate, 50m);
        var rateDate = new DateOnly(2024, 1, 1);
        var rate = new ExchangeRate("Canada-Dollar", 1.35m, rateDate);

        _repository.GetByIdAsync(purchase.Id, Arg.Any<CancellationToken>())
            .Returns(purchase);

        _client.GetRateAsync(
                Arg.Any<string>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(rate);

        // Act
        var result = await _sut.ConvertAsync(purchase.Id, "Canada-Dollar", CancellationToken.None);

        // Assert
        result.Id.Should().Be(purchase.Id);
        result.Description.Should().Be("Coffee Run");
        result.TransactionDate.Should().Be(transactionDate);
        result.AmountUsd.Should().Be(50m);
        result.ExchangeRateDate.Should().Be(rateDate);
    }
}