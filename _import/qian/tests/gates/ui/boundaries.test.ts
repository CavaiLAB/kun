import { ESLint } from 'eslint';
import path from 'node:path';
import { describe, expect, it } from 'vitest';
import { FRAMEWORK_PACKAGES } from '../../../eslint.boundaries.config.mjs';

// 依赖边界门禁自测：用根 eslint.boundaries.config.mjs 对虚拟文件路径 lint，文件不必真的存在——元素分类只看路径，
// 解析不到的 import 按说明符匹配。外壳与界面包只能经版本目录里钉住的 @cavailab/qian-* 引前端框架（S1-YUE-025）。
const ROOT = path.resolve(__dirname, '../../..');
const eslint = new ESLint({ cwd: ROOT, overrideConfigFile: path.join(ROOT, 'eslint.boundaries.config.mjs') });

async function lint(file: string, code: string) {
  const [result] = await eslint.lintText(code, { filePath: path.join(ROOT, file) });
  return (result?.messages ?? []).map((m) => `${m.ruleId}: ${m.message}`);
}

const UI_FILES = ['apps/qian-shell/src/__case__.ts', 'packages/qian-tenancy-ui/src/__case__.ts', 'packages/qian-tenancy-signup-ui/src/__case__.ts', 'packages/qian-identity-ui/src/__case__.ts',
  'packages/qian-shell-kit/src/__case__.ts', 'samples/qian-sample-crm-ui/src/__case__.tsx', 'tests/directory-e2e/src/__case__.tsx'];

describe('外壳与界面包的依赖边界', () => {
  it('放行的框架包就是版本目录里钉住的 9 个 @cavailab/qian-*', () => {
    expect(FRAMEWORK_PACKAGES).toEqual(['biz', 'contracts', 'core', 'data', 'form', 'plugin-sdk', 'sdk', 'shell', 'tokens'].map((name) => `@cavailab/qian-${name}`));
  });

  it.each(UI_FILES)('%s 可以引钉住的框架包与子路径导出', async (file) => {
    const code = "export { Button } from '@cavailab/qian-core';\nexport { safeNavigationUrl } from '@cavailab/qian-shell/menu';\nexport { ShellHost } from '@cavailab/qian-plugin-sdk';\n"
      + "export { adaptFederation } from '@cavailab/qian-plugin-sdk/federation';\nexport { UserSelect } from '@cavailab/qian-biz';\n";
    expect(await lint(file, code)).toEqual([]);
  });

  it.each(UI_FILES)('%s 不能引迁走的旧包名', async (file) => {
    for (const source of ['@idadawn/yue-core', '@idadawn/yue-shell/menu', '@yue/core', '@idadawn/qian-plugin-sdk', '@qian/plugin-sdk', '@idadawn/qian-runtime', '@qian/runtime']) {
      const messages = await lint(file, `export * from '${source}';\n`);
      expect(messages, source).toEqual([expect.stringMatching(/^boundaries\/dependencies: 前端依赖越界：不得依赖 /)]);
      expect(messages[0]).toContain(source);
    }
  });

  it('没钉进版本目录的 @cavailab/* 包被拒绝，即使它已经发布（平台运行时只给命令行与校验库的测试用）', async () => {
    for (const source of ['@cavailab/qian-runtime', '@cavailab/qian-designer', '@cavailab/other']) expect(await lint('apps/qian-shell/src/__violation__.ts', `export * from '${source}';\n`), source).toHaveLength(1);
  });

  it('外壳、界面包、外壳接线与参考远程之间互相引，第三方库、Node 内置模块与包内相对引用放行', async () => {
    expect(await lint('packages/qian-identity-ui/src/__ok__.ts', "export { createIdentityAuthProvider } from '@qian/shell-kit/identity';\nexport { clsx } from 'clsx';\nexport { browser } from './navigation';\n")).toEqual([]);
    expect(await lint('apps/qian-shell/src/__ok__.ts', "export { loadAppConfig } from '@qian/shell-kit';\nexport { readFileSync } from 'node:fs';\n")).toEqual([]);
  });
});
