using FluentAssertions;
using Kun.Authorization;
using Kun.Core.Cqrs;
using Kun.Core.Results;
using Kun.Core.Security;
using Kun.Mediator.Behaviors;
using Kun.Mediator.DependencyInjection;
using Kun.Mediator.Tests.Dispatch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Kun.Mediator.Tests;

internal sealed class OtherPingHandler : IRequestHandler<Ping, string>
{
    public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(request.Message);
}

public sealed class RegistrationTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static ServiceProviderOptions Strict => new() { ValidateScopes = true, ValidateOnBuild = true };

    [Fact]
    public async Task AddKunMediator_CalledTwice_StacksRegistrationsIntoOneMediator()
    {
        var services = new ServiceCollection();
        services.AddKunMediator(builder => builder.AddRequestHandler<Ping, string, PingHandler>());
        services.AddKunMediator(builder => builder.AddRequestHandler<DirectoryCommand, int, DirectoryHandler>());

        await using var provider = services.BuildServiceProvider(Strict);
        var sender = provider.GetRequiredService<ISender>();

        (await sender.Send(new Ping("hi"), Ct)).Should().Be("HI");
        (await sender.Send(new DirectoryCommand("four"), Ct)).Should().Be(4);
        sender.Should().BeSameAs(provider.GetRequiredService<IPublisher>());
        services.Count(d => d.ServiceType == typeof(ISender)).Should().Be(1);
    }

    [Fact]
    public async Task AddKunMediator_OnACollectionCopiedFromAnother_GetsItsOwnBuilder()
    {
        var first = new ServiceCollection();
        first.AddKunMediator(builder => builder.AddRequestHandler<Ping, string, PingHandler>());
        IServiceCollection copy = new ServiceCollection();
        foreach (var descriptor in first) copy.Add(descriptor);

        copy.AddKunMediator(builder => builder.AddRequestHandler<DirectoryCommand, int, DirectoryHandler>());

        await using var original = first.BuildServiceProvider(Strict);
        var act = async () => await original.GetRequiredService<ISender>().Send(new DirectoryCommand("four"), Ct);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"*{typeof(DirectoryCommand).FullName}*");
        await using var copied = copy.BuildServiceProvider(Strict);
        (await copied.GetRequiredService<ISender>().Send(new DirectoryCommand("four"), Ct)).Should().Be(4);
    }

    [Fact]
    public void AddRequestHandler_SecondHandlerForTheSameRequest_Throws()
    {
        var services = new ServiceCollection();
        services.AddKunMediator(builder => builder.AddRequestHandler<Ping, string, PingHandler>());

        var act = () => services.AddKunMediator(builder => builder.AddRequestHandler<Ping, string, OtherPingHandler>());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{typeof(Ping).FullName}*{typeof(PingHandler).FullName}*{typeof(OtherPingHandler).FullName}*");
    }

    [Fact]
    public void AddRequestHandler_SecondCapabilityHandlerForTheSameRequest_ThrowsBeforeRegisteringTheCapability()
    {
        var services = new ServiceCollection();
        services.AddKunMediator(builder => builder.AddRequestHandler<DirectoryCommand, int, DirectoryHandler>());

        var act = () => services.AddKunMediator(builder => builder.AddRequestHandler<DirectoryCommand, int, DirectoryHandler>(
            "sample:count", DirectoryJsonContext.Default.DirectoryCommand, DirectoryJsonContext.Default.Int32));

        act.Should().Throw<InvalidOperationException>().WithMessage($"*{typeof(DirectoryCommand).FullName}*");
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<ICapabilityRegistry>().GetAll().Should().BeEmpty();
    }

    [Fact]
    public async Task PermissionBehaviorOptions_ReplaceHowTheCheckerAndCurrentUserAreResolved()
    {
        // 服务里没有判定器与当前主体：缺省取法会判拒；经扩展点给的取法生效，判定算法不变。
        var calls = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(calls);
        var user = Substitute.For<ICurrentUser>();
        user.IsAuthenticated.Returns(true);
        user.UserId.Returns("user-1");
        user.PrincipalKind.Returns("user");
        var checker = Substitute.For<IPermissionChecker>();
#pragma warning disable CA2012 // 设置 mock 回调，不消费业务 ValueTask。
        checker.CheckAsync(Arg.Any<PermissionRequest>(), Arg.Any<CancellationToken>()).Returns(info =>
        {
            calls.Add("check:" + info.Arg<PermissionRequest>().Permission);
            return ValueTask.FromResult(new PermissionDecision(true, "test", []));
        });
#pragma warning restore CA2012
        services.AddKunMediator(builder => builder
            .AddRequestHandler<ProtectedCommand, Result<int>, ProtectedHandler>()
            .AddPermissionBehavior<ProtectedCommand, Result<int>>(Result<int>.Fail, options =>
            {
                options.CurrentUser = _ => user;
                options.PermissionChecker = _ => checker;
            }));

        await using var provider = services.BuildServiceProvider(Strict);
        var result = await provider.GetRequiredService<ISender>().Send(new ProtectedCommand(), Ct);

        result.IsSuccess.Should().BeTrue();
        calls.Should().Equal("check:sample:record:write", "handler");
    }

    [Fact]
    public async Task PermissionBehavior_WithoutAChecker_DeniesAndLogsAWarning()
    {
        var calls = new List<string>();
        var sink = new LogSink();
        var services = new ServiceCollection();
        services.AddSingleton(calls);
        services.AddSingleton(sink);
        services.AddSingleton(typeof(ILogger<>), typeof(SinkLogger<>));
        var user = Substitute.For<ICurrentUser>();
        user.IsAuthenticated.Returns(true);
        user.UserId.Returns("user-1");
        user.PrincipalKind.Returns("user");
        services.AddSingleton(user);
        services.AddKunMediator(builder => builder
            .AddRequestHandler<ProtectedCommand, Result<int>, ProtectedHandler>()
            .AddPermissionBehavior<ProtectedCommand, Result<int>>(Result<int>.Fail));

        await using var provider = services.BuildServiceProvider(Strict);
        var result = await provider.GetRequiredService<ISender>().Send(new ProtectedCommand(), Ct);

        result.Error.Code.Should().BeSameAs(Kun.Errors.PlatformErrors.AuthzDenied);
        calls.Should().BeEmpty();
        sink.Entries.Should().ContainSingle(entry => entry.Level == LogLevel.Warning
            && entry.Message.Contains(typeof(ProtectedCommand).FullName!, StringComparison.Ordinal)
            && entry.Message.Contains("no permission checker", StringComparison.Ordinal));
    }

    [Fact]
    public void PermissionBehaviorOptions_NullFactory_IsRejectedAtRegistration()
    {
        var services = new ServiceCollection();

        var act = () => services.AddKunMediator(builder => builder
            .AddRequestHandler<ProtectedCommand, Result<int>, ProtectedHandler>()
            .AddPermissionBehavior<ProtectedCommand, Result<int>>(Result<int>.Fail, options => options.PermissionChecker = null!));

        act.Should().Throw<InvalidOperationException>().WithMessage("*PermissionChecker*");
    }

    private sealed class LogSink
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];
    }

    private sealed class SinkLogger<T>(LogSink sink) : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            lock (sink.Entries) sink.Entries.Add((logLevel, formatter(state, exception)));
        }
    }
}
