// 单测用的样例 kun 仓库：临时目录里 git init，按给定日期逐次提交，打 tag，再接着提交 tag 之后的。只用 Node 内置模块与本机 git。
import { execFileSync } from 'node:child_process';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { pathToFileURL } from 'node:url';

export interface FixtureCommit {
  /** 提交日期 YYYY-MM-DD，对应镜像页 frontmatter 的 updated。 */
  date: string;
  files: Record<string, string>;
}

/** later 是打 tag 之后的提交：主干在 tag 之后继续往前走，浅克隆只拿到最新提交时 tag 的历史不在本地。 */
export function makeKunRepo(commits: FixtureCommit[], tag: string | null = 'v2026.9.13', later: FixtureCommit[] = []): string {
  const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'qian-kun-fixture-'));
  const git = (args: string[], env: Record<string, string> = {}) =>
    execFileSync('git', ['-C', dir, ...args], { stdio: 'pipe', env: { ...process.env, ...env } });
  git(['init', '-q']);
  const commit = ({ date, files }: FixtureCommit) => {
    for (const [file, content] of Object.entries(files)) {
      const target = path.join(dir, file);
      fs.mkdirSync(path.dirname(target), { recursive: true });
      fs.writeFileSync(target, content);
    }
    git(['add', '-A']);
    const stamp = `${date}T12:00:00+00:00`;
    git(
      ['-c', 'user.name=fixture', '-c', 'user.email=fixture@example.com', '-c', 'commit.gpgsign=false', 'commit', '-q', '--no-verify', '-m', date],
      { GIT_AUTHOR_DATE: stamp, GIT_COMMITTER_DATE: stamp },
    );
  };
  commits.forEach(commit);
  if (tag) git(['tag', tag]);
  later.forEach(commit);
  return dir;
}

/** 经 file:// 克隆样例仓库（--depth 等浅克隆选项只在 file:// 下生效），返回克隆目录。 */
export function cloneRepo(source: string, args: string[] = []): string {
  const dir = path.join(fs.mkdtempSync(path.join(os.tmpdir(), 'qian-kun-clone-')), 'repo');
  execFileSync('git', ['clone', '-q', ...args, pathToFileURL(source).href, dir], { stdio: 'pipe' });
  return dir;
}

/** 写一个只含 <KunVersion> 的 Directory.Packages.props，返回其所在目录（当作仓库根）。 */
export function makeRepoRoot(propsBody: string): string {
  const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'qian-root-fixture-'));
  fs.writeFileSync(path.join(dir, 'Directory.Packages.props'), `<Project>\n${propsBody}\n</Project>\n`);
  return dir;
}
