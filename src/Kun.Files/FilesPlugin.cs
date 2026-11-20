using System.Text.Json;
using Kun.EntityFrameworkCore;
using Kun.Errors;
using Kun.EventBus;
using Kun.Mediator;
using Kun.Plugin.Sdk;
using Kun.Runtime;
using Kun.Tenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Kun.Registry;

namespace Kun.Files;

/// <summary>
/// qian.files 的真实 Provider（global 或 tenant scope）：登记 local 存储后端与组件配置里的默认上传策略，
/// 平台宿主另从 Registry 预装快照汇总各组件 metadata/upload-policies.json 的声明（同用途以配置为准），按 storage.kind 选用后端，
/// 提供 Files 服务键（<see cref="IFileService"/>，能力门面）与六个能力的执行绑定；卸载撤销服务、能力、策略与错误码。
/// 宿主须先 AddKunData&lt;FilesDbContext&gt;、在 AddKunEventBus 中 RegisterFilesEvents、在 AddKunMediator 中 AddKunFiles，否则 Apply 失败。
/// </summary>
[KunPlugin("qian.files")]
public sealed class FilesPlugin : IKunPlugin
{
    /// <inheritdoc />
    public IReadOnlyList<ServiceKey> Inject => [DataDefinition.Key];

    /// <inheritdoc />
    public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ct.ThrowIfCancellationRequested();
        if (ctx.Scope != ScopeKey.Global && ctx.Scope.Kind != "tenant")
            throw new InvalidOperationException("Files 只允许挂载在 global 或 tenant scope。");
        var settings = FilesConfiguration.Read(config);
        var services = ctx.Services;
        _ = services.GetService<FilesCapabilitySource>()
            ?? throw new InvalidOperationException("宿主须在 AddKunMediator 中以 AddKunFiles 登记文件能力。");
        // 文件事件随完成 / 删除同事务进 Outbox；宿主漏登记会让每次写入失败，所以在装载时就拒绝。
        var events = services.GetService<IEventTypeRegistry>();
        if (events is null || events.GetEventType(typeof(FileObjectCreatedEvent)) != FilesEventIds.ObjectCreated ||
            events.GetEventType(typeof(FileObjectDeletedEvent)) != FilesEventIds.ObjectDeleted)
            throw new InvalidOperationException("宿主未登记文件事件：AddKunEventBus(registrar => registrar.RegisterFilesEvents()) 必须在装载 qian.files 前完成。");
        if (services.GetService<IDataContextCatalog>() is { } contexts && !contexts.ContextTypes.Contains(typeof(FilesDbContext)))
            throw new InvalidOperationException("宿主须以 AddKunData<FilesDbContext> 装配文件元数据上下文。");
        var options = services.GetService<IOptions<KunFilesOptions>>()?.Value ?? new KunFilesOptions();
        // 组件 metadata/upload-policies.json 的声明只在有 Registry 的平台宿主可见；Registry 不进 Inject（standalone 与 qian add 没有它，R34），经 TryInject 读取预装快照。
        var declared = new DeclaredUploadPolicies(() => ctx.TryInject<IRegistryService>(RegistryDefinition.Key));
        var policies = new UploadPolicyRegistry(declared);
        foreach (var policy in settings.Policies) ctx.Effect(policies.Register(policy));
        // Registry 已就绪时在挂载期就编译声明：坏的声明让 Apply 失败，而不是拖到第一次上传才报错。
        _ = declared.Policies;
        var runtime = new FilesRuntime(options.Clock, policies, settings.QuotaPerTenantBytes, () => ctx.TryInject<ITenantSettings>(TenantSettings.Key));
        runtime.Use(FileStorageMount.Create(settings.Kind, settings.Storage, options.Clock, services, options.LocalSigningKey));
        if (services.GetService<ErrorCatalog>() is { } catalog) ctx.Effect(FilesErrors.Register(catalog));
        ctx.Provide<IFileService>(FilesDefinition.Key, new FileService(services.GetRequiredService<ISender>(), runtime));
        ctx.ProvideCapability<PresignUploadCommand, PresignedUpload>();
        ctx.ProvideCapability<CompleteUploadCommand, FileDescriptor>();
        ctx.ProvideCapability<GetFileQuery, FileDescriptor>();
        ctx.ProvideCapability<DownloadUrlQuery, DownloadTicket>();
        ctx.ProvideCapability<DeleteFileCommand, FileDescriptor>();
        ctx.ProvideCapability<QuotaQuery, FileQuota>();
        return ValueTask.CompletedTask;
    }
}
