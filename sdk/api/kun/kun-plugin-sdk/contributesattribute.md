> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ContributesAttribute

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

声明具名扩展点贡献；服务键由指定类型的公开静态只读 ServiceKey 字段取得。

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ContributesAttribute : Attribute
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[Attribute](https://learn.microsoft.com/dotnet/api/system.attribute) ←
[ContributesAttribute](/sdk/api/kun/kun-plugin-sdk/contributesattribute.md)

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

### ContributesAttribute(Type, string, string) \{#Kun_Plugin_Sdk_ContributesAttribute__ctor_System_Type_System_String_System_String\_}

声明具名扩展点贡献；服务键由指定类型的公开静态只读 ServiceKey 字段取得。

```csharp
public ContributesAttribute(Type keyOwner, string keyMember, string name)
```

#### Parameters

`keyOwner` [Type](https://learn.microsoft.com/dotnet/api/system.type)

`keyMember` [string](https://learn.microsoft.com/dotnet/api/system.string)

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### KeyMember \{#Kun_Plugin_Sdk_ContributesAttribute_KeyMember}

服务键字段名。

```csharp
public string KeyMember { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### KeyOwner \{#Kun_Plugin_Sdk_ContributesAttribute_KeyOwner}

服务键所属类型。

```csharp
public Type KeyOwner { get; }
```

#### Property Value

[Type](https://learn.microsoft.com/dotnet/api/system.type)

### Name \{#Kun_Plugin_Sdk_ContributesAttribute_Name}

固定贡献名。

```csharp
public string Name { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### RegistryType \{#Kun_Plugin_Sdk_ContributesAttribute_RegistryType}

Register 的真实注册表类型；普通同名方法不能冒充贡献。

```csharp
public Type? RegistryType { get; set; }
```

#### Property Value

[Type](https://learn.microsoft.com/dotnet/api/system.type)?
