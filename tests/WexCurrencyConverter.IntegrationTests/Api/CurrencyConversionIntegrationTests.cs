using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WexCurrencyConverter.Application.Models;
using Microsoft.Extensions.Configuration;
using FluentAssertions;

namespace WexCurrencyConverter.IntegrationTests.Api;

public sealed class CurrencyConversionIntegrationTests : IAsyncLifetime
{
    private WireMockServer _wireMock = null!;
    private HttpClient _client = null!;
    private WebApplicationFactory<Program> _factory = null!;

    private string _dbPath = null!;

    public Task InitializeAsync()
    {
        _wireMock = WireMockServer.Start();
        _dbPath = $"test_{Guid.NewGuid()}.db";

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config)
    =>
                {
                    config.AddInMemoryCollection(new
    Dictionary<string, string?>
                    {
                        ["TreasuryRatesClient:BaseAddress"] =
    _wireMock.Url!,
                        ["ConnectionStrings:DefaultConnection"]
     = $"Data Source={_dbPath}"
                    });
                });
            });

        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _wireMock.Stop();
        _client.Dispose();
        await _factory.DisposeAsync();
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }

    [Fact]
    public async Task GetConversion_HappyPath_ReturnsConvertedAmount()
    {
        // Arrange
        _wireMock
            .Given(Request.Create()
                .WithPath("/services/api/fiscal_service/v1/accounting/od/rates_of_exchange")
                .WithParam("filter", WireMock.Matchers.MatchBehaviour.AcceptOnMatch)
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(File.ReadAllText("TestData/CanadaResult.json")));

        // POST a purchase
        var createRequest = new
        {
            description = "Coffee + Bagel Co",
            transactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            amountUsd = 100.00
        };

        var postResponse = await _client.PostAsJsonAsync("/api/purchase", createRequest);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var purchase = await postResponse.Content
            .ReadFromJsonAsync<PurchaseResponse>();

        // GET the conversion
        var getResponse = await _client
            .GetAsync($"/api/purchase/{purchase!.Id}?currency=Canada-Dollar");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await getResponse.Content
            .ReadFromJsonAsync<ConvertedPurchaseResponse>();

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(purchase.Id);
        result.Description.Should().Be("Coffee + Bagel Co");
        result.AmountUsd.Should().Be(100.00m);
        result.Currency.Should().Be("Canada-Dollar");
        result.ExchangeRate.Should().Be(1.326m);
        result.ExchangeRateDate.Should().Be(new DateOnly(2023, 12, 31));
        result.ConvertedAmount.Should().Be(132.60m);
    }

    [Fact]
    public async Task GetConversion_NoRateInWindow_Returns422()
    {
        // Arrange
        _wireMock
            .Given(Request.Create()
                .WithPath("/services/api/fiscal_service/v1/accounting/od/rates_of_exchange")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(File.ReadAllText("TestData/OutdatedResult.json")));

        // POST a purchase
        var createRequest = new
        {
            description = "Coffee + Bagel Co",
            transactionDate = new DateOnly(2024, 1, 15),
            amountUsd = 100.00
        };

        var postResponse = await _client.PostAsJsonAsync("/api/purchase", createRequest);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var purchase = await postResponse.Content
            .ReadFromJsonAsync<PurchaseResponse>();

        // GET the conversion
        var getResponse = await _client
            .GetAsync($"/api/purchase/{purchase!.Id}?currency=Canada-Dollar");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}