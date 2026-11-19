using System.Reflection;
using Kun.Runtime;

namespace Kun.Plugin.Sdk.Tests;

/// <summary>
/// 按清单工具同样的入口取源生成器产出的元数据工厂：程序集上的 <see cref="KunCodeMetadataAttribute"/>。
/// 工厂的类名照生成器的约定是「组件类型名 + KunCodeMetadata」，这里一并核对。源码里不直接写生成的类名：
/// 格式门禁只还原不构建，它的工作区里没有源生成器的产物。
/// </summary>
internal static class GeneratedMetadata
{
    internal static ComponentCodeMetadata For<TPlugin>() where TPlugin : IKunPlugin
    {
        var factory = typeof(TPlugin).Assembly.GetCustomAttributes<KunCodeMetadataAttribute>()
            .Select(attribute => attribute.FactoryType)
            .Single(type => type.Name == typeof(TPlugin).Name + "KunCodeMetadata");
        return ((IComponentCodeMetadataFactory)Activator.CreateInstance(factory)!).Create();
    }
}
