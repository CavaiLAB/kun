namespace Kun.EntityFrameworkCore.Tests;

internal static class Tenants
{
    internal const string AText = "01900000-0000-7000-8000-000000000001";
    internal static readonly Guid A = new(AText);
    internal const string BText = "01900000-0000-7000-8000-000000000002";
    internal static readonly Guid B = new(BText);
    internal const string OverrideText = "01900000-0000-7000-8000-000000000063";
    internal static readonly Guid Override = new(OverrideText);
    internal const string XText = "01900000-0000-7000-8000-000000000018";
    internal static readonly Guid X = new(XText);
    internal const string YText = "01900000-0000-7000-8000-000000000019";
    internal static readonly Guid Y = new(YText);
}
