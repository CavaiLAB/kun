using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace Kun.Integration.AotTests;

/// <summary>
/// 网关的独立宿主 <c>hosts/Kun.Gateway.Standalone</c> 按本机 RID 原生发布，起来后请求健康检查、JWKS 与一条要认证的代理路由。
/// AOT 冒烟宿主整体扎根的是网关的库；JwtBearer、gRPC-Web、健康检查与可观测性这些只有宿主才用到的部分，
/// 要宿主自己原生发布才会过 ILC。
/// </summary>
public sealed class GatewayStandaloneAotPublishTests
{
    private static readonly Regex IlDiagnostic = new(@"\b(?:warning|error) IL\d{4}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Kestrel 绑定系统分配的端口之后，启动日志里写出实际地址。
    private static readonly Regex Listening = new(@"Now listening on: (?<url>http://127\.0\.0\.1:\d+)", RegexOptions.Compiled);

    [Fact]
    public async Task StandaloneHost_NativePublish_ServesHealthAndJwks_AndRejectsAnonymousProxyRoute()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
        {
            Assert.Skip("原生发布并运行只在 Linux 与 macOS 上做；CI 的 sample-aot 作业在 Linux 上运行它。");
            return;
        }

        var ct = TestContext.Current.CancellationToken;
        var repository = LocateRepository();
        var directory = Path.Combine(Path.GetTempPath(), $"kun-gateway-standalone-aot-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();
        Process? host = null;
        try
        {
            // 隔离的 obj/bin，不吃仓库里普通构建的产物，也不与并行的用例互相覆盖。
            var publish = Path.Combine(directory, "publish");
            var published = await RunAsync(repository, TimeSpan.FromMinutes(10),
                "publish", Path.Combine(repository, "hosts", "Kun.Gateway.Standalone", "Kun.Gateway.Standalone.csproj"),
                "-c", "Release", "-r", RuntimeInformation.RuntimeIdentifier, "--artifacts-path", Path.Combine(directory, "artifacts"),
                "-o", publish, "-p:IlcTreatWarningsAsErrors=true", "-p:TrimmerSingleWarn=false",
                "-p:TreatWarningsAsErrors=true", "-p:UseSharedCompilation=false", "-m:1");
            IlDiagnostic.Matches(published.Output).Should().BeEmpty("独立宿主的原生发布不允许任何 IL 诊断。输出：\n{0}", published.Output);
            published.ExitCode.Should().Be(0, "独立宿主的原生发布必须成功。输出：\n{0}", published.Output);
            var binary = Path.Combine(publish, "Kun.Gateway.Standalone");
            File.Exists(binary).Should().BeTrue("发布目录里要有原生可执行文件");
            File.Exists(binary + ".dll").Should().BeFalse("执行的是原生宿主，不是托管启动器");

            using var key = RSA.Create(2048);
            var privateKey = Path.Combine(directory, "private.pem");
            var publicKey = Path.Combine(directory, "public.pem");
            await File.WriteAllTextAsync(privateKey, key.ExportRSAPrivateKeyPem(), ct);
            await File.WriteAllTextAsync(publicKey, key.ExportSubjectPublicKeyInfoPem(), ct);

            var listening = new TaskCompletionSource<Uri>(TaskCreationOptions.RunContinuationsAsynchronously);
            var start = new ProcessStartInfo(binary)
            {
                WorkingDirectory = publish, // 内容根是发布目录：读宿主自己的 appsettings.json（路由与集群的样板）
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            start.Environment["ASPNETCORE_URLS"] = "http://127.0.0.1:0";
            start.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
            start.Environment["Gateway__SigningKeyPath"] = privateKey;
            start.Environment["Gateway__PublicKeyPath"] = publicKey;
            start.Environment["KunObservability__TracingEnabled"] = "false";
            start.Environment["KunObservability__MetricsEnabled"] = "false";
            start.Environment["KunObservability__LoggingEnabled"] = "false";
            host = new Process { StartInfo = start };
            host.OutputDataReceived += (_, e) =>
            {
                if (e.Data is null) return;
                lock (stdout) stdout.AppendLine(e.Data);
                var match = Listening.Match(e.Data);
                if (match.Success) listening.TrySetResult(new Uri(match.Groups["url"].Value));
            };
            host.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is null) return;
                lock (stderr) stderr.AppendLine(e.Data);
            };
            host.Start();
            host.BeginOutputReadLine();
            host.BeginErrorReadLine();

            var exited = host.WaitForExitAsync(ct);
            var first = await Task.WhenAny(listening.Task, exited, Task.Delay(TimeSpan.FromSeconds(30), ct));
            first.Should().BeSameAs(listening.Task, "宿主要在 30 秒内开始监听。{0}", Output(stdout, stderr));
            using var client = new HttpClient(new SocketsHttpHandler { UseProxy = false })
            {
                BaseAddress = await listening.Task,
                Timeout = TimeSpan.FromSeconds(10)
            };

            // 开始监听之后，健康检查有上限地轮询到 200 才算就绪。
            var healthz = HttpStatusCode.ServiceUnavailable;
            for (var deadline = DateTime.UtcNow.AddSeconds(20); DateTime.UtcNow < deadline && healthz != HttpStatusCode.OK && !host.HasExited;)
            {
                try
                {
                    using var response = await client.GetAsync("/healthz", ct);
                    healthz = response.StatusCode;
                }
                catch (Exception exception) when (exception is HttpRequestException || (exception is TaskCanceledException && !ct.IsCancellationRequested))
                {
                    // 还没就绪：接着等，等到上限为止。
                }
                if (healthz != HttpStatusCode.OK) await Task.Delay(200, ct);
            }
            healthz.Should().Be(HttpStatusCode.OK, "/healthz 要在 20 秒内返回 200。{0}", Output(stdout, stderr));

            using (var readyz = await client.GetAsync("/readyz", ct))
                readyz.StatusCode.Should().Be(HttpStatusCode.OK, "签名密钥可用时 /readyz 是 200。{0}", Output(stdout, stderr));

            using (var jwks = await client.GetAsync("/.well-known/jwks.json", ct))
            {
                jwks.StatusCode.Should().Be(HttpStatusCode.OK, "JWKS 匿名可达。{0}", Output(stdout, stderr));
                jwks.Headers.CacheControl?.NoStore.Should().BeTrue("JWKS 的成功响应带 no-store");
                using var document = JsonDocument.Parse(await jwks.Content.ReadAsStringAsync(ct));
                var keys = document.RootElement.GetProperty("keys").EnumerateArray().ToList();
                keys.Should().ContainSingle("只配了一把签名密钥");
                keys[0].GetProperty("kty").GetString().Should().Be("RSA");
                keys[0].TryGetProperty("d", out _).Should().BeFalse("JWKS 不能带私钥参数");
            }

            using (var proxied = await client.GetAsync("/api/catalog/products", ct))
                proxied.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "代理路由缺省要认证，匿名请求不转发。{0}", Output(stdout, stderr));

            host.HasExited.Should().BeFalse("请求过程中宿主不能退出。{0}", Output(stdout, stderr));
        }
        finally
        {
            if (host is not null)
            {
                try
                {
                    if (!host.HasExited) host.Kill(entireProcessTree: true);
                    await host.WaitForExitAsync(CancellationToken.None);
                }
                catch (InvalidOperationException)
                {
                    // 进程没起来：没有要结束的。
                }
                host.Dispose();
            }
            Directory.Delete(directory, recursive: true);
        }
    }

    private static string Output(StringBuilder stdout, StringBuilder stderr)
    {
        string output, error;
        lock (stdout) output = stdout.ToString();
        lock (stderr) error = stderr.ToString();
        return error.Length == 0 ? $"\n标准输出：\n{output}" : $"\n标准错误：\n{error}\n标准输出：\n{output}";
    }

    private static string LocateRepository()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "Kun.slnx"))) return directory.FullName;
        throw new InvalidOperationException("Cannot locate Kun.slnx");
    }

    private static async Task<(int ExitCode, string Output)> RunAsync(string directory, TimeSpan timeout, params string[] arguments)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        start.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        start.Environment["DOTNET_CLI_USE_MSBUILD_SERVER"] = "0";
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Cannot start dotnet");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.CancelAfter(timeout);
        try
        {
            await process.WaitForExitAsync(cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync(CancellationToken.None);
            throw new TimeoutException($"dotnet {string.Join(' ', arguments)} 未在 {timeout} 内完成或测试已取消。\n{await stdout}\n{await stderr}");
        }
        return (process.ExitCode, await stdout + Environment.NewLine + await stderr);
    }
}
