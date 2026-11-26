import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import { describe, it } from 'node:test';
import { parseReleases } from '../theme/releases.ts';
import { readKunVersion } from './kun.ts';
import { REPOSITORY_ROOT } from './repo.ts';

describe('首页「最新动态」读仓库根 CHANGELOG.md 的发布段（S1-DOCS-010）', () => {
  it('跳过 Unreleased，摘要取前几个小节标题，去掉类别前缀与任务号；没有小节时取第一行正文', () => {
    const markdown = [
      '# Changelog',
      '',
      '## [Unreleased]',
      '',
      '### Added — 还没发布的',
      '',
      '## [2026.11.24] - 2026-11-24',
      '',
      '### Changed — 端点权限回调的校验',
      '',
      '- 正文不进摘要',
      '',
      '### Added — `IPermissionEnforcementRequirement` 标记接口（S1-AZ-009）',
      '',
      '### Fixed — 甲（S1-KUN-014、S1-QA-003）',
      '',
      '### Fixed — 乙',
      '',
      '## [2026.10.12-hotfix.3] - 2026-10-12',
      '',
      '- 只有一行 [说明](https://example.com) 的 `修复`',
      '',
      '## [0.1.0]',
      '',
      '### Added — 没写日期的段不列',
    ].join('\n');
    assert.deepEqual(parseReleases(markdown), [
      { version: '2026.11.24', date: '2026-11-24', summary: '端点权限回调的校验；IPermissionEnforcementRequirement 标记接口；甲，共 4 项' },
      { version: '2026.10.12-hotfix.3', date: '2026-10-12', summary: '只有一行 说明 的 修复' },
    ]);
  });

  it('仓库里的 CHANGELOG.md：第一个发布段就是站点镜像的 Kun 版本，每段都有摘要', () => {
    const releases = parseReleases(fs.readFileSync(path.join(REPOSITORY_ROOT, 'CHANGELOG.md'), 'utf8'));
    assert.equal(releases[0]?.version, readKunVersion());
    for (const release of releases.slice(0, 4)) assert.ok(release.summary, `${release.version} 没有摘要`);
  });
});
