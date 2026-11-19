using FluentAssertions;
using Kun.Authorization;
using Kun.Core.Cqrs;
using Kun.Core.Results;
using Kun.Core.Security;
using Kun.Mediator.Behaviors;
using Kun.Mediator.DependencyInjection;
using Kun.Mediator.Tests.Dispatch;
using Microsoft.Extensions.DependencyInjection;
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
    public void PermissionBehaviorOptions_NullFactory_IsRejectedAtRegistration()
    {
        var services = new ServiceCollection();

        var act = () => services.AddKunMediator(builder => builder
            .AddRequestHandler<ProtectedCommand, Result<int>, ProtectedHandler>()
            .AddPermissionBehavior<ProtectedCommand, Result<int>>(Result<int>.Fail, options => options.PermissionChecker = null!));

        act.Should().Throw<InvalidOperationException>().WithMessage("*PermissionChecker*");
    }
}
