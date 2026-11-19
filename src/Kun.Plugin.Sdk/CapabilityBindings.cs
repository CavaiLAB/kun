using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Kun.Core.MultiTenancy;
using Kun.Errors;
using Kun.Mediator;
using Kun.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Kun.Plugin.Sdk;

/// <summary>已挂载代码能力的真实执行端口；目录读取契约，调用仍经实际 Mediator 和基础策略。</summary>
public interface IExecutableCapability
{
    /// <summary>已编译的不可变输入/输出及 Agent 契约。</summary>
    CapabilityContract Contract { get; }
    /// <summary>调用者必须提供当前租户的实际可见视图；缓存的旧绑定不能绕过卸载或 restrict。</summary>
    ValueTask<JsonElement> InvokeAsync(IPluginContext caller, JsonElement input, CancellationToken cancellationToken = default);
}

/// <summary>SDK 层的可逆能力注册，不依赖 platform 的 Capabilities Provider。</summary>
public static class CapabilityBindingRegistration
{
    /// <summary>供目录、提示词和执行共同读取的具名注册集合。</summary>
    public static readonly ServiceKey Key = new("ExecutableCapabilities");

    /// <summary>在所属组件 Apply 中登记已经实际注册的 Mediator 请求；注册自动纳入 Effect。</summary>
    public static IDisposable ProvideCapability<TRequest, TResponse>(this IPluginContext context)
        where TRequest : ICapabilityRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(context);
        var contract = TRequest.KunCapability;
        if (contract.ComponentId != context.Entry.Plugin || contract.RequestTypeInfo is not JsonTypeInfo<TRequest> request || contract.ResponseTypeInfo is not JsonTypeInfo<TResponse> response)
            throw new ErrorException(new Error(PlatformErrors.InputInvalid));
        // 平台宿主登记了契约检查时，登记前生成目录条目并预编译输入校验器：不合格的契约在登记阶段拒绝，不留到首次调用。
        context.Services.GetService<ICapabilityContractCheck>()?.Check(contract);
        var binding = new Binding<TRequest, TResponse>(context, contract, request, response,
            context.Services.GetRequiredService<ISender>(), context.Services.GetRequiredService<ITenantContext>());
        context.Effect(binding.Dispose);
        return context.Provide<IExecutableCapability>(Key, contract.Id, binding);
    }

    private sealed class Binding<TRequest, TResponse>(IPluginContext owner, CapabilityContract contract,
        JsonTypeInfo<TRequest> request, JsonTypeInfo<TResponse> response, ISender sender, ITenantContext tenant) : IExecutableCapability, IDisposable
        where TRequest : ICapabilityRequest<TResponse>
    {
        private readonly (CancellationTokenSource Source, CancellationToken Token) _lifetime = CreateLifetime(); private int _disposed;
        public CapabilityContract Contract => contract;
        public async ValueTask<JsonElement> InvokeAsync(IPluginContext caller, JsonElement input, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(caller);
            if (Volatile.Read(ref _disposed) != 0 || tenant.TenantId is not { } id || id == Guid.Empty || caller.Scope != new ScopeKey("tenant", id.ToString("D"))) throw Missing();
            try
            {
                _ = owner.Services;
                if (!ReferenceEquals(caller.Inject<IExecutableCapability>(Key, contract.Id), this)) throw Missing();
            }
            catch (ObjectDisposedException) { throw Missing(); }
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetime.Token);
            TRequest command;
            try { command = input.Deserialize(request) ?? throw new JsonException(); }
            catch (JsonException) { throw new ErrorException(new Error(PlatformErrors.InputInvalid)); }
            var result = await sender.Send(command, linked.Token).ConfigureAwait(false);
            return JsonSerializer.SerializeToElement(result, response);
        }
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0) { try { _lifetime.Source.Cancel(); } finally { _lifetime.Source.Dispose(); } }
        }
        private static (CancellationTokenSource, CancellationToken) CreateLifetime() { var source = new CancellationTokenSource(); return (source, source.Token); }
        private static ErrorException Missing() => new(new Error(PlatformErrors.NotFound));
    }
}
