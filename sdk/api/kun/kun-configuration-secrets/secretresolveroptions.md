> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class SecretResolverOptions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Secrets` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Secrets`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Secrets)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration.Secrets](/sdk/api/kun/kun-configuration-secrets/index.md)\
Assembly: Kun.Configuration.Secrets.dll

只来自可信宿主引导，不从请求或模型输入建立授权。

```csharp
public sealed class SecretResolverOptions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[SecretResolverOptions](/sdk/api/kun/kun-configuration-secrets/secretresolveroptions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### SecretResolverOptions() \{#Kun_Configuration_Secrets_SecretResolverOptions__ctor}

```csharp
public SecretResolverOptions()
```

## Properties

### AllowDevelopmentReferences \{#Kun_Configuration_Secrets_SecretResolverOptions_AllowDevelopmentReferences}

只有开发宿主显式开启才可访问 dev 引用。

```csharp
public bool AllowDevelopmentReferences { get; init; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Bindings \{#Kun_Configuration_Secrets_SecretResolverOptions_Bindings}

公开引用绑定；tenant 绑定必须使用完整 tenant/id/path 形式及同一个 TenantId。

```csharp
public IReadOnlyList<SecretBinding> Bindings { get; init; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[SecretBinding](/sdk/api/kun/kun-configuration-secrets/secretbinding.md)>

### MountRoot \{#Kun_Configuration_Secrets_SecretResolverOptions_MountRoot}

只读 Secret 投射根目录。

```csharp
public required string MountRoot { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Policies \{#Kun_Configuration_Secrets_SecretResolverOptions_Policies}

插件或独立宿主的已审批策略。

```csharp
public IReadOnlyList<SecretAccessPolicy> Policies { get; init; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[SecretAccessPolicy](/sdk/api/kun/kun-configuration-secrets/secretaccesspolicy.md)>
