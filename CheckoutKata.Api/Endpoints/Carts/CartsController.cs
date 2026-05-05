using CheckoutKata.Api.Endpoints.Carts.Contracts.Requests;
using CheckoutKata.Api.Endpoints.Carts.Contracts.Responses;
using CheckoutKata.Api.Endpoints.Carts.Mapping;
using CheckoutKata.Api.Endpoints.Carts.Validation;
using CheckoutKata.Application.Pricing;
using CheckoutKata.Application.Carts;
using Microsoft.AspNetCore.Mvc;

namespace CheckoutKata.Api.Endpoints.Carts;

[ApiController]
[Route("api/carts")]
public sealed class CartsController(ICheckoutSessionService checkoutSessionService) : ControllerBase
{
    private readonly ICheckoutSessionService _checkoutSessionService = checkoutSessionService;

    [HttpPost]
    public async Task<ActionResult<CartSnapshotResponse>> CreateCart(CancellationToken cancellationToken)
    {
        var snapshot = await _checkoutSessionService.CreateCartAsync(cancellationToken);
        var response = CartSnapshotResponseMapper.ToResponse(snapshot);
        return CreatedAtAction(nameof(GetCart), new { cartId = response.CartId }, response);
    }

    [HttpGet("{cartId:guid}")]
    public async Task<ActionResult<CartSnapshotResponse>> GetCart(
        Guid cartId,
        [FromQuery] Guid? pricingVersionId,
        CancellationToken cancellationToken)
    {
        var requestedPricingVersionId = ToPricingVersionId(pricingVersionId);
        var snapshot = await _checkoutSessionService.GetCartAsync(
            cartId,
            requestedPricingVersionId,
            cancellationToken);

        return Ok(CartSnapshotResponseMapper.ToResponse(snapshot));
    }

    [HttpPost("{cartId:guid}/scan")]
    public async Task<ActionResult<CartSnapshotResponse>> ScanItem(
        Guid cartId,
        [FromBody] ScanItemRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var normalizedItem = CartRequestNormalizer.NormalizeItem(request.Item);
        var requestedPricingVersionId = ToPricingVersionId(request.PricingVersionId);

        var snapshot = await _checkoutSessionService.ScanItemAsync(
            cartId,
            normalizedItem,
            requestedPricingVersionId,
            cancellationToken);

        return Ok(CartSnapshotResponseMapper.ToResponse(snapshot));
    }

    [HttpPost("{cartId:guid}/scan-many")]
    public async Task<ActionResult<CartSnapshotResponse>> ScanMany(
        Guid cartId,
        [FromBody] ScanManyRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var normalizedItems = CartRequestNormalizer.NormalizeItems(request.Items);
        var requestedPricingVersionId = ToPricingVersionId(request.PricingVersionId);

        var snapshot = await _checkoutSessionService.ScanManyAsync(
            cartId,
            normalizedItems,
            requestedPricingVersionId,
            cancellationToken);

        return Ok(CartSnapshotResponseMapper.ToResponse(snapshot));
    }

    [HttpPost("{cartId:guid}/clear")]
    public async Task<ActionResult<CartSnapshotResponse>> Clear(
        Guid cartId,
        CancellationToken cancellationToken)
    {
        var snapshot = await _checkoutSessionService.ClearAsync(
            cartId,
            cancellationToken);

        return Ok(CartSnapshotResponseMapper.ToResponse(snapshot));
    }

    private static PricingVersionId? ToPricingVersionId(Guid? pricingVersionId) =>
        pricingVersionId.HasValue ? PricingVersionId.Parse(pricingVersionId.Value) : null;
}
