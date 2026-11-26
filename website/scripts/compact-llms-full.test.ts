import assert from 'node:assert/strict';
import { it } from 'node:test';
import { compactTables } from './compact-llms-full.ts';

it('压缩表格填充不改变单元格、转义竖线与代码块', () => {
  const source = '| 属性     | 类型                |\n| a       | `"a" \\| "b"`       |\n```txt\n| keep   | padding   |\n```';
  assert.equal(compactTables(source), '| 属性 | 类型 |\n| a | `"a" \\| "b"` |\n```txt\n| keep   | padding   |\n```');
});
