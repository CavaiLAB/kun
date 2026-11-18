using System.Reflection;

// AOT 冒烟宿主（S1-KUN-020）。真正的检查在发布时：ILC 对清单里的每个程序集整体扎根分析，有任何裁剪或 AOT 警告即失败。
// 运行时再核对一遍：清单里的程序集都编进了原生映像，然后正常退出。
var missing = new List<string>();
foreach (var name in AotSmokeAssemblies.Names)
{
    try
    {
        Assembly.Load(new AssemblyName(name));
    }
    catch (FileNotFoundException)
    {
        missing.Add(name);
    }
}

if (missing.Count > 0)
{
    Console.Error.WriteLine($"AOT 冒烟失败：原生映像里缺 {missing.Count} 个程序集：{string.Join("、", missing)}");
    return 1;
}

Console.WriteLine($"AOT 冒烟通过：清单里的 {AotSmokeAssemblies.Names.Length} 个程序集都在原生映像里。");
return 0;
