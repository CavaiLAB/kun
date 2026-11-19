# Kun.Data.Abstractions

Data seam 的 Definition：迁移协调、序号与雪花 ID 端口，基准类型、方言、数据连接、记录与过滤的契约，以及数据错误码。严格上下文基类 `KunStrictDbContext`、数据服务与会话端口和装配异常在内核的 [Kun.EntityFrameworkCore](../Kun.EntityFrameworkCore/README.md)，本包引用它。业务组件只依赖本包，Provider 位于 Kun.Data。

## 数据端口

IDataMigrator 是多副本迁移协调端口：逐个业务上下文在 schema 级 advisory lock 内执行未应用的迁移，返回每个上下文的 DataMigrationReport（实际执行的迁移与等锁时长）。ISequenceService 按租户规则原子取号，规则模式由 SequencePattern 解析与格式化（如 `'SO' + yyyyMMdd + seq(6)`）；ISnowflakeIdGenerator 给需要数值主键的场景发 64 位雪花 ID。实现与装配见 [Kun.Data](../Kun.Data/README.md)。

基准类型目录与数据连接档案的契约：

- `BaseDataType` 与 `DataTypeContracts.SystemTypes`：10 个系统类型来自嵌入的 base-data-types.json；
- `DbDialect`、`DialectTypeMapping`、`DataTypeLimit`、`DataValueRange` 是方言 SPI qian.data.dialect/v1 的数据。`DbDialect.Validate` 核对映射，`DataTypeContracts.ReadDialect` 读取并核对方言包的 JSON；
- `DialectTypeMapping.Map` 按长度、精度、小数位与取值区间校验 `DataFieldDefinition`，返回补齐缺省值的 `DataFieldMapping`；
- `IDataDialect` 是方言组件实现的 SPI（描述数据 + 连通探测），`IDataDialectRegistry` 是 `Data.Dialects` 注册表，`IDataTypeCatalog` 是 GET /api/data/meta/types 背后的目录；
- `IDataConnectionRegistry` 管理按租户隔离的连接档案。`DataConnectionDraft` 只接受 secretRef，反序列化时拒绝 password 等未声明成员；`DataConnectionProfile` 不含口令；
- `DataPermissions` 列出端点权限点。

JSON Schema 为 data-types.schema.json 与 data-connection.schema.json，源生成元数据在 `DataContractJson`。上下限在 JSON 中以十进制字符串表示，避免 64 位整数失真。

装配错误码 DATA_MISCONFIGURED 由 Kun.EntityFrameworkCore 的 `KunDataMisconfiguredException.Code` 公开，异常携带 Error。序号与雪花 ID 另有 DATA_SEQUENCE_RULE_INVALID、DATA_SEQUENCE_RULE_NOT_FOUND、DATA_SEQUENCE_EXHAUSTED 与 DATA_ID_WORKER_UNAVAILABLE，经 ErrorException 抛出。基准类型与数据连接另有 DATA_TYPE_*、DATA_DIALECT_NOT_FOUND、DATA_FIELD_*、DATA_CONNECTION_* 共 11 个错误码，宿主经 `DataErrors.TypesAndConnections` 一次登记，含义见 [Kun.Data](../Kun.Data/README.md)。宿主统一映射为 ErrorDetail，不自造错误体。

## 组件位置

本目录提供代码契约、PublicAPI 清单和说明，适用于全部 .NET profile。Kun.Data 已有真实 Provider 入口；完整 qian.data 安装表面、profile 接线与 qian add 验收仍在 S1-DATA-001 中推进。

## 模型体验

Agent 可识别稳定的错误码和系统字段，不获取数据库口令。Data 本身不发布模型可调用的 SQL 工具。

## 已知限制与延期项

参考应用的千行索引验收已通过；完整代码组件安装仍由 SDK / CLI 后续任务交付。Filter AST、运算符矩阵、系统变量、数据规则、IRecordHook、记录事件载荷 `RecordChangedPayload`（登记为 qian.record.changed.v1）与 after Hook 视图 `RecordChangedEvent` 由 S1-DATA-004 随本包交付，权威语义和示例见 [数据契约规格](../../_import/qian/docs/prd/specs/data-contracts.md)。视图从 Qian.Events 写入端生成的信封派生，所以本包引用同为 Definition 的 Qian.Events.Abstractions。S2 的 SQL 生成、Hook 调度及动态记录引擎仍由后续任务实现。

## Kun 依赖（S1-KUN-010）

本包引用内核契约 Kun.EntityFrameworkCore（严格基类与数据会话契约）。契约依赖契约，不需要门禁例外；不能通过关闭隔离校验消除依赖。
