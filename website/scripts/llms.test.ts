import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { LLMS_GUIDE, PROJECT_ORDER, START_HERE, normalizeRoute, renderLlmsTxt } from './llms.ts';

describe('llms.txt 按 project 分节（ADR-021 D21.5）', () => {
  it('先平台再 Kun 再前端框架 Qian，每个项目下按部分分节；没写 project 的页归平台', () => {
    const txt = renderLlmsTxt({
      title: 'Kun 开发文档',
      description: '一句话。',
      sections: [
        {
          title: '开始',
          pages: [
            { routePath: '/start/', link: '/start/index.md', title: '开始', description: '  两行\n摘要 ', frontmatter: { project: 'platform' } },
            { routePath: '/start/hosts', link: '/start/hosts.md', title: '宿主', frontmatter: {} },
          ],
        },
        {
          title: '组件目录',
          pages: [{ routePath: '/components/qian/', link: '/components/qian/index.md', title: '前端框架 Qian 的组件页已从本站拿掉', frontmatter: { project: 'qian' } }],
        },
        {
          title: 'SDK',
          pages: [
            { routePath: '/sdk/kun/', link: '/sdk/kun/index.md', title: 'Kun 框架', description: '镜像', frontmatter: { project: 'kun' } },
            { routePath: '/sdk/dotnet/', link: '/sdk/dotnet/index.md', title: '后端 SDK', frontmatter: { project: 'platform' } },
            { routePath: '/sdk/x', link: '/sdk/x.md', frontmatter: { project: 'unknown' } },
          ],
        },
        { title: '空的部分', pages: [] },
      ],
    });
    assert.equal(
      txt,
      [
        '# Kun 开发文档',
        '',
        '> 一句话。',
        '',
        '## 平台 · 开始',
        '',
        '- [开始](/start/index.md): 两行 摘要',
        '- [宿主](/start/hosts.md)',
        '',
        '## 平台 · SDK',
        '',
        '- [后端 SDK](/sdk/dotnet/index.md)',
        '- [/sdk/x](/sdk/x.md)',
        '',
        '## Kun · SDK',
        '',
        '- [Kun 框架](/sdk/kun/index.md): 镜像',
        '',
        '## 前端框架 Qian（已归档） · 组件目录',
        '',
        '- [前端框架 Qian 的组件页已从本站拿掉](/components/qian/index.md)',
        '',
      ].join('\n'),
    );
  });

  it('没有标题与描述时从第一节开始', () => {
    assert.equal(renderLlmsTxt({ sections: [{ title: 'A', pages: [{ routePath: '/a', link: '/a.md', title: 'a' }] }] }), '## 平台 · A\n\n- [a](/a.md)\n');
  });
});

describe('llms.txt 的读法与「从这里开始」（docs-site.md §5.2、S1-DOCS-010）', () => {
  const sections = [
    {
      title: '开始',
      pages: [
        { routePath: '/start/', link: '/start/index.md', title: '开始', frontmatter: { project: 'platform' } },
        { routePath: '/start/getting-started', link: '/start/getting-started.md', title: '上手', description: '装两个包。', frontmatter: { project: 'platform' } },
        { routePath: '/start/guides/index', link: '/start/guides/index.md', title: '指南', frontmatter: { project: 'platform' } },
      ],
    },
    {
      title: 'SDK',
      pages: [{ routePath: '/sdk/api/', link: '/sdk/api/index.md', title: 'API 参考', frontmatter: { project: 'kun' } }],
    },
  ];

  it('读法与「从这里开始」排在分节之前，按给定顺序列页；这些页在原来的分节里照旧出现', () => {
    const txt = renderLlmsTxt(
      { title: '站名', description: '一句话。', sections },
      { guide: '  第一行\n第二行  ', startHere: ['/sdk/api/', '/start/getting-started', '/start/guides/'] },
    );
    assert.equal(
      txt,
      [
        '# 站名',
        '',
        '> 一句话。',
        '',
        '第一行',
        '第二行',
        '',
        '## 从这里开始',
        '',
        '- [API 参考](/sdk/api/index.md)',
        '- [上手](/start/getting-started.md): 装两个包。',
        '- [指南](/start/guides/index.md)',
        '',
        '## 平台 · 开始',
        '',
        '- [开始](/start/index.md)',
        '- [上手](/start/getting-started.md): 装两个包。',
        '- [指南](/start/guides/index.md)',
        '',
        '## Kun · SDK',
        '',
        '- [API 参考](/sdk/api/index.md)',
        '',
      ].join('\n'),
    );
  });

  it('「从这里开始」列了站里没有的页就抛错', () => {
    assert.throws(() => renderLlmsTxt({ sections }, { startHere: ['/start/getting-started', '/start/gone'] }), /\/start\/gone/);
  });

  it('目录首页的几种写法视为同一页', () => {
    assert.equal(normalizeRoute('/start/guides/index'), '/start/guides/');
    assert.equal(normalizeRoute('/start/guides/index.html'), '/start/guides/');
    assert.equal(normalizeRoute('start/getting-started.html'), '/start/getting-started');
    assert.equal(normalizeRoute('/start/guides/'), '/start/guides/');
  });

  it('站点用的清单不重复，读法里提到了「从这里开始」', () => {
    assert.equal(new Set(START_HERE.map(normalizeRoute)).size, START_HERE.length);
    assert.match(LLMS_GUIDE, /「从这里开始」/);
  });

  it('读法里前端框架那一组的名字与分节用的组名一致，并说明组件页不在本站（S1-DOCS-011）', () => {
    const label = PROJECT_ORDER.find((p) => p.id === 'qian')?.label ?? '';
    assert.equal(label, '前端框架 Qian（已归档）');
    assert.ok(LLMS_GUIDE.includes(`「${label}」`));
    assert.match(LLMS_GUIDE, /组件页不在本站/);
    assert.doesNotMatch(LLMS_GUIDE, /qian 组件库/);
  });
});
