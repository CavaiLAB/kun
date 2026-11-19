# Kun.Security.Abstractions

服务间令牌的主体模型与声明词汇表，命名空间 `Kun.Security`：

- `Principal`：服务间令牌载荷的 DTO（签发方、受众、主体、主体类型、租户、角色、scope、会话、委托链与时间）。平台主体显式写出 `tenant_id: null`；`ToString()` 不输出会话、令牌标识与委托链。它只是数据，不验签、不签发、不判权限。
- `DelegationActor`：RFC 8693 的委托链，现在只预留，不签发。
- `PrincipalKind` 与 `PrincipalKindConverter`：主体类型写成 `user`、`service`、`agent` 三个小写字符串；读的时候数字、大小写不对与其它值都报错。转换器是公开类型，别的程序集里的 JSON 源生成上下文可以引用它。
- `IdentityClaimNames`：声明名的唯一词汇表。`S2sRequired` 与 `S2sOptional` 是服务间令牌载荷的必需与可选字段，`IdentityToken` 是用户令牌的词汇。

零依赖（只用 BCL），AOT 兼容。签发、校验与从声明解析主体的实现在 `Kun.Security`；公共表面带这一组的契约包（身份契约、`Kun.Plugin.Sdk`）只引用本包，不会带上 `Kun.Security` 的 JWT 依赖。

本包不带 `JsonSerializerContext`。要用源生成读写 `Principal` 的，在自己的上下文里写 `[JsonSerializable(typeof(Principal))]`，`PrincipalKind` 上的转换器特性会一起生效。
