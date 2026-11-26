// 第 2 部分「组件目录」的生成器（docs-site.md §2、§3）。
// S1-DOCS-002 之前仓库里还没有组件文件夹：本脚本先按 PRD §7 与组件模型规格 §7.7 的规划清单生成占位页，
// 让首页与其他部分的链接有落点、目录结构先定下来。S1-DOCS-002 把数据源换成各组件文件夹（README 契约 + docs/*.mdx），
// 页面改为 §3 的九节模板；生成目录不进 Git，也不接受手改。
// 前端框架 Qian 的组件页 2026-11-26 起不再生成（S1-DOCS-011，S1-DEC-047 D2）：/components/qian/ 只留一页说明（noticePage），
// 原来的地址由 component-redirects.ts 落到这一页。
import fs from 'node:fs';
import path from 'node:path';
import { LEGACY_YUE_PAGES, QIAN_ARCHIVE, REMOVED_PAGES, qianArchiveUrl } from './qian-archive.ts';
import { PLATFORM_DOCS_ROOT, REPO_ROOT, REPO_URL, repoFileDate, yamlString, type GeneratedFile } from './repo.ts';
import { designRoute } from './sync-specs.ts';

type Stage = 'S1' | 'S2' | 'S3' | 'S4';

export interface PlannedComponent {
  id: string;
  name: string;
  stage: Stage;
  /** 一句话用途，取自 PRD 对应小节或 docs-site.md §2a.2。 */
  summary: string;
  /** 设计依据：仓库内 docs/ 下的文件，生成时换成 /design/ 路由。 */
  refs: Array<{ text: string; doc: string }>;
}

const S1_PRD = 'docs/prd/07-s1-foundation.md';
/** 占位页的内容来自本脚本的规划清单，updated 取本脚本的提交日期（S1-DOCS-002 换成组件文件夹后改取各组件文档的日期）。 */
const SELF = 'website/scripts/collect-components.ts';
const SPEC = 'docs/prd/specs/component-model.md';

export const PLATFORM_COMPONENTS: PlannedComponent[] = [
  { id: 'qian.registry', name: '注册表', stage: 'S1', summary: '平台插件三件套之一：清单解析与校验、注册表、租户 scope patch 持久化、修订保留与回切记录；只进 platform profile。', refs: [{ text: 'PRD §7.2', doc: S1_PRD }, { text: '组件模型规格 §7.7、§9.5', doc: SPEC }] },
  { id: 'qian.lifecycle', name: '生命周期', stage: 'S1', summary: '平台插件三件套之一：制品 → 修订 → 激活与回切，租户级开通、升级、卸载，强制验签。', refs: [{ text: 'PRD §7.2', doc: S1_PRD }, { text: '组件模型规格 §5.6', doc: SPEC }] },
  { id: 'qian.capabilities', name: '能力目录', stage: 'S1', summary: '平台插件三件套之一：命令 / 查询 / 事件 / 实体四类能力目录、扩展点与契约仓库、OpenAPI 聚合。', refs: [{ text: 'PRD §7.2', doc: S1_PRD }] },
  { id: 'qian.hooks', name: '钩子', stage: 'S1', summary: '把组件 hooks.json 里的 command / http 监听器转成同模式的进程内监听器。', refs: [{ text: '组件模型规格 §4.4、§5.4', doc: SPEC }] },
  { id: 'qian.tenancy', name: '租户目录与生命周期', stage: 'S1', summary: '租户主档、隔离策略装配（默认共享库 + tenant_id + RLS）、显式状态机与开通编排。', refs: [{ text: 'PRD §7.3', doc: S1_PRD }, { text: 'ADR-004', doc: 'docs/adr/ADR-004-database-tenant-isolation.md' }] },
  { id: 'qian.identity', name: '身份与 SSO', stage: 'S1', summary: 'OIDC / OAuth2 端点、Person + Membership 双层账号、统一登录流水线，连接器 oidc / wecom。', refs: [{ text: 'PRD §7.4', doc: S1_PRD }, { text: 'ADR-005', doc: 'docs/adr/ADR-005-identity-sso.md' }] },
  { id: 'qian.authz', name: '鉴权引擎', stage: 'S1', summary: '权限点注册表、租户域 RBAC 与插件默认角色模板。', refs: [{ text: 'PRD §7.5', doc: S1_PRD }, { text: 'ADR-006', doc: 'docs/adr/ADR-006-authorization-model.md' }] },
  { id: 'qian.data', name: '数据存储默认安全装配', stage: 'S1', summary: 'RLS 拦截器、租户与软删过滤、审计字段、并发令牌与同库 Outbox 的默认装配。', refs: [{ text: 'PRD §7.6', doc: S1_PRD }, { text: 'ADR-004', doc: 'docs/adr/ADR-004-database-tenant-isolation.md' }] },
  { id: 'qian.files', name: '文件存储', stage: 'S1', summary: '租户前缀与配额、预签名上传下载、强制附件下载；对象存储 / S3 兼容 / 本地三种 Provider。', refs: [{ text: 'PRD §7.7', doc: S1_PRD }, { text: '组件模型规格 §6.2', doc: SPEC }] },
  { id: 'qian.config', name: '配置中心', stage: 'S1', summary: 'Apollo Provider（长轮询、访问密钥签名、本地缓存）、配置源顺序与先校验后生效的热更新。', refs: [{ text: 'PRD §7.8', doc: S1_PRD }, { text: 'ADR-007', doc: 'docs/adr/ADR-007-config-center-apollo.md' }] },
  { id: 'qian.gateway', name: '业务 API 网关', stage: 'S1', summary: '网关防护：强制认证、路由级限流、可信代理、出网 SSRF 基线。', refs: [{ text: 'PRD §7.9', doc: S1_PRD }, { text: 'ADR-010', doc: 'docs/adr/ADR-010-gateway-layers.md' }] },
  { id: 'qian.events', name: '事件总线', stage: 'S1', summary: 'Outbox 同事务写入与可靠发布、至少一次消费、幂等表、死信与租户回灌。', refs: [{ text: 'PRD §7.10', doc: S1_PRD }, { text: 'ADR-008', doc: 'docs/adr/ADR-008-events-messaging.md' }] },
  { id: 'qian.audit', name: '审计', stage: 'S1', summary: '命令级哈希链审计与实体级同事务审计，actor 含 agent。', refs: [{ text: 'PRD §7.11', doc: S1_PRD }, { text: 'ADR-009', doc: 'docs/adr/ADR-009-audit.md' }] },
  { id: 'qian.observability', name: '可观测', stage: 'S1', summary: 'OTel、租户 / 用户 / 插件 baggage 与计量事件。', refs: [{ text: 'PRD §7.11', doc: S1_PRD }] },
  { id: 'qian.notify', name: '通知', stage: 'S1', summary: '消息类型注册、模板与发送配置、异步投递，首批三个渠道。', refs: [{ text: 'PRD §7.12', doc: S1_PRD }] },
  { id: 'qian.scheduler', name: '调度', stage: 'S1', summary: 'PG 持久化的作业与触发器、集群安全领取、按业务键可取消。', refs: [{ text: 'PRD §7.13', doc: S1_PRD }] },
  { id: 'qian.realtime', name: '实时推送', stage: 'S1', summary: '可选（P1）：WebSocket / SSE 会话、一次性握手票据、Redis 背板扇出与在线状态。', refs: [{ text: 'PRD §7.13', doc: S1_PRD }] },
  { id: 'qian.entitlement', name: '授权 v0', stage: 'S1', summary: '「租户 × 插件 × 计划」授权表、运营手工授予与权益查询 API。', refs: [{ text: 'PRD §7.14', doc: S1_PRD }, { text: 'ADR-014 D14.5', doc: 'docs/adr/ADR-014-marketplace.md' }] },
  { id: 'qian.shell', name: '前端外壳', stage: 'S1', summary: 'Kun 旧应用外壳的静态制品与 API，包括 MF2 宿主、共享依赖和版本化插槽注册表。', refs: [{ text: 'PRD §7.15', doc: S1_PRD }, { text: 'ADR-013', doc: 'docs/adr/ADR-013-frontend-framework.md' }] },
  { id: 'qian.model-gateway', name: '模型服务网关底座', stage: 'S1', summary: '可选（P1）：GPUStack 注册为 Higress 上游、Provider Adapter、逻辑模型目录。', refs: [{ text: 'PRD §7.16', doc: S1_PRD }, { text: 'ADR-015', doc: 'docs/adr/ADR-015-model-gateway.md' }] },
  { id: 'qian.docs', name: '文档生成器', stage: 'S1', summary: '开发态组件：qian docs generate 从代码生成配置目录、模块图与事件图。', refs: [{ text: '组件模型规格 §7.7', doc: SPEC }, { text: 'ADR-020 D20.9', doc: 'docs/adr/ADR-020-component-model-v2.md' }] },
  { id: 'qian.workflow', name: '流程与自动化', stage: 'S2', summary: '审批内核与触发器：一套 JSON 定义、节点树视图设计器。', refs: [{ text: 'ADR-012', doc: 'docs/adr/ADR-012-workflow-engine.md' }, { text: 'PRD §8', doc: 'docs/prd/08-s2-lowcode.md' }] },
];

const PLACEHOLDER_NOTE =
  '> 占位页：组件文件夹尚未建立。S1-DOCS-002 起本页由收集脚本从组件文件夹生成，按[组件页模板](/design/prd/specs/docs-site)固定九节：用途、安装、配置、表面、API、示例、模型体验、已知限制与延期项、变更。';

function refLinks(refs: PlannedComponent['refs'], repoRoot: string): string {
  return refs
    .map((r) => {
      if (!fs.existsSync(path.join(repoRoot, r.doc))) throw new Error(`组件目录引用的文档不存在：${r.doc}`);
      return `- [${r.text}](${designRoute(r.doc)})`;
    })
    .join('\n');
}

function componentPage(c: PlannedComponent, repoRoot: string, docsRoot: string): string {
  return [
    '---',
    `title: ${yamlString(`${c.id} ${c.name}`)}`,
    `description: ${yamlString(c.summary)}`,
    'part: components',
    'project: platform',
    'audience: [developer, operator]',
    `updated: ${repoFileDate(SELF, repoRoot)}`,
    `component: ${c.id}`,
    `stage: ${c.stage}`,
    // S1 是当前阶段不另标；更后阶段在侧栏里显示阶段标记（docs-site.md §2a：规划中的能力打阶段标记）
    ...(c.stage === 'S1' ? [] : [`tag: ${c.stage}`]),
    '---',
    '',
    `# ${c.id} ${c.name}`,
    '',
    PLACEHOLDER_NOTE,
    '',
    `**阶段**：${c.stage}`,
    '',
    c.summary,
    '',
    '## 设计依据',
    '',
    refLinks(c.refs, docsRoot),
    '',
  ].join('\n');
}

export function collectComponents(repoRoot: string = REPO_ROOT, docsRoot: string = PLATFORM_DOCS_ROOT): GeneratedFile[] {
  const ids = new Set<string>();
  for (const c of PLATFORM_COMPONENTS) {
    if (ids.has(c.id)) throw new Error(`组件 id 重复：${c.id}`);
    ids.add(c.id);
  }
  const files: GeneratedFile[] = PLATFORM_COMPONENTS.map((c) => ({ path: `platform/${c.id}.md`, content: componentPage(c, repoRoot, docsRoot) }));

  const platformRows = PLATFORM_COMPONENTS.map((c) => `| [${c.id}](/components/platform/${c.id}) | ${c.name} | ${c.stage} | ${c.summary} |`).join('\n');
  files.push({
    path: 'platform/index.md',
    content: [
      '---',
      `title: ${yamlString('平台插件')}`,
      `description: ${yamlString('身份、鉴权、租户、文件、配置、事件、审计等平台插件的目录。')}`,
      'part: components',
      'project: platform',
      'audience: [developer, operator]',
      `updated: ${repoFileDate(SELF, repoRoot)}`,
      '---',
      '',
      '# 平台插件',
      '',
      '多租户平台的能力都以组件交付：租户、身份与 SSO、鉴权、配置、事件与消息、调度与实时、权益等。规划中的组件清单来自 PRD §7 与组件模型规格 §7.7；带代码的组件同时可作为 NuGet 包独立使用（ADR-019）。',
      '',
      PLACEHOLDER_NOTE.replace('本页由收集脚本', '各组件页由收集脚本'),
      '',
      '| 组件 | 名称 | 阶段 | 用途 |',
      '|---|---|---|---|',
      platformRows,
      '',
    ].join('\n'),
  });

  files.push({ path: 'qian/index.md', content: noticePage(repoRoot) });

  files.push({
    path: 'index.md',
    content: [
      '---',
      `title: ${yamlString('组件目录')}`,
      `description: ${yamlString('每个组件一页：怎么装、怎么配、暴露了什么；现在收录的是平台插件。')}`,
      'part: components',
      'project: platform',
      'audience: [developer, operator, isv]',
      `updated: ${repoFileDate(SELF, repoRoot)}`,
      '---',
      '',
      '# 组件目录',
      '',
      '「组件怎么用」只写在这一部分：每个组件一页，讲用途、安装（`qian add <id>`）、配置、表面、API、示例、模型体验与已知限制。编程模型与 API 原理在 [SDK](/sdk/)，命令与清单字段在 [CLI 与规格](/cli/)。',
      '',
      `- [平台插件](/components/platform/)：${PLATFORM_COMPONENTS.length} 个规划中的组件`,
      '',
      '前端框架 Qian 的组件页 2026-11-26 起不再收录：它的仓库已归档，组件文档在归档仓库的标签里，见[说明](/components/qian/)。',
      '',
    ].join('\n'),
  });

  // 说明页不进侧栏：组件目录首页与原来的地址都指向它
  files.push({ path: '_meta.json', content: `${JSON.stringify(['index', { type: 'dir', name: 'platform', label: '平台插件' }], null, 2)}\n` });
  files.push({ path: 'platform/_meta.json', content: `${JSON.stringify(['index', ...PLATFORM_COMPONENTS.map((c) => c.id)], null, 2)}\n` });
  return files;
}

/**
 * /components/qian/ 的说明页（S1-DOCS-011）：组件页从本站拿掉了，前端框架的仓库已归档，组件文档在归档仓库的哪个标签、哪个目录。
 * 原来的 90 页与 /components/yue/ 的旧书签都落到这一页（component-redirects.ts）。
 */
export function noticePage(repoRoot: string = REPO_ROOT): string {
  const pkgDocs = (name: string) => `[${name}](${qianArchiveUrl(`packages/qian/${name}/docs/index.mdx`, 'blob')})`;
  return [
    '---',
    `title: ${yamlString('前端框架 Qian 的组件页已从本站拿掉')}`,
    `description: ${yamlString(`本站 2026-11-26 起不再收录前端框架 Qian 的组件页；它的仓库已归档，组件文档在归档仓库的标签 ${QIAN_ARCHIVE.tag} 里。`)}`,
    'part: components',
    'project: qian',
    'audience: [developer, agent]',
    `updated: ${repoFileDate(SELF, repoRoot)}`,
    '---',
    '',
    '# 前端框架 Qian 的组件页已从本站拿掉',
    '',
    `本站 2026-11-26 起不再收录前端框架 Qian 的组件页（产品负责人定，登记在任务 [S1-DEC-047](${REPO_URL}/blob/main/pm/tasks/S1-DEC-047.md) D2）。原来 \`/components/qian/\` 下的 ${REMOVED_PAGES.length + 1} 页，连同更早的 \`/components/yue/\` 书签（${LEGACY_YUE_PAGES.length} 个），现在都落到这一页。`,
    '',
    `前端框架 Qian 的仓库 [${QIAN_ARCHIVE.repo}](${QIAN_ARCHIVE.url}) 2026-11-25 已归档，只读，不再发新版；已发布的 \`@cavailab/qian-*\` 留在 GitHub Packages，继续可以安装。`,
    '',
    '## 组件文档在哪里',
    '',
    `原来的组件页是构建时从归档仓库的标签 \`${QIAN_ARCHIVE.tag}\`（提交 \`${QIAN_ARCHIVE.commit.slice(0, 12)}\`）生成的，源都在这个标签里：`,
    '',
    '| 原来的页面 | 原来的地址 | 归档仓库里的源 |',
    '|---|---|---|',
    `| 基础组件（52 个入口）与通用上下文 API | \`/components/qian/core/<组件名>\`、\`/components/qian/core/shared-api\` | [packages/qian/core/docs/](${qianArchiveUrl('packages/qian/core/docs')})：用途说明 \`catalog.json\`、示例 \`examples/catalog.tsx\`、状态清单 \`scenarios.json\`；属性表取自 [packages/qian/core/src/](${qianArchiveUrl('packages/qian/core/src')}) 的 TypeScript 类型 |`,
    '| 继承的 DOM、ARIA 与事件属性 | `/components/qian/api/native-<编号>` | 同上，取自组件属性继承的 React 类型声明 |',
    `| 设计令牌 | \`/components/qian/tokens\` | [packages/qian/tokens/](${qianArchiveUrl('packages/qian/tokens')}) |`,
    `| 表单与弹层、数据组件、外壳与认证、业务选择器 | \`/components/qian/form\`、\`/components/qian/data\`、\`/components/qian/shell\`、\`/components/qian/biz\` | 各包的 \`docs/index.mdx\`：${['form', 'data', 'shell', 'biz'].map(pkgDocs).join('、')} |`,
    '',
    `包名、包源与令牌的配置见归档仓库的[安装与升级说明](${qianArchiveUrl('docs/migrating-from-yue.md', 'blob')})。`,
    '',
    '## 本站还有哪些与它有关',
    '',
    '- [前端 SDK](/sdk/frontend/)：旧外壳的插件树、远程模块与插槽。',
    '- [宿主与框架](/start/hosts)：旧外壳的归档源码。',
    `- [前端框架规格](${designRoute('docs/prd/specs/frontend.md')})：设计与阶段规划。`,
    '- [版本矩阵](/start/versions)：本仓库钉住的 `@cavailab/qian-*` 版本。',
    '',
    '本仓库里的旧外壳、三个界面包与参考应用的远程模块还在用 Qian 已发布的包；本站的主题也用它的令牌包 `@cavailab/qian-tokens`。',
    '',
  ].join('\n');
}
