// llms.txt 的排版（docs-site.md §5、§5.2，ADR-021 D21.5）：标题与一句话定位之后，先是一段给大模型的读法与「从这里开始」一节（S1-DOCS-010），
// 再按 project 分（顺序与显示名见 PROJECT_ORDER），每个项目下按顶部导航的部分分节，一节一个 H2「<项目> · <部分>」，
// 每页一行 `- [标题](<md 地址>): 摘要`。给 rspress.config.ts 的 llms.llmsTxt 用，纯函数便于单测。

export interface LlmsPage {
  routePath: string;
  link: string;
  title?: string;
  description?: string;
  frontmatter?: Record<string, unknown>;
}

export interface LlmsSection {
  title: string;
  pages: LlmsPage[];
}

export interface LlmsTxtInput {
  title?: string;
  description?: string;
  sections: LlmsSection[];
}

export interface LlmsTxtOptions {
  /** 放在一句话定位之后的读法说明（纯文本，可以多行）。 */
  guide?: string;
  /** 「从这里开始」一节按顺序收录的页面路由；站里找不到其中一页即抛错，免得清单烂掉。 */
  startHere?: readonly string[];
}

/** 项目顺序与显示名；frontmatter 没写 project 的页归入平台（手写页默认 platform，docs-site.md §4）。 */
export const PROJECT_ORDER: ReadonlyArray<{ id: string; label: string }> = [
  { id: 'platform', label: '平台' },
  { id: 'kun', label: 'Kun' },
  { id: 'qian', label: 'qian 组件库' },
];

/** 给大模型的读法（docs-site.md §5.2）。说的是各节是什么、先读哪里，不替页面许诺能力。 */
export const LLMS_GUIDE = [
  '给大模型的读法：要写调用 Kun 的代码，先读「从这里开始」一节，照那几页写；那几页的步骤在本机跑通过，页里写明了核对时的 Kun 版本。',
  '每一行的链接就是该页的 Markdown，全站正文合在同一目录的 llms-full.txt 里。',
  '接口的精确签名查「Kun · SDK」一节里的 API 参考，它只收内核档的包。',
  '「平台 · 设计与路线」是需求与决策记录，讲为什么这样设计，里面有不少规划，不是用法；哪些能力已有、哪些还是规划，看「指南」一页的表。',
  '「qian 组件库」开头的几节是前端框架 Qian 的组件文档，它的仓库已归档。',
].join('\n');

/** 「从这里开始」一节的页面，按顺序（docs-site.md §5.2）：使用文档在前，再是清单、命令行、API 与版本的入口。 */
export const START_HERE: readonly string[] = [
  '/start/getting-started',
  '/start/guides/write-a-component',
  '/start/guides/authorization-dotnet',
  '/start/guides/',
  '/start/concepts/component',
  '/cli/plugin-json',
  '/cli/commands',
  '/sdk/api/',
  '/sdk/kun/',
  '/start/versions',
];

function projectOf(page: LlmsPage): string {
  const project = page.frontmatter?.project;
  return typeof project === 'string' && PROJECT_ORDER.some((p) => p.id === project) ? project : 'platform';
}

function line(page: LlmsPage): string {
  const title = page.title?.trim() || page.routePath;
  const description = page.description?.replace(/\s+/g, ' ').trim();
  return `- [${title}](${page.link})${description ? `: ${description}` : ''}`;
}

/** 路由的比较形式：目录首页 `/a/`、`/a/index`、`/a/index.html` 视为同一页，`.html` 后缀去掉。 */
export function normalizeRoute(route: string): string {
  const path = (route.startsWith('/') ? route : `/${route}`).replace(/\.html$/, '');
  return path.endsWith('/index') ? path.slice(0, -'index'.length) : path;
}

function startHereSection(input: LlmsTxtInput, routes: readonly string[]): string[] {
  const pages = new Map<string, LlmsPage>();
  for (const section of input.sections)
    for (const page of section.pages) if (!pages.has(normalizeRoute(page.routePath))) pages.set(normalizeRoute(page.routePath), page);
  const picked = routes.map((route) => {
    const page = pages.get(normalizeRoute(route));
    if (!page) throw new Error(`llms.txt 的「从这里开始」列了 ${route}，站里没有这一页；改 website/scripts/llms.ts 的 START_HERE`);
    return page;
  });
  return ['', '## 从这里开始', '', ...picked.map(line)];
}

export function renderLlmsTxt(input: LlmsTxtInput, options: LlmsTxtOptions = {}): string {
  const out: string[] = [];
  if (input.title) out.push(`# ${input.title}`);
  if (input.description) out.push('', `> ${input.description}`);
  if (options.guide?.trim()) out.push('', options.guide.trim());
  if (options.startHere?.length) out.push(...startHereSection(input, options.startHere));
  for (const project of PROJECT_ORDER) {
    for (const section of input.sections) {
      const pages = section.pages.filter((page) => projectOf(page) === project.id);
      if (pages.length === 0) continue;
      out.push('', `## ${project.label} · ${section.title}`, '', ...pages.map(line));
    }
  }
  return `${out.join('\n').trimStart()}\n`;
}
