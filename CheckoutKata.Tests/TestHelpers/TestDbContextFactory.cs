using CheckoutKata.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CheckoutKata.Tests.TestHelpers;

internal static class TestDbContextFactory
{
    public static IDbContextFactory<CheckoutKataDbContext> Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<CheckoutKataDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString("N"))
            .Options;

        return new StaticOptionsDbContextFactory(options);
    }

    private sealed class StaticOptionsDbContextFactory(DbContextOptions<CheckoutKataDbContext> options)
        : IDbContextFactory<CheckoutKataDbContext>
    {
        public CheckoutKataDbContext CreateDbContext() => new(options);

        public Task<CheckoutKataDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new CheckoutKataDbContext(options));
    }
}
