> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class DockerRequirement

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing.Containers` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing.Containers`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing.Containers)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Testing.Containers](/sdk/api/kun/kun-testing-containers/index.md)\
Assembly: Kun.Testing.Containers.dll

没有 Docker 时集成测试怎么办：本机跳过；CI 上直接失败，因为 PR 级门禁要求集成测试必须真跑（PRD §14.4 C10）。

```csharp
public static class DockerRequirement
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[DockerRequirement](/sdk/api/kun/kun-testing-containers/dockerrequirement.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### RequireVariable \{#Kun_Testing_Containers_DockerRequirement_RequireVariable}

设为 `1` 或 `true` 时，即使不在 CI 上也要求 Docker（缺失即失败而不是跳过）。

```csharp
public const string RequireVariable = "QIAN_REQUIRE_DOCKER"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### IsRequired(Func\<string, string?>?) \{#Kun_Testing_Containers_DockerRequirement_IsRequired_System_Func_System_String_System_String\_\_}

CI 环境或明确设置 [DockerRequirement.RequireVariable](/sdk/api/kun/kun-testing-containers/dockerrequirement.md#Kun_Testing_Containers_DockerRequirement_RequireVariable) 时要求 Docker；CI 的空值、0、false 或 no 表示本机。

```csharp
public static bool IsRequired(Func<string, string?>? getEnvironmentVariable = null)
```

#### Parameters

`getEnvironmentVariable` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)?>?

读取环境变量的函数，默认 [`GetEnvironmentVariable`](https://learn.microsoft.com/dotnet/api/system.environment.getenvironmentvariable)；单测可替换。

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### ReportUnavailable(string) \{#Kun_Testing_Containers_DockerRequirement_ReportUnavailable_System_String\_}

告知 xUnit 此环境缺少 Docker；要求 Docker 的环境抛出异常。

```csharp
[DoesNotReturn]
public static void ReportUnavailable(string message)
```

#### Parameters

`message` [string](https://learn.microsoft.com/dotnet/api/system.string)
