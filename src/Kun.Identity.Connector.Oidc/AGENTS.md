# 通用 OIDC 连接器约定

- 协议与签名校验由内核提供者 `Kun.Identity` 实现。平台端口依赖 `Kun.Identity.Abstractions` 与 `Kun.Configuration.Abstractions`，跨域依赖遵守仓库根的档位门禁。
- 连接器无状态：state、nonce、PKCE verifier 由身份服务生成并核对关联，这里只按协议使用；不要在连接器里保存流程状态或签发平台令牌。
- 客户端密钥只经 secretRef 现取现用，不缓存明文、不写日志、不进异常消息；配置与清单的 `ToString` 不打印内容。
- id_token 校验（签名算法族、iss、aud、azp、exp、nonce、RFC 9207 的 iss 参数）任何一项都不能放宽；改动这些校验要配篡改用例，并做变异验证确认用例能拦住。
- 配置 Schema 的唯一来源是 `ComponentConfiguration.Schema`；组件目录的 `schemas/config.schema.json` 由清单工具派生。稳定入口只注册 oidc；程序化适配器从同一声明移除 name 的 const，允许宿主自定注册名。改配置成员要同步 `OidcConnectorOptions`、代码 Schema 与 README。
- 不复制其他产品的身份代码。连接器只回 `ExternalAssertion`，绑定、JIT 建号与会话签发归身份 Provider（`Kun.Identity.Server`）；id_token 一律验签。
