using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WexCurrencyConverter.Infrastructure.Persistence;

namespace WexCurrencyConverter.IntegrationTests;

public class IntegrationTestFactory : WebApplicationFactory<Program>
{
    public string WireMockUrl { get; set; } = "http://localhost:9999";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TreasuryRatesClient:BaseAddress"] = WireMockUrl
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove EF Core registrations
            var descriptors = services
                .Where(d =>
                    d.ServiceType.FullName != null &&
                    d.ServiceType.FullName.Contains("EntityFrameworkCore"))
                .ToList();

            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            // Replace with unique in-memory database per test
            services.AddDbContext<PurchaseDbContext>(options =>
                options.UseInMemoryDatabase($"WexTestDb_{Guid.NewGuid()}"));

            // Replace IMemoryCache with fresh instance per test
            var cacheDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(IMemoryCache));

            if (cacheDescriptor != null)
                services.Remove(cacheDescriptor);

            services.AddSingleton<IMemoryCache>(
                _ => new MemoryCache(new MemoryCacheOptions()));
        });
    }
}