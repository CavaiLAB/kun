> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class JobContract

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

绑定真实能力的不可变作业目录项；能力本身仍负责执行策略。

```csharp
public sealed class JobContract
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[JobContract](/sdk/api/kun/kun-plugin-sdk/jobcontract.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### JobContract(string, CapabilityContract) \{#Kun_Plugin_Sdk_JobContract__ctor_System_String_Kun_Plugin_Sdk_CapabilityContract\_}

创建代码声明，保留源生成能力的输入输出 Schema。

```csharp
public JobContract(string code, CapabilityContract capability)
```

#### Parameters

`code` [string](https://learn.microsoft.com/dotnet/api/system.string)

`capability` [CapabilityContract](/sdk/api/kun/kun-plugin-sdk/capabilitycontract.md)

## Properties

### Capability \{#Kun_Plugin_Sdk_JobContract_Capability}

真正注册并执行的能力。

```csharp
public CapabilityContract Capability { get; }
```

#### Property Value

[CapabilityContract](/sdk/api/kun/kun-plugin-sdk/capabilitycontract.md)

### Code \{#Kun_Plugin_Sdk_JobContract_Code}

组件内作业代码。

```csharp
public string Code { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)
