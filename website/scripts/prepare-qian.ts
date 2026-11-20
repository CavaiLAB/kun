// 准备 CavaiLAB/qian 的检出（见 qian.ts）：定位、核对版本与依赖、建 website/.cache/qian-source 链接。
// 生成器与演示构建自己也会调用；类型检查之前单跑它，让 tsc 与编辑器能解析到检出里的组件源码。
import { openQian } from './qian.ts';

const qian = openQian();
console.log(`[qian-docs] CavaiLAB/qian ${qian.tag}（${qian.commit.slice(0, 12)}）：${qian.dir}`);
