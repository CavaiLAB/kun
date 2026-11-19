using Kun.Core.MultiTenancy;

var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:0");
await using var app = builder.Build();
app.MapGet("/health", (HttpContext context) =>
    context.Response.WriteAsync(NullTenantContext.Instance.IsMultiTenant ? "tenant" : "ready"));
await app.StartAsync();
using var client = new HttpClient();
var actual = await client.GetStringAsync(app.Urls.Single() + "/health");
await app.StopAsync();
if (actual != "ready") return 1;
Console.WriteLine("Kun kernel consumer ready");
return 0;
