using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using WexCurrencyConverter.Infrastructure.Persistence;

namespace WexCurrencyConverter.IntegrationTests;

public class IntegrationTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptors = services
                .Where(d =>
                    d.ServiceType.FullName != null &&
                    d.ServiceType.FullName.Contains("EntityFrameworkCore"))
                .ToList();

            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            var dbName = $"WexTestDb_{Guid.NewGuid()}";
            services.AddDbContext<PurchaseDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            var cacheDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(IMemoryCache));

            if (cacheDescriptor != null)
                services.Remove(cacheDescriptor);

            services.AddSingleton<IMemoryCache>(
                _ => new MemoryCache(new MemoryCacheOptions()));
        });
    }
}