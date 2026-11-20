// Rspress 同源 iframe 资源；它是文档站的一部分，不是独立应用或部署入口。
// 演示与示例源码来自 CavaiLAB/qian 的检出（qian.ts）：@cavailab/qian-* 的导出入口全部指到检出里的源码，只有一份；
// 检出里的组件与第三方库也引 react，dedupe 让它们与演示入口共用本站的一份。
import fs from 'node:fs';
import path from 'node:path';
import { createRequire } from 'node:module';
import { createRsbuild } from '@rsbuild/core';
import { pluginReact } from '@rsbuild/plugin-react';
import { pluginTailwindcss } from '@rsbuild/plugin-tailwindcss';
import { readYueDocs } from './yue-docs.ts';
import { WEBSITE_ROOT } from './repo.ts';
import { openQian, qianPackageDir, qianSourceAliases } from './qian.ts';
const require = createRequire(import.meta.url);
const cache = path.join(WEBSITE_ROOT, '.cache/yue');
fs.mkdirSync(cache, { recursive: true });
const qian = openQian();
const data = readYueDocs(qian);
const example = (name: 'data' | 'shell' | 'biz' | 'form', file: string) => fs.readFileSync(path.join(qianPackageDir(qian.dir, name), 'docs/examples', file), 'utf8');
const dataSource = example('data', 'data-demo.tsx');
const shellSource = example('shell', 'shell-demo.tsx');
const bizSource = example('biz', 'biz-demo.tsx');
const bizComponents = ['ApiSelect', 'DictSelect', 'OrganizeSelect', 'UserSelect', 'RoleSelect', 'AuthorizationTree', 'IconPicker'];
const dataComponents = ['DataTable', 'Tree', 'TreeSelect', 'Cascader', 'DatePicker', 'DateRange', 'TimePicker', 'TimeRange', 'Upload', 'ImagePreview'];
fs.writeFileSync(path.join(cache, 'sources.json'), JSON.stringify({
  ...Object.fromEntries(data.docs.map(d => [d.name, d.source])),
  ...Object.fromEntries(dataComponents.map(name => [name, dataSource])),
  ...Object.fromEntries(['AppShell', 'Authentication', 'FallbackPage'].map(name => [name, shellSource])),
  ...Object.fromEntries(bizComponents.map(name => [name, bizSource])),
  SchemaForm: example('form', 'schema-form-demo.tsx'),
}));
const vendor = path.join(WEBSITE_ROOT, 'docs/public/yue-vendor');
fs.rmSync(vendor, { recursive: true, force: true });
fs.mkdirSync(vendor, { recursive: true });
fs.copyFileSync(require.resolve('@babel/standalone/babel.min.js'), path.join(vendor, 'babel.min.js'));
fs.cpSync(path.join(path.dirname(require.resolve('monaco-editor/package.json')), 'min/vs'), path.join(vendor, 'vs'), {
  recursive: true,
  // 工作区只提供中/英界面；英语内置，保留简繁中文资源，不携带其它语言包。
  filter: source => { const locale = /^nls\.messages\.(.+)\.js$/.exec(path.basename(source))?.[1]; return !locale || ['zh-cn', 'zh-tw'].includes(locale); },
});
fs.copyFileSync(require.resolve('monaco-editor/LICENSE'), path.join(vendor, 'monaco-LICENSE'));
fs.copyFileSync(require.resolve('@babel/standalone/LICENSE'), path.join(vendor, 'babel-LICENSE'));
const rsbuild = await createRsbuild({ rsbuildConfig: {
  root: WEBSITE_ROOT,
  server: { publicDir: false },
  plugins: [pluginReact(), pluginTailwindcss()],
  source: { entry: { index: path.join(WEBSITE_ROOT, 'demo/main.tsx') } },
  resolve: { alias: { 'yue-demo-sources': path.join(cache, 'sources.json'), ...qianSourceAliases(qian.dir) }, dedupe: ['react', 'react-dom'] },
  output: { distPath: { root: path.join(WEBSITE_ROOT, 'docs/public/yue-demo') }, assetPrefix: './', cleanDistPath: true, copy: [] },
  html: { title: 'qian 组件交互演示', template: path.join(WEBSITE_ROOT, 'demo/index.html') },
} });
await rsbuild.build();
