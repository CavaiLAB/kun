import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { PLATFORM_COMPONENTS, collectComponents } from './collect-components.ts';
import { LEGACY_YUE_PAGES, NOTICE_PAGE, QIAN_ARCHIVE, REMOVED_PAGES } from './qian-archive.ts';

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
});

describe('前端框架的组件页拿掉之后（S1-DOCS-011）', () => {
  const files = collectComponents();
  const byPath = new Map(files.map((f) => [f.path, f.content]));
  const notice = byPath.get('qian/index.md') ?? '';

  it('qian/ 下只生成说明页，不再有组件页，侧栏也不列它', () => {
    assert.deepEqual(files.filter((f) => f.path.startsWith('qian/')).map((f) => f.path), ['qian/index.md']);
    assert.equal(NOTICE_PAGE, 'components/qian/index', '跳转页的目标就是这一页');
    assert.deepEqual(JSON.parse(byPath.get('_meta.json') ?? '[]'), ['index', { type: 'dir', name: 'platform', label: '平台插件' }]);
  });

  it('说明页写明仓库已归档、组件文档在归档仓库的哪个标签与哪些目录，链接钉在标签所在的提交上', () => {
    assert.match(notice, /^project: qian$/m);
    assert.match(notice, /2026-11-25 已归档/);
    assert.ok(notice.includes(`\`${QIAN_ARCHIVE.tag}\``));
    for (const dir of ['packages/qian/core/docs', 'packages/qian/core/src', 'packages/qian/tokens']) {
      assert.ok(notice.includes(`${QIAN_ARCHIVE.url}/tree/${QIAN_ARCHIVE.commit}/${dir})`), dir);
    }
    for (const name of ['form', 'data', 'shell', 'biz']) {
      assert.ok(notice.includes(`${QIAN_ARCHIVE.url}/blob/${QIAN_ARCHIVE.commit}/packages/qian/${name}/docs/index.mdx)`), name);
    }
    assert.ok(notice.includes(`原来 \`/components/qian/\` 下的 91 页`));
    assert.ok(notice.includes(`（${LEGACY_YUE_PAGES.length} 个）`));
    assert.doesNotMatch(notice, /\(\/components\/qian\/(?:core|api|tokens|form|data|shell|biz)/, '说明页不能再链接拿掉的页面');
  });

  it('组件目录首页只列平台插件，指向说明页', () => {
    const index = byPath.get('index.md') ?? '';
    assert.ok(index.includes('(/components/platform/)'));
    assert.ok(index.includes('[说明](/components/qian/)'));
    assert.doesNotMatch(index, /qian 组件库/);
  });

  it('原来的地址清单：90 页加首页共 91 页，/components/yue/ 的旧书签一一对应，没有重复', () => {
    assert.equal(REMOVED_PAGES.length, 90);
    assert.equal(new Set(REMOVED_PAGES).size, REMOVED_PAGES.length);
    assert.ok(!REMOVED_PAGES.includes(NOTICE_PAGE));
    assert.equal(REMOVED_PAGES.filter((r) => r.startsWith('components/qian/core/')).length, 53, '52 个基础组件入口加通用上下文 API');
    assert.equal(REMOVED_PAGES.filter((r) => r.startsWith('components/qian/api/native-')).length, 32);
    assert.equal(LEGACY_YUE_PAGES.length, 91);
    assert.ok(LEGACY_YUE_PAGES.every((r) => r.startsWith('components/yue/')));
  });
});
