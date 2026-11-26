// 外壳与界面包的令牌门禁（pnpm ui:lint 的第一步；自测在 tests/gates/ui/tokens.test.ts）。
// apps/* 的样式表不得重新定义令牌角色：用已发布的 @cavailab/qian-tokens 导出的 lintTokenCss 查，令牌目录跟版本目录钉的版本走。
// 其余消费方（界面包、外壳接线、参考远程、真实目录验收页）只查不强制灰阶抗锯齿。文档站的组件演示 2026-11-26 起拿掉了（S1-DOCS-011）。
// 令牌本身的生成结果、对比度与首帧脚本由 CavaiLAB/qian 查，这里不重复。
import { readFileSync, readdirSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { lintTokenCss } from '@cavailab/qian-tokens';

/** 跑令牌角色检查的目录（同时查抗锯齿）。 */
export const TOKEN_ROOTS = ['apps'];
/** 只查抗锯齿的消费方。 */
export const SMOOTHING_ROOTS = ['packages/qian-tenancy-ui', 'packages/qian-tenancy-signup-ui', 'packages/qian-identity-ui', 'packages/qian-shell-kit', 'samples/qian-sample-crm-ui', 'tests/directory-e2e'];

const SKIP = new Set(['node_modules', 'dist', '.turbo', 'test-results', 'playwright-report', '.lighthouseci']);
// 正文字形按原型走浏览器缺省：令牌层不强制灰阶抗锯齿，应用与示例的样式表也不能再加回来（S1-YUE-019 验收发现过一次）。
const SMOOTHING = /@apply[^;]*\bantialiased\b|-webkit-font-smoothing\s*:\s*antialiased|-moz-osx-font-smoothing\s*:\s*grayscale/;

/** 查一份样式表；roles 为真时同时查令牌角色。返回错误文字（不带文件名）。 */
export function checkCss(css, { roles }) {
  const errors = roles ? lintTokenCss(css) : [];
  if (SMOOTHING.test(css.replace(/\/\*[\s\S]*?\*\//g, ''))) errors.push('不要强制灰阶抗锯齿（antialiased / font-smoothing），正文字形用浏览器缺省。');
  return errors;
}

function* stylesheets(directory) {
  for (const entry of readdirSync(directory, { withFileTypes: true })) {
    if (SKIP.has(entry.name)) continue;
    const file = path.join(directory, entry.name);
    if (entry.isDirectory()) yield* stylesheets(file);
    else if (entry.name.endsWith('.css')) yield file;
  }
}

/** 查整个工作区，返回「相对路径: 错误」。 */
export function checkTokens(root) {
  const errors = [];
  for (const [roots, roles] of [[TOKEN_ROOTS, true], [SMOOTHING_ROOTS, false]])
    for (const dir of roots)
      for (const file of stylesheets(path.join(root, dir)))
        errors.push(...checkCss(readFileSync(file, 'utf8'), { roles }).map((error) => `${path.relative(root, file)}: ${error}`));
  return errors;
}

if (process.argv[1] === fileURLToPath(import.meta.url)) {
  const errors = checkTokens(path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..'));
  if (errors.length) {
    process.stderr.write(errors.join('\n') + '\n');
    process.exitCode = 1;
  } else console.log('令牌门禁通过');
}
