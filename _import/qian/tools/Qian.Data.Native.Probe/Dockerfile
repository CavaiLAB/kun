FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:747cbade5bfd918a175ac32f8f1d931edec2c7d0d5397a3de72eaef1ac1f66d2
RUN apt-get update && apt-get install --no-install-recommends -y clang zlib1g-dev && rm -rf /var/lib/apt/lists/*
WORKDIR /src
# 构建上下文是仓库根（S1-KUN-020）：本目录的项目从源码编译仓库根 src/ 下的 Kun 项目。
# 构建命令见 tools/Qian.Data.Native.Probe/README.md（在 _import/qian 下运行，上下文 ../..）；下面的路径相对 _import/qian。
COPY . .
WORKDIR /src/_import/qian
ARG TARGETARCH
RUN case "$TARGETARCH" in arm64) native_data_rid=linux-arm64 ;; amd64) native_data_rid=linux-x64 ;; *) exit 2 ;; esac && \
    dotnet publish tools/Qian.Data.Native.Probe -c Release -r "$native_data_rid" -p:IlcTreatWarningsAsErrors=true -p:TrimmerSingleWarn=false -o /probe && \
    test -x /probe/Qian.Data.Native.Probe && test ! -e /probe/Qian.Data.Native.Probe.dll
ENTRYPOINT ["/probe/Qian.Data.Native.Probe"]
