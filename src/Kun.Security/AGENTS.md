# Kun.Security

`AddKunSecurity()` 以 `TryAdd` 登记 HTTP 上下文访问器与 `ICurrentUser` 的适配器 `JwtCurrentUser`。需要请求租户的宿主另行登记 `AddKunMultiTenancy()`，然后在认证之后调用 `UseKunMultiTenancy()`。数据与审计组件可以先于身份适配器登记。详见 `docs/design/when-to-use-currentuser.md` 的「端口登记与次序」。
