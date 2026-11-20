// 前端框架的组件文档源（S1-YUE-025）：组件库已迁到 CavaiLAB/qian，本站的组件页、演示与目录从它的源码生成。
// 版本只钉一处：pnpm-workspace.yaml 版本目录里 @cavailab/qian-core 的精确版本；检出 CavaiLAB/qian 的同名标签
// @cavailab/qian-core@<版本>，在检出里按 qian 自己的锁文件装依赖（API 表要 TypeScript checker 读源码，默认值取源码解构，
// 已发布的包只有 dist）。这是组件页来源定下来之前的过渡做法（F5）。
// CI 把标签检出到仓库根的 .qian、装好依赖，经 QIAN_DOCS_PATH 传进来；本机可把 QIAN_DOCS_PATH 指向自己的检出
// （停在该标签、装好依赖），都没有时把该标签克隆到 website/.cache/qian-repo（完整历史、不取文件内容的部分克隆）并装依赖。
// 生成器、演示的样式扫描、类型检查与浏览器验收都经固定的相对链接 website/.cache/qian-source 读检出。
import { execFileSync } from 'node:child_process';
import fs from 'node:fs';
import path from 'node:path';
import { REPO_ROOT, WEBSITE_ROOT } from './repo.ts';

export const QIAN_OWNER_REPO = 'CavaiLAB/qian';
export const QIAN_REPO_URL = `https://github.com/${QIAN_OWNER_REPO}`;
/** 8 个层包：目录 packages/qian/<名>，包名 @cavailab/qian-<名>，同一版本。 */
export const QIAN_LAYER_PACKAGES = ['tokens', 'contracts', 'sdk', 'core', 'form', 'data', 'shell', 'biz'] as const;
export type QianLayerPackage = (typeof QIAN_LAYER_PACKAGES)[number];
/** 检出的固定入口（相对链接，CI 的浏览器验收在容器里挂载仓库根目录时也能解析）。 */
export const QIAN_SOURCE_LINK = path.join(WEBSITE_ROOT, '.cache', 'qian-source');
const CACHE_CLONE = path.join(WEBSITE_ROOT, '.cache', 'qian-repo');
const VERSION_RE = /^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$/;

/** 版本目录里 @cavailab/qian-core 的版本：必须是精确版本，不写范围。 */
export function readQianVersion(repoRoot: string = REPO_ROOT): string {
  const text = fs.readFileSync(path.join(repoRoot, 'pnpm-workspace.yaml'), 'utf8');
  const version = /^\s*["']?@cavailab\/qian-core["']?\s*:\s*["']?([^"'\s#]+)["']?\s*(?:#.*)?$/m.exec(text)?.[1] ?? '';
  if (!VERSION_RE.test(version)) {
    throw new Error(`pnpm-workspace.yaml 的版本目录里 @cavailab/qian-core 应写精确版本，现为「${version}」：组件文档按它检出 CavaiLAB/qian 的同名标签`);
  }
  return version;
}

export const qianTag = (version: string): string => `@cavailab/qian-core@${version}`;

export interface QianCheckout {
  /** 检出目录（实际路径）。 */
  dir: string;
  version: string;
  /** @cavailab/qian-core@<version> */
  tag: string;
  /** 检出所在的提交。 */
  commit: string;
  /** 该提交的日期 YYYY-MM-DD。 */
  date: string;
}

function git(dir: string, args: string[]): string {
  return execFileSync('git', ['-C', dir, ...args], { encoding: 'utf8', maxBuffer: 64 * 1024 * 1024, stdio: ['ignore', 'pipe', 'pipe'] });
}

function isGitRepo(dir: string): boolean {
  try {
    git(dir, ['rev-parse', '--git-dir']);
    return true;
  } catch {
    return false;
  }
}

const readJson = <T>(file: string): T => JSON.parse(fs.readFileSync(file, 'utf8')) as T;
export const qianPackageDir = (dir: string, name: QianLayerPackage): string => path.join(dir, 'packages', 'qian', name);
const dependenciesInstalled = (dir: string): boolean => fs.existsSync(path.join(qianPackageDir(dir, 'core'), 'node_modules', 'react', 'package.json'));
/** 只装 8 个层包的依赖，按检出里 qian 自己的锁文件（公共 npm 源上的包，不需要令牌）。 */
export const QIAN_INSTALL_ARGS = ['install', '--frozen-lockfile', '--filter', './packages/qian/*'] as const;

/**
 * 没给 QIAN_DOCS_PATH 时的兜底：克隆该标签到缓存目录并装依赖；缓存停在别的版本就重新克隆。
 * 要完整历史（不取文件内容的部分克隆，同 kun.ts）：页面的 updated 取文件在该标签上的最后提交日期，浅克隆只剩标签那一个提交。
 */
function cachedClone(version: string): string {
  const tag = qianTag(version);
  const current = fs.existsSync(path.join(qianPackageDir(CACHE_CLONE, 'core'), 'package.json'))
    ? readJson<{ version?: string }>(path.join(qianPackageDir(CACHE_CLONE, 'core'), 'package.json')).version
    : undefined;
  if (!isGitRepo(CACHE_CLONE) || current !== version) {
    fs.rmSync(CACHE_CLONE, { recursive: true, force: true });
    fs.mkdirSync(path.dirname(CACHE_CLONE), { recursive: true });
    try {
      execFileSync('git', ['clone', '--quiet', '--filter=blob:none', '--branch', tag, `${QIAN_REPO_URL}.git`, CACHE_CLONE], { stdio: ['ignore', 'pipe', 'pipe'] });
    } catch (error) {
      throw new Error(`没有设置 QIAN_DOCS_PATH，自动克隆 ${QIAN_REPO_URL} 的标签 ${tag} 也失败了；把 QIAN_DOCS_PATH 指向本机的检出（website/README.md）：${(error as Error).message}`);
    }
  }
  if (!dependenciesInstalled(CACHE_CLONE)) execFileSync('pnpm', [...QIAN_INSTALL_ARGS], { cwd: CACHE_CLONE, stdio: 'inherit' });
  return CACHE_CLONE;
}

/** 让 website/.cache/qian-source 以相对路径指向检出；检出就是缓存克隆时也照样建链接，读的一方只认这一个入口。 */
function linkSource(dir: string): void {
  const target = path.relative(path.dirname(QIAN_SOURCE_LINK), dir) || '.';
  fs.mkdirSync(path.dirname(QIAN_SOURCE_LINK), { recursive: true });
  try {
    if (fs.readlinkSync(QIAN_SOURCE_LINK) === target) return;
  } catch {
    // 还没有链接，或是同名的普通文件或目录
  }
  fs.rmSync(QIAN_SOURCE_LINK, { recursive: true, force: true });
  fs.symlinkSync(target, QIAN_SOURCE_LINK, 'dir');
}

/** 核对检出：8 个层包的名字与版本对得上版本目录、依赖已装；不对就失败，不回退别的版本。 */
export function verifyQianCheckout(dir: string, version: string): void {
  for (const name of QIAN_LAYER_PACKAGES) {
    const file = path.join(qianPackageDir(dir, name), 'package.json');
    if (!fs.existsSync(file)) throw new Error(`${dir} 不是 CavaiLAB/qian 的检出：缺 packages/qian/${name}/package.json`);
    const pkg = readJson<{ name?: string; version?: string }>(file);
    if (pkg.name !== `@cavailab/qian-${name}` || pkg.version !== version) {
      throw new Error(`检出里的 ${pkg.name ?? name}@${pkg.version ?? '?'} 与版本目录钉的 @cavailab/qian-${name}@${version} 不一致：检出标签 ${qianTag(version)}`);
    }
  }
  if (!dependenciesInstalled(dir)) {
    throw new Error(`检出 ${dir} 里还没装依赖：在检出目录运行 pnpm ${QIAN_INSTALL_ARGS.join(' ')}`);
  }
}

let opened: QianCheckout | undefined;

/** 打开检出（每个进程一次）：定位、核对，再把固定入口指过去。 */
export function openQian(env: NodeJS.ProcessEnv = process.env, repoRoot: string = REPO_ROOT): QianCheckout {
  if (opened) return opened;
  const version = readQianVersion(repoRoot);
  const dir = fs.realpathSync(env.QIAN_DOCS_PATH ? path.resolve(env.QIAN_DOCS_PATH) : cachedClone(version));
  verifyQianCheckout(dir, version);
  if (!isGitRepo(dir)) throw new Error(`QIAN_DOCS_PATH=${dir} 不是 git 检出：页面的 source 要写检出所在的提交`);
  const commit = git(dir, ['rev-parse', 'HEAD']).trim();
  let tagged = '';
  try {
    tagged = git(dir, ['rev-parse', '--verify', '--quiet', `refs/tags/${qianTag(version)}^{commit}`]).trim();
  } catch {
    // 浅检出可能不带标签引用；上面已按包版本核对过
  }
  if (tagged && tagged !== commit) throw new Error(`检出 ${dir} 不在标签 ${qianTag(version)} 上（HEAD ${commit.slice(0, 12)}，标签 ${tagged.slice(0, 12)}）`);
  const date = git(dir, ['log', '-1', '--format=%cs', commit]).trim();
  linkSource(dir);
  opened = { dir, version, tag: qianTag(version), commit, date };
  return opened;
}

/** 检出里文件的最后提交日期（浅检出时就是检出提交的日期）。 */
export function qianFileDate(qian: QianCheckout, file: string): string {
  return git(qian.dir, ['log', '-1', '--format=%cs', qian.commit, '--', file]).trim() || qian.date;
}

/** frontmatter 的 source：<owner>/<repo>@<提交>:<路径>（标签名里有 @，ref 写提交）。 */
export function qianSource(qian: QianCheckout, file: string): string {
  return `${QIAN_OWNER_REPO}@${qian.commit}:${file}`;
}

/**
 * 演示构建的别名：@cavailab/qian-* 的每个导出入口都指到检出里的源码（按各包 package.json 的 exports），
 * 本站的演示入口、检出里的示例与包之间互相引用都落到同一份源码。
 */
export function qianSourceAliases(dir: string): Record<string, string> {
  const aliases: Record<string, string> = {};
  for (const name of QIAN_LAYER_PACKAGES) {
    const base = qianPackageDir(dir, name);
    const exports = readJson<{ exports?: Record<string, string> }>(path.join(base, 'package.json')).exports ?? {};
    for (const [key, target] of Object.entries(exports)) {
      if (typeof target !== 'string' || target.startsWith('./dist/')) continue;
      aliases[`@cavailab/qian-${name}${key === '.' ? '' : key.slice(1)}$`] = path.join(base, target);
    }
  }
  return aliases;
}
