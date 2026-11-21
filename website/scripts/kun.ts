// Kun 小节的公共部分（docs-site.md §2b，ADR-021 D21.4）：读 Kun 最新的发布版、打开带 Kun 版本标签的 git 仓库、在标签上读文件。
// 版本取仓库根 CHANGELOG.md 里最新的发布段：发布工作流先创建 v<版本> 标签，
// 随后合入版本日志 PR。标签在本仓库里，所以默认就读本仓库：本地开发与 CI 都不用另外克隆；
// 本仓库缺这个标签时（如浅克隆）从 origin 只取这一个标签与它的历史。也可以用 KUN_DOCS_PATH 指向另一个有该标签的克隆。
// 指南与 API 读取标签上的 Git 对象。发布历史以已提交的正式版本段为准。
import { execFileSync } from 'node:child_process';
import fs from 'node:fs';
import path from 'node:path';
import { OWNER_REPO, REPO_ROOT, REPO_URL, REPOSITORY_ROOT, WEBSITE_ROOT } from './repo.ts';

/** Kun 的源码与版本标签所在的仓库，就是本仓库。 */
export const KUN_OWNER_REPO = OWNER_REPO;
export const KUN_REPO_URL = REPO_URL;
/** 生成过程的缓存（API 参考），不进 Git。 */
export const CACHE_DIR = path.join(WEBSITE_ROOT, '.cache');

/** CHANGELOG.md 的版本段标题：## [2026.10.25] - 2026-10-25；第一个不是 [Unreleased] 的就是最新的发布版。 */
const RELEASE_HEADING_RE = /^## \[([^\]\n]*)\]/gm;

/** Kun 最新的发布版：仓库根 CHANGELOG.md 里第一个已发布的版本段。没有即失败，不回退 main（docs-site.md §2b.1）。 */
export function readKunVersion(repositoryRoot: string = REPOSITORY_ROOT): string {
  const file = path.join(repositoryRoot, 'CHANGELOG.md');
  const text = fs.existsSync(file) ? fs.readFileSync(file, 'utf8') : '';
  const version = [...text.matchAll(RELEASE_HEADING_RE)].map((m) => (m[1] ?? '').trim()).find((v) => v.toLowerCase() !== 'unreleased') ?? '';
  if (version === '') {
    throw new Error(`${file} 里没有已发布的版本段（## [版本] - 日期）：Kun 文档按最新的发布版镜像，没有时构建失败，不回退 main（docs-site.md §2b.1）`);
  }
  if (!/^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$/.test(version)) {
    throw new Error(`${file} 里最新的发布段写成了「${version}」，应为 Kun 的 CalVer 版本号，如 2026.9.13`);
  }
  return version;
}

export interface KunCheckout {
  /** 本地 git 仓库目录。 */
  dir: string;
  version: string;
  /** v<版本> */
  tag: string;
  /** tag 指向的提交。 */
  commit: string;
  /** 该提交的日期 YYYY-MM-DD。 */
  date: string;
  /** 正文经过同仓整理时指向编辑提交；发行版本与标签仍按原包记录。 */
  editorialRef?: string;
  /** 标签创建后另行合入的正式版本日志提交，仅用于发布历史页。 */
  changelogRef?: string;
}

function git(dir: string, args: string[]): string {
  return execFileSync('git', ['-C', dir, ...args], {
    encoding: 'utf8',
    maxBuffer: 256 * 1024 * 1024,
    stdio: ['ignore', 'pipe', 'pipe'],
  });
}

function tagCommit(dir: string, tag: string): string | null {
  try {
    return git(dir, ['rev-parse', '--verify', '--quiet', `refs/tags/${tag}^{commit}`]).trim() || null;
  } catch {
    return null;
  }
}

function isGitRepo(dir: string): boolean {
  try {
    git(dir, ['rev-parse', '--git-dir']);
    return true;
  } catch {
    return false;
  }
}

/** git 命令要在仓库顶层跑：在子目录里，ls-tree 只列该子目录，log 的路径也按子目录算。裸仓库没有顶层，原样用。 */
function topLevel(dir: string): string {
  try {
    return git(dir, ['rev-parse', '--show-toplevel']).trim() || dir;
  } catch {
    return dir;
  }
}

/** 本仓库缺标签时从 origin 只取这一个标签。不带 --depth：浅克隆里取到的也是该标签的完整历史。 */
function fetchTag(dir: string, tag: string): void {
  try {
    git(dir, ['fetch', '--quiet', '--no-tags', 'origin', `+refs/tags/${tag}:refs/tags/${tag}`]);
  } catch (error) {
    throw new Error(
      `本仓库没有 tag ${tag}，从 origin 取也失败了。先手动取：git fetch origin tag ${tag}，` +
        `或把 KUN_DOCS_PATH 指向有这个 tag 的克隆（website/README.md §1）：${(error as Error).message}`,
    );
  }
}

/** 浅克隆里该提交的历史可能被截断：镜像页的 updated 取文件在 tag 上的最后提交日期，历史不全会算错，所以直接失败。 */
function assertFullHistory(dir: string, commit: string, tag: string): void {
  const shallowFile = path.resolve(dir, git(dir, ['rev-parse', '--git-path', 'shallow']).trim());
  if (!fs.existsSync(shallowFile)) return;
  for (const boundary of fs.readFileSync(shallowFile, 'utf8').split('\n').filter(Boolean)) {
    let truncated = true;
    try {
      git(dir, ['merge-base', '--is-ancestor', boundary, commit]);
    } catch {
      truncated = false;
    }
    if (truncated) {
      throw new Error(`${dir} 是浅克隆，${tag} 的历史不全（镜像页的 updated 会算错）；先取完整历史：git -C ${dir} fetch --unshallow`);
    }
  }
}

/**
 * 打开 Kun 的版本标签 v<version> 所在的仓库并确认标签存在；不存在即失败。
 * 默认是 repoRoot 所在的 git 仓库（本仓库），缺标签时从 origin 取；KUN_DOCS_PATH 指定的克隆不去取，缺标签直接失败。
 */
export function openKun(version: string, env: NodeJS.ProcessEnv = process.env, repoRoot: string = REPO_ROOT): KunCheckout {
  const tag = `v${version}`;
  const fromEnv = env.KUN_DOCS_PATH;
  const given = path.resolve(fromEnv || repoRoot);
  if (!isGitRepo(given)) {
    throw new Error(fromEnv ? `KUN_DOCS_PATH=${given} 不是 git 仓库` : `${given} 不在 git 仓库里：Kun 小节要读本仓库的 tag ${tag}`);
  }
  const dir = topLevel(given);
  let commit = tagCommit(dir, tag);
  if (commit === null && !fromEnv) {
    fetchTag(dir, tag);
    commit = tagCommit(dir, tag);
  }
  if (commit === null) {
    const hint = fromEnv ? `；本机克隆可先 git -C ${dir} fetch origin tag ${tag}` : '';
    throw new Error(`${fromEnv ? 'KUN_DOCS_PATH 指向的克隆' : '本仓库'}里没有 tag ${tag}（仓库根 CHANGELOG.md 最新的发布版 = ${version}）${hint}`);
  }
  assertFullHistory(dir, commit, tag);
  const date = git(dir, ['log', '-1', '--format=%cs', commit]).trim();
  return { dir, version, tag, commit, date };
}

/** 首次合并前的最后一个正式标签；其文档的同仓整理版经 Kun #17 审核。 */
export const LAST_SEPARATE_RELEASE = 'abad3ccfd8d7dfb670eee416efb76ee7dc75498c';
const REVIEWED_GUIDES = '97503ce39963fa7f34d95e332e093d6564ebe4b6';

export function guideEdition(release: KunCheckout): KunCheckout {
  if (release.commit !== LAST_SEPARATE_RELEASE) return release;
  try {
    git(release.dir, ['cat-file', '-e', REVIEWED_GUIDES + '^{commit}']);
  } catch {
    git(release.dir, ['fetch', '--quiet', '--no-tags', 'origin', REVIEWED_GUIDES]);
  }
  assertFullHistory(release.dir, REVIEWED_GUIDES, REVIEWED_GUIDES);
  return { ...release, commit: REVIEWED_GUIDES, editorialRef: REVIEWED_GUIDES };
}

/** 发布工作流先打标签，再合入正式版本段。其他指南仍从标签或原审核版本读取。 */
export function releaseNotes(kun: KunCheckout, repoRoot: string): { source: KunCheckout; markdown: string } {
  const file = 'CHANGELOG.md';
  const heading = (text: string) => [...text.matchAll(RELEASE_HEADING_RE)].find((m) => m[1] === kun.version);
  let source = kun;
  let markdown = readKunFile(kun, file);
  let start = heading(markdown);
  if (!start && !kun.editorialRef) {
    const dir = topLevel(repoRoot);
    // 浅克隆会把早已有的行归给边界提交，来源地址和日期都会错。
    assertFullHistory(dir, 'HEAD', 'HEAD');
    const current = git(dir, ['show', 'HEAD:' + file]);
    const currentHeading = heading(current);
    if (!currentHeading) throw new Error(`站点提交的 ${file} 缺少 ${kun.version} 的正式发布段`);
    const line = current.slice(0, currentHeading.index).split('\n').length;
    const commit = git(dir, ['blame', '--porcelain', '-L', `${line},${line}`, 'HEAD', '--', file]).split(' ')[0]!;
    source = { ...kun, dir, commit, changelogRef: commit };
    markdown = readKunFile(source, file);
    start = heading(markdown);
  }
  if (!start) throw new Error(`变更记录缺少 ${kun.version} 的正式发布段`);
  return { source, markdown: '# Kun 版本记录\n\n' + markdown.slice(start.index) };
}

/** tag 上的全部文件路径（/ 分隔）。 */
export function listKunFiles(kun: KunCheckout): string[] {
  return git(kun.dir, ['ls-tree', '-r', '--name-only', kun.commit]).split('\n').filter(Boolean);
}

export function readKunFile(kun: KunCheckout, file: string): string {
  return git(kun.dir, ['show', `${kun.commit}:${file}`]);
}

/** 文件在该来源版本上的最后提交日期（frontmatter 的 updated，docs-site.md §2b.3）。 */
export function kunFileDate(kun: KunCheckout, file: string): string {
  return git(kun.dir, ['log', '-1', '--format=%cs', kun.commit, '--', file]).trim() || kun.date;
}

/** 标明实际来源版本的文件或目录地址。 */
export function kunGithubUrl(kun: KunCheckout, file: string, isDir = false): string {
  const rel = file.replace(/\/$/, '');
  return `${KUN_REPO_URL}/${isDir ? 'tree' : 'blob'}/${kun.changelogRef ?? kun.editorialRef ?? kun.tag}${rel === '' || rel === '.' ? '' : `/${rel}`}`;
}

/** frontmatter 的 source：<owner>/<repo>@<来源版本>:<path>（docs-site.md §4）。 */
export function kunSource(kun: KunCheckout, file: string): string {
  return `${KUN_OWNER_REPO}@${kun.changelogRef ?? kun.editorialRef ?? kun.tag}:${file}`;
}
