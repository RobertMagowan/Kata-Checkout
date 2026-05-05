using CheckoutKata.Application.Carts;

namespace CheckoutKata.Api.Infrastructure.BackgroundServices;

public sealed class ExpiredCartCleanupService(
    ICheckoutSessionMaintenance checkoutSessionMaintenance,
    CartSessionOptions options) : BackgroundService
{
    private readonly ICheckoutSessionMaintenance _checkoutSessionMaintenance = checkoutSessionMaintenance;
    private readonly CartSessionOptions _options = options;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _checkoutSessionMaintenance.EvictExpiredCarts();
            await Task.Delay(_options.SweepInterval, stoppingToken);
        }
    }
}
