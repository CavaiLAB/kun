using System.Text.Json;
using Kun.Docs;
using Kun.Docs.Abstractions;
using Kun.Profiles;
using Kun.Profiles.Abstractions;
using Kun.Surfaces;

// CLI 仅发送结构化输入；宿主把实际 Provider 组合在这里，组件库不依赖 Provider。
var input = JsonSerializer.Deserialize<Input>(await Console.In.ReadToEndAsync(), JsonSerializerOptions.Web)
    ?? throw new InvalidOperationException("缺少生成输入。");
var reader = new SurfaceValidator();
var result = new DocumentGenerator(new ProfileLoader(reader)).Generate(new DocumentOptions
{
    Profile = new ProfileLoadOptions { WorkingDirectory = input.WorkingDirectory, Profile = input.Profile, Patches = input.Patches, HomeDirectory = input.HomeDirectory, IncludeHomePatch = input.IncludeHomePatch },
    SourceDirectories = input.SourceDirectories,
    SourceRoot = input.SourceRoot
});
Console.WriteLine(JsonSerializer.Serialize(result, JsonSerializerOptions.Web));
return result.IsValid ? 0 : 1;

internal sealed record Input(string WorkingDirectory, string Profile, string[] Patches, string? HomeDirectory, bool IncludeHomePatch, string[] SourceDirectories, string? SourceRoot);
