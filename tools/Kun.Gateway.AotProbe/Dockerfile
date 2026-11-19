FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:747cbade5bfd918a175ac32f8f1d931edec2c7d0d5397a3de72eaef1ac1f66d2
RUN apt-get update && apt-get install --no-install-recommends -y clang zlib1g-dev && rm -rf /var/lib/apt/lists/*
WORKDIR /src
# 构建上下文是仓库根（S1-KUN-020）：本目录的项目从源码编译仓库根 src/ 下的 Kun 项目。
# 构建命令见 tools/Kun.Gateway.AotProbe/README.md；构建上下文和项目路径均相对仓库根。
COPY . .
RUN dotnet publish tools/Kun.Gateway.AotProbe -c Release -r linux-arm64 -p:IlcTreatWarningsAsErrors=true -p:TrimmerSingleWarn=false -o /probe
