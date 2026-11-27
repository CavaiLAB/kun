> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class SecretBinding

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Secrets` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Secrets`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Secrets)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration.Secrets](/sdk/api/kun/kun-configuration-secrets/index.md)\
Assembly: Kun.Configuration.Secrets.dll

公开引用到 Kubernetes 挂载文件的可信绑定。

```csharp
public sealed record SecretBinding : IEquatable<SecretBinding>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[SecretBinding](/sdk/api/kun/kun-configuration-secrets/secretbinding.md)

#### Implements

[IEquatable\<SecretBinding>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### SecretBinding(string, KubernetesSecretLocation, string?) \{#Kun_Configuration_Secrets_SecretBinding__ctor_System_String_Kun_Configuration_Secrets_KubernetesSecretLocation_System_String\_}

公开引用到 Kubernetes 挂载文件的可信绑定。

```csharp
public SecretBinding(string Reference, KubernetesSecretLocation Location, string? TenantId = null)
```

#### Parameters

`Reference` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Location` [KubernetesSecretLocation](/sdk/api/kun/kun-configuration-secrets/kubernetessecretlocation.md)

`TenantId` [string](https://learn.microsoft.com/dotnet/api/system.string)?

## Properties

### Location \{#Kun_Configuration_Secrets_SecretBinding_Location}

```csharp
public KubernetesSecretLocation Location { get; init; }
```

#### Property Value

[KubernetesSecretLocation](/sdk/api/kun/kun-configuration-secrets/kubernetessecretlocation.md)

### Reference \{#Kun_Configuration_Secrets_SecretBinding_Reference}

```csharp
public string Reference { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### TenantId \{#Kun_Configuration_Secrets_SecretBinding_TenantId}

```csharp
public string? TenantId { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

## Methods

### \<Clone>$() \{#Kun_Configuration_Secrets_SecretBinding__Clone\_\_}

```csharp
public SecretBinding <Clone>$()
```

#### Returns

[SecretBinding](/sdk/api/kun/kun-configuration-secrets/secretbinding.md)

### Deconstruct(out string, out KubernetesSecretLocation, out string?) \{#Kun_Configuration_Secrets_SecretBinding_Deconstruct_System_String__Kun_Configuration_Secrets_KubernetesSecretLocation__System_String\_\_}

```csharp
public void Deconstruct(out string Reference, out KubernetesSecretLocation Location, out string? TenantId)
```

#### Parameters

`Reference` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Location` [KubernetesSecretLocation](/sdk/api/kun/kun-configuration-secrets/kubernetessecretlocation.md)

`TenantId` [string](https://learn.microsoft.com/dotnet/api/system.string)?

### Equals(object?) \{#Kun_Configuration_Secrets_SecretBinding_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(SecretBinding?) \{#Kun_Configuration_Secrets_SecretBinding_Equals_Kun_Configuration_Secrets_SecretBinding\_}

```csharp
public bool Equals(SecretBinding? other)
```

#### Parameters

`other` [SecretBinding](/sdk/api/kun/kun-configuration-secrets/secretbinding.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Configuration_Secrets_SecretBinding_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Configuration_Secrets_SecretBinding_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(SecretBinding?, SecretBinding?) \{#Kun_Configuration_Secrets_SecretBinding_op_Equality_Kun_Configuration_Secrets_SecretBinding_Kun_Configuration_Secrets_SecretBinding\_}

```csharp
public static bool operator ==(SecretBinding? left, SecretBinding? right)
```

#### Parameters

`left` [SecretBinding](/sdk/api/kun/kun-configuration-secrets/secretbinding.md)?

`right` [SecretBinding](/sdk/api/kun/kun-configuration-secrets/secretbinding.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(SecretBinding?, SecretBinding?) \{#Kun_Configuration_Secrets_SecretBinding_op_Inequality_Kun_Configuration_Secrets_SecretBinding_Kun_Configuration_Secrets_SecretBinding\_}

```csharp
public static bool operator !=(SecretBinding? left, SecretBinding? right)
```

#### Parameters

`left` [SecretBinding](/sdk/api/kun/kun-configuration-secrets/secretbinding.md)?

`right` [SecretBinding](/sdk/api/kun/kun-configuration-secrets/secretbinding.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
