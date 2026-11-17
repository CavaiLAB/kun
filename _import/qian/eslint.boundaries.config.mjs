// 外壳与界面包的依赖边界（pnpm ui:boundaries；根 eslint.config.mjs 也带上这一段）。自测在 tests/gates/ui/boundaries.test.ts。
// 前端框架已迁到 CavaiLAB/qian（S1-YUE-025）：这里的代码只能引版本目录里钉住的 @cavailab/qian-* 已发布包，
// 迁走的旧包名（@idadawn/yue-*、设计文档简称 @yue/*、@idadawn/qian-plugin-sdk、@idadawn/qian-runtime 及其简称 @qian/plugin-sdk、@qian/runtime）
// 与没钉进版本目录的 @cavailab/* 包一律拒绝。框架内部的分层矩阵由 CavaiLAB/qian 自己查，这里不重复。
import { readFileSync } from 'node:fs';
import boundaries from 'eslint-plugin-boundaries';
import tseslint from 'typescript-eslint';

/** 外壳与界面包可以引的前端框架包：pnpm-workspace.yaml 版本目录里钉住的 @cavailab/qian-*（加新包先钉版本）。 */
export const FRAMEWORK_PACKAGES = [...readFileSync(new URL('./pnpm-workspace.yaml', import.meta.url), 'utf8').matchAll(/^[ \t]+"(@cavailab\/qian-[a-z0-9-]+)":[ \t]*\S+/gm)]
  .map((match) => match[1])
  .sort();

/** 受这套边界约束的目录：外壳应用、三个界面包、外壳接线、参考应用的远程模块与真实目录验收页。 */
export const UI_ROOTS = ['apps', 'packages/qian-tenancy-ui', 'packages/qian-tenancy-signup-ui', 'packages/qian-identity-ui', 'packages/qian-shell-kit', 'samples/qian-sample-crm-ui', 'tests/directory-e2e'];

const elements = UI_ROOTS.map((root) => ({ type: 'ui', pattern: root === 'apps' ? 'apps/*' : root }));
const retired = '@idadawn/**,@yue/**,@qian/plugin-sdk{,/**},@qian/runtime{,/**}';

const policies = [
  {
    from: { element: { type: 'ui' } },
    allow: {
      to: [
        // 外壳、界面包、外壳接线与参考远程之间互相引（经工作区解析到源码），包内相对引用同理。
        { element: { type: 'ui' } },
        // 钉住的框架包与它们的子路径导出。
        { module: { source: FRAMEWORK_PACKAGES.map((name) => `${name}{,/**}`) } },
        // 第三方库与 Node 内置模块放行；@cavailab/* 只走上一条，旧包名不放行。
        { module: { source: `!{@cavailab/**,${retired}}` } },
      ],
    },
  },
];

/** 只含边界规则的 ESLint 配置片段（根 eslint.config.mjs 与本文件的默认导出共用）。 */
export const uiBoundariesConfig = [
  {
    files: UI_ROOTS.map((root) => `${root}/**/*.{ts,tsx,mts,mjs}`),
    languageOptions: { parser: tseslint.parser, ecmaVersion: 'latest', sourceType: 'module' },
    plugins: { boundaries },
    settings: {
      'boundaries/elements': elements,
      'boundaries/dependency-nodes': ['import', 'dynamic-import', 'export'],
      'boundaries/ignore': ['**/node_modules/**', '**/dist/**'],
      'import/resolver': {
        typescript: { project: UI_ROOTS.map((root) => (root === 'apps' ? 'apps/*/tsconfig.json' : `${root}/tsconfig.json`)) },
      },
    },
    rules: {
      'boundaries/dependencies': [
        'error',
        {
          default: 'disallow',
          checkAllOrigins: true,
          checkUnknownLocals: true,
          message: '前端依赖越界：不得依赖 {{ dependency.source }}。框架只能引 pnpm-workspace.yaml 版本目录里钉住的 @cavailab/qian-* 包（规则在 eslint.boundaries.config.mjs）',
          policies,
        },
      ],
      'boundaries/no-unknown-files': 'off',
    },
  },
];

export default [
  { ignores: ['**/node_modules/**', '**/dist/**', 'website/**', 'tests/gates/**'] },
  ...uiBoundariesConfig,
];
