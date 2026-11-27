> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class CapabilityContractException

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

能力契约在登记阶段被拒绝：带组件 ID、能力短名、出错位置与规则。同样的值写入 [`Data`](https://learn.microsoft.com/dotnet/api/system.exception.data)（含原因码 `INPUT_INVALID`），
供 scope 装载失败与管理面诊断读取；消息不含 Schema 或配置原文。

```csharp
public sealed class CapabilityContractException : InvalidOperationException, ISerializable
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[Exception](https://learn.microsoft.com/dotnet/api/system.exception) ←
[SystemException](https://learn.microsoft.com/dotnet/api/system.systemexception) ←
[InvalidOperationException](https://learn.microsoft.com/dotnet/api/system.invalidoperationexception) ←
[CapabilityContractException](/sdk/api/kun/kun-plugin-sdk/capabilitycontractexception.md)

#### Implements

[ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

#### Inherited Members

[Exception.GetBaseException()](https://learn.microsoft.com/dotnet/api/system.exception.getbaseexception),
[Exception.ToString()](https://learn.microsoft.com/dotnet/api/system.exception.tostring),
[Exception.GetType()](https://learn.microsoft.com/dotnet/api/system.exception.gettype),
[Exception.TargetSite](https://learn.microsoft.com/dotnet/api/system.exception.targetsite),
[Exception.Message](https://learn.microsoft.com/dotnet/api/system.exception.message),
[Exception.Data](https://learn.microsoft.com/dotnet/api/system.exception.data),
[Exception.InnerException](https://learn.microsoft.com/dotnet/api/system.exception.innerexception),
[Exception.HelpLink](https://learn.microsoft.com/dotnet/api/system.exception.helplink),
[Exception.Source](https://learn.microsoft.com/dotnet/api/system.exception.source),
[Exception.HResult](https://learn.microsoft.com/dotnet/api/system.exception.hresult),
[Exception.StackTrace](https://learn.microsoft.com/dotnet/api/system.exception.stacktrace),
[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### CapabilityContractException(string, string, string, string, Exception?) \{#Kun_Plugin_Sdk_CapabilityContractException__ctor_System_String_System_String_System_String_System_String_System_Exception\_}

创建一次登记拒绝。

```csharp
public CapabilityContractException(string componentId, string name, string location, string rule, Exception? innerException = null)
```

#### Parameters

`componentId` [string](https://learn.microsoft.com/dotnet/api/system.string)

拥有该能力的组件。

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

组件内能力短名。

`location` [string](https://learn.microsoft.com/dotnet/api/system.string)

出错位置：以能力 ID 为文件的 JSON Pointer，如 `qian.sample.crm/customer.create#/inputSchema/properties/tenantId`。

`rule` [string](https://learn.microsoft.com/dotnet/api/system.string)

违反的规则：`tool-invalid`、`entry-invalid`、`tenant-field`、`unsupported-key`、`unsupported-number`、`unresolved-reference`、`schema-invalid`，
其余无法生成条目的情况为 `contract-invalid`。

`innerException` [Exception](https://learn.microsoft.com/dotnet/api/system.exception)?

底层失败；只用于诊断，不进入消息。

## Properties

### ComponentId \{#Kun_Plugin_Sdk_CapabilityContractException_ComponentId}

拥有该能力的组件。

```csharp
public string ComponentId { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Location \{#Kun_Plugin_Sdk_CapabilityContractException_Location}

出错位置（JSON Pointer）。

```csharp
public string Location { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Name \{#Kun_Plugin_Sdk_CapabilityContractException_Name}

组件内能力短名。

```csharp
public string Name { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### ReasonCode \{#Kun_Plugin_Sdk_CapabilityContractException_ReasonCode}

对应的平台错误码，恒为 `INPUT_INVALID`。

```csharp
public string ReasonCode { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Rule \{#Kun_Plugin_Sdk_CapabilityContractException_Rule}

违反的规则。

```csharp
public string Rule { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)
