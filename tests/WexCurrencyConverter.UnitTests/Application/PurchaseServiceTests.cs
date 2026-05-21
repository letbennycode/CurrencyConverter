using FluentAssertions;
using Moq;
using WexCurrencyConverter.Application.Abstractions.Interfaces;
using WexCurrencyConverter.Application.Models;
using WexCurrencyConverter.Application.Services;
using WexCurrencyConverter.Domain.Exceptions;
using WexCurrencyConverter.Domain.Purchases;

namespace WexCurrencyConverter.UnitTests.Application;

public sealed class PurchaseServiceTests
{
    private readonly Mock<IPurchaseRepository> _repositoryMock;
    private readonly PurchaseService _sut;
    private readonly CancellationToken _ct = CancellationToken.None;

    public PurchaseServiceTests()
    {
        _repositoryMock = new Mock<IPurchaseRepository>();
        _sut = new PurchaseService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsPurchaseResponse()
    {
        // Arrange
        var request = ValidRequest();

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Purchase>(), _ct))
            .ReturnsAsync((Purchase p, CancellationToken _) => p);

        // Act
        var result = await _sut.CreateAsync(request, _ct);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Should().NotBeNull();
        result.Description.Should().Be(request.Description);
        result.AmountUsd.Should().Be(request.AmountUsd);
        result.TransactionDate.Should().Be(request.TransactionDate);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CallsRepositoryOnce()
    {
        // Arrange
        var request = ValidRequest();

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Purchase>(), _ct))
            .ReturnsAsync((Purchase p, CancellationToken _) => p);

        // Act
        await _sut.CreateAsync(request, _ct);

        // Assert
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Purchase>(), _ct),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ZeroAmount_ThrowsInvalidPurchaseAmountException()
    {
        // Arrange
        var request = ValidRequest() with { AmountUsd = 0m };

        // Act
        var act = () => _sut.CreateAsync(request, _ct);

        // Assert
        await act.Should().ThrowAsync<InvalidPurchaseAmountException>();
    }

    [Fact]
    public async Task CreateAsync_NegativeAmount_ThrowsInvalidPurchaseAmountException()
    {
        // Arrange
        var request = ValidRequest() with { AmountUsd = -10.00m };

        // Act
        var act = () => _sut.CreateAsync(request, _ct);

        // Assert
        await act.Should().ThrowAsync<InvalidPurchaseAmountException>();
    }

    [Fact]
    public async Task CreateAsync_AmountWithMoreThanTwoDecimalPlaces_ThrowsInvalidPurchaseAmountException()
    {
        // Arrange
        var request = ValidRequest() with { AmountUsd = 10.999m };

        // Act
        var act = () => _sut.CreateAsync(request, _ct);

        // Assert
        await act.Should().ThrowAsync<InvalidPurchaseAmountException>();
    }

    [Fact]
    public async Task CreateAsync_FutureTransactionDate_ThrowsInvalidTransactionDateException()
    {
        // Arrange
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var request = ValidRequest() with { TransactionDate = tomorrow };

        // Act
        var act = () => _sut.CreateAsync(request, _ct);

        // Assert
        await act.Should().ThrowAsync<InvalidTransactionDateException>();
    }

    [Fact]
    public async Task CreateAsync_EmptyDescription_ThrowsInvalidTransactionDescriptionException()
    {
        // Arrange
        var request = ValidRequest() with { Description = string.Empty };

        // Act
        var act = () => _sut.CreateAsync(request, _ct);

        // Assert
        await act.Should().ThrowAsync<InvalidTransactionDescriptionException>();
    }

    [Fact]
    public async Task CreateAsync_DescriptionExceedsMaxLength_ThrowsInvalidTransactionDescriptionException()
    {
        // Arrange
        var request = ValidRequest() with { Description = new string('a', 51) };

        // Act
        var act = () => _sut.CreateAsync(request, _ct);

        // Assert
        await act.Should().ThrowAsync<InvalidTransactionDescriptionException>();
    }

    private static CreatePurchaseRequest ValidRequest() => new()
    {
        Description = "Fancy Scone Inc LLC",
        TransactionDate = new DateOnly(2026, 5, 19),
        AmountUsd = 49.99m
    };
}