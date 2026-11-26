// 版本矩阵（docs-site.md §2b.4，ADR-021 D21.2 第 3 条）：从仓库里的钉版文件产出 docs/start/versions.md，不手改。
// 来源：仓库根的 CHANGELOG.md（Kun 最新的发布版）与 global.json（.NET SDK）；仓库根 src/ 下的可打包平台项目与版本、
// package.json（Node、pnpm）、pnpm-workspace.yaml 版本目录里的前端框架包（@cavailab/qian-*）、deploy/baseline.json（K3s）。
import fs from 'node:fs';
import path from 'node:path';
import { kunTier } from './build-kun-api.ts';
import { readKunVersion } from './kun.ts';
import { REPO_ROOT, REPOSITORY_ROOT, repoFileDate, yamlString, type GeneratedFile } from './repo.ts';

export const VERSIONS_PAGE = 'start/versions.md';

export interface PackageVersion {
  name: string;
  /** 没有版本号（还没发布）时为 null。 */
  version: string | null;
}

export interface VersionMatrix {
  /** 文档版本：多版本开启前只有 main（开发版）。 */
  docVersion: string;
  kun: string;
  platform: PackageVersion[];
  /** 前端框架（@cavailab/qian-*）。 */
  frontend: PackageVersion[];
  dotnetSdk: string;
  dotnetRollForward: string;
  node: string;
  pnpm: string;
  /** 部署基线里的 K3s 版本；没有基线文件时为 null。 */
  k3s: string | null;
  /** 每一项的钉版文件：kun、platform 与 dotnet 相对仓库的顶层，其余相对本目录（REPO_ROOT）。 */
  sources: Record<string, string>;
}

const PACKAGE_VERSION_RE = /<(?:Version|VersionPrefix)>\s*([^<\s]+)\s*<\/(?:Version|VersionPrefix)>/;

/** 从仓库根 src 下读取平台档包，项目中显式声明的版本优先。 */
export function readPlatformPackages(repositoryRoot: string = REPOSITORY_ROOT): PackageVersion[] {
  const dir = path.join(repositoryRoot, 'src');
  const sharedVersion = PACKAGE_VERSION_RE.exec(fs.readFileSync(path.join(dir, 'Directory.Build.props'), 'utf8'))?.[1] ?? null;
  const packages: PackageVersion[] = [];
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    if (!entry.isDirectory()) continue;
    const projectFile = path.join(dir, entry.name, `${entry.name}.csproj`);
    if (!fs.existsSync(projectFile)) continue;
    const project = fs.readFileSync(projectFile, 'utf8');
    if (kunTier(project) !== 'platform' || !/<IsPackable>\s*true\s*<\/IsPackable>/i.test(project)) continue;
    packages.push({ name: entry.name, version: PACKAGE_VERSION_RE.exec(project)?.[1] ?? sharedVersion });
  }
  if (packages.length === 0) throw new Error('仓库根 src 下缺少可打包的平台档项目，无法生成版本矩阵');
  return packages.sort((a, b) => a.name.localeCompare(b.name));
}

/**
 * 前端框架的包：pnpm-workspace.yaml 版本目录（catalog）里的 @cavailab/qian-*，写精确版本（S1-YUE-025）。
 * 包的源码在 CavaiLAB/qian，本仓库按钉住的版本消费。
 */
export const FRONTEND_PACKAGE_PREFIX = '@cavailab/qian-';
export function readFrontendPackages(repoRoot: string = REPO_ROOT): PackageVersion[] {
  const workspace = path.join(repoRoot, 'pnpm-workspace.yaml');
  if (!fs.existsSync(workspace)) return [];
  const catalog = /^catalog:[ \t]*\n((?:[ \t]+.*\n|[ \t]*\n)*)/m.exec(fs.readFileSync(workspace, 'utf8'))?.[1] ?? '';
  const entries = [...catalog.matchAll(/^[ \t]+["']?(@cavailab\/qian-[a-z0-9-]+)["']?[ \t]*:[ \t]*["']?([^"'\s#]+)["']?/gm)];
  return entries.map((m) => ({ name: m[1] ?? '', version: m[2] ?? null })).sort((a, b) => a.name.localeCompare(b.name));
}

export function readVersionMatrix(repoRoot: string = REPO_ROOT, repositoryRoot: string = REPOSITORY_ROOT): VersionMatrix {
  const globalJson = JSON.parse(fs.readFileSync(path.join(repositoryRoot, 'global.json'), 'utf8')) as { sdk?: { version?: string; rollForward?: string } };
  const rootPkg = JSON.parse(fs.readFileSync(path.join(repoRoot, 'package.json'), 'utf8')) as { packageManager?: string; engines?: { node?: string } };
  const baselineFile = path.join(repoRoot, 'deploy', 'baseline.json');
  const baseline = fs.existsSync(baselineFile) ? (JSON.parse(fs.readFileSync(baselineFile, 'utf8')) as { k3s?: string }) : {};
  const sdk = globalJson.sdk?.version ?? '';
  if (!sdk) throw new Error('global.json 里没有 sdk.version，版本矩阵缺 .NET SDK');
  return {
    docVersion: 'main',
    kun: readKunVersion(repositoryRoot),
    platform: readPlatformPackages(repositoryRoot),
    frontend: readFrontendPackages(repoRoot),
    dotnetSdk: sdk,
    dotnetRollForward: globalJson.sdk?.rollForward ?? '',
    node: rootPkg.engines?.node ?? '',
    pnpm: (rootPkg.packageManager ?? '').replace(/^pnpm@/, ''),
    k3s: baseline.k3s ?? null,
    sources: {
      kun: 'CHANGELOG.md',
      platform: 'src/*/*.csproj',
      frontend: 'pnpm-workspace.yaml',
      dotnet: 'global.json',
      node: 'package.json',
      k3s: 'deploy/baseline.json',
    },
  };
}

const cell = (text: string) => text.replace(/\|/g, '\\|');

function summarize(packages: PackageVersion[], unreleased: string, none: string): string {
  if (packages.length === 0) return none;
  const versions = [...new Set(packages.map((p) => p.version).filter((v): v is string => v !== null))];
  if (versions.length === 0) return `${unreleased}（${packages.length} 个包）`;
  return versions.length === 1 ? `${versions[0]}（${packages.length} 个包）` : packages.map((p) => `${p.name} ${p.version ?? '未发布'}`).join('、');
}

/** 版本矩阵页：一张矩阵表 + 来源表 + 包清单。multi-version 开启后每个文档版本一行。 */
export function renderVersionsPage(m: VersionMatrix, updated: string): string {
  const platformSummary = summarize(m.platform, '未发布，开发版 main', '尚无可打包的平台项目');
  const frontendSummary = summarize(m.frontend, '未写版本', '版本目录里没有');
  const packageList = (packages: PackageVersion[]) => packages.map((p) => `- \`${p.name}\`${p.version ? ` ${p.version}` : '（未发布）'}`).join('\n');
  return [
    '---',
    `title: ${yamlString('版本矩阵')}`,
    `description: ${yamlString('当前文档版本对应的 Kun、平台包、@cavailab/qian-*、.NET SDK、Node 与 K3s 基线，由仓库钉版文件生成。')}`,
    'part: start',
    'project: platform',
    'audience: [developer, operator]',
    `updated: ${updated}`,
    '---',
    '',
    '# 版本矩阵',
    '',
    '> 本页由 `website/scripts/build-versions.ts` 从仓库钉版文件生成（[ADR-021](/design/adr/adr-021-product-family-structure) D21.2 第 3 条），不要手改；改版本去改下表列出的源文件。',
    '',
    '平台包清单列出仓库根 `src/` 下平台档项目的开发版本。使用方安装合并版时，以相应 GitHub Release 附带的 `release-packages.json` 为准。Kun 一列取仓库根 `CHANGELOG.md` 中最新的正式版本。[Kun 框架](/sdk/kun/)小节与 [Kun API 参考](/sdk/api/kun/)都按它镜像。',
    '',
    'Kun 的同日修复版 `-hotfix.N` 是正式修复：最新的发布版按 `CHANGELOG.md` 里的先后算，不按 NuGet 的预发布排序。',
    '',
    '| 文档版本 | Kun | 平台包（源码开发版本） | 前端版本目录 | .NET SDK | Node / pnpm | K3s |',
    '|---|---|---|---|---|---|---|',
    `| ${m.docVersion}（开发版） | ${cell(m.kun)} | ${cell(platformSummary)} | ${cell(frontendSummary)} | ${cell(m.dotnetSdk)}${m.dotnetRollForward ? `（rollForward ${cell(m.dotnetRollForward)}）` : ''} | ${cell(m.node || '未钉版')} / ${cell(m.pnpm || '未钉版')} | ${m.k3s ? cell(m.k3s) : '未钉版'} |`,
    '',
    '本站当前展示 `main` 文档。Qian 组件参考从前端仓库对应的发布标签生成。',
    '',
    '## 来源',
    '',
    '| 项 | 钉版文件 | 说明 |',
    '|---|---|---|',
    `| Kun | 仓库根的 \`${m.sources.kun}\` | 最新的发布段 \`## [版本] - 日期\`，CalVer；发布工作流核对包源并建立 \`v<版本>\` 标签，随后合入版本日志 PR |`,
    `| 平台包 | \`${m.sources.platform}\` | 平台档可打包项目。版本优先取项目文件，默认值来自 \`src/Directory.Build.props\` |`,
    `| @cavailab/qian-* | \`${m.sources.frontend}\` | 版本目录（catalog）里的前端框架包，写精确版本；源码在 CavaiLAB/qian |`,
    `| .NET SDK | 仓库根的 \`${m.sources.dotnet}\` | \`sdk.version\` 与 \`rollForward\` |`,
    `| Node / pnpm | \`${m.sources.node}\` | \`engines.node\` 与 \`packageManager\` |`,
    `| K3s | \`${m.sources.k3s}\` | 平台部署采用的 Kubernetes 基线版本 |`,
    '',
    '## 平台包清单',
    '',
    packageList(m.platform),
    '',
    '## 前端版本目录中的框架包',
    '',
    'Kun 旧应用外壳按 Qian 0.2.1 归档，业务预发保留原有部署和数据。前端框架 Qian 的仓库已归档，它的组件页不在本站，组件文档在哪里见[说明](/components/qian/)。\n\nQian 的 8 个层包统一定版本。TypeScript 运行时 `@cavailab/qian-runtime` 与前端插件开发包 `@cavailab/qian-plugin-sdk` 各自定版本。见 [Qian 的版本策略](https://github.com/CavaiLAB/qian#现状)。',
    '',
    '下列项目来自根版本目录。`@cavailab/qian-runtime` 的版本由 CLI 与声明式表面校验库的 `package.json` 单独钉住，见[仓库的前端依赖说明](https://github.com/CavaiLAB/kun#前端依赖与包源)。',
    '',
    m.frontend.length > 0 ? packageList(m.frontend) : '- （版本目录里没有）',
    '',
  ].join('\n');
}

/** 生成 docs/start/versions.md；updated 取各钉版文件中最新的提交日期。 */
export function buildVersions(repoRoot: string = REPO_ROOT, repositoryRoot: string = REPOSITORY_ROOT): GeneratedFile {
  const matrix = readVersionMatrix(repoRoot, repositoryRoot);
  const sources: [string, string][] = [
    ...['CHANGELOG.md', 'global.json', 'src/Directory.Build.props', ...matrix.platform.map((p) => `src/${p.name}/${p.name}.csproj`)].map((rel): [string, string] => [repositoryRoot, rel]),
    ...['package.json', 'pnpm-workspace.yaml', 'deploy/baseline.json'].map((rel): [string, string] => [repoRoot, rel]),
  ];
  const updated = sources
    .filter(([root, rel]) => fs.existsSync(path.join(root, rel)))
    .map(([root, rel]) => repoFileDate(rel, root))
    .sort()
    .at(-1) ?? repoFileDate('global.json', repositoryRoot);
  return { path: VERSIONS_PAGE, content: renderVersionsPage(matrix, updated) };
}
