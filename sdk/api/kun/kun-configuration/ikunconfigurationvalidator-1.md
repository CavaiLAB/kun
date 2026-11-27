> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IKunConfigurationValidator\<TOptions>

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.dll

Reflection-free options validator — the AOT-safe alternative to
DataAnnotations. Register one per options type via
`services.AddSingleton<IKunConfigurationValidator<T>, MyValidator>()`
and `AddKunOptions<T>` will merge the results with any
DataAnnotations-driven validation.

```csharp
public interface IKunConfigurationValidator<in TOptions> where TOptions : class
```

#### Type Parameters

`TOptions`

The options type being validated.

## Methods

### Validate(string?, TOptions) \{#Kun_Configuration_IKunConfigurationValidator_1_Validate_System_String__0\_}

Validates `options`. Return
[`Success`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.options.validateoptionsresult.success) on success or
[`Fail`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.options.validateoptionsresult.fail)
with section-qualified messages on failure.

```csharp
ValidateOptionsResult Validate(string? name, TOptions options)
```

#### Parameters

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The named-options name, or [null](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null) for the default instance.

`options` TOptions

The populated options object.

#### Returns

[ValidateOptionsResult](https://learn.microsoft.com/dotnet/api/microsoft.extensions.options.validateoptionsresult)
