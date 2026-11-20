import { readFileSync } from 'node:fs';
import { describe, expect, it } from 'vitest';

// 钉住的副本：读它们的检查不装 Node 依赖（.NET 测试所在的 CI 分片），所以本仓库留一份已发布包里的文件。
// 副本必须与安装目录里同版本的包逐字节相同；pnpm-workspace.yaml 的版本目录升级时，照安装目录里的新文件一起更新副本。
// 包的 exports 不导出这些文件，按外壳的直接依赖链接读。
const root = new URL('../../../', import.meta.url);
const PINS = [
  { copy: '../../tests/Kun.Identity.Server.Tests/Contracts/biz.openapi.json', installed: 'apps/qian-shell/node_modules/@cavailab/qian-biz/contracts/biz.openapi.json' },
];

describe('钉住的副本与安装的包逐字节相同', () => {
  for (const pin of PINS) {
    it(pin.copy, () => {
      expect(readFileSync(new URL(pin.copy, root)).equals(readFileSync(new URL(pin.installed, root))), `${pin.copy} 与 ${pin.installed} 不同`).toBe(true);
    });
  }
});
