FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:747cbade5bfd918a175ac32f8f1d931edec2c7d0d5397a3de72eaef1ac1f66d2
RUN apt-get update && apt-get install --no-install-recommends -y clang zlib1g-dev && rm -rf /var/lib/apt/lists/*
WORKDIR /src
# 构建上下文是仓库根（S1-KUN-020）：本目录的项目从源码编译仓库根 src/ 下的 Kun 项目。
# 构建命令见 tools/Qian.Gateway.AotProbe/README.md（在 _import/qian 下运行，上下文 ../..）；下面的路径相对 _import/qian。
COPY . .
WORKDIR /src/_import/qian
RUN dotnet publish tools/Qian.Gateway.AotProbe -c Release -r linux-arm64 -p:IlcTreatWarningsAsErrors=true -p:TrimmerSingleWarn=false -o /probe
