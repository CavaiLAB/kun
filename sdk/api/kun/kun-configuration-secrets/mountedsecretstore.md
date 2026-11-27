> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class MountedSecretStore

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Secrets` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Secrets`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Secrets)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration.Secrets](/sdk/api/kun/kun-configuration-secrets/index.md)\
Assembly: Kun.Configuration.Secrets.dll

读取受信任根目录中的 K8s 投射文件，支持根内的 ..data 原子轮换。

```csharp
public sealed class MountedSecretStore
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[MountedSecretStore](/sdk/api/kun/kun-configuration-secrets/mountedsecretstore.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### MountedSecretStore(string) \{#Kun_Configuration_Secrets_MountedSecretStore__ctor_System_String\_}

根目录应只读挂载；文件布局为 namespace/Secret/key。

```csharp
public MountedSecretStore(string rootDirectory)
```

#### Parameters

`rootDirectory` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### ReadAsync(KubernetesSecretLocation, CancellationToken) \{#Kun_Configuration_Secrets_MountedSecretStore_ReadAsync_Kun_Configuration_Secrets_KubernetesSecretLocation_System_Threading_CancellationToken\_}

可信宿主的低层读取入口；组件应使用带权限检查的 ISecretResolver。

```csharp
public ValueTask<SecretValue> ReadAsync(KubernetesSecretLocation location, CancellationToken cancellationToken = default)
```

#### Parameters

`location` [KubernetesSecretLocation](/sdk/api/kun/kun-configuration-secrets/kubernetessecretlocation.md)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<SecretValue>
