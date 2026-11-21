---
title: 后端 SDK（.NET）
description: Kun.Runtime 的编程模型：插件、共享上下文 ctx、服务注册与注入、类型化事件、可逆副作用与 scope。
part: sdk
project: platform
audience: [developer]
updated: 2026-11-20
---

# 后端 SDK（.NET）

内核包 `Kun.Runtime` 提供插件上下文、服务注册、类型化事件、可逆副作用与装载器。平台组件通过同一套运行时提供有业务含义的服务。

## 安装

首个合并版发布前，仓库内的组件和宿主使用同一解决方案的项目引用。合并版发布后，可从所选 [Kun Release](https://github.com/CavaiLAB/kun/releases) 下载该版本的 `NuGet.config` 和 `release-packages.json`。正式版的内核包从 nuget.org 还原，平台包逐个映射到 CavaiLAB 的 GitHub Packages。beta 的两档包均从 GitHub Packages 还原。将 `KUN_PLATFORM_FEED_USER` 与 `KUN_PLATFORM_FEED_TOKEN` 设为有读包权限的凭据。

组件项目引用同一发布版本的 SDK 与源生成器。下面的 `KunReleaseVersion` 是消费项目自定义的 MSBuild 属性，填写所选 Release 的实际版本号。

```xml
<PackageReference Include="Kun.Plugin.Sdk" Version="$(KunReleaseVersion)" />
<PackageReference Include="Kun.Plugin.Sdk.Generators" Version="$(KunReleaseVersion)" PrivateAssets="all" />
```

## 编程模型

- **插件**：实现 `IKunPlugin` / `IKunPlugin<TConfig>`，在 `ApplyAsync` 里向上下文贡献内容；运行时先按清单的 `configSchema` 校验配置再反序列化。
- **服务**：`Provide(key, instance)` 填充服务键，`Inject<T>(key)` 取用；`ctx.Files()` 这类扩展方法由 Definition 包提供，等价于 `Inject`。装载器按 `inject → provides` 决定挂载顺序，依赖未就绪的插件保持 `Pending`。
- **事件**：事件在 Definition 包里定义并标注分发模式，见[事件与中间件](/sdk/dotnet/events)。
- **副作用皆可逆**：`Provide`、`On` 等注册都返回释放函数并压入插件的效果栈，`Effect(...)` 把第三方资源也纳入同一栈；卸载时按注册逆序释放。
- **scope**：`Scoped(scope)` 返回子上下文，之后的注册归属该 scope；见 [scope](/start/concepts/scope)。

## API 草图

节选自[组件模型规格](/design/prd/specs/component-model) §5.2 的 API 草图（以规格为准）：

```csharp
public interface IKunPlugin
{
    IReadOnlyList<ServiceKey> Inject { get; }
    ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct);
}

public interface IPluginContext
{
    ScopeKey Scope { get; }
    IDisposable Provide<TService>(ServiceKey key, TService instance) where TService : class;
    TService Inject<TService>(ServiceKey key) where TService : class;
    void Effect(Func<ValueTask> disposeAsync);
    IDisposable On<TPayload, TResult>(KunEvent<TPayload, TResult> evt, DispatchMode mode, Delegate handler, int priority = 0);
    ValueTask<TResult> DispatchAsync<TPayload, TResult>(KunEvent<TPayload, TResult> evt, TPayload payload, CancellationToken ct);
    IPluginContext Scoped(ScopeKey scope);
}
```

组件入口用 `[KunPlugin("<component-id>")]` 标注，源生成器派生 `runtime.entry`、`inject[]`、`provides[]` 以及配置、权限、事件和工具表面。同程序集支持多个不同组件 ID，每个组件保持唯一清单；工具按清单 ID 选择对应元数据。已派生、手写字段和 S2 边界见[能力与唯一清单](/sdk/dotnet/capabilities)。`Provide` 的登记归属当前插件的 scope 与可撤销生命周期；跨进程集成事件由 [qian.events](/components/platform/qian.events) 桥接到 Kun 的 Mediator + Outbox。

## 示例

把文件存储后端包装为 `ctx.Files` Provider 的完整示例见规格 §5.7。已发布内核包的 XML 注释收录在 [Kun API 参考](/sdk/api/kun/)。平台档包的 API 参考列在 [S1-DOCS-002 的交付范围](/sdk/api/)。
