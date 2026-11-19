
namespace Kun.EventBus;

internal sealed record EventsBrokerBinding(string? Reference)
{
    internal static void ValidateReference(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference) || !reference.StartsWith("secretRef://", StringComparison.Ordinal) ||
            reference.Any(char.IsWhiteSpace) || !Uri.TryCreate(reference, UriKind.Absolute, out var uri) ||
            uri.Host is not ("platform" or "dev") || uri.AbsolutePath.Length <= 1 ||
            uri.UserInfo.Length != 0 || uri.Query.Length != 0 || uri.Fragment.Length != 0 || !uri.IsDefaultPort)
            throw new KunEventsException(EventsErrors.Misconfigured, "broker 只能是宿主登记的 platform/dev secretRef，凭据不写进组件配置。");
    }
}
