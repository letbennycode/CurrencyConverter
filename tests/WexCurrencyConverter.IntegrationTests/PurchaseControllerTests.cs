using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WexCurrencyConverter.Application.Models;
using WexCurrencyConverter.Infrastructure.Persistence;

namespace WexCurrencyConverter.IntegrationTests;

public class PurchaseControllerTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFactory _factory;

    public PurchaseControllerTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Validate happy path - 201
    [Fact]
    public async Task PostPurchase_ValidRequest_Returns201WithMatchingBody()
    {
        // Arrange
        var request = ValidRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/purchase", request);
        var body = await response.Content.ReadFromJsonAsync<PurchaseResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body.Should().NotBeNull();
        body!.Description.Should().Be(request.Description);
        body.AmountUsd.Should().Be(request.AmountUsd);
        body.TransactionDate.Should().Be(request.TransactionDate);
        body.Id.Should().NotBeEmpty();
    }

    // Validate any failures
    [Fact]
    public async Task PostPurchase_MissingDescription_Returns400()
    {
        // Arrange
        var request = ValidRequest() with { Description = string.Empty };

        // Act
        var response = await _client.PostAsJsonAsync("/api/purchase", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostPurchase_DescriptionExceedsMaxLength_Returns422()
    {
        // Arrange
        var request = ValidRequest() with { Description = new string('a', 51) };

        // Act
        var response = await _client.PostAsJsonAsync("/api/purchase", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PostPurchase_ZeroAmount_Returns422()
    {
        // Arrange
        var request = ValidRequest() with { AmountUsd = 0m };

        // Act
        var response = await _client.PostAsJsonAsync("/api/purchase", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PostPurchase_NegativeAmount_Returns422()
    {
        // Arrange
        var request = ValidRequest() with { AmountUsd = -10.00m };

        // Act
        var response = await _client.PostAsJsonAsync("/api/purchase", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PostPurchase_AmountWithMoreThanTwoDecimalPlaces_Returns422()
    {
        // Arrange
        var request = ValidRequest() with { AmountUsd = 10.999m };

        // Act
        var response = await _client.PostAsJsonAsync("/api/purchase", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PostPurchase_FutureTransactionDate_Returns422()
    {
        // Arrange
        var request = ValidRequest() with { TransactionDate = DateOnly.FromDateTime(DateTime.Now).AddDays(1) };

        // Act
        var response = await _client.PostAsJsonAsync("/api/purchase", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // Verify data persistence to DB
    [Fact]
    public async Task PostPurchase_ValidRequest_PersistsToDatabase()
    {
        // Arrange
        var request = ValidRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/purchase", request);
        var body = await response.Content.ReadFromJsonAsync<PurchaseResponse>();

        // Assert — resolve the DbContext directly and query for the record
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PurchaseDbContext>();

        var saved = await db.Purchases.FindAsync(body!.Id);

        saved.Should().NotBeNull();
        saved!.Description.Should().Be(request.Description);
        saved.AmountUsd.Should().Be(request.AmountUsd);
        saved.TransactionDate.Should().Be(request.TransactionDate);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static CreatePurchaseRequest ValidRequest() => new()
    {
        Description = "Powdered French Toast (Sensing a Theme Here)",
        TransactionDate = DateOnly.FromDateTime(DateTime.Now),
        AmountUsd = 49.99m
    };
}