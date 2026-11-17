// 运行时一致性用例的 TypeScript 一侧：对包源上已发布的 @cavailab/qian-runtime 跑共用场景（.NET 一侧是 Qian.Runtime.Tests）。
// 运行时的源码在 CavaiLAB/qian；那边的一致性用例作业也读本仓库的 dist/profiles.js 与 tck/runtime 下的两个适配器，路径不要动。
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {parse} from 'yaml';
import * as runtime from '@cavailab/qian-runtime';
import {loadProfile} from '../dist/profiles.js';
import {runtimeScenarios} from '../../../tck/runtime/harness-ts.mjs';
import {assemblyScenarios} from '../../../tck/runtime/assembly-ts.mjs';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'../../..');
runtimeScenarios(runtime,parse,loadProfile,root);
assemblyScenarios(runtime.PluginLoader,loadProfile,root);
