# Kun.Security.Abstractions

- 只放服务间令牌的数据模型、枚举与声明名；签发、校验、从声明解析主体等实现都在 `Kun.Security`，不要放进来。
- 零依赖：只许 BCL，不加任何项目引用与包引用（`KunZeroDependency`，依赖门禁的规则 C）。
- 线上形状不改：`Principal` 与 `DelegationActor` 的 JSON 属性名与先后、`tenant_id` 的显式 null、主体类型的三个字符串。要改先改服务间令牌的 Schema 与测试。
- `PrincipalKindConverter` 必须是公开类型（跨程序集的 JSON 源生成要引用它）。不要换成 `JsonStringEnumConverter<PrincipalKind>`：以特性指定时它收数字，校验会变松。
- 公共 API 登记在 PublicAPI 文件里。
