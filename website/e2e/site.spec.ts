// 站点级的浏览器验收：在真实静态产物上跑。前端框架的组件页、演示与编辑器 2026-11-26 起不在本站（S1-DOCS-011），
// 原来那批组件交互用例随之拿掉；这里核对旧地址落到说明页、说明页的无障碍、主题与快捷键、离线、规格的旧书签。
import AxeBuilder from '@axe-core/playwright';
import { expect, test } from '@playwright/test';
import { settleVisuals } from '../../apps/qian-shell/e2e/settle';
import { LEGACY_YUE_PAGES, QIAN_ARCHIVE, REMOVED_PAGES } from '../scripts/qian-archive.ts';

const NOTICE = /\/components\/qian\/index\.html$/;
const seriousViolations = async (page: import('@playwright/test').Page) => {
  await settleVisuals(page);
  const violations = (await new AxeBuilder({ page }).analyze()).violations.filter((v) => ['critical', 'serious'].includes(v.impact ?? ''));
  return violations.map((v) => `${v.id}: ${v.nodes.map((n) => n.target).join(', ')}`);
};

test('拿掉的组件页与 /components/yue/ 的旧书签都落到说明页', async ({ page }) => {
  const samples = [
    'components/qian/core/button.html',
    'components/qian/tokens.html',
    'components/qian/shell.html',
    `${REMOVED_PAGES.find((r) => r.includes('/api/native-'))}.html`,
    'components/yue/core/button.html?from=bookmark#api',
    `${LEGACY_YUE_PAGES[0]}.html`,
  ];
  for (const old of samples) {
    await page.goto(old);
    await expect(page, old).toHaveURL(NOTICE);
    // 标题前有 Rspress 的锚点「#」，按包含比
    await expect(page.getByRole('heading', { level: 1 })).toContainText('前端框架 Qian 的组件页已从本站拿掉');
  }
  await expect(page.locator(`a[href="${QIAN_ARCHIVE.url}"]`).first()).toHaveText(QIAN_ARCHIVE.repo);
  await expect(page.locator(`a[href^="${QIAN_ARCHIVE.url}/tree/${QIAN_ARCHIVE.commit}/"]`).first()).toBeVisible();
  await expect(page.locator('a[href*="/components/qian/core/"]')).toHaveCount(0);
});

test('旧的 Markdown 地址指向说明页的 Markdown，说明页写明归档标签', async ({ request }) => {
  for (const [old, target] of [['components/qian/core/button.md', '../index.md'], ['components/yue/form.md', '../qian/index.md']] as const) {
    const response = await request.get(old);
    expect(response.ok(), old).toBe(true);
    expect(await response.text()).toContain(`](${target})`);
  }
  const notice = await (await request.get('components/qian/index.md')).text();
  expect(notice).toContain(QIAN_ARCHIVE.tag);
  expect(notice).not.toContain('<ComponentDemo');
});

test('组件目录只列平台插件，说明页不进侧栏', async ({ page }) => {
  await page.goto('components/index.html');
  await expect(page.getByRole('heading', { level: 1 })).toContainText('组件目录');
  await expect(page.locator('main a[href$="components/qian/index.html"]')).toHaveCount(1);
  await expect(page.locator('a[href*="components/qian/core/"]')).toHaveCount(0);
  await expect(page.getByRole('link', { name: '平台插件' }).first()).toBeVisible();
});

test('说明页在明暗两态下没有严重的无障碍问题', async ({ page }) => {
  for (const theme of ['light', 'dark'] as const) {
    await page.emulateMedia({ colorScheme: theme });
    await page.goto('components/qian/index.html');
    if (theme === 'dark') await expect(page.locator('html')).toHaveClass(/dark/);
    else await expect(page.locator('html')).not.toHaveClass(/dark/);
    expect(await seriousViolations(page)).toEqual([]);
  }
});

test('首页与说明页断外网也能完整打开，不请求外部资源', async ({ page, baseURL }) => {
  const origin = new URL(baseURL!).origin;
  const external: string[] = [];
  await page.route('**/*', (route) => {
    if (new URL(route.request().url()).origin !== origin) { external.push(route.request().url()); return route.abort(); }
    return route.continue();
  });
  const errors: string[] = [];
  page.on('pageerror', (e) => errors.push(e.message));
  for (const path of ['index.html', 'components/qian/index.html']) {
    await page.goto(path);
    await expect(page.getByRole('heading', { level: 1 }).first()).toBeVisible();
  }
  expect(external).toEqual([]);
  expect(errors).toEqual([]);
});

test('站点主题支持键盘切换，刷新后保持', async ({ page }) => {
  await page.emulateMedia({ colorScheme: 'light' });
  await page.goto('start/index.html');
  const toggle = page.getByRole('button', { name: '切换主题', exact: true });
  await expect(toggle.locator('svg:visible')).toHaveCount(1);
  await toggle.press('Enter');
  await expect(toggle.locator('svg:visible')).toHaveCount(1);
  await expect(page.locator('html')).toHaveClass(/dark/);
  await page.reload();
  await expect(page.locator('html')).toHaveClass(/dark/);
});

test('桌面快捷键文字在明暗主题均有足够对比度', async ({ page }) => {
  await page.addInitScript(() => { Object.defineProperty(navigator, 'platform', { get: () => 'Linux x86_64' }); });
  for (const theme of ['light', 'dark'] as const) {
    await page.emulateMedia({ colorScheme: theme });
    await page.goto('start/index.html');
    await expect(page.locator('.rp-search-button__hotkey')).toBeVisible();
    await expect(page.locator('.rp-search-button__hotkey')).toContainText('Ctrl');
    await settleVisuals(page);
    const violations = (await new AxeBuilder({ page }).include('.rp-search-button').analyze()).violations;
    expect(violations).toEqual([]);
  }
});

test('前端框架规格的旧书签跳到同一页并保留定位', async ({ page }) => {
  const section = '0-定位与阅读方式';
  await page.goto(`design/prd/specs/yue.html#${encodeURIComponent(section)}`);
  await expect(page).toHaveURL((url) =>
    url.pathname.endsWith('/design/prd/specs/frontend.html') && decodeURIComponent(url.hash.slice(1)) === section);
  await expect(page.locator(`h2[id="${section}"]`)).toBeInViewport();
});
