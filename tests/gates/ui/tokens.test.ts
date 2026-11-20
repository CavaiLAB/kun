import path from 'node:path';
import { describe, expect, it } from 'vitest';
import { checkCss, checkTokens } from '../../../scripts/ui-tokens.mjs';

// 令牌门禁（scripts/ui-tokens.mjs）自测：令牌角色检查用已发布的 @cavailab/qian-tokens 导出的 lintTokenCss，抗锯齿检查是本仓库的规则。
const ROOT = path.resolve(__dirname, '../../..');

describe('外壳与界面包的令牌门禁', () => {
  it('应用样式表不得重新定义令牌角色；站点或应用自己的变量、对令牌的引用放行', () => {
    expect(checkCss(':root { --qian-primary: 0.5 0.2 250; }', { roles: true })).toEqual(['令牌角色只能在唯一目录中定义：--qian-primary']);
    expect(checkCss(':root { --docs-card-min: 12rem; } a { color: oklch(var(--qian-primary)); }', { roles: true })).toEqual([]);
    // 注释里提到的不算定义。
    expect(checkCss('/* --qian-primary: 0 0 0; */ a { color: red; }', { roles: true })).toEqual([]);
  });

  it('任何消费方都不得强制灰阶抗锯齿；只查抗锯齿时不查令牌角色', () => {
    for (const css of ['body { @apply font-sans antialiased; }', 'body { -webkit-font-smoothing: antialiased; }', 'body { -moz-osx-font-smoothing: grayscale; }'])
      expect(checkCss(css, { roles: false }), css).toHaveLength(1);
    expect(checkCss('/* 不要写 -webkit-font-smoothing: antialiased */ body { margin: 0; }', { roles: false })).toEqual([]);
    expect(checkCss(':root { --qian-primary: 0 0 0; }', { roles: false })).toEqual([]);
  });

  it('真实的外壳、界面包、真实目录验收页与文档站演示通过', () => {
    expect(checkTokens(ROOT)).toEqual([]);
  });
});
