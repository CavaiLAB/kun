> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KubernetesSecretLocation

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Secrets` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Secrets`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Secrets)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration.Secrets](/sdk/api/kun/kun-configuration-secrets/index.md)\
Assembly: Kun.Configuration.Secrets.dll

可信引导中的 Kubernetes 文件定位，不作为公开组件引用。

```csharp
public sealed class KubernetesSecretLocation
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KubernetesSecretLocation](/sdk/api/kun/kun-configuration-secrets/kubernetessecretlocation.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### KubernetesSecretLocation(string, string, string) \{#Kun_Configuration_Secrets_KubernetesSecretLocation__ctor_System_String_System_String_System_String\_}

创建 namespace/Secret/key 定位，所有部分只允许单个合法名称。

```csharp
public KubernetesSecretLocation(string @namespace, string name, string key)
```

#### Parameters

`namespace` [string](https://learn.microsoft.com/dotnet/api/system.string)

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`key` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### Key \{#Kun_Configuration_Secrets_KubernetesSecretLocation_Key}

投射文件键。

```csharp
public string Key { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Name \{#Kun_Configuration_Secrets_KubernetesSecretLocation_Name}

Secret 资源名。

```csharp
public string Name { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Namespace \{#Kun_Configuration_Secrets_KubernetesSecretLocation_Namespace}

Kubernetes namespace。

```csharp
public string Namespace { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### Parse(string) \{#Kun_Configuration_Secrets_KubernetesSecretLocation_Parse_System_String\_}

兼容任务中的底层 secretRef://k8s/namespace/name#key 定位。

```csharp
public static KubernetesSecretLocation Parse(string locator)
```

#### Parameters

`locator` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

[KubernetesSecretLocation](/sdk/api/kun/kun-configuration-secrets/kubernetessecretlocation.md)

### ToString() \{#Kun_Configuration_Secrets_KubernetesSecretLocation_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)
