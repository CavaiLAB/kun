import { createInstance } from '@module-federation/enhanced/runtime';
import * as React from 'react';
import { adaptFederation, type SharedDependencies } from '@cavailab/qian-plugin-sdk/federation';
import { SDK_VERSION } from '@cavailab/qian-plugin-sdk';

/** 宿主显式使用 MF2 runtime；延迟共享包工厂避免启动时加载完整 UI 命名空间。 */
export function createHostFederation() {
  const share = (version: string, load: () => Promise<unknown>) => ({ version, get: async () => { const module = await load(); return () => module; }, shareConfig: { singleton: true, requiredVersion: version, strictVersion: true } });
  const shared: SharedDependencies = {
    react: { version: React.version, lib: () => React, shareConfig: { singleton: true, requiredVersion: React.version, strictVersion: true } },
    'react-dom': share(React.version, () => import('react-dom')),
    'react-dom/client': share(React.version, () => import('react-dom/client')),
    'react/jsx-runtime': share(React.version, () => import('react/jsx-runtime')),
    'react/jsx-dev-runtime': share(React.version, () => import('react/jsx-dev-runtime')),
    '@cavailab/qian-core': share(__QIAN_VERSION__, () => import('@cavailab/qian-core')),
    '@cavailab/qian-tokens': share(__QIAN_VERSION__, () => import('@cavailab/qian-tokens')),
    '@cavailab/qian-contracts': share(__QIAN_VERSION__, () => import('@cavailab/qian-contracts')),
    '@cavailab/qian-sdk': share(__QIAN_VERSION__, () => import('@cavailab/qian-sdk')),
    '@cavailab/qian-form': share(__QIAN_VERSION__, () => import('@cavailab/qian-form')),
    '@cavailab/qian-data': share(__QIAN_VERSION__, () => import('@cavailab/qian-data')),
    '@cavailab/qian-shell': share(__QIAN_VERSION__, () => import('@cavailab/qian-shell')),
    '@cavailab/qian-biz': share(__QIAN_VERSION__, () => import('@cavailab/qian-biz')),
    '@cavailab/qian-plugin-sdk': share(SDK_VERSION, () => import('@cavailab/qian-plugin-sdk')),
  };
  const instance = createInstance({ name: 'qian_shell', shared, remotes: [] });
  return { transport: adaptFederation(instance), instance };
}
