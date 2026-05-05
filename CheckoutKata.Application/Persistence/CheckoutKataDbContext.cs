using Microsoft.EntityFrameworkCore;

namespace CheckoutKata.Application.Persistence;

public sealed class CheckoutKataDbContext(DbContextOptions<CheckoutKataDbContext> options) : DbContext(options)
{
    public DbSet<PricingVersionEntity> PricingVersions => Set<PricingVersionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PricingVersionEntity>(builder =>
        {
            builder.HasKey(version => version.Id);
            builder.Property(version => version.RulesJson).IsRequired();
        });
    }
}
