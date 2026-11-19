using Xunit;

namespace Kun.Core.Tests;

// 临时探针：验证「重跑失败的作业」之后，测试分片汇总按最新一次尝试的结果核对。第 1 次尝试必败，重跑必过；验证完删掉。
public class RerunProbeTests
{
    [Fact]
    public void FailsOnlyOnTheFirstAttempt()
    {
        Assert.NotEqual("1", Environment.GetEnvironmentVariable("GITHUB_RUN_ATTEMPT"));
    }
}
