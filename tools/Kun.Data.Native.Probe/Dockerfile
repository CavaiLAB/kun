FROM mcr.microsoft.com/dotnet/sdk:10.0-noble-aot@sha256:7d223c2b945173edb8c73e5f5d48a64e7cd0ca632ccf95170b857e8eea78913c
WORKDIR /src
# 构建上下文是仓库根（S1-KUN-020）：本目录的项目从源码编译仓库根 src/ 下的 Kun 项目。
# 构建命令见 tools/Kun.Data.Native.Probe/README.md（在仓库根运行，上下文 .）；下面的路径相对仓库根。
COPY . .
ARG TARGETARCH
RUN case "$TARGETARCH" in arm64) native_data_rid=linux-arm64 ;; amd64) native_data_rid=linux-x64 ;; *) exit 2 ;; esac && \
    dotnet publish tools/Kun.Data.Native.Probe -c Release -r "$native_data_rid" -p:IlcTreatWarningsAsErrors=true -p:TrimmerSingleWarn=false -o /probe && \
    test -x /probe/Kun.Data.Native.Probe && test ! -e /probe/Kun.Data.Native.Probe.dll
ENTRYPOINT ["/probe/Kun.Data.Native.Probe"]
