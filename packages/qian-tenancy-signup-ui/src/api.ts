import { HttpResponseError, PlatformClient, PlatformError, type ErrorDetail, type RequestTransport } from '@cavailab/qian-sdk';
import type { Client } from 'openapi-fetch';
import type { paths } from './api.generated';
import { i18n } from './i18n';

// 返回类型显式写出：远程模块要生成类型声明（@mf-types），openapi-fetch 是 @cavailab/qian-sdk 的依赖，推断的类型从本包引不到（TS2883）。
export function apiOf(client: PlatformClient): Client<paths> { return client.api<paths>(); }
export type Api = ReturnType<typeof apiOf>;

/** 申请人公开页的客户端：同源、只靠宿主写的 HttpOnly 会话 Cookie，不持有也不发送任何令牌。 */
export function publicClient(transport?: RequestTransport): PlatformClient {
  return new PlatformClient({ baseUrl: location.origin, transport, middleware: [async (request, next) => { request.headers.set('Accept-Language', i18n.instance.language); return next(request); }] });
}
export function data<T>(result: { data?: T }): T { if (result.data === undefined) throw new Error('signup.contract.response'); return result.data; }
export interface Issue { status?: number; code?: string; message: string }
export function issue(error: unknown): Issue {
  if (error instanceof PlatformError) return { status: error.status, code: error.detail.code, message: error.message };
  return { status: error instanceof HttpResponseError ? error.status : undefined, message: i18n.t('signup.offline') };
}
/** 未登录（申请人会话不存在或已过期）按「没有会话」处理，其余错误照常抛出。 */
export function unauthenticated(error: unknown): boolean {
  return (error instanceof PlatformError || error instanceof HttpResponseError) && error.status === 401;
}
/** 页面本地校验（如两次密码不一致）按 ErrorDetail 交给表单，错误落在对应字段。 */
export function fieldError(pointer: string, message: string): ErrorDetail {
  // 只给表单定位字段用（SchemaForm 读 errors / pointer / message），不是服务端响应，所以没有 trace_id。
  return { type: 'about:blank', title: message, status: 400, code: 'INPUT_INVALID', message, errors: [{ pointer, message }] } as unknown as ErrorDetail;
}
const pad = (value: number) => String(value).padStart(2, '0');
export function day(value: string): string { const date = new Date(value); return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`; }
export function moment(value: string): string { const date = new Date(value); return `${day(value)} ${pad(date.getHours())}:${pad(date.getMinutes())}`; }
