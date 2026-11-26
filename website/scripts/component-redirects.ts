// 构建末尾写旧地址的跳转页，目标一律写相对路径，带前缀的预览站也能用：
// - 改过名的设计文档：design/prd/specs/yue → frontend，保留查询参数与锚点；
// - 拿掉的前端框架组件页（S1-DOCS-011）：/components/qian/ 下原来的 90 页与 /components/yue/ 下原来的 91 个旧书签，
//   HTML 与 Markdown 两种地址都落到 /components/qian/ 的说明页（清单见 qian-archive.ts）。
import fs from 'node:fs';
import path from 'node:path';
import { LEGACY_YUE_PAGES, NOTICE_PAGE, REMOVED_PAGES } from './qian-archive.ts';
import { listFiles, WEBSITE_ROOT } from './repo.ts';

/** 改过名的页面：current 是现在的产物路径，返回它原来的路径（没有改过名时 undefined）。 */
export function previousPath(current: string): string | undefined {
  if (/^design\/prd\/specs\/frontend\.(?:html|md)$/.test(current)) return current.replace('/frontend.', '/yue.');
}

export function redirectContents(previous: string, current: string): string {
  const target = path.posix.relative(path.posix.dirname(previous), current);
  if (previous.endsWith('.md')) return `[阅读当前文档](${target})\n`;
  return `<!doctype html>
<html lang="zh"><head><meta charset="utf-8"><title>文档已迁移</title>
<link rel="canonical" href="${target}">
<script>location.replace(${JSON.stringify(target)} + location.search + location.hash)</script>
<meta http-equiv="refresh" content="0;url=${target}"></head>
<body><a href="${target}">阅读当前文档</a></body></html>\n`;
}

/** 落到说明页的旧地址（相对产物根，带扩展名）：每个旧地址 HTML 与 Markdown 各一个。 */
export function removedPageFiles(): string[] {
  return [...REMOVED_PAGES, ...LEGACY_YUE_PAGES].flatMap((route) => [`${route}.html`, `${route}.md`]);
}

/** 拿掉的页面的跳转页：HTML 直接跳到说明页（原来的锚点在说明页上没有，不带过去），Markdown 只放一个指向说明页的链接。 */
export function removedPageContents(previous: string): string {
  const notice = `${NOTICE_PAGE}${path.posix.extname(previous)}`;
  const target = path.posix.relative(path.posix.dirname(previous), notice);
  if (previous.endsWith('.md')) return `本页已从站里拿掉，前端框架 Qian 的组件文档在哪里见[说明](${target})。\n`;
  return `<!doctype html>
<html lang="zh"><head><meta charset="utf-8"><title>本页已从站里拿掉</title>
<link rel="canonical" href="${target}">
<script>location.replace(${JSON.stringify(target)})</script>
<meta http-equiv="refresh" content="0;url=${target}"></head>
<body><a href="${target}">本页已从站里拿掉，前端框架 Qian 的组件文档在哪里见说明</a></body></html>\n`;
}

export function writeComponentRedirects(root: string): void {
  const write = (rel: string, contents: string) => {
    const file = path.join(root, rel);
    fs.mkdirSync(path.dirname(file), { recursive: true });
    fs.writeFileSync(file, contents);
  };
  for (const current of listFiles(root)) {
    const previous = previousPath(current);
    if (previous) write(previous, redirectContents(previous, current));
  }
  for (const previous of removedPageFiles()) {
    const contents = removedPageContents(previous);
    // 旧地址上又生成了页面（内容不是跳转页），说明清单或生成器改错了：不覆盖，直接失败
    const file = path.join(root, previous);
    if (fs.existsSync(file) && fs.readFileSync(file, 'utf8') !== contents) {
      throw new Error(`${previous} 是拿掉的旧地址，产物里却有这一页：改 website/scripts/qian-archive.ts 或生成器`);
    }
    write(previous, contents);
  }
}

if (import.meta.filename === process.argv[1]) writeComponentRedirects(path.join(WEBSITE_ROOT, 'doc_build'));
