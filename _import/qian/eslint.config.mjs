// 外壳与界面包（apps/*、可部署的前端包、外壳接线、参考应用的远程模块与真实目录验收页）的 ESLint 配置：代码规则 + 依赖边界 + 文案规则。
// 只跑边界门禁用 eslint.boundaries.config.mjs；文档站 website/ 不在此列。前端框架本身（CavaiLAB/qian）的规则在那个仓库里查。
import js from '@eslint/js';
import reactHooks from 'eslint-plugin-react-hooks';
import globals from 'globals';
import tseslint from 'typescript-eslint';
import { UI_ROOTS, uiBoundariesConfig } from './eslint.boundaries.config.mjs';
import { noHardcodedText } from './eslint.i18n.mjs';

export default tseslint.config(
  {
    ignores: ['**/node_modules/**', '**/dist/**', 'website/**', '**/.turbo/**', '**/playwright-report/**', '**/test-results/**', 'tests/gates/ui/fixtures/**'],
  },
  {
    files: UI_ROOTS.map((root) => `${root}/**/*.{ts,tsx,mts}`),
    extends: [js.configs.recommended, ...tseslint.configs.recommended, reactHooks.configs.flat.recommended],
    languageOptions: { globals: { ...globals.browser, ...globals.node } },
    rules: {
      '@typescript-eslint/consistent-type-imports': ['error', { fixStyle: 'inline-type-imports' }],
      '@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_', varsIgnorePattern: '^_' }],
      '@typescript-eslint/no-explicit-any': 'error',
      'no-console': ['error', { allow: ['warn', 'error'] }],
    },
  },
  {
    files: [...UI_ROOTS.map((root) => `${root}/**/*.mjs`), '**/*.cjs'],
    extends: [js.configs.recommended],
    languageOptions: { globals: { ...globals.node } },
  },
  ...uiBoundariesConfig,
  {
    files: ['packages/qian-tenancy-ui/src/**/*.{ts,tsx}', 'packages/qian-tenancy-signup-ui/src/**/*.{ts,tsx}', 'packages/qian-identity-ui/src/**/*.{ts,tsx}'],
    ignores: ['**/*.test.ts', '**/*.test.tsx', '**/test/**', '**/*.generated.ts', '**/i18n.ts', '**/locales.ts'],
    plugins: { 'qian-i18n': { rules: { 'no-hardcoded-text': noHardcodedText } } },
    rules: { 'qian-i18n/no-hardcoded-text': 'error' },
  },
);
