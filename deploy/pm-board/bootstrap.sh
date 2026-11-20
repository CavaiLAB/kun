#!/usr/bin/env bash
# 任务看板上 K3s：一次性初始化 / 状态核验。
#
#   bash deploy/pm-board/bootstrap.sh                     # 本机有 kubectl
#   bash deploy/pm-board/bootstrap.sh --kube-ssh qd        # kubectl 在 ssh 主机 qd 上
#   bash deploy/pm-board/bootstrap.sh --check [--kube-ssh qd]   # 只核验：工作负载、看板是否与仓库 main 一致
#
# 看板经 HTTPS 匿名拉取公开仓库 CavaiLAB/kun，不需要 deploy key 与 Secret。
# 平时由 ArgoCD 托管（Application 在部署仓库里），初始化只在重建集群时用；幂等。
set -euo pipefail

REPO="CavaiLAB/kun"
NS="qian-tools"
HERE="$(cd "$(dirname "$0")" && pwd)"
KUBE_SSH=""; CHECK=0
while [[ $# -gt 0 ]]; do
  case "$1" in
    --kube-ssh) KUBE_SSH="$2"; shift 2 ;;
    --check) CHECK=1; shift ;;
    -h|--help) sed -n '2,9p' "$0"; exit 0 ;;
    *) echo "未知参数：$1" >&2; exit 2 ;;
  esac
done

# 远端或本地执行 kubectl / shell
rsh() { if [[ -n "$KUBE_SSH" ]]; then ssh -o BatchMode=yes "$KUBE_SSH" "$@"; else bash -c "$*"; fi; }
kc()  { rsh "kubectl $* 2>&1 | grep -v 'level=warning' || true"; }

need() { command -v "$1" >/dev/null || { echo "缺少命令：$1" >&2; exit 2; }; }
need git; [[ -n "$KUBE_SSH" ]] && need ssh || need kubectl

echo "== 环境：kubectl 在 ${KUBE_SSH:-本机}"

# ---------- 核验 ----------
check() {
  local ok=1
  echo "-- 工作负载"
  local ready; ready="$(kc -n "$NS" get deploy qian-pm-board -o jsonpath='{.status.readyReplicas}')"
  if [[ "$ready" == "1" ]]; then echo "   ok：1/1 就绪"; else echo "   未就绪（readyReplicas=${ready:-0}）"; ok=0; fi
  echo "-- 同步的仓库与前缀"
  local repo prefix
  repo="$(kc -n "$NS" get deploy qian-pm-board -o jsonpath="'{.spec.template.spec.containers[?(@.name==\"git-sync\")].env[?(@.name==\"GITSYNC_REPO\")].value}'")"
  prefix="$(kc -n "$NS" get deploy qian-pm-board -o jsonpath="'{.spec.template.spec.containers[?(@.name==\"builder\")].env[?(@.name==\"BOARD_PREFIX\")].value}'")"
  echo "   仓库 ${repo:-?}，前缀 ${prefix:-（检出根）}"
  if [[ "$repo" != "https://github.com/${REPO}.git" ]]; then echo "   仓库不是 ${REPO}"; ok=0; fi
  if [[ -n "$prefix" ]]; then echo "   BOARD_PREFIX 应为空，任务位于检出根"; ok=0; fi
  echo "-- 同步到的提交 vs origin/main"
  local synced head
  synced="$(kc -n "$NS" exec deploy/qian-pm-board -c nginx -- readlink /git/current | tr -d '\r' | sed 's#.*/##')"
  head="$(git ls-remote -q "https://github.com/${REPO}.git" refs/heads/main 2>/dev/null | cut -f1 || true)"
  if [[ -n "$synced" && "$synced" == "$head" ]]; then echo "   ok：${synced:0:7}"; else echo "   看板 ${synced:0:7} ≠ main ${head:0:7}（等下一次同步或检查 git-sync 日志）"; ok=0; fi
  echo "-- 生成看板的提交 vs 同步到的提交"
  local built
  built="$(kc -n "$NS" exec deploy/qian-pm-board -c nginx -- readlink /site/current | tr -d '\r')"
  if [[ -n "$built" && "$built" == "$synced" ]]; then echo "   ok：${built:0:7}"; else echo "   生成 ${built:0:7} ≠ 同步 ${synced:0:7}（等几秒再查，或看 builder 容器日志）"; ok=0; fi
  echo "-- 任务源文件（/tasks/ 经 /site/source 取 /git/current/pm/tasks/）"
  local tasks
  tasks="$(kc -n "$NS" exec deploy/qian-pm-board -c nginx -- sh -c "'ls /site/source/pm/tasks | wc -l'" | tr -d '\r ')"
  if [[ "$tasks" =~ ^[0-9]+$ && "$tasks" -gt 0 ]]; then echo "   ok：${tasks} 个文件"; else echo "   取不到任务源文件（${tasks:-无输出}）"; ok=0; fi
  echo "-- 入口"
  local np lb
  np="$(kc -n "$NS" get svc qian-pm-board -o jsonpath='{.spec.ports[0].nodePort}')"
  lb="$(kc -n "$NS" get svc qian-pm-board -o jsonpath='{.status.loadBalancer.ingress[0].ip}')"
  echo "   NodePort ${np:-?}，LoadBalancer ${lb:-未分配}，Ingress $(grep -m1 'host:' "$HERE/ingress.yaml" | awk '{print $3}')"
  [[ $ok == 1 ]] && echo "== 核验通过" || { echo "== 核验未通过"; return 1; }
}

if [[ $CHECK == 1 ]]; then check; exit $?; fi

# ---------- 初始化 ----------
echo "1/2 应用清单（含命名空间）"
if [[ -n "$KUBE_SSH" ]]; then kubectl kustomize "$HERE" | ssh -o BatchMode=yes "$KUBE_SSH" 'kubectl apply -f -' 2>&1 | grep -v 'level=warning'
else kubectl apply -k "$HERE"; fi
kc -n "$NS" rollout status deploy/qian-pm-board --timeout=180s

echo "2/2 核验"
check || true
echo "之后交给 ArgoCD：参照 argocd-application.example.yaml 在部署仓库里加 Application（公开仓库，不需要仓库凭据）。"
