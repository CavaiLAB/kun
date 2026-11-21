// 兼容既有组件页的书签和 Markdown 地址。采用相对目标以支持带前缀的预览站。
import fs from 'node:fs';
import path from 'node:path';
import { listFiles, WEBSITE_ROOT } from './repo.ts';

export function previousComponentPath(current: string): string | undefined {
  if (!/\.(?:html|md)$/.test(current)) return;
  if (current.startsWith('components/qian/')) return current.replace('components/qian/', 'components/yue/');
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

export function writeComponentRedirects(root: string): void {
  for (const current of listFiles(root)) {
    const previous = previousComponentPath(current);
    if (!previous) continue;
    const file = path.join(root, previous);
    fs.mkdirSync(path.dirname(file), { recursive: true });
    fs.writeFileSync(file, redirectContents(previous, current));
  }
}

if (import.meta.filename === process.argv[1]) writeComponentRedirects(path.join(WEBSITE_ROOT, 'doc_build'));
