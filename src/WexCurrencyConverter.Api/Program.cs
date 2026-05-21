using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WexCurrencyConverter.Api.Middleware;
using WexCurrencyConverter.Application.Abstractions.Interfaces;
using WexCurrencyConverter.Application.Services;
using WexCurrencyConverter.Infrastructure.ExternalServices;
using WexCurrencyConverter.Infrastructure.Persistence;
using WexCurrencyConverter.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// --- Controllers ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Exception Handling ---
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

// --- Application ---
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<ICurrencyConversionService, CurrencyConversionService>();

// --- Infrastructure ---
builder.Services.AddDbContext<PurchaseDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddOptions<TreasuryRatesOptions>()
    .BindConfiguration(TreasuryRatesOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();

builder.Services.AddMemoryCache();

builder.Services
    .AddHttpClient<ITreasuryRatesClient, TreasuryRatesClient>((serviceProvider, c) =>
    {
        var treasuryOptions = serviceProvider.GetRequiredService<IOptions<TreasuryRatesOptions>>().Value;
        c.BaseAddress = new Uri(treasuryOptions.BaseAddress);
        c.Timeout = TimeSpan.FromSeconds(treasuryOptions.TimeoutSeconds);
    })
    .AddStandardResilienceHandler(o =>
    {
        var retryCount = builder.Configuration
            .GetSection(TreasuryRatesOptions.SectionName)
            .GetValue<int?>("RetryCount") ?? new TreasuryRatesOptions().RetryCount;
        o.Retry.MaxRetryAttempts = retryCount;
    });

var app = builder.Build();

// --- Database Migration ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PurchaseDbContext>();

    if (db.Database.IsRelational())    // Skip for in-memory provider used in tests
        db.Database.Migrate();
}

// --- Middleware Pipeline ---
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();