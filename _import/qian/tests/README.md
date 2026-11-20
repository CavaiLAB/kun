# 平台资源测试

.NET 测试、容器夹具、四个 CI 分片与分析器门禁的运行方式见[测试与门禁](../../../tests/README.md)。本目录保留平台资产的 Python 自测、外壳界面包门禁与浏览器验收。

- 在平台工作区运行 `python3 -m unittest discover -s tests/gates -v` 核对资源与工作流脚本。
- 运行 `pnpm ui:gates` 核对外壳与界面包，运行 `pnpm e2e:directory` 验收目录选择器。
