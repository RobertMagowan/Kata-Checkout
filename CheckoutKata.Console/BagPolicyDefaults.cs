namespace CheckoutKata.Console;

internal static class BagPolicyDefaults
{
    public const int BagCost = 1;
    public const int ItemsPerBag = 5;
    public const int BagVolumeCapacity = 10;

    private static readonly IReadOnlyDictionary<string, int> Volumes = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["A"] = 2,
        ["B"] = 5,
        ["C"] = 1,
        ["D"] = 3
    };

    public static IReadOnlyDictionary<string, int> VolumeByItem => Volumes;
}
