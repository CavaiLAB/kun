> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# staging 首次部署：平台与身份同进程（S1-DEP-011）

> 本页由构建脚本镜像自仓库 [`docs/ops/staging-first-deploy.md`](https://github.com/CavaiLAB/kun/blob/main/docs/ops/staging-first-deploy.md)，请修改源文件，不要改本页。

> 文中的域名使用文档示例值，地址与主机用占位或角色表示，执行时请代入所在环境的配置。

「初步可用」里程碑期间（S1-DEC-033 D2，R143），staging 按过渡形状部署：

- 平台宿主 `hosts/Kun.Platform.Host` 以 `Identity:InProcess=true` 并入身份服务与开通申请组件；
- 网关宿主 `hosts/Kun.Gateway.Host` 独立成进程，并托管外壳；
- 清单在部署仓库手写（ADR-017 §10）。

本手册覆盖构建、部署、回退与首个平台运营账号的引导，并列出相对 PRD 7.17 的已知差异。

- 装配方式与取值：产品负责人 2026-11-11 的裁决 R150 与 R151。R150 定扩展登记与 overlay、主机名与证书、引导 Job，以及 D4 只开运营开户，做法见下文各节；R151 定 staging 的系统组件修订在构建主机上签名、每次部署由 Job 安装并激活，宿主预装的 system 级组件视为已授权，开通时也不要反向权限批准。
- 配置键：以 [平台宿主](https://github.com/CavaiLAB/kun/blob/main/hosts/Kun.Platform.Host/README.md) 与 [网关宿主](https://github.com/CavaiLAB/kun/blob/main/hosts/Kun.Gateway.Host/README.md) 的文档为准。
- 清单与 Secret 引导脚本：在部署仓库里（应用层清单与 staging 环境配置），对象名以那里为准。

凡是改动集群状态的操作（apply 或同步、建角色、建 Secret、改网络策略、跑 Job），都要先经产品负责人确认；只读查询不需要。密钥只经 Secret 注入，不进 git。

## 1 形状

| 工作负载                             | 镜像                                             | 说明                                                                                                                                         |
| -------------------------------- | ---------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| `qian-platform`（Deployment，1 副本） | `<镜像仓库>/qian/qian-platform@sha256:…`（JIT）      | 平台插件树加身份分支。平台侧：平台表面与运营接口。身份侧：OIDC 协议面、账号接口、认证页族 `/login`、申请人接口与公开页 `/signup`。前端在镜像的 `/app/web/identity`、`/app/web/signup`                  |
| `qian-gateway`（Deployment，1 副本）  | `<镜像仓库>/qian/qian-gateway@sha256:…`（NativeAOT） | BFF 会话、S2S 签发与路由：平台簇与运营簇都指向 `http://qian-platform:8080/`。另有原生审计、外壳与远程托管（`/app/web/shell`、`/app/web/remotes/<组件>`）。配置源 `Config:Source=host` |
| 迁移、系统组件修订、首个运营引导（Job）            | 与 `qian-platform` 同镜像、同配置                      | `--migrate-only`、`--install-system-revisions`、`--bootstrap-operator`，见第 4 节                                                                |
| 建角色（Job）                         | PostgreSQL 客户端                                 | 用基线的管理员 Secret 建受限登录与 NOLOGIN 函数属主                                                                                                         |
| `qian-postgres`、`qian-redis`     | 基线（S1-DEP-001）                                 | 已在 `qian-staging` 运行，钉在承载 staging 数据服务的工作节点（下称数据节点）                                                                                        |

主机名（R150 ②）：

- 外壳与网关：`https://gateway.staging.example`
- 身份发行方：`https://identity.staging.example/`

公网链路与现有站点相同：浏览器 → 边缘主机上的 nginx（TLS 在此终止，Let's Encrypt 证书）→ WireGuard → 控制面节点 → MetalLB → Traefik（按 Host 分流）→ 两个 Service。

**转发头。** Traefik 对 web / websecure 入口信任节点网段与 Pod 网段的转发头，边缘主机设的 `X-Forwarded-Proto: https` 会原样到 Pod。

- 平台宿主：身份服务只收 HTTPS，所以身份分支要从可信代理的转发头恢复原始 scheme 和客户端地址。staging 的 `Identity:TrustedProxyNetworks` 配直连的 Traefik Pod 所在网段与 Traefik 看到的 WireGuard 一跳所在网段，`Identity:ForwardLimit=2`。
- 网关：`Gateway:TrustedProxyNetworks` 配直连的 Traefik Pod 所在网段。

**回环与依赖。**

- 网关取发现文档、换码、刷新都走身份发行方的公网地址，要经边缘主机绕一圈回来：网关的出口策略要放行边缘主机公网地址的 443。网关的就绪检查也要取到发现文档与 JWKS，所以 DNS、边缘主机的 `443` vhost 与证书就位之前，网关不就绪。
- 平台取网关的 JWKS 走集群内的 `http://qian-gateway:8080/.well-known/jwks.json`（`S2S:AllowHttpJwks=true`）。
- 平台不连 Redis：环境 overlay 把 events 条目的配置置空，Outbox 用 skip-locked、不带 broker。网关连 Redis（会话、限流）与 PostgreSQL（原生审计）。

## 2 一次性前置（产品负责人）

1. DNS：两个主机名的 A 记录指向边缘主机的公网地址。
2. Harbor（只经 HTTP 提供服务）：
   - 建私有项目 `qian`。
   - 推送：构建主机的 Docker 登录 Harbor 后推送到 `qian`。要收窄权限，另建一个只对 `qian` 有推送权限的机器人账号，在构建主机上用它 `docker login <Harbor 地址>:<端口>`。
   - 拉取用机器人账号加 Secret（产品负责人 2026-11-12 确认）。项目是私有的，而节点的 `registries.yaml` 对 Harbor 只配了 HTTP 端点、没有凭据，所以拉取要靠 imagePullSecrets。
     1. 在项目 `qian` 的机器人账号页新建一个，权限只勾仓库的拉取。记下 Harbor 显示的完整名称（形如 `robot$qian+staging-pull`）和只显示一次的令牌。
     2. 在有集群 kubeconfig 的运维主机上建 `qian-staging` 里的 `qian-harbor-pull`。执行下面这条后粘贴令牌、回车：`read -rs` 不回显，令牌也不进 shell 历史。用户名换成上一步记下的完整名称，单引号不能省，因为名称里有 `$`。
        ```bash
        read -rs QIAN_PULL_TOKEN && kubectl --context <上下文> -n qian-staging create secret docker-registry qian-harbor-pull --docker-server=<Harbor 地址> --docker-username='robot$qian+staging-pull' --docker-password="$QIAN_PULL_TOKEN"; unset QIAN_PULL_TOKEN
        ```
     3. `--docker-server` 写不带端口的 Harbor 地址，与清单里的镜像引用一致。凭据对不对，要到首次同步、Pod 拉镜像时才见分晓：失败时 Pod 停在 `ImagePullBackOff`，`kubectl describe pod` 的事件里能看到 401。
3. 边缘主机的证书与 vhost，分几步（按部署仓库的上线顺序）：

   1. DNS 生效后，在边缘主机上按部署仓库的边缘配置说明跑 `deploy.sh --apply`，发布两个 vhost 的 `:80` 段。这一段只服务 ACME 验证、其余跳转 https，不引用证书。
   2. 用 DNS-01 签发两个主机名的证书，各一条。云厂商对未备案的域名可能在 80 端口间歇返回拦截页，webroot（HTTP-01）验证不可靠。证书仍由 certbot 管理，路径不变；验证改用部署仓库里的两个 certbot 钩子，经 DNS 服务商的 API 增删 `_acme-challenge` TXT 记录。一次性准备与签发命令见部署仓库的边缘配置说明：DNS API 用的子账号只授本域名解析记录的增删查，访问密钥只在边缘主机上交互配置。
   3. 证书就位后，合并补 `443` 段的部署仓库 PR，再跑一次 `deploy.sh --apply`。

   部署脚本先 `nginx -t`、自带备份，失败整体回滚。`443` 段引用的证书文件不存在时 `nginx -t` 必然失败，还会挡住边缘主机上其它 vhost 的发布，所以 `443` 段不能先于证书合入。
4. 系统组件修订的签名钥（R151 ①）。在构建主机上生成一次，私钥只留在构建主机，不进集群、不进 git：
   ```bash
   install -d -m 700 ~/.qian
   openssl genpkey -algorithm ed25519 -out ~/.qian/staging-system-revisions.pem
   chmod 600 ~/.qian/staging-system-revisions.pem
   openssl pkey -in ~/.qian/staging-system-revisions.pem -pubout -out ~/.qian/staging-system-revisions.pub.pem
   ```
   公钥建进 Lifecycle 信任根的 Secret（第 4 节第 1 步）。平台宿主固定把 `secretRef://platform/trust/platform` 解析到 `<Secrets:MountRoot>/qian/trust-platform/public-key`。

## 3 构建镜像与签名清单

前端产物由开发机生成，镜像由具备 Docker 和仓库访问权限的 amd64 构建主机生成。构建入口是 [scripts/images.py](https://github.com/CavaiLAB/kun/blob/main/scripts/images.py) 的 `build`，模板与构建上下文见 [deploy/images](https://github.com/CavaiLAB/kun/blob/main/deploy/images/README.md)。

源码取本仓库 [CavaiLAB/kun](https://github.com/CavaiLAB/kun) 里要部署的提交。两个镜像从同一个提交构建、一起换：外壳与远程打在网关镜像里，界面清单在平台镜像的组件文件夹里，外壳按界面清单的 `engines.qian` 决定装不装远程，新旧两边混用时远程会被拒绝。在仓库根目录执行前端命令和镜像脚本。`--project` 相对仓库根目录，`--web` 指向组装好的前端产物目录。

1. 前端（开发机，在本仓库根目录，检出要构建的提交）。前端框架的包 `@cavailab/qian-*` 装自 GitHub Packages，那里的 npm 源连公开包也要令牌（有 `read:packages` 即可）：照 [README「前端依赖与包源」](https://github.com/CavaiLAB/kun/blob/main/README.md#前端依赖与包源)，在用户级 npm 配置里写一行引用环境变量 `NODE_AUTH_TOKEN` 的令牌行；仓库里的 `.npmrc` 只写包源，不写令牌。

   ```bash
   NODE_AUTH_TOKEN=$(gh auth token) pnpm install --frozen-lockfile
   ```

   ```bash
   pnpm exec turbo run build --filter=qian-shell --filter=@qian/identity-ui --filter=@qian/tenancy-ui --filter=@qian/tenancy-signup-ui
   ```

   ```bash
   pnpm --filter @qian/tenancy-ui run build:remote
   ```

   ```bash
   pnpm --filter @qian/tenancy-signup-ui run build:remote
   ```

   组装两个目录，分别作为两个镜像的 `--web`（来源相对仓库根目录）：

   | 目录                                        | 内容                       | 来源                                            |
   | ----------------------------------------- | ------------------------ | --------------------------------------------- |
   | `web-platform/identity`                   | 认证页族（base `/login/`）     | `packages/qian-identity-ui/dist`              |
   | `web-platform/signup`                     | 开通申请公开页（base `/signup/`） | `packages/qian-tenancy-signup-ui/dist/public` |
   | `web-gateway/shell`                       | 外壳                       | `apps/qian-shell/dist/host`                   |
   | `web-gateway/remotes/qian.tenancy`        | 租户运营远程                   | `packages/qian-tenancy-ui/dist/remote`        |
   | `web-gateway/remotes/qian.tenancy.signup` | 开通申请远程                   | `packages/qian-tenancy-signup-ui/dist/remote` |

2. 在构建主机上检出源码，再把前端制品拷过去。本仓库是公开的，构建主机直接从 GitHub 克隆。构建证据要记源码 SHA，所以要用 git 检出，不能只拷一份源码树；已有检出时先 `git -C kun-build/kun fetch origin`，再检出。
   ```bash
   ssh <构建主机> 'git clone --filter=blob:none --no-checkout https://github.com/CavaiLAB/kun.git kun-build/kun && git -C kun-build/kun checkout --detach <提交>'
   ```
   ```bash
   rsync -a web-platform web-gateway <构建主机>:kun-build/
   ```
   还没推到 GitHub 的提交，改用 `git bundle create kun.bundle <分支>` 打包拷过去，在构建主机上 `git clone -b <分支> kun.bundle kun`。

3. 构建与推送在构建机的 `~/kun-build/kun` 下执行。设置 `QIAN_IMAGE_REGISTRY` 为镜像仓库的主机名及端口，`QIAN_IMAGE_REVISION` 为源码提交的短 SHA。Harbor 只经 HTTP 提供服务时，构建机的 Docker 需要在仓库地址中显式写出端口。构建工具推送镜像后从结果读取 digest。
   ```bash
   python3 scripts/images.py build --kind jit --project hosts/Kun.Platform.Host/Kun.Platform.Host.csproj --assembly Kun.Platform.Host --tag "$QIAN_IMAGE_REGISTRY/qian/qian-platform:$QIAN_IMAGE_REVISION" --web ~/kun-build/web-platform
   ```
   ```bash
   python3 scripts/images.py build --kind aot --project hosts/Kun.Gateway.Host/Kun.Gateway.Host.csproj --assembly Kun.Gateway.Host --tag "$QIAN_IMAGE_REGISTRY/qian/qian-gateway:$QIAN_IMAGE_REVISION" --web ~/kun-build/web-gateway
   ```
   - 每条命令打印一段 JSON：`ref` 是按 digest 的引用，`build` 是构建证据（源码 SHA、profile 与组件目录的树修订、`webDigest` 等）。两段都随部署记录保存，任务进度日志只写结论。
   - 推送失败时命令以非零退出、不打印 JSON，镜像留在本机；修好仓库权限后重跑，BuildKit 缓存让重跑很快。
   - 清单只按 `ref` 里的 digest 引用镜像，标签只用来推送。部署仓库里 digest 只写在 staging 环境 kustomization 的 `images` 段这一处。

4. 签名清单（构建主机，同一目录）。对推送后的同一个 digest 生成签名清单并自查，结果放进部署仓库的签名清单 ConfigMap：
   ```bash
   python3 scripts/sign-system-revisions.py sign --key ~/.qian/staging-system-revisions.pem --image <镜像仓库>/qian/qian-platform@sha256:<digest> --out system-revisions.json
   ```
   ```bash
   python3 scripts/sign-system-revisions.py verify --public-key ~/.qian/staging-system-revisions.pub.pem --signatures system-revisions.json --image <镜像仓库>/qian/qian-platform@sha256:<digest>
   ```
   签名清单是公开数据（组件标识、版本、目录摘要与签名），可以进 git。平台镜像里的组件文件夹一变，摘要就变，所以每换一次平台镜像都要重签。

5. 部署前检查（构建主机，同一目录；第 2 步之后、构建之前就能跑）。拿部署仓库 main 上现用的签名清单核对新检出的组件文件夹：
   ```bash
   python3 scripts/sign-system-revisions.py verify --public-key ~/.qian/staging-system-revisions.pub.pem --signatures <现用的签名清单> --plugins plugins
   ```
   通过说明 system 级组件的文件夹都没变，可以原地换镜像。报出的组件就是文件夹变了：「摘要不一致」是版本号没升，修订 Job 在已有数据的库上必然失败；「没有签名」是升了版本或新加的组件，Job 会成功，但已有租户对升了版本的组件静默失权（第 9 节）。S1-DEP-013 落地之前，已有组件的文件夹一变，就按第 7 节「同版本换摘要」清库重来，先经产品负责人确认。

## 4 部署顺序

部署仓库里的应用层清单与 staging 环境配置由 Argo CD 应用 `qian-staging-baseline` 手动同步。同步时依次执行：

- 配置 ConfigMap（sync-wave -5）；
- 三个 Sync 钩子 Job：建角色（-3）、迁移（-2）、系统组件修订（-1）；
- 工作负载与入口。

钩子 Job 不自动重试（`backoffLimit: 0`），任一失败即中止本次同步，工作负载不变更；看过日志、修好原因后重新同步。首个运营引导不随同步执行，是一个挂起的 CronJob 模板，手动创建一次。

只做完整同步。选择性同步不执行 Sync 钩子，迁移与修订 Job 不会跑；工作负载是 Recreate，旧副本先停，新副本又因待执行的迁移拒绝启动，平台会中断到下一次完整同步。配置 ConfigMap 名带内容哈希，换配置后旧的那份要在同步时勾选 Prune 才会删除。

做过选择性同步之后，Argo CD 会把那次的对象清单留在 `status.operationState.operation.sync.resources` 里，下一次完整同步续跑时读回它，就成了跳过钩子的选择性同步：几秒内报成功，事件里是「Partial sync operation … succeeded」。触发完整同步前先查这个字段，非空就删掉（命令见部署仓库的发布说明）。

每一步都先经产品负责人确认再执行；做完用只读命令检查：

```bash
kubectl -n qian-staging get jobs,pods -o wide
```

```bash
kubectl -n qian-staging logs job/<Job 名>
```

1. **Secret（同步前）。** 部署仓库的 Secret 引导脚本（参数 `--context <上下文> --namespace qian-staging --create-missing --app --trust-public-key <公钥.pem>`）只建缺失项，不覆盖、不打印值。它生成：

   - 新登录的口令与连接串文件；
   - 网关 S2S 的私钥与公钥；
   - 官方客户端（网关 BFF）的密钥；
   - 身份服务的签名、加密证书与私钥（RSA）；
   - 首个平台运营的初始密码；
   - Lifecycle 信任根公钥：运维从构建主机导出后经 `--trust-public-key` 传入，脚本不生成私钥。

   脚本输出信任根的 keyId，核对它与签名清单的 `keyId` 一致。Harbor 拉取 Secret `qian-harbor-pull` 另建（第 2 节）。各 Secret 的键与挂载位置见部署仓库的应用说明。
2. **改 Application 的 revision 并同步。** 应用层清单与镜像 digest 经部署仓库 PR 合入后，另开一个小 PR，把 staging 应用定义里的 `targetRevision` 改到那次合入的提交。合入后由管理 Argo CD 应用定义的自动同步应用把它应用到集群，不用手工 apply；再手动同步 `qian-staging-baseline`。基线清单没有变化时，同步只新增或更新应用层，不重启 PG 与 Redis。
3. **建角色 Job（钩子，-3）。** 用基线的管理员 Secret 执行 `db-roles.sql`，口令取自第 1 步。建的角色：

   - 迁移属主：可在库上建 schema；
   - 两个平台登录：`qian_platform_authz`、`qian_platform_audit`；
   - 网关原生审计的两个登录；
   - 两个 NOLOGIN 的审计函数属主。

   应用登录 `qian_app` 由基线初始化。受限登录都是 `NOINHERIT NOSUPERUSER NOBYPASSRLS NOCREATEDB NOCREATEROLE NOREPLICATION`，函数属主没有任何成员关系。可重复执行：已存在的角色只校正属性与口令。
4. **迁移 Job（钩子，-2，`--migrate-only`）。** 三步，都可重复执行：

   1. 管理员连接做离线 DDL 与授权，包括网关原生审计的离线迁移；
   2. 迁移属主执行包内迁移；
   3. 管理员把业务 schema 授给应用登录。

   - 迁移 Job 只挂迁移属主与管理员的连接串。迁移模式下宿主把在线登录的连接一律改读迁移属主的连接串（`PlatformHostSettings.Connection`），在线登录的口令不给迁移 Job。
   - 在线副本不挂迁移属主与管理员的凭据。
   - 离线 DDL 不做跨版本的表结构升级，详见[平台宿主的 Job](https://github.com/CavaiLAB/kun/blob/main/hosts/Kun.Platform.Host/README.md#job)。
5. **系统组件修订 Job（钩子，-1，`--install-system-revisions`）。** 每次同步都跑，在迁移之后。
   - 它按签名清单把镜像预装的 system 级组件装成修订并激活：同摘要已激活则不动，新版本的新摘要安装后激活。
   - 同一版本换了摘要（组件文件夹变了、版本号没变）装不进去：Lifecycle 拒绝把同一版本装成另一个制品，authz 的权限目录也按组件版本不可变（内容不同返回 CONFLICT，平台插件树挂不起来）。在已有数据的库上，这时 Job 必然失败，处理见第 7 节「同版本换摘要」。
   - 文件夹变了、版本也升了（R167 要求的做法），Job 会成功，同步也是 Synced，但已有租户对该组件静默失权（第 9 节）。S1-DEP-013 落地之前同样按第 7 节清库，不能原地部署。要不要清库，部署前用第 3 节第 5 步的检查确定。
   - 签名清单缺项、验签失败或摘要对不上，都会列出组件并以退出码 1 失败。此时先核对签名清单 ConfigMap 是否对应当前平台镜像 digest。
6. **在线副本。** `qian-platform` 与 `qian-gateway` 两个 Deployment，都用 Recreate、钉在数据节点，`/readyz` 返回 `ready` 后才就绪。
   - 平台在线副本启动时只校验（受限角色、模型、没有待执行的迁移），不建表；迁移没跑或没跑完就拒绝启动。
   - 网关启动不等发行方（挂载身份客户端不联网），但 `/readyz` 含身份客户端检查，要经边缘主机取到发现文档与 JWKS。平台、Ingress、DNS 与边缘主机的 `443` vhost 和证书都就位之前，网关不就绪，外壳主机名整体 503。
   - 网关就绪探针超时 10 秒。启动探针 90 秒：网关入口不拦截未处理异常，依赖挂载失败时 PID 1 可能空转，靠探针超时重启。
7. **入口。** 两个主机名的 Traefik Ingress 随清单下发。身份主机只暴露身份分支的路径：`/connect`、`/.well-known`、`/login`、`/api/identity`、`/signup` 与四个申请人接口。边缘主机的 vhost 按第 2 节第 3 步部署。
8. **首个运营引导（只在首次，手动）。** 两个工作负载都就绪后执行，见第 5 节：
   ```bash
   kubectl --context <上下文> -n qian-staging create job qian-bootstrap-operator-1 --from=cronjob/qian-bootstrap-operator
   ```

## 5 首个平台运营账号

引导 Job 只在库里还没有平台超管时执行（R150 ③）：

1. 经身份账号端口建平台身份，用户名与显示名来自清单，初始密码来自引导 Secret，并标记首登必须改密；
2. 在平台角色锁里授平台超管；
3. 写一条平台审计链记录（操作者 `platform-bootstrap`，通道 `job`）。

库里已有平台超管时 Job 拒绝执行（退出码 1），不能拿它二次提权。上次在授权之前失败时，同名身份沿用，不重置密码。

引导后：

1. 平台运营打开 `https://gateway.staging.example`，跳到身份发行方的 `/login`，用初始密码登录，按提示改密；
2. 回到外壳，进平台控制台（租户运营、开通申请）；
3. 确认能登录后删除初始密码的 Secret 与引导 Job。之后的平台超管授予与撤销走平台运营流程。
   ```bash
   kubectl --context <上下文> -n qian-staging delete job qian-bootstrap-operator-1
   kubectl --context <上下文> -n qian-staging delete secret qian-bootstrap-operator
   ```
   引导 Job 完成 24 小时后会被自动清理，Secret 不会，要手工删。清单里挂起的 CronJob 仍引用这个 Secret，属预期。日后带 `--app` 重跑第 4 节第 1 步的脚本，会按缺失项重新生成它（新的随机密码）；库里已有平台超管时引导 Job 拒绝执行，用不上它，照样删掉。

## 6 部署后检查

逐条对应任务验收，结果记进 `pm/tasks/S1-DEP-011.md` 的进度日志。

E2E 部分由 [scripts/staging-e2e.py](https://github.com/CavaiLAB/kun/blob/main/scripts/staging-e2e.py) 经公网主机名、照外壳的真实路径跑一遍，在能访问两个公网主机名的运维主机上执行。证据 JSON 随部署记录保存、进度日志只写结论，测试账号与租户留在 staging，代码带运行后缀：

```bash
python3 scripts/staging-e2e.py --shell https://gateway.staging.example --issuer https://identity.staging.example --operator <平台运营用户名> --operator-password-file <当前密码文件> --operator-new-password-file ~/.qian/staging-operator-password --out staging-e2e.json
```

- 平台运营若还没首登，脚本按要求改密：新密码生成后写进 `--operator-new-password-file`（0600，不打印），之后用这个文件登录。

- 首登完成后，删除初始密码的 Secret（第 5 节）。

- **就绪与镜像。** 只读确认以下几项，并核对 Deployment 和 Job 里没有 `:latest`：
  - 两个 Pod Ready；
  - 镜像按 digest 引用；
  - `runAsNonRoot` 且 UID 非 0；
  - `readOnlyRootFilesystem: true`。

- **从外部探测外壳。** 网关只对接受 HTML 的 GET / HEAD 回落到外壳的 `index.html`。curl 缺省的 `Accept: */*` 得到 404（`application/problem+json`），不代表外壳不可用。探测首页要带 `-H 'Accept: text/html'`，或者取 `/app-config.json`。

- **登录、开户与租户隔离（E2E）。**

  1. 平台运营经外壳登录进平台控制台；
  2. 新建管理员账号，开户并激活一个租户；
  3. 该租户管理员登录、选租户后进工作台；
  4. 另开一个租户，它的管理员读第一个租户的摘要得到 403。

  同一流程在集成测试 `PlatformIdentityHostTests` 里对生产入口跑通。

- **自助申请（D4，R150 ④）。** `POST /api/signup/verifications` 返回 503 `SIGNUP_UNAVAILABLE`，运营开户照常。staging 没有发件实现，申请到审核通过这一段由集成测试覆盖。

- **没有开发期内容。**
  - `/__dev/` 下的路由都不是 200；
  - 平台 profile 没有开发种子，也没有开发期发件箱；
  - 运行期连接串只用受限登录（`rolsuper`、`rolbypassrls` 都为假）；
  - 凭据只经 Secret 进入：在线副本与平台镜像 Job 以 Secret 卷挂载，建角色 Job 用 `secretKeyRef` 环境变量（唯一的例外）；ConfigMap 与清单里没有明文凭据。

- **迁移、审计链与超管旁路。**
  - Job 完成时间早于在线副本启动；
  - 平台宿主日志里审计与授权条目已就绪；
  - 运营接口的超管旁路放行后，平台审计链出现对应记录（`qian.authz` 的平台链）。

## 7 回退

- **只换回应用版本。** 在部署仓库把 `images` 段的 digest 改回上一版（git revert 那次写回），合并后同步；然后对上一版平台镜像重跑系统组件修订 Job。上一版的修订仍在 Lifecycle 里时，Job 会把它切回激活；两版之间有 system 级组件的文件夹变了的（不论版本号升没升），S1-DEP-013 落地之前回退同样走不通，按下面「同版本换摘要」处理。
  - 迁移只向前：上一版能跑在新表结构上时才能直接回退。
  - 离线 DDL 不做跨版本升级，所以首个版本之后凡是改表结构的部署，都要在迁移 Job 前先备份：在 `qian-postgres` Pod 里用 `pg_dump`，备份放到集群节点之外。
- **同版本换摘要（清库重来）。** 新镜像里有 system 级组件的文件夹变了（第 3 节第 5 步的检查报出组件）时，组件版本规则改好之前（R167：S1-DEP-013 落地之前），staging 清库重来（产品负责人已确认这一做法），不论版本号升没升，每次都要先经产品负责人确认：

  - 版本号没变：系统组件修订 Job 在已有数据的库上必然失败；钩子失败会中止同步，工作负载不变。
  - 版本号升了：Job 会成功，但已有租户对该组件的全部权限点都被拒绝（第 9 节）。这种情况不会自己失败，所以清库要排在同步之前做。

  步骤：

  1. 用 `pg_dump` 备份，备份放到集群节点之外；
  2. 删除 `qian` 库里全部 `plugin_qian_*` schema，库、角色与 Redis 保留；
  3. 重新做一次完整同步：迁移 Job 从空库首装，修订 Job 全新安装并激活；
  4. 带 `--app` 重跑第 4 节第 1 步的 Secret 脚本，它只补回初始密码的 Secret；按第 5 节重新引导平台运营；
  5. 跑 E2E（首登改密），然后删除初始密码的 Secret 与引导 Job。

  staging 上的账号、租户与审计链随之清空。
- **撤掉整套应用。** 停止同步，再 revert 部署仓库里本任务新增的应用清单：两个 Deployment、Job、ConfigMap、Ingress、应用的网络策略。按基线手册，Namespace、PVC 与基线 Secret 一律不删。边缘主机上撤掉两个 vhost 时，`deploy.sh` 不会删除 live 目录里多出的文件：先在部署仓库删掉两个文件，再在边缘主机上手工删除 `/etc/nginx/conf.d/` 里对应的两个文件，`nginx -t` 后 reload。
- **数据恢复。** 用迁移前的 `pg_dump` 恢复；没有备份时只能清库重来（staging 可以接受，但要先经产品负责人确认）。
- **签名钥泄露或丢失。** 在构建主机上重新生成钥对，换掉信任根公钥的 Secret，对当前镜像重签并重跑系统组件修订 Job。旧修订的签名不会被重新核验；正式的信任根轮换归 S1-DEP-005。

## 8 相对 PRD 7.17 的已知差异

这些差异都是过渡形状的取舍，不计入 7.17 的验收，由 S1-DEP-005 / S1-DEP-008 收口。

| 方面      | PRD 7.17                                                                                                               | staging 首次部署                                                                                            |
| ------- | ---------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| 镜像形状    | 三个 profile 镜像：`qian-platform`（JIT，含 `shell` 与 `model-gateway` 组合包）、`qian-identity`（JIT）、`qian-gateway`（AOT distroless） | 两个镜像：`qian-platform`（JIT，平台加身份分支，认证页族与公开页在 `/app/web`）与 `qian-gateway`（NativeAOT）；没有 `qian-identity` 镜像 |
| 外壳制品    | 随 `qian-platform` 镜像发布、经网关提供                                                                                           | 直接打进 `qian-gateway` 镜像（`/app/web/shell` 与远程），由网关托管                                                      |
| 供应链     | SBOM（Syft）、漏洞扫描、Cosign 签名，镜像与插件包推 Harbor                                                                               | 只构建并推送 Harbor，按 digest 引用；没有 SBOM、扫描与 Cosign 签名（S1-DEP-008）                                             |
| 发布链     | 不可变发布事件 → 验签 → 构建推送 → 迁移 Job 先行 → `kustomize edit set image` 写回 → Argo 收敛 → smoke，中间无人工步骤                              | 构建主机上手工构建；digest 经部署仓库 PR 手工写回；Job 与同步逐步人工确认执行；smoke 人工跑                                                |
| 部署物料    | `qian pack` 从清单与 profile 生成 Kustomize 片段                                                                               | 部署仓库手写（ADR-017 §10）                                                                                     |
| 配置      | Apollo（引导项由 Kustomize 注入），环境 overlay 由 ConfigMap 挂载                                                                    | 不连 Apollo：网关 `Config:Source=host`，平台读宿主配置根（`qian.config`）；环境 overlay 照旧由 ConfigMap 挂载                   |
| 消息与依赖   | RabbitMQ（quorum queue）、Apollo、MySQL；四依赖从真实宿主验收                                                                         | 只有 PG 与 Redis；events 不带 broker，Outbox 用 skip-locked                                                     |
| 迁移      | 迁移 Job 先行，取 advisory lock                                                                                              | 迁移 Job 先行，由单实例 Job 保证串行；Tenancy、Authz、Audit、Registry 由离线 DDL 首装，不做跨版本升级                                 |
| 系统组件信任根 | 发布链签名                                                                                                                  | staging 专用 Ed25519 钥，私钥只在构建主机（R151 ①）；签名清单经 ConfigMap 交给安装 Job                                          |
| 身份部署    | 独立 `identity` profile 与进程                                                                                              | 与平台同进程（R143）；授权变更经进程内端口递增身份修订（S1-DEC-027）                                                               |
| 高可用     | `qian-platform` 至少两副本、PDB、kill 一个副本不漏 5xx                                                                              | 每个 Deployment 一个副本，Recreate；数据保护密钥在临时卷，重启要重新登录                                                          |
| 自助申请    | 发件实现可用                                                                                                                 | D4：只开运营开户，发验证码 503（R150 ④）                                                                              |
| 配置漂移门禁  | 镜像内 `qian --dump-config` 与 git 合成结果一致                                                                                  | 本次不跑（随 S1-DEP-008）                                                                                      |

## 9 已知限制

- 平台与网关的数据保护密钥都放在临时卷：Pod 重建后，账号 Cookie 与网关会话解不开，用户要重新登录。多副本之前要改成共享目录。
- 网关的转发头只回溯一跳（Kun 网关 `ForwardLimit` 固定为 1）。
- Traefik 的代理信任清单包含节点网段与 Pod 网段；平台身份分支把直连的 Traefik Pod 与 WireGuard 一跳当作可信代理，按两跳解析转发头。
- 基线给 `qian-platform`、`qian-postgres`、`qian-redis` 都预置了 `minAvailable: 1` 的 PDB，而它们都只有一个副本：`kubectl drain` 数据节点会一直等待。节点维护前先临时放宽或删除这三个 PDB，维护后恢复。
- 网关的就绪依赖边缘主机与身份发行方：边缘主机或身份服务故障时，网关被摘出 Service 端点，外壳整体不可用，连静态页也打不开。要改需要 Qian 侧调整就绪检查的口径。
- 网关入口（`hosts/Kun.Gateway.Host/Program.cs`）没有像平台宿主那样拦截未处理异常：依赖挂载失败时，容器里的 PID 1 可能空转不退出，要等 90 秒启动探针超时才被重启。
- 新 Pod 的地址要等 K3s 网络策略控制器写进 PG 的放行集合后才连得上，在此之前连接被拒（Connection refused），刚启动就连库的 Job 会因此失败。三个发布 Job 与引导模板连 PG 前先用 `pg_isready` 等它可达，至多约 60 秒；实际通常重试一次就连上。两个 Deployment 不加等待：部署仓库门禁要求 Deployment 只用自身镜像，且每个容器都带三类探针。首次连库失败靠 kubelet 重启恢复，例如网关首启时连 Redis 被拒（`RedisConnectionException`），进程退出后重启即恢复。
- 环境 overlay 不给 events 配 broker，宿主就不登记 Outbox 投递服务（平台宿主只在 events 条目配了 `broker` 时，才随 `AddKunEventsBroker` 一并调用 `AddKunOutboxRelay`）。各组件 `outbox_messages` 里的集成事件只写不发：首次同步后就有 `plugin.activated` 事件停在待发状态。同进程形状里没有跨进程订阅者，功能不受影响，但这些表会随开户等操作一直增长。接 RabbitMQ 之前，接受这种增长或定期清理。
- 平台镜像的三个 Job 也挂身份服务的签名、加密证书与客户端密钥（宿主装配身份分支时就读取）；修订 Job 与引导模板另挂在线登录的连接串，迁移 Job 不挂。
- K3s 的 Secret 静态加密（`--secrets-encryption`）状态要用 root 核对，部署后检查里没有这一项。
- system 级组件的文件夹一变，已有数据的 staging 只能清库重来（第 7 节「同版本换摘要」）；摘要不变的重签（例如签名清单只换 source）可以原地进行。变没变，部署前用第 3 节第 5 步的检查看。
  - 同一版本不能换摘要（第 4 节第 5 步）。
  - 只升版本也不行：修订 Job 会成功，但已有租户的授权策略与开通记录停在旧版本，没有生产路径把它们升上来，这些租户对该组件的全部权限点都会被拒绝（S1-DEC-037）。按 R167，门禁与护栏归 S1-DEP-012，租户对齐归 S1-DEP-013；S1-DEP-013 落地前照第 7 节处理。
  - 平台宿主每个组件只预装一个版本，挂载时的契约准入找不到基线，升版本时既不做契约兼容比较、也不拦；宿主没有装配 OpenAPI / proto 兼容检查器，这一点在升版本时不起作用。
