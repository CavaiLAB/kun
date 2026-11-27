> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class CapabilityAttribute

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

声明命令或查询的契约；源生成器验证并生成强类型能力定义。

```csharp
[AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct, Inherited = false)]
public sealed class CapabilityAttribute : Attribute
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[Attribute](https://learn.microsoft.com/dotnet/api/system.attribute) ←
[CapabilityAttribute](/sdk/api/kun/kun-plugin-sdk/capabilityattribute.md)

#### Inherited Members

[Attribute.GetCustomAttributes(MemberInfo, Type)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes#system-attribute-getcustomattributes\(system-reflection-memberinfo-system-type\)),
[Attribute.GetCustomAttributes(MemberInfo, Type, bool)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes#system-attribute-getcustomattributes\(system-reflection-memberinfo-system-type-system-boolean\)),
[Attribute.GetCustomAttributes(MemberInfo)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes#system-attribute-getcustomattributes\(system-reflection-memberinfo\)),
[Attribute.GetCustomAttributes(MemberInfo, bool)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes#system-attribute-getcustomattributes\(system-reflection-memberinfo-system-boolean\)),
[Attribute.IsDefined(MemberInfo, Type)](https://learn.microsoft.com/dotnet/api/system.attribute.isdefined#system-attribute-isdefined\(system-reflection-memberinfo-system-type\)),
[Attribute.IsDefined(MemberInfo, Type, bool)](https://learn.microsoft.com/dotnet/api/system.attribute.isdefined#system-attribute-isdefined\(system-reflection-memberinfo-system-type-system-boolean\)),
[Attribute.GetCustomAttribute(MemberInfo, Type)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattribute#system-attribute-getcustomattribute\(system-reflection-memberinfo-system-type\)),
[Attribute.GetCustomAttribute(MemberInfo, Type, bool)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattribute#system-attribute-getcustomattribute\(system-reflection-memberinfo-system-type-system-boolean\)),
[Attribute.GetCustomAttributes(ParameterInfo)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes#system-attribute-getcustomattributes\(system-reflection-parameterinfo\)),
[Attribute.GetCustomAttributes(ParameterInfo, Type)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes#system-attribute-getcustomattributes\(system-reflection-parameterinfo-system-type\)),
[Attribute.GetCustomAttributes(ParameterInfo, Type, bool)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes#system-attribute-getcustomattributes\(system-reflection-parameterinfo-system-type-system-boolean\)),
[Attribute.GetCustomAttributes(ParameterInfo, bool)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes#system-attribute-getcustomattributes\(system-reflection-parameterinfo-system-boolean\)),
[Attribute.IsDefined(ParameterInfo, Type)](https://learn.microsoft.com/dotnet/api/system.attribute.isdefined#system-attribute-isdefined\(system-reflection-parameterinfo-system-type\)),
[Attribute.IsDefined(ParameterInfo, Type, bool)](https://learn.microsoft.com/dotnet/api/system.attribute.isdefined#system-attribute-isdefined\(system-reflection-parameterinfo-system-type-system-boolean\)),
[Attribute.GetCustomAttribute(ParameterInfo, Type)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattribute#system-attribute-getcustomattribute\(system-reflection-parameterinfo-system-type\)),
[Attribute.GetCustomAttribute(ParameterInfo, Type, bool)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattribute#system-attribute-getcustomattribute\(system-reflection-parameterinfo-system-type-system-boolean\)),
[Attribute.GetCustomAttributes(Module, Type)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes#system-attribute-getcustomattributes\(system-reflection-module-system-type\)),
[Attribute.GetCustomAttributes(Module)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes#system-attribute-getcustomattributes\(system-reflection-module\)),
[Attribute.GetCustomAttributes(Module, bool)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes#system-attribute-getcustomattributes\(system-reflection-module-system-boolean\)),
[Attribute.GetCustomAttributes(Module, Type, bool)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes#system-attribute-getcustomattributes\(system-reflection-module-system-type-system-boolean\)),
[Attribute.IsDefined(Module, Type)](https://learn.microsoft.com/dotnet/api/system.attribute.isdefined#system-attribute-isdefined\(system-reflection-module-system-type\)),
[Attribute.IsDefined(Module, Type, bool)](https://learn.microsoft.com/dotnet/api/system.attribute.isdefined#system-attribute-isdefined\(system-reflection-module-system-type-system-boolean\)),
[Attribute.GetCustomAttribute(Module, Type)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattribute#system-attribute-getcustomattribute\(system-reflection-module-system-type\)),
[Attribute.GetCustomAttribute(Module, Type, bool)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattribute#system-attribute-getcustomattribute\(system-reflection-module-system-type-system-boolean\)),
[Attribute.GetCustomAttributes(Assembly, Type)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes#system-attribute-getcustomattributes\(system-reflection-assembly-system-type\)),
[Attribute.GetCustomAttributes(Assembly, Type, bool)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes#system-attribute-getcustomattributes\(system-reflection-assembly-system-type-system-boolean\)),
[Attribute.GetCustomAttributes(Assembly)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes#system-attribute-getcustomattributes\(system-reflection-assembly\)),
[Attribute.GetCustomAttributes(Assembly, bool)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes#system-attribute-getcustomattributes\(system-reflection-assembly-system-boolean\)),
[Attribute.IsDefined(Assembly, Type)](https://learn.microsoft.com/dotnet/api/system.attribute.isdefined#system-attribute-isdefined\(system-reflection-assembly-system-type\)),
[Attribute.IsDefined(Assembly, Type, bool)](https://learn.microsoft.com/dotnet/api/system.attribute.isdefined#system-attribute-isdefined\(system-reflection-assembly-system-type-system-boolean\)),
[Attribute.GetCustomAttribute(Assembly, Type)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattribute#system-attribute-getcustomattribute\(system-reflection-assembly-system-type\)),
[Attribute.GetCustomAttribute(Assembly, Type, bool)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattribute#system-attribute-getcustomattribute\(system-reflection-assembly-system-type-system-boolean\)),
[Attribute.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.attribute.equals),
[Attribute.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.attribute.gethashcode),
[Attribute.Match(object?)](https://learn.microsoft.com/dotnet/api/system.attribute.match),
[Attribute.IsDefaultAttribute()](https://learn.microsoft.com/dotnet/api/system.attribute.isdefaultattribute),
[Attribute.TypeId](https://learn.microsoft.com/dotnet/api/system.attribute.typeid),
[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### CapabilityAttribute(string, Type, Type) \{#Kun_Plugin_Sdk_CapabilityAttribute__ctor_System_String_System_Type_System_Type\_}

声明命令或查询的契约；源生成器验证并生成强类型能力定义。

```csharp
public CapabilityAttribute(string name, Type responseType, Type serializerContext)
```

#### Parameters

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`responseType` [Type](https://learn.microsoft.com/dotnet/api/system.type)

`serializerContext` [Type](https://learn.microsoft.com/dotnet/api/system.type)

## Properties

### AgentExpose \{#Kun_Plugin_Sdk_CapabilityAttribute_AgentExpose}

是否向 Agent 暴露。

```csharp
public bool AgentExpose { get; set; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Async \{#Kun_Plugin_Sdk_CapabilityAttribute_Async}

none 或 task。

```csharp
public string Async { get; set; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Audit \{#Kun_Plugin_Sdk_CapabilityAttribute_Audit}

是否审计；未显式赋值时按命令或查询默认值派生。

```csharp
public bool Audit { get; set; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### ComponentId \{#Kun_Plugin_Sdk_CapabilityAttribute_ComponentId}

所属组件 ID；单组件程序集可省略，多组件程序集必须明确填写。

```csharp
public string? ComponentId { get; set; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### ContractVersion \{#Kun_Plugin_Sdk_CapabilityAttribute_ContractVersion}

稳定契约族版本。

```csharp
public string ContractVersion { get; set; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### CostTier \{#Kun_Plugin_Sdk_CapabilityAttribute_CostTier}

成本级别。

```csharp
public string CostTier { get; set; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### DataClassification \{#Kun_Plugin_Sdk_CapabilityAttribute_DataClassification}

数据分级。

```csharp
public string DataClassification { get; set; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Description \{#Kun_Plugin_Sdk_CapabilityAttribute_Description}

面向调用者的意图式描述。

```csharp
public string Description { get; set; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Idempotent \{#Kun_Plugin_Sdk_CapabilityAttribute_Idempotent}

写操作必须具有幂等键。

```csharp
public bool Idempotent { get; set; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Name \{#Kun_Plugin_Sdk_CapabilityAttribute_Name}

组件内短名。

```csharp
public string Name { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### PolicyHint \{#Kun_Plugin_Sdk_CapabilityAttribute_PolicyHint}

冻结契约中的策略提示；destructive 默认 confirm。

```csharp
public string PolicyHint { get; set; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### RequiredPermission \{#Kun_Plugin_Sdk_CapabilityAttribute_RequiredPermission}

执行所需权限点；不是令牌内授权。

```csharp
public string? RequiredPermission { get; set; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### RequiredScopes \{#Kun_Plugin_Sdk_CapabilityAttribute_RequiredScopes}

执行所需 OAuth scope。

```csharp
public string[] RequiredScopes { get; set; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)\[]

### RequiredSecrets \{#Kun_Plugin_Sdk_CapabilityAttribute_RequiredSecrets}

所需 Secret 名称，不包含值。

```csharp
public string[] RequiredSecrets { get; set; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)\[]

### RequiresConfirmation \{#Kun_Plugin_Sdk_CapabilityAttribute_RequiresConfirmation}

Agent 调用是否要求确认。

```csharp
public bool RequiresConfirmation { get; set; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### ResponseType \{#Kun_Plugin_Sdk_CapabilityAttribute_ResponseType}

能力的响应类型。

```csharp
public Type ResponseType { get; }
```

#### Property Value

[Type](https://learn.microsoft.com/dotnet/api/system.type)

### SerializerContext \{#Kun_Plugin_Sdk_CapabilityAttribute_SerializerContext}

包含请求与响应的 System.Text.Json 源生成上下文。

```csharp
public Type SerializerContext { get; }
```

#### Property Value

[Type](https://learn.microsoft.com/dotnet/api/system.type)

### SideEffect \{#Kun_Plugin_Sdk_CapabilityAttribute_SideEffect}

冻结契约中的副作用类型。

```csharp
public string SideEffect { get; set; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)
