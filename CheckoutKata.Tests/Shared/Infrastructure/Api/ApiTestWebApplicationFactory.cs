using CheckoutKata.Application.Carts;
using CheckoutKata.Application.Persistence;
using CheckoutKata.Tests.Shared.Infrastructure.Time;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CheckoutKata.Tests.Shared.Infrastructure.Api;

internal sealed class ApiTestWebApplicationFactory(
    TestTimeProvider timeProvider,
    CartSessionOptions? sessionOptions = null) : WebApplicationFactory<Program>
{
    public TestTimeProvider TimeProvider { get; } = timeProvider;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextFactory<CheckoutKataDbContext>>();
            services.RemoveAll<ICheckoutSessionService>();
            services.RemoveAll<ICheckoutSessionMaintenance>();
            services.RemoveAll<CheckoutSessionService>();
            services.RemoveAll<CartSessionOptions>();

            services.AddDbContextFactory<CheckoutKataDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString("N")));

            services.AddSingleton(sessionOptions ?? new CartSessionOptions());
            services.AddSingleton<CheckoutSessionService>(serviceProvider =>
            {
                var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<CheckoutKataDbContext>>();
                var options = serviceProvider.GetRequiredService<CartSessionOptions>();
                return new CheckoutSessionService(
                    dbContextFactory,
                    TimeProvider,
                    options);
            });

            services.AddSingleton<ICheckoutSessionService>(serviceProvider =>
                serviceProvider.GetRequiredService<CheckoutSessionService>());

            services.AddSingleton<ICheckoutSessionMaintenance>(serviceProvider =>
                serviceProvider.GetRequiredService<CheckoutSessionService>());
        });
    }
}
