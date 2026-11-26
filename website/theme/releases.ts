// 首页「最新动态」的数据：仓库根 CHANGELOG.md 里已发布的版本段（docs-site.md §2a.1 的 2026-11-26 标注，S1-DOCS-010）。
// 纯函数，不依赖 Rspress，单测见 scripts/releases.test.ts。

export interface ReleaseEntry {
  /** 版本号，如 2026.11.24。 */
  version: string;
  /** 发布日期，取版本段标题里的日期。 */
  date: string;
  /** 摘要：前几个小节的标题（去掉「Changed — 」这类类别前缀与括号里的任务号）；没有小节时取第一行正文。 */
  summary: string;
}

/** Keep a Changelog 的版本段标题：`## [2026.11.24] - 2026-11-24`。 */
const RELEASE_HEADING = /^##\s+\[([^\]\n]+)\](?:\s+-\s+(\d{4}-\d{2}-\d{2}))?/;
const CATEGORY_PREFIX = /^[A-Za-z]+\s+—\s+/;
const TASK_SUFFIX = /（S\d-[A-Z]+-\d+(?:[、，,]\s*S\d-[A-Z]+-\d+)*）$/;

function plain(text: string): string {
  return text
    .replace(/\[([^\]]*)\]\([^)]*\)/g, '$1')
    .replace(/`([^`]*)`/g, '$1')
    .trim();
}

function subsectionTitle(line: string): string {
  return plain(line.replace(/^###\s+/, ''))
    .replace(CATEGORY_PREFIX, '')
    .replace(TASK_SUFFIX, '')
    .trim();
}

/** 按文件顺序（新的在前）列出已发布的版本段；跳过 [Unreleased] 与没写日期的段。 */
export function parseReleases(markdown: string, maxTitles = 3): ReleaseEntry[] {
  const releases: Array<{ version: string; date: string; titles: string[]; firstLine: string }> = [];
  let current: (typeof releases)[number] | null = null;
  for (const line of markdown.split('\n')) {
    const heading = RELEASE_HEADING.exec(line);
    if (heading) {
      const version = (heading[1] ?? '').trim();
      const date = heading[2] ?? '';
      current = date && version.toLowerCase() !== 'unreleased' ? { version, date, titles: [], firstLine: '' } : null;
      if (current) releases.push(current);
      continue;
    }
    if (!current) continue;
    if (/^##\s/.test(line)) {
      current = null;
      continue;
    }
    if (/^###\s/.test(line)) {
      const title = subsectionTitle(line);
      if (title) current.titles.push(title);
      continue;
    }
    if (!current.firstLine) {
      const text = plain(line.replace(/^\s*[-*]\s+/, ''));
      if (text && !text.startsWith('#') && !text.startsWith('>')) current.firstLine = text;
    }
  }
  return releases.map(({ version, date, titles, firstLine }) => ({
    version,
    date,
    summary:
      titles.length === 0
        ? firstLine
        : titles.length > maxTitles
          ? `${titles.slice(0, maxTitles).join('；')}，共 ${titles.length} 项`
          : titles.join('；'),
  }));
}
