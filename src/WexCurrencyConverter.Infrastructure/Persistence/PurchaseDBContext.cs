using Microsoft.EntityFrameworkCore;
using WexCurrencyConverter.Domain.Purchases;

namespace WexCurrencyConverter.Infrastructure.Persistence;

public class PurchaseDbContext : DbContext
{
    public PurchaseDbContext(DbContextOptions<PurchaseDbContext> options)
        : base(options)
    {
    }

    public DbSet<Purchase> Purchases => Set<Purchase>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Description)
                  .IsRequired()
                  .HasMaxLength(50);
            entity.Property(p => p.TransactionDate)
                  .IsRequired();
            entity.Property(p => p.AmountUsd)
                  .IsRequired()
                  .HasColumnType("decimal(19,2)");
        });
    }
}