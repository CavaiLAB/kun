FROM mcr.microsoft.com/dotnet/sdk:10.0-noble-aot@sha256:7d223c2b945173edb8c73e5f5d48a64e7cd0ca632ccf95170b857e8eea78913c
WORKDIR /src
# 构建上下文是仓库根（S1-KUN-020）：本目录的项目从源码编译仓库根 src/ 下的 Kun 项目。
# 在仓库根运行 tools/Kun.Audit.Postgres.Probe/README.md 中的构建命令。
COPY . .
WORKDIR /src
ARG TARGETARCH
RUN case "$TARGETARCH" in arm64) native_audit_rid=linux-arm64 ;; amd64) native_audit_rid=linux-x64 ;; *) exit 2 ;; esac && \
    dotnet publish tools/Kun.Audit.Postgres.Probe -c Release -r "$native_audit_rid" -p:IlcTreatWarningsAsErrors=true -p:TrimmerSingleWarn=false -o /probe && \
    test -x /probe/Kun.Audit.Postgres.Probe && test ! -e /probe/Kun.Audit.Postgres.Probe.dll
ENTRYPOINT ["/probe/Kun.Audit.Postgres.Probe"]
