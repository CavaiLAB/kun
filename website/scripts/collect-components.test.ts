import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { PLATFORM_COMPONENTS, collectComponents } from './collect-components.ts';

describe('collectComponents（S1-DOCS-002 之前的占位目录）', () => {
  const files = collectComponents();
  const byPath = new Map(files.map((f) => [f.path, f.content]));

  it('每个规划中的组件一页，目录页列出全部组件', () => {
    const index = byPath.get('platform/index.md') ?? '';
    for (const c of PLATFORM_COMPONENTS) {
      assert.ok(byPath.has(`platform/${c.id}.md`), c.id);
      assert.ok(index.includes(`(/components/platform/${c.id})`), `目录页缺少 ${c.id}`);
    }
  });

  it('S2 及以后的组件在侧栏打阶段标记，S1 不打', () => {
    assert.match(byPath.get('platform/qian.workflow.md') ?? '', /^tag: S2$/m);
    assert.doesNotMatch(byPath.get('platform/qian.files.md') ?? '', /^tag:/m);
  });

  it('组件目录标明 Qian 冻结版本与标签来源', () => {
    assert.match(byPath.get('index.md') ?? '', /冻结在 0\.2\.1.*发布标签生成/);
    assert.match(byPath.get('qian/index.md') ?? '', /旧应用外壳按 Qian 0\.2\.1 归档，业务预发保留原有部署和数据/);
  });

  it('首页与其他部分链接到的组件页都存在', () => {
    for (const id of ['qian.gateway', 'qian.audit', 'qian.observability', 'qian.workflow', 'qian.identity', 'qian.authz', 'qian.files']) {
      assert.ok(byPath.has(`platform/${id}.md`), id);
    }
  });

  it('组件 id 重复时报错', () => {
    const first = PLATFORM_COMPONENTS[0];
    assert.ok(first);
    PLATFORM_COMPONENTS.push({ ...first });
    try {
      assert.throws(() => collectComponents(), /重复/);
    } finally {
      PLATFORM_COMPONENTS.pop();
    }
  });

  it('外壳页来自包目录且在索引、侧栏与三个共用示例中可发现', () => {
    const shell = byPath.get('qian/shell.mdx') ?? '';
    for (const name of ['AppShell', 'Authentication', 'FallbackPage']) assert.ok(shell.includes(`name="${name}"`));
    assert.ok(byPath.get('qian/index.md')?.includes('/components/qian/shell'));
    assert.ok(JSON.parse(byPath.get('qian/_meta.json') ?? '[]').includes('shell'));
  });

  it('业务选择器页来自包目录且在索引、侧栏与七个共用示例中可发现', () => {
    const biz = byPath.get('qian/biz.mdx') ?? '';
    for (const name of ['ApiSelect', 'DictSelect', 'OrganizeSelect', 'UserSelect', 'RoleSelect', 'AuthorizationTree', 'IconPicker']) assert.ok(biz.includes(`name="${name}"`));
    assert.ok(biz.includes('(/design/prd/specs/frontend)'));
    assert.ok(byPath.get('qian/index.md')?.includes('/components/qian/biz'));
    assert.ok(JSON.parse(byPath.get('qian/_meta.json') ?? '[]').includes('biz'));
  });
});
