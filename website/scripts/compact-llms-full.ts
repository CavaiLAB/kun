// SSG-MD 会按一列里最长的单元格给整张表补对齐空格；构建后把 llms-full.txt 里表格的对齐填充去掉，单元格内容与代码块不变。
// 原来同时收敛前端框架组件页的 .md（属性表的类型很长）；那些页面 2026-11-26 起不再生成（S1-DOCS-011），只剩 llms-full.txt 这一步。
// 各页自己的 .md 不动：知识库索引的 sha256 按它们算。
import fs from 'node:fs';
import path from 'node:path';
import { WEBSITE_ROOT } from './repo.ts';

export function compactTables(markdown: string): string {
  let fence = '';
  return markdown.split('\n').map(line => {
    const marker = /^\s*(`{3,}|~{3,})/.exec(line)?.[1];
    if (marker) { if (!fence) fence = marker; else if (marker[0] === fence[0] && marker.length >= fence.length) fence = ''; return line; }
    if (fence || !line.startsWith('|') || !line.trimEnd().endsWith('|')) return line;
    const cells = line.trimEnd().split(/(?<!\\)\|/);
    return `| ${cells.slice(1, -1).map(cell => cell.trim()).join(' | ')} |`;
  }).join('\n');
}

export function compactLlmsFull(root = path.join(WEBSITE_ROOT, 'doc_build')): void {
  const full = path.join(root, 'llms-full.txt');
  fs.writeFileSync(full, compactTables(fs.readFileSync(full, 'utf8')));
}

if (process.argv[1]?.endsWith('compact-llms-full.ts')) compactLlmsFull();
