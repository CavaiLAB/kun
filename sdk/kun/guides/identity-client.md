> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Identity 客户端、浏览器会话与迁移

:::info 发布版本来源
本页由本仓库的发布标签生成，源文件为 [`docs/guides/identity-client.md`](https://github.com/CavaiLAB/kun/blob/v2026.11.24/docs/guides/identity-client.md)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

内核 Kun.Identity 提供 OIDC 依赖方协议与会话端口。平台包 Kun.Identity.Server 提供基于 OpenIddict 的身份服务，Kun.Identity.Client 将平台会话接入宿主。S2S 的唯一新入口是网关 [S2sTokenIssuer](/sdk/kun/security/s2s-jwt.md)，本指南说明外部身份进入浏览器会话的路径。

## 装配

```csharp
services.AddKunIdentity(identity =>
{
    identity.AddOidcClient("primary", options =>
    {
        options.Authority = "https://identity.example.com";
        options.ClientId = "web-app";
        options.ClientSecret = configuration["Identity:ClientSecret"]!;
    });
    identity.AddCookieSession(options => options.UseRedisStore("web-app:identity:"));
});
// 预先注册 IConnectionMultiplexer，或通过 Kun.Caching.UseRedis 配置连接。
services.AddKunIdentityAspNetCore();
services.AddAuthorization();

app.UseKunCookieAuth();
app.UseAuthentication();
app.UseAuthorization();
```

将 `Authority` 设为发现文档公布的 `issuer` 原值，包括根路径末尾的斜杠。协议客户端对两个标识逐字比较。

Web 装配登记 SignInHelper、ChallengeHelper、OidcLoginService、OidcLogoutService、当前用户接口及防跨站请求服务。默认使用 KunCookieAuth 方案返回 401；若宿主已选择认证方案则保留其选择。服务登记幂等，所有请求辅助类在请求 scope 中解析。生产副本必须共享 Data Protection 密钥和相同的会话 Redis 前缀；不同应用自行选择隔离前缀。开发/测试可显式 UseInMemoryStore，生产会话不走缓存失败后的本地内存回退。

登录入口调用 `ChallengeHelper.BuildLoginUrlAsync`，redirectUri 必须来自受信任配置中的公开回调地址。该方法生成 PKCE S256、state、nonce，将原始回调地址与本地 returnUrl 一同加密认证后放入短期 cookie。回调调用 `OidcLoginService.CompleteAsync(context, code, state, ct)`，成功后以返回的本地路径执行 LocalRedirect；宿主将无效事务或令牌映射为统一的 400/401 错误响应。

CompleteAsync 先验证保护 cookie、期限、本地返回地址和一次性 state，再发送原始 code_verifier 换码，并校验 ID Token 的签名、issuer、client audience、寿命与 nonce。旧字符串签名 ExchangeCodeAsync 和静态 ConsumeChallengeCookie 不再允许不带绑定信息的流程。SignInHelper 只接受客户端已校验的令牌结果，或应用已信任的 SessionData；不能把客户端上传的令牌对象当作已验证身份。兼容构造器中的额外 IJwtValidator 策略仍然执行，不会代替 nonce 绑定校验。

## 续期、吊销和跨站请求

ISessionRegistry 在 ISessionStore 的基础上提供 RenewAsync、RevokeUserAsync、ApplyLogoutAsync 和 TryUseNonceAsync。续期只修改仍存在的有效会话，吊销后返回 false，绝不通过 SetAsync 重新创建旧会话。SetAsync 是创建/显式替换入口，不可用作滑动续期。Redis 的会话索引、吊销与重放标记在脚本内原子更新；内存实现仅用于单进程开发测试。

浏览器 cookie 认证的非安全方法默认验证防跨站令牌。宿主可提供受保护的 GET 端点，通过 IAntiforgery.GetAndStoreTokens 返回请求令牌，前端在写请求附 `X-CSRF-TOKEN`，并保留对应防跨站 cookie。缺失或错误令牌在业务处理器前被拒绝；不要为普通业务写端点关闭此验证。

`MapKunBackChannelLogout("/auth/backchannel", "primary")` 处理身份提供方的表单 POST。它验证签名、issuer/audience、iat/exp、logout event、sub/sid，并禁止 nonce，再按 issuer 和主体/提供方会话定位本地会话。jti 消费与吊销一起原子提交，重放不会吊销后续新登录；已退出会话仍返回成功。该端点由签名令牌认证，所以显式匿名且豁免浏览器防跨站机制。协议依据：[OpenID 后通道登出](https://openid.net/specs/openid-connect-backchannel-1_0.html#Validation)、[ID Token 校验](https://openid.net/specs/openid-connect-core-1_0.html#IDTokenValidation)。

日志只记录本地 sid 的 SHA256 和错误类型，不传入可能含 sid 的后端异常内容。scripts/check-identity-boundaries.py 与真实异常日志测试共同守住该边界。

## 客户端令牌操作

- ExchangeCodeAsync 接收 OidcCodeExchange（code、redirectUri、原始 verifier、原始 nonce）。
- RefreshAsync 发送 refresh_token，并在提供方返回新 ID Token 时核对原 subject；刷新结果由会话拥有者负责持久化，不会自动延长已吊销会话。
- ClientCredentialsAsync 请求提供方的机器令牌，不在 Kun 内创建产品凭据库。
- discovery 默认最多缓存五分钟，可通过 DiscoveryRefreshInterval 配置；JWKS 在令牌校验时重新获取。生产端点默认要求 HTTPS。

## 升级边界

1. 替换手写明文 challenge cookie 和无 PKCE/nonce 的换码调用，改用完整辅助流程；旧接口发出 KUNOIDC001。
2. 字节缓存版 RedisSessionStore 构造器仅保留兼容（KUNSESSION001），不具备原子索引能力。UseRedisStore 改用直接 Redis 连接；新键格式与旧缓存不兼容，升级时安排重新登录，不能把旧会话当作已迁移。
3. 不再用 SetAsync 做滑动续期；使用原子 RenewAsync。定制存储需实现 ISessionRegistry。
4. 网关只签规范的 UUID 租户/会话/成员/项目标识；不能把不透明的提供方 sid 或浏览器 cookie 直接当作 S2S session_id。应用的可信主体解析器负责从身份权威投影完整字段及有效修订。
5. 已退役的签发类型迁至归档。新调用方使用 Kun.Gateway.S2sTokenIssuer。
6. Sandbox 默认模板为 kun-default；ProviderExtensions 由调用方显式选择后端支持的键。旧命名扩展仍按旧协议序列化并标记废弃，防止丢弃已有调用方明确要求的安全策略；新扩展不能覆盖核心资源字段。

## 连接外部身份源

平台的 OIDC 连接器与应用依赖方共用发现、PKCE 换码、JWKS 缓存和 ID Token 验证。Issuer 必须与发现文档逐字相同。回调把发行方返回的 `iss` 传入 `OidcCodeExchange.ResponseIssuer`。声明支持 RFC 9207 的发行方需要返回该参数。

两个入口默认允许 RS、PS、ES 系列的 256、384、512 位签名算法，均可按发行方收窄。应用依赖方默认要求单一受众，平台连接器可接受附带本客户端 azp 的多受众令牌。时钟容差缺省两分钟，可设为零至五分钟。平台身份服务负责创建并一次性消费登录事务，连接器通过 secretRef 在每次换码时解析客户端密钥。

验收项目覆盖真实 OpenIddict 登录与刷新、Redis 浏览器会话、签名密钥轮换及带目标受众的网关令牌。[身份服务](https://github.com/CavaiLAB/kun/blob/v2026.11.24/src/Kun.Identity.Server/README.md) · [平台客户端](https://github.com/CavaiLAB/kun/blob/v2026.11.24/src/Kun.Identity.Client/README.md) · [OIDC 连接器](https://github.com/CavaiLAB/kun/blob/v2026.11.24/src/Kun.Identity.Connector.Oidc/README.md)
