> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class GatewayHosting

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Gateway` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Gateway`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Gateway)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Gateway](/sdk/api/kun/kun-gateway/index.md)\
Assembly: Kun.Gateway.dll

网关宿主装配；Identity.Client 的 Provider 由宿主独立组合，组件库不直接依赖其他 Provider。

```csharp
public static class GatewayHosting
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[GatewayHosting](/sdk/api/kun/kun-gateway/gatewayhosting.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### MapKunGateway(IEndpointRouteBuilder) \{#Kun_Gateway_GatewayHosting_MapKunGateway_Microsoft_AspNetCore_Routing_IEndpointRouteBuilder\_}

映射受保护静态代理、公开 JWKS 及基础健康端点；完整依赖健康聚合由 GW-003 接续。

```csharp
public static IEndpointRouteBuilder MapKunGateway(this IEndpointRouteBuilder endpoints)
```

#### Parameters

`endpoints` [IEndpointRouteBuilder](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.routing.iendpointroutebuilder)

#### Returns

[IEndpointRouteBuilder](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.routing.iendpointroutebuilder)

### ReadConfiguration(JsonElement) \{#Kun_Gateway_GatewayHosting_ReadConfiguration_System_Text_Json_JsonElement\_}

从声明式 JSON 读取严格配置；拒绝重复字段、未知字段与无效静态路由。

```csharp
public static GatewayConfiguration ReadConfiguration(JsonElement configuration)
```

#### Parameters

`configuration` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

#### Returns

GatewayConfiguration

### UseKunGateway(IApplicationBuilder) \{#Kun_Gateway_GatewayHosting_UseKunGateway_Microsoft_AspNetCore_Builder_IApplicationBuilder\_}

在 UseForwardedHeaders、UseRouting、UseKunIdentityClient 之后调用；业务请求还须通过真实组件挂载门。

```csharp
public static IApplicationBuilder UseKunGateway(this IApplicationBuilder app)
```

#### Parameters

`app` [IApplicationBuilder](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.builder.iapplicationbuilder)

#### Returns

[IApplicationBuilder](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.builder.iapplicationbuilder)
