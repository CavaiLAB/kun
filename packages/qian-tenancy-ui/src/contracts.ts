import contract from './http-contract.generated.json';
import type { FormSchema, UiNode } from '@cavailab/qian-contracts';
import type { paths } from './api.generated';

export type Tenant = NonNullable<paths['/api/tenancy/tenants']['get']['responses'][200]['content']['application/json']['items']>[number];
export type TenantCounts = NonNullable<paths['/api/tenancy/tenants']['get']['responses'][200]['content']['application/json']['counts']>;
export type StateMachine = paths['/api/tenancy/meta/state-machine']['get']['responses'][200]['content']['application/json'];
export type Quota = paths['/api/tenancy/tenants/{id}/quota']['get']['responses'][200]['content']['application/json'];
export type CreateRequest = paths['/api/tenancy/tenants']['post']['requestBody']['content']['application/json'];
export type UpdateRequest = paths['/api/tenancy/tenants/{id}']['put']['requestBody']['content']['application/json'];
export type ActivateRequest = paths['/api/tenancy/tenants/{id}:activate']['post']['requestBody']['content']['application/json'];
export type AdminAccount = paths['/api/tenancy/admin-accounts']['post']['responses'][201]['content']['application/json'];
export type TenantAdmin = paths['/api/tenancy/tenants/{id}/admins']['get']['responses'][200]['content']['application/json']['items'][number];
export type LookupRequest = paths['/api/tenancy/admin-accounts:lookup']['post']['requestBody']['content']['application/json'];
export type AccountRequest = paths['/api/tenancy/admin-accounts']['post']['requestBody']['content']['application/json'];
export type ResetRequest = paths['/api/tenancy/tenants/{id}/admins/{membershipId}:reset-password']['post']['requestBody']['content']['application/json'];
export type ReplaceRequest = paths['/api/tenancy/tenants/{id}/admins']['put']['requestBody']['content']['application/json'];
type ObjectNode = Record<string, unknown>;
function object(value: unknown): ObjectNode {
  if (!value || typeof value !== 'object' || Array.isArray(value)) throw new Error('tenant.contract.invalid');
  return value as ObjectNode;
}
function dereference(value: unknown, depth = 0): unknown {
  if (depth > 40) throw new Error('tenant.contract.depth');
  if (Array.isArray(value)) return value.map((item) => dereference(item, depth + 1));
  if (!value || typeof value !== 'object') return value;
  const node = object(value);
  if (typeof node.$ref === 'string') {
    if (!node.$ref.startsWith('#/')) throw new Error('tenant.contract.reference');
    let source: unknown = contract;
    for (const key of node.$ref.slice(2).split('/')) source = object(source)[key.replaceAll('~1', '/').replaceAll('~0', '~')];
    const { $ref: _ref, ...siblings } = node;
    return dereference({ ...object(source), ...siblings }, depth + 1);
  }
  return Object.fromEntries(Object.entries(node).map(([key, item]) => [key, dereference(item, depth + 1)]));
}
export function requestForm(path: keyof paths, method: 'post' | 'put', id: string, layout: UiNode[]): FormSchema {
  const operation = object(object(object(contract.paths)[path])[method]);
  const schema = object(object(object(operation.requestBody).content)['application/json']).schema;
  const data = object(dereference(schema));
  if (data.type !== 'object' || data.additionalProperties !== false) throw new Error('tenant.contract.request');
  return { $schema: 'https://schema.qian.dev/form/v1', version: '1.0.0', id, data: data as FormSchema['data'], ui: { layout }, rules: {}, binding: {}, auth: {} };
}
const quota: UiNode = { field: 'quotas', children: [{ field: 'users', labelKey: 'tenant.quota', descriptionKey: 'tenant.quotaHint' }] };
// 开户管理员由页面经身份组件查找或新建后填入（S1-TEN-010），表单只收主档与配额；其余字段仍按契约校验。
function omit(form: FormSchema, field: string): FormSchema {
  const { [field]: _omitted, ...properties } = form.data.properties;
  const required = Array.isArray(form.data.required) ? form.data.required.filter((name) => name !== field) : form.data.required;
  return { ...form, data: { ...form.data, properties, required } };
}
const createForm = requestForm('/api/tenancy/tenants', 'post', 'qian.tenancy.create', [
  { field: 'code', labelKey: 'tenant.code', descriptionKey: 'tenant.codeHint' }, { field: 'name', labelKey: 'tenant.name' }, quota,
]);
export const createSchema = omit(createForm, 'administrator');
export const editSchema = requestForm('/api/tenancy/tenants/{id}', 'put', 'qian.tenancy.edit', [{ field: 'name', labelKey: 'tenant.name' }, quota]);
export const activateSchema = requestForm('/api/tenancy/tenants/{id}:activate', 'post', 'qian.tenancy.activate', [{ field: 'trial', labelKey: 'tenant.trial' }, { field: 'validUntil', labelKey: 'tenant.validUntil', descriptionKey: 'tenant.validUntilHint' }]);
export const detailSchema: FormSchema = { ...createSchema, id: 'qian.tenancy.detail', ui: { layout: [{ field: 'code', labelKey: 'tenant.code' }, { field: 'name', labelKey: 'tenant.name' }] } };
const secret = { type: 'password', autoComplete: 'new-password' };
export const lookupSchema = requestForm('/api/tenancy/admin-accounts:lookup', 'post', 'qian.tenancy.admin.lookup', [{ field: 'identifier', labelKey: 'tenant.adminIdentifier', descriptionKey: 'tenant.adminIdentifierHint' }]);
export const accountSchema = requestForm('/api/tenancy/admin-accounts', 'post', 'qian.tenancy.admin.account', [
  { field: 'displayName', labelKey: 'tenant.adminDisplayName' }, { field: 'identifierKind', labelKey: 'tenant.adminIdentifierKind' },
  { field: 'identifier', labelKey: 'tenant.adminIdentifier' }, { field: 'initialPassword', labelKey: 'tenant.adminInitialPassword', descriptionKey: 'tenant.adminInitialPasswordHint', props: secret },
]);
export const resetSchema = requestForm('/api/tenancy/tenants/{id}/admins/{membershipId}:reset-password', 'post', 'qian.tenancy.admin.reset', [
  { field: 'newPassword', labelKey: 'tenant.adminNewPassword', descriptionKey: 'tenant.adminNewPasswordHint', props: secret },
]);

// 管理员的初始密码（R163）：账号没有经本人验证的手机号或邮箱时，开户与更换管理员都要给它设初始密码，账号的密码随即重置、下次登录强制修改。
// 长度约束取契约；契约以 null 表示「不给」，表单里出现这一栏就必填，类型收紧为字符串。
const initialPasswordNode = (field: string): UiNode => ({ field, labelKey: 'tenant.adminInitialPassword', descriptionKey: 'tenant.adminInitialPasswordHint', props: secret });
function initialPassword(source: unknown): ObjectNode { return { ...object(source), type: 'string' }; }
// 开户：administrator 只留 initialPassword（成员指针由页面填入），表单位置与服务端错误的位置 #/administrator/initialPassword 一致；排在主档前面，紧挨管理员。
const administrator = object(createForm.data.properties.administrator);
export const createWithPasswordSchema: FormSchema = {
  ...createForm,
  data: { ...createForm.data, properties: { ...createForm.data.properties, administrator: { ...administrator, required: ['initialPassword'], properties: { initialPassword: initialPassword(object(administrator.properties).initialPassword) } } } },
  ui: { layout: [{ field: 'administrator', children: [initialPasswordNode('initialPassword')] }, ...createForm.ui.layout] },
};
// 更换管理员：选定账号后单独一步只收初始密码，提交时放进 items[0]；服务端错误的位置 #/items/0/initialPassword 由页面换成本表单的 #/initialPassword。
const replaceForm = requestForm('/api/tenancy/tenants/{id}/admins', 'put', 'qian.tenancy.admin.designate', []);
const designation = object(object(object(replaceForm.data.properties).items).items);
export const designateSchema: FormSchema = {
  ...replaceForm,
  data: { type: 'object', additionalProperties: false, required: ['initialPassword'], properties: { initialPassword: initialPassword(object(designation.properties).initialPassword) } },
  ui: { layout: [initialPasswordNode('initialPassword')] },
};
// 与服务端同一口径：至少有一个手机号或邮箱、且全部经本人验证的账号，指定为管理员时不需要初始密码。
export function needsInitialPassword(account: Pick<AdminAccount, 'identifiers'>): boolean {
  const contacts = account.identifiers.filter((item) => item.kind === 'phone' || item.kind === 'email');
  return contacts.length === 0 || contacts.some((item) => !item.verified);
}

export type TenantAction = 'activate' | 'suspend' | 'resume';
export function legalActions(machine: StateMachine, status: Tenant['status']): TenantAction[] {
  const supported = new Set(['activate', 'suspend', 'resume']);
  return machine.transitions.filter((transition) => {
    const from = Array.isArray(transition.from) ? transition.from : [transition.from];
    const to = Array.isArray(transition.to) ? transition.to : [transition.to];
    return supported.has(transition.action) && (from.includes(status) || from.includes('any-s1')) && !to.every((next) => next === status)
      && machine.actions.some((action) => action.name === transition.action && action.available !== false);
  }).map((transition) => transition.action as TenantAction);
}
