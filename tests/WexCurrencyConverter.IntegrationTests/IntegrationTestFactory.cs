using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WexCurrencyConverter.Infrastructure.Persistence;

namespace WexCurrencyConverter.IntegrationTests;

public class IntegrationTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
       {
           // Remove ALL EF Core related registrations
           var descriptors = services
               .Where(d =>
                   d.ServiceType.FullName != null &&
                   d.ServiceType.FullName.Contains("EntityFrameworkCore"))
               .ToList();

           foreach (var descriptor in descriptors)
               services.Remove(descriptor);

           // Replace with in-memory database
           services.AddDbContext<PurchaseDbContext>(options =>
               options.UseInMemoryDatabase("WexTestDb"));
       });
    }
}