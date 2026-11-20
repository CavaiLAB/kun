namespace Kun.Capabilities.AspNetCore;

internal static class HttpContractTypeName
{
    internal static string For(Type type)
    {
        if (type.ContainsGenericParameters || type.IsPointer || type.IsByRef || type.IsFunctionPointer)
            throw new ArgumentException("HTTP contract schemas require a closed data type.", nameof(type));

        return $"{FullName(type)}, {type.Assembly.GetName().Name}";
    }

    private static string FullName(Type type)
    {
        if (type.IsArray)
        {
            int rank = type.GetArrayRank();
            string shape = type.IsSZArray ? "[]" : rank == 1 ? "[*]" : $"[{new string(',', rank - 1)}]";
            return FullName(type.GetElementType()!) + shape;
        }

        if (type.IsConstructedGenericType)
        {
            var arguments = type.GetGenericArguments().Select(argument => $"[{For(argument)}]");
            return $"{type.GetGenericTypeDefinition().FullName}[{string.Join(",", arguments)}]";
        }

        return type.FullName ?? throw new ArgumentException("The HTTP contract type must have a full name.", nameof(type));
    }
}
