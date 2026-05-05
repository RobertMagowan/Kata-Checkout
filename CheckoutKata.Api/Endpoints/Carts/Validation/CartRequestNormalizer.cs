namespace CheckoutKata.Api.Endpoints.Carts.Validation;

internal static class CartRequestNormalizer
{
    public static string NormalizeItem(string item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item.Trim().ToUpperInvariant();
    }

    public static IReadOnlyList<string> NormalizeItems(IReadOnlyList<string> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
        {
            throw new ArgumentException("At least one item is required.", nameof(items));
        }

        return items.Select(NormalizeItem).ToArray();
    }
}
