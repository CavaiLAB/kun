# Kun.EventBus.EntityFrameworkCore

- 权威设计为 PRD §7.10、ADR-008、S1-EVT-001 与 S1-EVT-002；主文档为 README.md。
- 宿主装配通过 AddKunOutboxRelay 启动 EF 投递；业务组件只依赖 Kun.EventBus.Abstractions。
- 投递以「确认后立即逐条标记」为前提：不要改回整批事务提交，也不要在租约到期后继续发布已领取的行，否则停机或 broker 变慢时会重复发布。
- 真实数据库与 RabbitMQ 测试复用 Kun.Testing.Containers，按非超级用户角色验证；拓扑声明以 RabbitMQ 4 的默认限制为准。
- 目录接口与 ACL 属于 S1-EVT-003，死信重放工具未做；未交付项不得写成已实现。
