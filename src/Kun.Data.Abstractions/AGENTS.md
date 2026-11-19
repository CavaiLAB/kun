# Kun.Data.Abstractions

- 本包拥有数据层的平台契约与错误码；严格上下文基类与数据会话契约在 `Kun.EntityFrameworkCore`（隔离检查、启动安全范围与 DeletedBy 的约定见那里的 README 与 AGENTS.md）。保持业务组件只依赖 Definition 的方向。
- 完整数据功能边界见本目录 README；未交付项不得写成已实现。
- 公共 API 变动登记 PublicAPI 文件；新增错误码在本包定义，遵守统一 ErrorDetail。
