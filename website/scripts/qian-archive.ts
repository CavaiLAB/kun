// 前端框架 Qian 的组件页从本站拿掉之后留下的东西（S1-DOCS-011；产品负责人 2026-11-26 定，S1-DEC-047 D2）：
// 说明页指向的归档仓库、标签与提交，以及原来的地址清单。
// 原来 /components/qian/ 下有 91 页，构建时从归档仓库的标签检出源码生成。现在 /components/qian/ 只留一页说明
// （collect-components.ts 生成）；其余 90 页与 /components/yue/ 下的旧书签都落到这一页：component-redirects.ts 在构建末尾写跳转页，
// check-site.ts 逐个核对。清单取自拿掉之前最后一次构建的产物；前端框架已归档、不再发版，清单不会再变。

/** 组件页原来的来源：已归档的仓库、标签与标签所在的提交（标签名里有 @ 与 /，链接一律写提交）。 */
export const QIAN_ARCHIVE = {
  repo: 'CavaiLAB/qian',
  url: 'https://github.com/CavaiLAB/qian',
  tag: '@cavailab/qian-core@0.2.1',
  commit: 'e4becd2e1c3c2ff471a6bfce671f86b0ef0df786',
} as const;

/** 归档仓库里某个路径在该提交上的地址；目录用 tree，文件用 blob。 */
export function qianArchiveUrl(file: string, kind: 'tree' | 'blob' = 'tree'): string {
  return `${QIAN_ARCHIVE.url}/${kind}/${QIAN_ARCHIVE.commit}/${file}`;
}

/** 说明页的路由（相对站点根，不带扩展名）。 */
export const NOTICE_PAGE = 'components/qian/index';

/** 基础组件页（52 个入口）与通用上下文 API 页：/components/qian/core/<名>。 */
const CORE_PAGES = [
  'accordion', 'alert', 'alertdialog', 'avatar', 'backtop', 'badge', 'breadcrumb', 'button',
  'buttongroup', 'card', 'checkbox', 'combobox', 'contextmenu', 'descriptions', 'dialog', 'drawer',
  'dropdownmenu', 'empty', 'field', 'fullscreen', 'hovercard', 'icon', 'iconbutton', 'input',
  'inputnumber', 'inputpassword', 'label', 'menu', 'notification', 'pagination', 'pininput', 'popover',
  'progress', 'radio', 'radiogroup', 'resizable', 'result', 'scrollarea', 'segmented', 'select',
  'separator', 'shared-api', 'sheet', 'skeleton', 'spinner', 'steps', 'switch', 'tabs',
  'tag', 'textarea', 'toast', 'toggle', 'tooltip',
] as const;

/** 继承的 DOM / ARIA / 事件属性页：/components/qian/api/native-<编号>，编号是属性声明的哈希。 */
const NATIVE_PAGES = [
  '0046cbe90146', '065cda6a77e9', '0ab372837fc1', '0fa39ce9a3fd', '11e3e0a70cc9', '16dbe9cf1ef0',
  '1e3540c2279a', '28007fc0a502', '41309d54b431', '514bf678cd10', '52e94e9c5561', '681ba4f0bef3',
  '6ebce1f6516c', '71dace0561de', '7b2e359a5d6a', '8055a350dc83', '850dbb022e07', '85495ff78641',
  '89e0d722a8fa', '8b1c75c1ad21', '9210e42aed88', '956e3a1abdbb', 'b8ae826ff3d0', 'ca945eeb63c4',
  'd798bbc8c701', 'e9bfd0277f59', 'eb5ff1f5b58d', 'ecb207c60294', 'ee81d4127916', 'f312f5257ffa',
  'f7c58fed18dd', 'f8fa5f397fbe',
] as const;

/** 原来 /components/qian/ 下除首页以外的 90 页（相对站点根，不带扩展名）：设计令牌、四个包级页、基础组件与属性页。 */
export const REMOVED_PAGES: readonly string[] = [
  ...['tokens', 'form', 'data', 'shell', 'biz'].map((name) => `components/qian/${name}`),
  ...CORE_PAGES.map((name) => `components/qian/core/${name}`),
  ...NATIVE_PAGES.map((id) => `components/qian/api/native-${id}`),
];

/** /components/yue/ 是更早的旧地址（前端框架改名前），原来逐页跳到 /components/qian/ 的同名页；91 个都落到说明页。 */
export const LEGACY_YUE_PAGES: readonly string[] = [NOTICE_PAGE, ...REMOVED_PAGES].map((route) => route.replace('components/qian/', 'components/yue/'));
