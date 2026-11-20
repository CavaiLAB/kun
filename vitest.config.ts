import { defineConfig } from 'vitest/config';

// 外壳与界面包的测试入口（pnpm ui:test）：
// - ui-gates：门禁自测（依赖边界、循环依赖、文案规则、令牌、外壳产物校验、钉住的副本），Node 环境；
// - qian-shell-kit：外壳的 Qian 专有接线（运行时配置、BFF 会话、bootstrap、请求中间件、认证页族适配），Node 环境、fetch 由用例注入；
// - packages/qian-identity-ui/vitest.config.ts、packages/qian-tenancy-signup-ui/vitest.config.ts：各包的浏览器测试（Vitest browser mode + Playwright）。
// 前端框架（@cavailab/qian-*）自己的测试在 CavaiLAB/qian 里跑。
export default defineConfig({
  test: {
    projects: [
      { test: { name: 'ui-gates', environment: 'node', include: ['tests/gates/ui/**/*.test.ts'], testTimeout: 120_000, hookTimeout: 120_000 } },
      { test: { name: 'qian-shell-kit', environment: 'node', include: ['packages/qian-shell-kit/src/**/*.test.ts'] } },
      'packages/qian-identity-ui/vitest.config.ts',
      'packages/qian-tenancy-signup-ui/vitest.config.ts',
    ],
  },
});
