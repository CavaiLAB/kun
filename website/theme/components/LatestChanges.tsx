// 首页「最新动态」：读仓库根 CHANGELOG.md 里已发布的版本段，取最近几个版本（docs-site.md §2a.1 的 2026-11-26 标注），
// 链接到由发布段所在提交镜像的「Kun 版本记录」，不另维护一份。解析见 theme/releases.ts。
import { withBase } from '@rspress/core/runtime';
import { Link } from '@rspress/core/theme';
import changelog from '../../../CHANGELOG.md?raw';
import { parseReleases } from '../releases';

/** 「Kun 版本记录」：scripts/kun.ts 从发布段所在提交的 CHANGELOG.md 生成。 */
export const RELEASES_ROUTE = '/sdk/kun/releases/';

export function LatestChanges({ count = 4 }: { count?: number }) {
  const entries = parseReleases(changelog).slice(0, count);
  if (import.meta.env.SSG_MD) {
    return (
      <ul>
        {entries.map((entry) => (
          <li key={entry.version}>
            {entry.date} <a href={withBase(`${RELEASES_ROUTE}index.md`)}>Kun {entry.version}</a>
            {entry.summary ? `：${entry.summary}` : ''}
          </li>
        ))}
      </ul>
    );
  }
  return (
    <ul className="qian-latest rp-not-doc">
      {entries.map((entry) => (
        <li key={entry.version}>
          <time dateTime={entry.date}>{entry.date}</time>
          <div>
            <Link href={RELEASES_ROUTE}>Kun {entry.version}</Link>
            {entry.summary ? <p>{entry.summary}</p> : null}
          </div>
        </li>
      ))}
    </ul>
  );
}
