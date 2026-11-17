import {test} from 'node:test';
import assert from 'node:assert/strict';
import {mkdtempSync,mkdirSync,writeFileSync,readFileSync,rmSync,realpathSync,existsSync} from 'node:fs';
import path from 'node:path';
import os from 'node:os';
import {createHash,generateKeyPairSync,sign} from 'node:crypto';
import {createServer} from 'node:http';
import {once} from 'node:events';
import {execFile} from 'node:child_process';
import {promisify} from 'node:util';
import {stringify} from 'yaml';
import {npmExecutable,inspectPackageWorkspace} from '../dist/package-manager.js';
import {installMarketplacePlugins} from '../dist/marketplace-install.js';
import {removeMarketplacePlugins} from '../dist/marketplace-remove.js';
import {planEngineVerification,readHostEngineVersions,verifyComponentEngines} from '../dist/engine-compatibility.js';
import {parsePackageReference} from '../dist/package-references.js';
const execute=promisify(execFile);
const write=(root,name,bytes)=>{const file=path.join(root,name);mkdirSync(path.dirname(file),{recursive:true});writeFileSync(file,bytes);};
const ref=(kind,name)=>parsePackageReference(kind,name);
const incompatible=error=>error.diagnostics?.some(item=>item.error.errors.some(detail=>detail.code==='SURFACE_ENGINE_INCOMPATIBLE'));
// engines.qian（前端框架版本）只约束声明了界面表面的组件：声明它的样例组件一并带一份界面清单。
const withUi=(directory,engines)=>{if(!engines?.qian)return{};write(directory,'ui/manifest.json',JSON.stringify({$schema:'https://qian.dev/schemas/ui/v2',federation:{manifest:'/remotes/test/mf-manifest.json',exposes:['Page']},engines:{qian:engines.qian},pages:[{route:'/test',expose:'Page',permission:'test.engine:item:read',nav:{title:'样例'}}]}));return{surfaces:{ui:'./ui/manifest.json'}};};

function fixture(){
 const root=mkdtempSync(path.join(realpathSync(os.tmpdir()),'qian-engine-plan-')),host=path.join(root,'host'),catalog=path.join(root,'catalog');
 const keys=generateKeyPairSync('ed25519'),pem=keys.publicKey.export({type:'spki',format:'pem'});
 write(host,'profiles/minimal/qian.profile.yml','name: minimal\nbundles: []\npatch: ./qian.patch.yml\n');write(host,'profiles/minimal/qian.patch.yml','[]\n');
 write(catalog,'key.pub',pem);
 const market={name:'catalog',interface:{displayName:'版本计划验收'},owner:{name:'Fixture',publicKeys:['./key.pub']},plugins:[{name:'engine',source:{type:'local',path:'./plugins/test.engine'},category:'operations',policy:{installation:'auto',authentication:'ON_INSTALL',profiles:['minimal']}}]};
 const persist=()=>{const bytes=Buffer.from(JSON.stringify(market));write(catalog,'.qian/marketplace.json',bytes);write(catalog,'.qian/marketplace.json.sig',sign(null,createHash('sha256').update(bytes).digest(),keys.privateKey));};persist();
 write(host,'.qian/config.yml',stringify({defaultProfile:'minimal',marketplaces:[{name:'catalog',source:catalog,priority:1}],trust:{keys:[pem]}}));
 const component=(version,packages,engines)=>write(catalog,'plugins/test.engine/.qian-plugin/plugin.json',JSON.stringify({id:'test.engine',version,description:'使用真实包还原图验证引擎版本预检与升级的组件。',author:{name:'Fixture'},license:'Apache-2.0',interface:{displayName:'引擎版本',category:'operations'},inject:[],provides:[],policy:{tier:'official',sandbox:'none',authentication:'ON_INSTALL'},scopes:['global'],engines,...withUi(path.join(catalog,'plugins/test.engine'),engines),runtime:{form:'module',entry:'Fixture.Entry',...packages}}));
 const surface=(name,engines,version='1.0.0')=>{
  write(catalog,'plugins/test.'+name+'/.qian-plugin/plugin.json',JSON.stringify({id:'test.'+name,version,description:'依赖宿主前端框架范围的无代码指引，用于验证受影响组件的末检范围。',author:{name:'Fixture'},license:'Apache-2.0',interface:{displayName:'无代码指引',category:'operations'},inject:[],provides:[],policy:{tier:'private',sandbox:'none',authentication:'ON_INSTALL'},scopes:['global'],engines,...withUi(path.join(catalog,'plugins/test.'+name),engines)}));
  write(catalog,'plugins/test.'+name+'/skills/check/SKILL.md','---\nname: check\ndescription: 当用户需要查看宿主配置及其版本信息时使用，不执行修改。\n---\n只读检查。\n');
  if(!market.plugins.some(item=>item.name===name))market.plugins.push({name,source:{type:'local',path:'./plugins/test.'+name},category:'operations',policy:{installation:'auto',authentication:'ON_INSTALL',profiles:['minimal']}});persist();
 };
 return{root,host,catalog,component,surface,install:(dryRun=false)=>installMarketplacePlugins(['engine'],{workingDirectory:host,profile:'minimal',includeHome:false,yes:true,dryRun}),dispose:()=>rmSync(root,{recursive:true,force:true})};
}
async function npmFeed(f){
 const name='@cavailab/qian-core',packages=new Map(),archives=new Map();
 for(const[pkg,version,dependencies]of [[name,'0.1.0',{}],[name,'0.2.0',{}],['qian-engine-helper','1.0.0',{[name]:'0.1.0'}],['qian-engine-helper','2.0.0',{[name]:'0.2.0'}]]){
  const folder=path.join(f.root,pkg.replaceAll('/','-')+'-'+version);write(folder,'package.json',JSON.stringify({name:pkg,version,dependencies,main:'index.js'}));write(folder,'index.js','module.exports='+JSON.stringify(version)+';');
  const output=await execute(process.execPath,[npmExecutable(),'pack','--ignore-scripts','--json','--pack-destination',folder],{cwd:folder,encoding:'utf8'});
  const bytes=readFileSync(path.join(folder,JSON.parse(output.stdout)[0].filename)),archive='/'+pkg.replaceAll('/','-')+'-'+version+'.tgz';archives.set(archive,bytes);
  if(!packages.has(pkg))packages.set(pkg,{});packages.get(pkg)[version]={name:pkg,version,dependencies,dist:{path:archive,integrity:'sha512-'+createHash('sha512').update(bytes).digest('base64')}};
 }
 const server=createServer((request,response)=>{
  const url=decodeURIComponent(request.url),bytes=archives.get(url);if(bytes){response.end(bytes);return;}
  const versions=packages.get(url.slice(1));if(!versions){response.writeHead(404).end();return;}
  const origin='http://127.0.0.1:'+server.address().port;
  response.setHeader('content-type','application/json');response.end(JSON.stringify({name:url.slice(1),'dist-tags':{latest:Object.keys(versions).at(-1)},versions:Object.fromEntries(Object.entries(versions).map(([version,value])=>[version,{...value,dist:{integrity:value.dist.integrity,tarball:origin+value.dist.path}}]))}));
 });server.listen(0,'127.0.0.1');await once(server,'listening');
 write(f.host,'package.json','{"private":true}\n');// 用户级配置可能把 @cavailab 作用域指到 GitHub Packages（CI 的 setup-node 就这样写），作用域也指到样例源。
 write(f.host,'.npmrc','registry=http://127.0.0.1:'+server.address().port+'/\n@cavailab:registry=http://127.0.0.1:'+server.address().port+'/\nfetch-retries=0\ncache='+path.join(f.root,'npm-cache')+'\n');
 return async()=>{server.closeAllConnections();await new Promise(resolve=>server.close(resolve));};
}

test('真实 npm 托管的前端框架从 0.1 升到 0.2，预检采用计划精确版本且仍拒绝不兼容的计划',async()=>{
 const f=fixture();let close;try{
  close=await npmFeed(f);const name='@cavailab/qian-core';f.component('1.0.0',{npm:[{name,version:'0.1.0'}]},{qian:'^0.1.0'});
  assert.equal((await f.install()).status,'installed');assert.equal(JSON.parse(readFileSync(path.join(f.host,'node_modules',name,'package.json'),'utf8')).version,'0.1.0');
  f.component('2.0.0',{npm:[{name,version:'0.2.0'}]},{qian:'^0.2.0'});const plan=await f.install(true);
  assert.equal(plan.enginePlan.evidence.find(item=>item.package===name).version,'0.2.0');assert.equal(plan.enginePlan.evidence.find(item=>item.package===name).source,'package-plan/npm');
  const upgraded=await f.install();assert.equal(upgraded.status,'installed');assert.equal(upgraded.verification[0].engines.find(item=>item.package===name).version,'0.2.0');
  assert.equal(JSON.parse(readFileSync(path.join(f.host,'node_modules',name,'package.json'),'utf8')).version,'0.2.0');
  const lock=readFileSync(path.join(f.host,'.qian/lock.json'),'utf8');f.component('3.0.0',{npm:[{name,version:'0.2.0'}]},{qian:'^0.3.0'});
  await assert.rejects(f.install(),incompatible);assert.equal(readFileSync(path.join(f.host,'.qian/lock.json'),'utf8'),lock);
 }finally{await close?.();f.dispose();}
});

test('传递的前端框架随真实 npm 升级重算，计划标 pending；末检不匹配时恢复旧依赖和锁',async()=>{
 const f=fixture();let close;try{
  close=await npmFeed(f);const name='qian-engine-helper';f.component('1.0.0',{npm:[{name,version:'1.0.0'}]},{qian:'^0.1.0'});
  assert.equal((await f.install()).status,'installed');
  f.component('2.0.0',{npm:[{name,version:'2.0.0'}]},{qian:'^0.2.0'});const plan=await f.install(true);assert.ok(plan.enginePlan.pending.includes('qian'));assert.ok(!plan.enginePlan.evidence.some(item=>item.engine==='qian'));
  const upgraded=await f.install();assert.equal(upgraded.status,'installed');assert.equal(upgraded.verification[0].engines.find(item=>item.engine==='qian').version,'0.2.0');
  const lock=readFileSync(path.join(f.host,'.qian/lock.json'),'utf8'),npmLock=readFileSync(path.join(f.host,'package-lock.json'),'utf8');
  f.component('3.0.0',{npm:[{name,version:'1.0.0'}]},{qian:'^0.3.0'});const invalidPlan=await f.install(true);assert.ok(invalidPlan.enginePlan.pending.includes('qian'));
  await assert.rejects(f.install(),incompatible);assert.equal(readFileSync(path.join(f.host,'.qian/lock.json'),'utf8'),lock);assert.equal(readFileSync(path.join(f.host,'package-lock.json'),'utf8'),npmLock);assert.equal(readHostEngineVersions(f.host).find(item=>item.engine==='qian').version,'0.2.0');
 }finally{await close?.();f.dispose();}
});

test('前端框架升级重新核验同宿主已安装的相关组件，不兼容时回滚且不重复检查无关组件',async()=>{
 const f=fixture();let close;try{
  close=await npmFeed(f);const name='@cavailab/qian-core',options={workingDirectory:f.host,profile:'minimal',includeHome:false,yes:true};
  f.component('1.0.0',{npm:[{name,version:'0.1.0'}]},{qian:'^0.1.0'});await f.install();
  f.surface('consumer',{qian:'^0.1.0'});f.surface('unrelated',{});await installMarketplacePlugins(['consumer','unrelated'],options);
  const lock=readFileSync(path.join(f.host,'.qian/lock.json'),'utf8'),patch=readFileSync(path.join(f.host,'profiles/minimal/qian.patch.yml'),'utf8'),consumer=readFileSync(path.join(f.host,'.qian/plugins/test.consumer/.qian-plugin/plugin.json'),'utf8');
  f.component('2.0.0',{npm:[{name,version:'0.2.0'}]},{qian:'^0.2.0'});
  await assert.rejects(f.install(),error=>incompatible(error)&&JSON.stringify(error.diagnostics).includes('test.consumer'));
  assert.equal(readFileSync(path.join(f.host,'.qian/lock.json'),'utf8'),lock);assert.equal(readFileSync(path.join(f.host,'profiles/minimal/qian.patch.yml'),'utf8'),patch);assert.equal(readFileSync(path.join(f.host,'.qian/plugins/test.consumer/.qian-plugin/plugin.json'),'utf8'),consumer);assert.equal(readHostEngineVersions(f.host).find(item=>item.engine==='qian').version,'0.1.0');
  f.surface('consumer',{qian:'>=0.1.0 <0.3.0'},'2.0.0');await installMarketplacePlugins(['consumer'],options);
  const installed=await f.install();assert.equal(installed.status,'installed');assert.deepEqual(installed.verification[0].components.sort(),['test.consumer','test.engine']);assert.ok(!installed.verification.some(result=>result.components.includes('test.unrelated')));
 }finally{await close?.();f.dispose();}
});

test('真实 NuGet 图中候选传递的 Kun.Core 2026.10.1 不覆盖宿主直引 2026.10.12，借用后卸载仍保留宿主版本',async()=>{
 const f=fixture();try{
  // 两版 Kun.Core 在样例源里现打，只用这个源（不连 nuget.org），版本证据只可能来自这里。
  const feed=path.join(f.root,'feed');mkdirSync(feed);const xml=value=>value.replaceAll('&','&amp;').replaceAll('"','&quot;').replaceAll('<','&lt;');
  const config='<configuration><packageSources><clear/><add key="fixture" value="'+xml(feed)+'"/></packageSources><config><add key="globalPackagesFolder" value="'+xml(path.join(f.root,'nuget-cache'))+'"/></config></configuration>';
  write(f.root,'NuGet.Config',config);
  const dotnet=async(directory,args)=>execute('dotnet',args,{cwd:directory,encoding:'utf8',timeout:120000,maxBuffer:8*1024*1024,env:{...process.env,DOTNET_NOLOGO:'1',MSBUILDDISABLENODEREUSE:'1',DOTNET_CLI_USE_MSBUILD_SERVER:'0'}});
  for(const version of ['2026.10.1','2026.10.12']){
   const folder=path.join(f.root,'kun-'+version);write(folder,'Kun.csproj','<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><PackageId>Kun.Core</PackageId><AssemblyName>Kun.Core</AssemblyName><Version>'+version+'</Version><NuGetAudit>false</NuGetAudit></PropertyGroup></Project>');write(folder,'Code.cs','namespace Kun.Core; public static class Marker { public const string Version = "'+version+'"; }');
   await dotnet(folder,['pack','Kun.csproj','-c','Release','-o',feed,'--disable-build-servers','-m:1']);
  }
  const provider='Qian.EngineFixture.Provider',folder=path.join(f.root,'provider');write(folder,'Provider.csproj','<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><PackageId>'+provider+'</PackageId><Version>1.0.0</Version><NuGetAudit>false</NuGetAudit></PropertyGroup><ItemGroup><PackageReference Include="Kun.Core" Version="[2026.10.1,)"/></ItemGroup></Project>');write(folder,'Code.cs','public static class Provider { public static string KunVersion => Kun.Core.Marker.Version; }');
  await dotnet(folder,['pack','Provider.csproj','-c','Release','-o',feed,'--disable-build-servers','-m:1']);
  assert.ok(JSON.parse(readFileSync(path.join(folder,'obj/project.assets.json'),'utf8')).libraries['Kun.Core/2026.10.1']);
  write(f.host,'Host.csproj','<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><NuGetAudit>false</NuGetAudit></PropertyGroup><ItemGroup><PackageReference Include="Kun.Core" Version="[2026.10.12]"/></ItemGroup></Project>');
  await dotnet(f.host,['restore','Host.csproj']);assert.equal(readHostEngineVersions(f.host).find(item=>item.package==='kun.core').version,'2026.10.12');
  f.component('1.0.0',{nuget:[{id:provider,version:'1.0.0'}]},{kun:'>=2026.10.12'});
  const planned=await f.install(true);assert.equal(planned.enginePlan.evidence.find(item=>item.package==='kun.core').version,'2026.10.12');assert.match(planned.enginePlan.evidence.find(item=>item.package==='kun.core').source,/project.assets.json/);
  const installed=await f.install();assert.equal(installed.status,'installed');assert.equal(installed.verification[0].engines.find(item=>item.package==='kun.core').version,'2026.10.12');
  const lock=readFileSync(path.join(f.host,'.qian/lock.json'),'utf8');f.component('2.0.0',{nuget:[{id:provider,version:'1.0.0'}]},{kun:'>=2026.11.1'});await assert.rejects(f.install(),incompatible);assert.equal(readFileSync(path.join(f.host,'.qian/lock.json'),'utf8'),lock);
  assert.equal((await removeMarketplacePlugins(['engine'],{workingDirectory:f.host,profile:'minimal',purge:true})).status,'removed');
  const workspace=await inspectPackageWorkspace(f.host,['nuget']);assert.equal(workspace.nuget.dependencies.find(item=>item.name==='Kun.Core').version,'2026.10.12');assert.equal(readHostEngineVersions(f.host,workspace).find(item=>item.package==='kun.core').version,'2026.10.12');
 }finally{f.dispose();}
});

test('只读或没有相关还原的 engines 保持严格，pending 不能豁免范围语法或已钉定版本',()=>{
 const old={engine:'qian',version:'0.1.0',package:'@cavailab/qian-core',source:'node_modules/@cavailab/qian-core/package.json'},surfaces={ui:'./ui/manifest.json'};
 assert.throws(()=>verifyComponentEngines({engines:{qian:'^0.2.0'},surfaces},planEngineVerification([old]).evidence),incompatible);
 const workspace={root:'/fixture',npm:{manifest:'/fixture/package.json',dependencies:[{name:old.package,version:'0.1.0',section:'dependencies'}]}};
 const plan=planEngineVerification([old],workspace,{add:[ref('npm','qian-engine-helper@1.0.0')],remove:[]});assert.equal(plan.evidence[0].version,'0.1.0');assert.ok(!plan.pending.includes('qian'));
 assert.throws(()=>verifyComponentEngines({engines:{qian:'^0.2.0'},surfaces},plan.evidence,undefined,plan.pending),incompatible);
 // 没有界面表面的组件不受前端框架范围约束；范围本身仍要能解析。
 assert.deepEqual(verifyComponentEngines({engines:{qian:'^0.2.0'}},plan.evidence),[]);
 assert.throws(()=>verifyComponentEngines({engines:{qian:'not-a-range'}},[],undefined,['qian']),incompatible);
});
