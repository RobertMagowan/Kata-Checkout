using CheckoutKata.Api.Infrastructure.BackgroundServices;
using CheckoutKata.Api.Infrastructure.ErrorHandling;
using CheckoutKata.Application.Carts;
using CheckoutKata.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CheckoutKata.Api.Composition;

internal static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddCheckoutKataApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(timeProvider);

        var pricingStoreDatabaseName = configuration.GetValue<string>("Checkout:PricingStore:DatabaseName")
            ?? "CheckoutKataPricing";
        var cartSessionOptions = new CartSessionOptions
        {
            SlidingTtl = configuration.GetValue<TimeSpan?>("Checkout:Session:SlidingTtl") ?? TimeSpan.FromMinutes(30),
            MaxCarts = configuration.GetValue<int?>("Checkout:Session:MaxCarts") ?? 10_000,
            SweepInterval = configuration.GetValue<TimeSpan?>("Checkout:Session:SweepInterval") ?? TimeSpan.FromMinutes(1),
            AbsoluteMaxAge = configuration.GetValue<TimeSpan?>("Checkout:Session:AbsoluteMaxAge")
        };

        services.AddControllers();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalApiExceptionHandler>();
        services.AddSingleton(cartSessionOptions);
        services.AddDbContextFactory<CheckoutKataDbContext>(options =>
            options.UseInMemoryDatabase(pricingStoreDatabaseName));
        services.AddSingleton<CheckoutSessionService>(serviceProvider =>
        {
            var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<CheckoutKataDbContext>>();
            var options = serviceProvider.GetRequiredService<CartSessionOptions>();
            return new CheckoutSessionService(
                dbContextFactory,
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
