# Kun.Configuration

## For AI Agents

- `IOptionsMonitor<T>` 在变更时会重建并重新校验。`AddKunOptions<T>` 现在注册保留最后有效值的监视器：绑定或校验失败不替换旧对象，也不通知消费者，同时发出 `kun.configuration.reload.rejected` 指标与无配置值的告警。`ValidateOnStart` 仍拒绝首份无效的关键配置。
- 传入 `JsonTypeInfo<T>` 的重载会登记 `OptionsSchema`；`Export()` 输出 JSON Schema 2020-12。Schema 表达 JSON 契约，DataAnnotations 与自定义语义校验仍在 validator 中执行。
- AOT 宿主使用带绑定委托的 `AddKunOptions<T>`，通过 `IValidateOptions<T>` 校验。三个反射绑定重载依赖动态代码。
