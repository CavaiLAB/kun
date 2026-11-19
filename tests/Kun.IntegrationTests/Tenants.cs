namespace Kun.IntegrationTests;

internal static class Tenants
{
    internal const string AText = "01900000-0000-7000-8000-000000000001";
    internal const string BText = "01900000-0000-7000-8000-000000000002";
    internal static readonly Guid A = new(AText);
    internal static readonly Guid B = new(BText);
}
