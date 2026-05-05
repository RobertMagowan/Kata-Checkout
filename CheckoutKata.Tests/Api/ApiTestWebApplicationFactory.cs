using CheckoutKata.Application.Pricing;
using CheckoutKata.Application.Carts;
using CheckoutKata.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CheckoutKata.Tests.Api;

internal sealed class ApiTestWebApplicationFactory(
    TestTimeProvider timeProvider,
    CartSessionOptions? sessionOptions = null) : WebApplicationFactory<Program>
{
    public TestTimeProvider TimeProvider { get; } = timeProvider;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IPricingVersionRepository>();
            services.RemoveAll<ICheckoutSessionService>();
            services.RemoveAll<ICheckoutSessionMaintenance>();
            services.RemoveAll<CheckoutSessionService>();
            services.RemoveAll<CartSessionOptions>();

            services.AddSingleton<IPricingVersionRepository, InMemoryPricingVersionRepository>();
            services.AddSingleton(sessionOptions ?? new CartSessionOptions());
            services.AddSingleton<CheckoutSessionService>(serviceProvider =>
            {
                var pricingRulesRepository = serviceProvider.GetRequiredService<IPricingVersionRepository>();
                var options = serviceProvider.GetRequiredService<CartSessionOptions>();
                return new CheckoutSessionService(
                    pricingRulesRepository,
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
