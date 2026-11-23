import {test} from 'node:test';
import assert from 'node:assert/strict';
import {mkdtempSync,mkdirSync,readFileSync,writeFileSync,rmSync,realpathSync,existsSync,cpSync,readdirSync} from 'node:fs';
import path from 'node:path';
import os from 'node:os';
import {fileURLToPath} from 'node:url';
import {execFileSync,spawnSync} from 'node:child_process';
import {generateKeyPairSync,createHash,sign} from 'node:crypto';
import {stringify,parse as parseYaml} from 'yaml';

// 空白 ASP.NET Core 项目通过 CLI 安装已发布的 NuGet 组件。
// 市场是 CLI 已支持的签名本地目录，条目指向仓库里的真实组件文件夹；按发布清单分别从平台源和公开源还原。
// 平台源通过 KUN_PLATFORM_FEED_USER 和 KUN_PLATFORM_FEED_TOKEN 读取已发布包。
const repo=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'../../..');
const cli=path.join(repo,'packages/qian-cli/dist/main.js');
const publishedPlatform='https://nuget.pkg.github.com/CavaiLAB/index.json';
const publishedPublic='https://api.nuget.org/v3/index.json';
const releaseTool=path.join(repo,'scripts/release_packages.py');
const market='qian-published';
const components=[{name:'data',id:'qian.data'},{name:'events',id:'qian.events'},{name:'config',id:'qian.config'}];
const xml=value=>value.replaceAll('&','&amp;').replaceAll('"','&quot;').replaceAll('<','&lt;');
function write(root,name,value){const target=path.join(root,name);mkdirSync(path.dirname(target),{recursive:true});writeFileSync(target,value);}
function qian(root,...args){return spawnSync(process.execPath,[cli,'--cwd',root,'--no-home','--json',...args],{encoding:'utf8',timeout:900000});}
function dotnet(cwd,...args){return spawnSync('dotnet',args,{cwd,encoding:'utf8',timeout:900000,env:{...process.env,DOTNET_NOLOGO:'1',DOTNET_CLI_TELEMETRY_OPTOUT:'1'}});}
function files(root,base=root){return readdirSync(root,{withFileTypes:true}).flatMap(item=>item.isDirectory()?files(path.join(root,item.name),base):[path.relative(base,path.join(root,item.name))]).sort();}
function manifest(id){return JSON.parse(readFileSync(path.join(repo,'plugins',id,'.qian-plugin/plugin.json'),'utf8'));}

// 宿主程序：usings 与 services 是各组合的宿主装配，其余按 CLI 解析出的 profile 挂载已安装组件。
const program=(usings,services,checks='')=>`using System.Text.Json;
${usings}using Kun.Runtime;

// 按 CLI 解析出的 profile 挂载已安装组件：代码入口取自 .qian/plugins 里的组件清单，配置取自 profile 条目。
var builder = WebApplication.CreateBuilder(args);
PluginLoader? loader = null;
${services}var app = builder.Build();
using var resolved = JsonDocument.Parse(File.ReadAllText(args[0]));
var definitions = new List<PluginDefinition>();
foreach (var item in resolved.RootElement.GetProperty("entries").EnumerateArray())
{
    var plugin = item.GetProperty("plugin").GetString()!;
    var id = plugin[..plugin.IndexOf('@')];
    using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(".qian", "plugins", id, ".qian-plugin", "plugin.json")));
    // 入口类型只从清单直接引用的程序集解析；缺入口或类型不存在即失败。
    var runtimeManifest = document.RootElement.GetProperty("runtime");
    var entry = runtimeManifest.GetProperty("entry").GetString()!;
    var pinned = runtimeManifest.GetProperty("nuget").EnumerateArray().Select(reference => reference.GetProperty("id").GetString()!).ToArray();
    var comma = entry.IndexOf(',');
    if (comma < 1 || !pinned.Contains(entry[(comma + 1)..].Trim(), StringComparer.OrdinalIgnoreCase))
        throw new InvalidOperationException(id + " 的清单必须直接钉住入口程序集所在的包：" + entry);
    var type = Type.GetType(entry, throwOnError: true)!;
    var provides = item.GetProperty("provides").EnumerateArray().Select(key => new ServiceKey(key.GetString()!)).ToArray();
    definitions.Add(new PluginDefinition(new PluginEntry(item.GetProperty("id").GetString()!, id, plugin[(plugin.IndexOf('@') + 1)..], ScopeKey.Global, 0, item.GetProperty("config").Clone()),
        (IKunPlugin)Activator.CreateInstance(type)!, provides));
}
loader = new PluginLoader(app.Services);
await using var runtime = loader;
await runtime.MountAsync(definitions);
foreach (var definition in definitions)
    if (runtime.GetState(definition.Entry.Id, ScopeKey.Global) != PluginState.Ready)
        throw new InvalidOperationException(definition.Entry.Plugin + " 未就绪", runtime.GetFailure(definition.Entry.Id, ScopeKey.Global));
Console.WriteLine("installed-providers-ready:" + string.Join(',', definitions.Select(definition => definition.Entry.Plugin + "=" + definition.Plugin.GetType().Assembly.GetName().Version)));
${checks}
`;
// flags 只允许 platform profile。使用配置组件带入的同一程序集，直接验证它声明的入口与依赖。
const flags=manifest('qian.config.flags');
const flagsCheck=`var flagsType = Type.GetType(${JSON.stringify(flags.runtime.entry)}, throwOnError: true)!;
var flagsPlugin = (IKunPlugin)Activator.CreateInstance(flagsType)!;
var expectedKeys = new string[] { ${flags.inject.map(key=>JSON.stringify(key)).join(', ')} };
if (!flagsPlugin.Inject.Select(key => key.Name).Order().SequenceEqual(expectedKeys.Order()) ||
    flagsType.Assembly.GetName().Version!.ToString() != ${JSON.stringify(assemblyVersion(entryPackage('qian.config.flags').version))})
    throw new InvalidOperationException("特性开关入口的程序集版本或服务依赖与清单不符");`;
const host=program('using Kun.EventBus.DependencyInjection;\nusing Kun.EventBus;\n','builder.Services.AddKunEventBus();\nbuilder.Services.AddKunEvents();\n',flagsCheck);
// qian.files 的宿主装配照组件 README：显式选择租户上下文（宿主直接引用的 Kun.MultiTenancy 提供 AsyncLocalTenantContext），
// AddKunEventBus 登记文件事件，AddKunMediator 登记文件能力（能力源取 files 条目的上下文）。
// 本门只验 Provider 挂载到 Ready（R100）；能力的命令管道还要当前主体与数据会话等宿主装配，归 S1-QA-005 的独立样例，
// 所以这里不在构建时校验整棵依赖树。
const filesHost=program('using Kun.Core.MultiTenancy;\nusing Kun.EventBus.DependencyInjection;\nusing Kun.Mediator.DependencyInjection;\nusing Kun.MultiTenancy;\nusing Kun.Files;\n',
 'builder.Host.UseDefaultServiceProvider(options => options.ValidateOnBuild = false);\nbuilder.Services.AddSingleton<ITenantContext>(new AsyncLocalTenantContext());\n' +
 'builder.Services.AddKunEventBus(registrar => registrar.RegisterFilesEvents());\nbuilder.Services.AddKunMediator(registry => registry.AddKunFiles(_ => loader!.GetContext("files", ScopeKey.Global)));\n');
// 公开客户端只需要 Identity.Client 的宿主装配；发行方和 clientId 与 profile 配置一致，不创建替身 Provider。
const identityHost=program('using Kun.Identity.Abstractions;\nusing Kun.Identity.Client;\n',
 'builder.Services.AddKunIdentityClient(new IdentityClientOptions { Authority = new Uri("https://id.qian.test/"), ClientId = "qian-public", RedirectUri = new Uri("https://rp.qian.test/auth/callback"), UseInMemorySessionStore = true });\n',
 `var context = runtime.GetContext("client", ScopeKey.Global);
var identity = context.Inject<IIdentityService>(IdentityDefinition.Key);
if (identity.CurrentPrincipal is not null) throw new InvalidOperationException("公开客户端的初始主体必须匿名");
app.UseKunIdentityClient();
app.MapKunIdentityClient();
app.Urls.Clear();
app.Urls.Add("http://127.0.0.1:0");
await app.StartAsync();
try
{
    using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
    using var active = await client.GetAsync("/auth/session");
    if (active.StatusCode != System.Net.HttpStatusCode.Unauthorized) throw new InvalidOperationException("已挂载会话端点应以 401 拒绝匿名请求");
    await runtime.UnmountAsync("client", ScopeKey.Global);
    using var inactive = await client.GetAsync("/auth/session");
    if (inactive.StatusCode != System.Net.HttpStatusCode.NotFound) throw new InvalidOperationException("卸载后会话端点必须不可见");
}
finally { await app.StopAsync(); }
if (runtime.DumpConfig().Count != 0) throw new InvalidOperationException("身份组件卸载后仍在挂载树里");
var revoked = false;
try { _ = context.Inject<IIdentityService>(IdentityDefinition.Key); }
catch (ObjectDisposedException) { revoked = true; }
if (!revoked) throw new InvalidOperationException("身份服务旧上下文未撤销");
Console.WriteLine("identity-service-revoked");`);
// 六个组件的七条包引用须属于同一正式发布批次。
const pinnedComponents=['qian.config','qian.config.flags','qian.data','qian.events','qian.files','qian.identity.client'];
function entryPackage(id){
 const {entry,nuget}=manifest(id).runtime;
 const assembly=entry.split(',')[1]?.trim();
 const reference=nuget.find(item=>item.id===assembly);
 assert.ok(reference,id+' 的清单应直接引用入口程序集所在包 '+assembly);
 return reference;
}
function assemblyVersion(version){return version.split('-')[0]+'.0';}
const ready=items=>items.map(({id})=>id+'='+assemblyVersion(entryPackage(id).version)).sort();
let releaseTiers;
let sourceConfig;
function nugetConfig(project,cache){
 assert.ok(process.env.KUN_PLATFORM_FEED_USER && process.env.KUN_PLATFORM_FEED_TOKEN,'需要 KUN_PLATFORM_FEED_USER 和有读包权限的 KUN_PLATFORM_FEED_TOKEN');
 const pins=pinnedComponents.flatMap(id=>manifest(id).runtime.nuget);
 assert.equal(pins.length,7);
 for(const id of pinnedComponents)entryPackage(id);
 const versions=[...new Set(pins.map(pin=>pin.version))];
 assert.equal(versions.length,1,'六个组件应使用同一发布批次');
 assert.ok(pins.every(pin=>/^sha256:[a-f0-9]{64}$/.test(pin.digest)),'每个包都须钉住归档摘要');
 const shared=entryPackage('qian.config');
 assert.deepEqual(entryPackage('qian.config.flags'),shared,'配置和特性开关应共享同一份已发布制品');
 assert.match(versions[0],/^\d{4}\.\d{1,2}\.\d{1,2}(?:-hotfix\.[1-9]\d*)?$/,'组件安装验收使用正式发布版或修复版');
 releaseTiers ??= new Map(JSON.parse(execFileSync('python3',[releaseTool,'list','--json'],{encoding:'utf8'})).map(pkg=>[pkg.id,pkg.tier]));
 sourceConfig ??= execFileSync('python3',[releaseTool,'nuget-config','--version',versions[0]],{encoding:'utf8'});
 const config=sourceConfig.replace('</configuration>',
  '  <config><add key="globalPackagesFolder" value="'+xml(cache)+'" /></config>\n</configuration>');
 write(project,'NuGet.Config',config);
}
// 签名本地市场：条目指向仓库里的真实组件文件夹，只允许装进给定 profile；公钥写进项目信任。
function signedMarket(project,catalog,items,profile){
 for(const item of items)cpSync(path.join(repo,'plugins',item.id),path.join(catalog,'plugins',item.id),{recursive:true});
 const keys=generateKeyPairSync('ed25519'),publicPem=keys.publicKey.export({type:'spki',format:'pem'});
 write(catalog,'keys/publisher.pub',publicPem);
 const marketplace={name:market,interface:{displayName:'已发布组件'},owner:{name:'Qian',publicKeys:['./keys/publisher.pub']},categories:['platform'],
  plugins:items.map(item=>({name:item.name,source:{type:'local',path:'./plugins/'+item.id},category:'platform',policy:{installation:'auto',authentication:'ON_INSTALL',profiles:[profile]}}))};
 const document=Buffer.from(JSON.stringify(marketplace,null,2)+'\n');
 write(catalog,'.qian/marketplace.json',document);write(catalog,'.qian/marketplace.json.sig',sign(null,createHash('sha256').update(document).digest(),keys.privateKey));
 write(project,'.qian/config.yml',stringify({defaultProfile:profile,marketplaces:[{name:market,source:catalog,priority:10}],trust:{keys:[publicPem]}}));
}
// 组件清单里的每个 NuGet 包：按确切版本钉进项目（精确区间 [x]），按该版本发布映射还原，归档摘要与清单、lock 三方一致；表面逐字节等于仓库组件文件夹。
function assertInstalled(project,cache,plugins,lock,items){
 const projectXml=readFileSync(path.join(project,'Shop.csproj'),'utf8');
 const assets=JSON.parse(readFileSync(path.join(project,'obj/project.assets.json'),'utf8'));
 assert.deepEqual(Object.keys(assets.packageFolders).map(folder=>path.resolve(folder)),[path.resolve(cache)],'应使用该空白宿主的独立包目录');
 for(const [identity,library] of Object.entries(assets.libraries)){
  assert.equal(library.type,'package');
  const [id,version]=identity.split('/');
  const tier=releaseTiers.get(id);
  if(id.startsWith('Kun.')){
   assert.ok(tier,id+' 须在正式发包清单中');
   assert.equal(version,entryPackage(items[0].id).version,id+' 须属于本次装配批次');
  }
  const directory=path.join(cache,id.toLowerCase(),version.toLowerCase());
  const actual=JSON.parse(readFileSync(path.join(directory,'.nupkg.metadata'),'utf8')).source;
  assert.equal(actual,tier==='platform'?publishedPlatform:publishedPublic,id+' 包源');
 }
 for(const item of items){
  const declared=manifest(item.id).runtime.nuget,reported=plugins.find(plugin=>plugin.id===item.id),record=lock.plugins.find(plugin=>plugin.id===item.id);
  // CLI 按包 ID 排列 NuGet 引用，与清单顺序无关。
  assert.equal(reported.status,'installed');assert.deepEqual([...reported.packages.nuget].sort(),declared.map(reference=>reference.id+'@'+reference.version).sort());
  const source=path.join(repo,'plugins',item.id),target=path.join(project,'.qian/plugins',item.id);
  assert.deepEqual(files(target),files(source));
  for(const name of files(source))assert.ok(readFileSync(path.join(target,name)).equals(readFileSync(path.join(source,name))),item.id+'/'+name);
  assert.equal(record.market,market);assert.match(record.digest,/^sha256:[0-9a-f]{64}$/);
  for(const reference of declared){
   assert.ok(projectXml.includes('<PackageReference Include="'+reference.id+'" Version="['+reference.version+']" />'),projectXml);
   const restored=path.join(cache,reference.id.toLowerCase(),reference.version);
   assert.equal('sha256:'+createHash('sha256').update(readFileSync(path.join(restored,reference.id.toLowerCase()+'.'+reference.version+'.nupkg'))).digest('hex'),reference.digest);
   assert.equal(record.packageArtifacts.find(artifact=>artifact.name===reference.id).digest,reference.digest);
  }
 }
}

test('空白 ASP.NET Core 项目经签名市场安装已发布的 data / events / config：表面进 .qian、NuGet 按清单钉版并从指定源还原、patch 与 lock 记录来源和 digest，真实 Provider 运行，remove 逆序撤销',()=>{
 const root=mkdtempSync(path.join(realpathSync(os.tmpdir()),'qian-feed-e2e-')),project=path.join(root,'shop'),catalog=path.join(root,'catalog'),cache=path.join(root,'nuget-packages');
 try{
  const created=dotnet(root,'new','web','--name','Shop','--output',project,'--framework','net10.0','--no-restore');
  assert.equal(created.status,0,created.stdout+created.stderr);
  nugetConfig(project,cache);
  // 安装器不猜测宿主组装：先放入仓库交付的 minimal profile 与其空组合包（init 模板归 S1-RT-012）。
  cpSync(path.join(repo,'profiles/minimal'),path.join(project,'profiles/minimal'),{recursive:true});
  cpSync(path.join(repo,'plugins/qian.bundle.minimal'),path.join(project,'plugins/qian.bundle.minimal'),{recursive:true});
  signedMarket(project,catalog,components,'minimal');
  const projectFile=path.join(project,'Shop.csproj'),patchFile=path.join(project,'profiles/minimal/qian.patch.yml');
  const before={project:readFileSync(projectFile,'utf8'),patch:readFileSync(patchFile,'utf8'),files:files(project)};

  const installed=qian(project,'add','data','events','config','--yes');
  assert.equal(installed.status,0,installed.stderr);
  assert.ok(!(installed.stdout+installed.stderr).includes(process.env.KUN_PLATFORM_FEED_TOKEN),'安装输出不得包含读包令牌');
  assertInstalled(project,cache,JSON.parse(installed.stdout).plugins,JSON.parse(readFileSync(path.join(project,'.qian/lock.json'),'utf8')),components);
  assert.deepEqual(parseYaml(readFileSync(patchFile,'utf8')).map(entry=>[entry.id,entry.plugin]).sort(),components.map(item=>[item.name,item.id+'@'+manifest(item.id).version]).sort());

  // 宿主用 CLI 解析出的 profile 挂载已安装组件：三个 Provider 都以从指定发布源还原的程序集运行。
  const resolved=qian(project,'dump-config');assert.equal(resolved.status,0,resolved.stderr);
  const programFile=path.join(project,'Program.cs'),original=readFileSync(programFile,'utf8');
  write(root,'resolved.json',resolved.stdout);writeFileSync(programFile,host);
  const started=dotnet(project,'run','--no-restore','-p:UseSharedCompilation=false','--',path.join(root,'resolved.json'));
  assert.equal(started.status,0,started.stdout+started.stderr);
  const line=started.stdout.split('\n').find(text=>text.startsWith('installed-providers-ready:'));
  assert.ok(line,started.stdout);assert.deepEqual(line.slice('installed-providers-ready:'.length).split(',').sort(),ready(components));
  writeFileSync(programFile,original);

  const listed=qian(project,'list');assert.equal(listed.status,0,listed.stderr);
  assert.deepEqual(JSON.parse(listed.stdout).plugins.map(plugin=>[plugin.id,plugin.market,plugin.packages.nuget.join(',')]).sort(),
   components.map(item=>[item.id,market,manifest(item.id).runtime.nuget.map(reference=>reference.id+'@'+reference.version).join(',')]).sort());

  // 逆序移除并清除配置：项目文件、profile patch 与文件集合回到安装前，列表为空。
  const removed=qian(project,'remove','config','events','data','--purge');assert.equal(removed.status,0,removed.stderr);
  assert.equal(readFileSync(projectFile,'utf8'),before.project);assert.equal(readFileSync(patchFile,'utf8'),before.patch);
  for(const item of components)assert.equal(existsSync(path.join(project,'.qian/plugins',item.id)),false);
  assert.deepEqual(JSON.parse(qian(project,'list').stdout).plugins,[]);
  const remaining=files(project).filter(name=>!name.startsWith('obj/')&&!name.startsWith('bin/')&&!name.startsWith('.qian/'));
  // 工作区状态不随组件撤销：首次安装写入的 .qian 忽略规则保留（.qian/ 本身仍在）；CLI 不创建 NuGet 锁文件。
  assert.deepEqual(remaining,[...before.files.filter(name=>!name.startsWith('.qian/')),'.gitignore'].sort());
  assert.equal(readFileSync(path.join(project,'.gitignore'),'utf8'),'.qian/journal/\n.qian/.staging/\n');
  assert.equal(existsSync(path.join(project,'packages.lock.json')),false);
 }finally{rmSync(root,{recursive:true,force:true});}
});

test('空白 ASP.NET Core 项目经签名市场把已发布的 data / files 装进 standalone profile：files 的每个 NuGet 包按清单钉版并从指定源还原，预置的存储配置随条目沿用，真实 Provider 运行，remove 逆序撤销',()=>{
 const selected=[{name:'data',id:'qian.data'},{name:'files',id:'qian.files'}];
 const root=mkdtempSync(path.join(realpathSync(os.tmpdir()),'qian-feed-files-')),project=path.join(root,'shop'),catalog=path.join(root,'catalog'),cache=path.join(root,'nuget-packages'),objects=path.join(root,'objects');
 try{
  const created=dotnet(root,'new','web','--name','Shop','--output',project,'--framework','net10.0','--no-restore');
  assert.equal(created.status,0,created.stdout+created.stderr);
  nugetConfig(project,cache);
  // 文件宿主选择事件总线和租户上下文的实现，并按同一发布版本登记。
  const hostVersion='['+entryPackage('qian.files').version+']';
  for(const dependency of ['Kun.EventBus','Kun.MultiTenancy']){
   const added=dotnet(project,'add','Shop.csproj','package',dependency,'--version',hostVersion,'--no-restore');
   assert.equal(added.status,0,added.stdout+added.stderr);
  }
  // files 的 policy.profiles 只允许 platform / standalone：项目自带一个名为 standalone、只引用空组合包的 profile，不带仓库 standalone 组合包里的其它组件。
  // qian add 只按 configSchema 缺省值合成条目配置，而 storage 必填：先放一条 disabled 的 files 条目写好存储配置，安装时沿用它。
  const storage={kind:'local',root:objects,endpoint:'http://127.0.0.1:5080/files/blobs/'};
  write(project,'profiles/standalone/qian.profile.yml',stringify({name:'standalone',description:'发布包验收的 standalone profile',bundles:['qian.bundle.minimal@^1'],patch:'./qian.patch.yml'}));
  write(project,'profiles/standalone/qian.patch.yml',stringify([{id:'files',plugin:'qian.files',disabled:true,config:{storage}}]));
  cpSync(path.join(repo,'plugins/qian.bundle.minimal'),path.join(project,'plugins/qian.bundle.minimal'),{recursive:true});
  signedMarket(project,catalog,selected,'standalone');
  const projectFile=path.join(project,'Shop.csproj'),patchFile=path.join(project,'profiles/standalone/qian.patch.yml');
  const before=readFileSync(projectFile,'utf8');

  const installed=qian(project,'add','data','files','--yes');
  assert.equal(installed.status,0,installed.stderr);
  assert.ok(!(installed.stdout+installed.stderr).includes(process.env.KUN_PLATFORM_FEED_TOKEN),'安装输出不得包含读包令牌');
  assertInstalled(project,cache,JSON.parse(installed.stdout).plugins,JSON.parse(readFileSync(path.join(project,'.qian/lock.json'),'utf8')),selected);
  const entries=parseYaml(readFileSync(patchFile,'utf8'));
  assert.deepEqual(entries.map(entry=>[entry.id,entry.plugin,entry.disabled===true]).sort(),selected.map(item=>[item.name,item.id+'@'+manifest(item.id).version,false]).sort());
  // 预置的存储配置原样沿用，其余键补 configSchema 缺省值（平台默认配额 10 GiB）。
  assert.deepEqual(entries.find(entry=>entry.id==='files').config,{storage,quota:{perTenantBytes:10*1024**3}});

  // 宿主选用的事件总线与组件使用的契约须来自同一发布批次。
  const libraries=JSON.parse(readFileSync(path.join(project,'obj/project.assets.json'),'utf8')).libraries;
  const restoredVersions=id=>Object.entries(libraries).filter(([name,library])=>library.type==='package'&&name.startsWith(id+'/')).map(([name])=>name.slice(id.length+1));
  const contracts=restoredVersions('Kun.EventBus.Abstractions');
  assert.equal(contracts.length,1,'已发布组件必须还原单一 Kun.EventBus.Abstractions 版本');
  assert.deepEqual(restoredVersions('Kun.EventBus'),contracts,'已还原的 EventBus 实现与契约必须属于同一发布批次');
  const resolved=qian(project,'dump-config');assert.equal(resolved.status,0,resolved.stderr);
  const programFile=path.join(project,'Program.cs'),original=readFileSync(programFile,'utf8');
  write(root,'resolved.json',resolved.stdout);writeFileSync(programFile,filesHost);
  const started=dotnet(project,'run','--no-restore','-p:UseSharedCompilation=false','--',path.join(root,'resolved.json'));
  assert.equal(started.status,0,started.stdout+started.stderr);
  const line=started.stdout.split('\n').find(text=>text.startsWith('installed-providers-ready:'));
  assert.ok(line,started.stdout);assert.deepEqual(line.slice('installed-providers-ready:'.length).split(',').sort(),ready(selected));
  // local 后端按沿用的存储配置建好了对象根目录。
  assert.ok(existsSync(objects));
  writeFileSync(programFile,original);

  const listed=qian(project,'list');assert.equal(listed.status,0,listed.stderr);
  assert.deepEqual(JSON.parse(listed.stdout).plugins.map(plugin=>[plugin.id,[...plugin.packages.nuget].sort().join(',')]).sort(),
   selected.map(item=>[item.id,manifest(item.id).runtime.nuget.map(reference=>reference.id+'@'+reference.version).sort().join(',')]).sort());

  // 逆序移除并清除配置：项目文件回到安装前；--purge 连同被沿用的预置 files 条目一起删除，patch 为空，列表为空。
  const removed=qian(project,'remove','files','data','--purge');assert.equal(removed.status,0,removed.stderr);
  assert.equal(readFileSync(projectFile,'utf8'),before);assert.deepEqual(parseYaml(readFileSync(patchFile,'utf8')),[]);
  for(const item of selected)assert.equal(existsSync(path.join(project,'.qian/plugins',item.id)),false);
  assert.deepEqual(JSON.parse(qian(project,'list').stdout).plugins,[]);
 }finally{rmSync(root,{recursive:true,force:true});}
});

test('空白宿主从平台源安装 identity：精确版本与归档摘要、声明式配置沿用、真实 Identity seam 就绪并撤销，remove 清理组件与包引用',()=>{
 const selected=[{name:'identity',id:'qian.identity.client'}];
 const root=mkdtempSync(path.join(realpathSync(os.tmpdir()),'qian-feed-identity-')),project=path.join(root,'shop'),catalog=path.join(root,'catalog'),cache=path.join(root,'nuget-packages');
 try{
  const created=dotnet(root,'new','web','--name','Shop','--output',project,'--framework','net10.0','--no-restore');
  assert.equal(created.status,0,created.stdout+created.stderr);
  nugetConfig(project,cache);
  cpSync(path.join(repo,'profiles/minimal'),path.join(project,'profiles/minimal'),{recursive:true});
  cpSync(path.join(repo,'plugins/qian.bundle.minimal'),path.join(project,'plugins/qian.bundle.minimal'),{recursive:true});
  // 常规组件的默认条目 ID 取组件 ID 的最后一段（client），identity 是市场简写。
  // issuer/clientId 没有缺省值，由宿主明确提供；add 只启用条目并沿用配置，不猜测 OIDC 发行方。
  const configuration={issuer:'https://id.qian.test/',clientId:'qian-public'};
  write(project,'profiles/minimal/qian.patch.yml',stringify([{id:'client',plugin:'qian.identity.client',disabled:true,config:configuration}]));
  signedMarket(project,catalog,selected,'minimal');
  const projectFile=path.join(project,'Shop.csproj'),patchFile=path.join(project,'profiles/minimal/qian.patch.yml');
  const before=readFileSync(projectFile,'utf8');

  const installed=qian(project,'add','identity','--yes');
  assert.equal(installed.status,0,installed.stderr);
  assert.ok(!(installed.stdout+installed.stderr).includes(process.env.KUN_PLATFORM_FEED_TOKEN),'安装输出不得包含读包令牌');
  assertInstalled(project,cache,[JSON.parse(installed.stdout)],JSON.parse(readFileSync(path.join(project,'.qian/lock.json'),'utf8')),selected);
  assert.deepEqual(parseYaml(readFileSync(patchFile,'utf8')),[{id:'client',scope:'global',plugin:'qian.identity.client@'+manifest('qian.identity.client').version,config:configuration,disabled:false}]);

  const resolved=qian(project,'dump-config');assert.equal(resolved.status,0,resolved.stderr);
  const programFile=path.join(project,'Program.cs'),original=readFileSync(programFile,'utf8');
  write(root,'resolved.json',resolved.stdout);writeFileSync(programFile,identityHost);
  const started=dotnet(project,'run','--no-restore','-p:UseSharedCompilation=false','--',path.join(root,'resolved.json'));
  assert.equal(started.status,0,started.stdout+started.stderr);
  const line=started.stdout.split('\n').find(text=>text.startsWith('installed-providers-ready:'));
  assert.ok(line,started.stdout);assert.deepEqual(line.slice('installed-providers-ready:'.length).split(','),ready(selected));
  assert.ok(started.stdout.split(/\r?\n/).includes('identity-service-revoked'),started.stdout);
  writeFileSync(programFile,original);

  const listed=qian(project,'list');assert.equal(listed.status,0,listed.stderr);
  assert.deepEqual(JSON.parse(listed.stdout).plugins.map(plugin=>plugin.id),['qian.identity.client']);
  const removed=qian(project,'remove','identity','--purge');assert.equal(removed.status,0,removed.stderr);
  assert.equal(readFileSync(projectFile,'utf8'),before);assert.deepEqual(parseYaml(readFileSync(patchFile,'utf8')),[]);
  assert.equal(existsSync(path.join(project,'.qian/plugins/qian.identity.client')),false);
  assert.equal(existsSync(path.join(project,'packages.lock.json')),false);
  assert.deepEqual(JSON.parse(qian(project,'list').stdout).plugins,[]);
 }finally{rmSync(root,{recursive:true,force:true});}
});
