// llms.txt 的排版（docs-site.md §5、ADR-021 D21.5）：先按 project 分（顺序与显示名见 PROJECT_ORDER），每个项目下再按顶部导航的部分分节，
// 一节一个 H2「<项目> · <部分>」，每页一行 `- [标题](<md 地址>): 摘要`。给 rspress.config.ts 的 llms.llmsTxt 用，纯函数便于单测。

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

/** 项目顺序与显示名；frontmatter 没写 project 的页归入平台（手写页默认 platform，docs-site.md §4）。 */
export const PROJECT_ORDER: ReadonlyArray<{ id: string; label: string }> = [
  { id: 'platform', label: '平台' },
  { id: 'kun', label: 'Kun' },
  { id: 'qian', label: 'qian 组件库' },
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

export function renderLlmsTxt(input: LlmsTxtInput): string {
  const out: string[] = [];
  if (input.title) out.push(`# ${input.title}`);
  if (input.description) out.push('', `> ${input.description}`);
  for (const project of PROJECT_ORDER) {
    for (const section of input.sections) {
      const pages = section.pages.filter((page) => projectOf(page) === project.id);
      if (pages.length === 0) continue;
      out.push('', `## ${project.label} · ${section.title}`, '', ...pages.map(line));
    }
  }
  return `${out.join('\n').trimStart()}\n`;
}
