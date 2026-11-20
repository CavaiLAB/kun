using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Kun.Capabilities.AspNetCore;
using Kun.Errors;

namespace Kun.Capabilities.Tests;

public sealed partial class HttpContractTypeNameTests
{
    public static TheoryData<Type> ClosedTypes => new()
    {
        typeof(ErrorDetail),
        typeof(Dictionary<string, ErrorDetail>),
        typeof(List<KeyValuePair<string, int?>>),
        typeof(int?),
        typeof(ErrorDetail[]),
        typeof(ErrorDetail[][]),
        typeof(int[,]),
        typeof(int).MakeArrayType(1),
        typeof(List<ErrorDetail[]>),
        typeof(Outer.Inner),
        typeof(Outer<ErrorDetail>.Inner),
    };

    [Fact]
    public void PlainTypeKeepsItsAssemblySimpleName() =>
        Assert.Equal("Kun.Errors.ErrorDetail, Kun.Errors", HttpContractTypeName.For(typeof(ErrorDetail)));

    [Theory]
    [MemberData(nameof(ClosedTypes))]
    public void ClosedTypesMatchTheClrNameWithoutReleaseMetadata(Type type)
    {
        var name = HttpContractTypeName.For(type);
        Assert.Equal(AssemblyMetadata().Replace(type.AssemblyQualifiedName!, ""), name);
        Assert.DoesNotContain("Version=", name);
        Assert.DoesNotContain("Culture=", name);
        Assert.DoesNotContain("PublicKeyToken=", name);
    }

    [Fact]
    public void RejectsTypesThatCannotDescribeAContract()
    {
        Type generic = typeof(List<>);
        Type[] unsupported = [generic, generic.GetGenericArguments()[0], typeof(int).MakePointerType(), typeof(int).MakeByRefType()];
        foreach (Type type in unsupported)
            Assert.Throws<ArgumentException>(() => HttpContractTypeName.For(type));
    }

    [Fact]
    public void SchemaKeysDistinguishDirectionsAndAssemblies()
    {
        var type = typeof(ErrorDetail);
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("ContractTypeFixture"), AssemblyBuilderAccess.Run);
        var twin = assembly.DefineDynamicModule("Models").DefineType(type.FullName!).CreateType()!;
        var key = AspNetCoreHttpContractSource.SchemaKey("example.catalog", type, false);

        Assert.Equal(type.FullName, twin.FullName);
        Assert.Equal(key, AspNetCoreHttpContractSource.SchemaKey("example.catalog", type, false));
        Assert.NotEqual(key, AspNetCoreHttpContractSource.SchemaKey("example.catalog", type, true));
        Assert.NotEqual(key, AspNetCoreHttpContractSource.SchemaKey("example.catalog", twin, false));
    }

    [GeneratedRegex(@", Version=[^,\]]+, Culture=[^,\]]+, PublicKeyToken=[^,\]]+")]
    private static partial Regex AssemblyMetadata();

    private static class Outer { internal sealed class Inner; }
    private static class Outer<T> { internal sealed class Inner; }
}
