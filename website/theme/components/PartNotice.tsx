// 第 2、3、4 部分每页顶部显示站点和组件参考的版本来源（docs-site.md §5）。
// Kun 小节与 Kun API 参考（frontmatter project: kun）不显示：它们按 Kun 最新的发布版镜像，各页横幅已写明版本（§2b）。
import { usePage } from '@rspress/core/runtime';
import { Link } from '@rspress/core/theme';
import { VERSIONED_PARTS } from '../site';

export function PartNotice() {
  const { page } = usePage();
  // Markdown 产物里不放这条界面提示：版本信息已在 frontmatter 的 version 字段
  if (import.meta.env.SSG_MD) return null;
  const part = VERSIONED_PARTS.find((p) => page.routePath.startsWith(p.prefix));
  if (!part || page.frontmatter?.project === 'kun') return null;
  return (
    <div className="qian-part-notice">
      {part.name} · main 文档。
      <Link href="/start/versions">查看版本矩阵</Link>。
    </div>
  );
}
