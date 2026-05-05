using CheckoutKata.Api.Infrastructure.BackgroundServices;
using CheckoutKata.Api.Infrastructure.ErrorHandling;
using CheckoutKata.Application.Carts;
using CheckoutKata.Application.Pricing;

namespace CheckoutKata.Api.Composition;

internal static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddCheckoutKataApiServices(
        this IServiceCollection services,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(timeProvider);

        services.AddControllers();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalApiExceptionHandler>();
        services.AddSingleton(new CartSessionOptions
        {
            SlidingTtl = TimeSpan.FromMinutes(30),
            MaxCarts = 10_000,
            SweepInterval = TimeSpan.FromMinutes(1)
        });
        services.AddSingleton<IPricingVersionRepository, InMemoryPricingVersionRepository>();
        services.AddSingleton<CheckoutSessionService>(serviceProvider =>
        {
            var pricingVersionRepository = serviceProvider.GetRequiredService<IPricingVersionRepository>();
            var options = serviceProvider.GetRequiredService<CartSessionOptions>();
            return new CheckoutSessionService(
                pricingVersionRepository,
                timeProvider,
                options);
        });
        services.AddSingleton<ICheckoutSessionService>(serviceProvider =>
            serviceProvider.GetRequiredService<CheckoutSessionService>());
        services.AddSingleton<ICheckoutSessionMaintenance>(serviceProvider =>
            serviceProvider.GetRequiredService<CheckoutSessionService>());
        services.AddHostedService<ExpiredCartCleanupService>();

        return services;
    }
}
