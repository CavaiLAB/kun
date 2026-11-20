import {test} from 'node:test';
import assert from 'node:assert/strict';
import {mkdtempSync,mkdirSync,writeFileSync,readFileSync,existsSync,rmSync} from 'node:fs';
import path from 'node:path';
import os from 'node:os';
import {fileURLToPath} from 'node:url';
import {spawnSync} from 'node:child_process';
import {validateComponent} from '@qian/surfaces';
import {renderDocumentationTemplates} from '../dist/documentation-templates.js';
const repo=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'../../..');
function run(root,...args){return spawnSync(process.execPath,[path.join(repo,'packages/qian-cli/dist/main.js'),'--cwd',root,'--json',...args],{encoding:'utf8',timeout:120000});}
function write(root,file,content){mkdirSync(path.dirname(path.join(root,file)),{recursive:true});writeFileSync(path.join(root,file),content);}
const source=`using Kun.Runtime; namespace Demo; public static class Events { [KunEvent(Mode=DispatchMode.Emit)] public static readonly KunEvent<string,string?> Changed=KunEvent.Define<string,string?>("demo/changed",DispatchMode.Emit); public static void Bind(IPluginContext ctx) { ctx.On(Changed,DispatchMode.Emit,(Action<string>)(_=>{})); } }`;
test('文档模板有两份必需节，真实 init 消费模板且不覆盖已有文件',()=>{
 const root=mkdtempSync(path.join(os.tmpdir(),'qian-init-docs-'));
 try{
  const expected=renderDocumentationTemplates('test.guide','使用指引');
  for(const body of Object.values(expected)){assert.ok(body.includes('## 模型体验'));assert.ok(body.includes('## 已知限制与延期项'));}
  const init=run(root,'init','--template','skill-only','--id','test.guide','--name','使用指引');assert.equal(init.status,0,init.stderr);
  for(const[name,body]of Object.entries(expected))assert.equal(readFileSync(path.join(root,name),'utf8'),body);
  const component=validateComponent(root,{profile:'minimal'});assert.ok(component.isValid,JSON.stringify(component.diagnostics));
  const retry=run(root,'init','--template','skill-only','--id','test.guide');assert.equal(retry.status,1);assert.equal(readFileSync(path.join(root,'README.md'),'utf8'),expected['README.md']);
 }finally{rmSync(root,{recursive:true,force:true});}
});
test('docs generate 生成四份文件；模式错误返回映射且保留上次有效文档',()=>{
 const root=mkdtempSync(path.join(os.tmpdir(),'qian-docs-cli-'));
 try{
  write(root,'profiles/minimal/qian.profile.yml','name: minimal\nbundles: []\n');
  write(root,'src/Events.cs',source);
  const first=run(root,'docs','generate','--profile','minimal');assert.equal(first.status,0,first.stderr);const result=JSON.parse(first.stdout);assert.equal(result.files.length,4);assert.equal(result.events[0].consumers.length,1);
  const originals=Object.fromEntries(result.files.map(file=>[file,readFileSync(path.join(root,file),'utf8')]));
  write(root,'src/Events.cs',source.replace('ctx.On(Changed,DispatchMode.Emit','ctx.On(Changed,DispatchMode.Waterfall'));
  const invalid=run(root,'docs','generate','--profile','minimal');assert.equal(invalid.status,1);assert.equal(invalid.stdout,'');const error=JSON.parse(invalid.stderr);assert.ok(error.diagnostics.some(d=>d.error.errors[0].code==='DOC_EVENT_MODE_MISMATCH'));assert.equal(error.events[0].consumers.length,1);
  for(const[file,body]of Object.entries(originals))assert.equal(readFileSync(path.join(root,file),'utf8'),body);
  write(root,'src/Events.cs',source);
  write(root,'handwritten/config-catalog.md','人工说明\n');
  const refused=run(root,'docs','generate','--profile','minimal','--out','handwritten');assert.equal(refused.status,1);assert.equal(readFileSync(path.join(root,'handwritten/config-catalog.md'),'utf8'),'人工说明\n');assert.ok(!existsSync(path.join(root,'handwritten/event-graph.md')));
 }finally{rmSync(root,{recursive:true,force:true});}
});
test('docs generate --source-root 取工作区的上级目录：位置相对源码根，源码目录越出源码根时失败且不写文件',()=>{
 const base=mkdtempSync(path.join(os.tmpdir(),'qian-docs-root-')),root=path.join(base,'app');
 try{
  write(root,'profiles/minimal/qian.profile.yml','name: minimal\nbundles: []\n');
  write(base,'lib/Events.cs',source);
  const first=run(root,'docs','generate','--profile','minimal','--source-root','..','--source','lib');assert.equal(first.status,0,first.stderr);
  const result=JSON.parse(first.stdout);assert.equal(result.events[0].declaration.file,'lib/Events.cs');assert.equal(result.events[0].consumers[0].file,'lib/Events.cs');
  assert.ok(readFileSync(path.join(root,'docs/generated/event-graph.md'),'utf8').includes('` lib/Events.cs `'));
  const escaped=run(root,'docs','generate','--profile','minimal','--source-root','..','--source','../outside','--out','escaped');assert.equal(escaped.status,1);
  assert.ok(JSON.parse(escaped.stderr).diagnostics.some(d=>d.error.errors[0].code==='DOC_SOURCE_INVALID'));assert.ok(!existsSync(path.join(root,'escaped')));
 }finally{rmSync(base,{recursive:true,force:true});}
});
