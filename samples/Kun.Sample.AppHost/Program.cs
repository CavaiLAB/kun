var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure — containers persist across AppHost restarts (Aspire
// ContainerLifetime.Persistent) + named data volumes so state survives restarts
// and subsequent launches attach in ~2s instead of re-pulling/re-initializing.
var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("kun-postgres-data")
    .WithPgAdmin()
    .AddDatabase("kun");

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("kun-rabbitmq-data")
    .WithManagementPlugin();

var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("kun-redis-data");

// Kun services
var catalog = builder.AddProject<Projects.Kun_Sample_Catalog>("catalog")
    .WithReference(postgres, "CatalogDb")
    .WithReference(rabbitmq)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(rabbitmq);

var order = builder.AddProject<Projects.Kun_Sample_Order>("order")
    .WithReference(postgres, "OrderDb")
    .WithEnvironment("CatalogGrpc__Address", catalog.GetEndpoint("https"))
    .WithReference(rabbitmq)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(rabbitmq);

if (builder.Configuration["SampleCredentials"] is { Length: > 0 } credentials)
{
    catalog.WithEnvironment("CatalogAuth__PublicJwksPath", Path.Combine(credentials, "public.jwks"));
    order.WithEnvironment("CatalogGrpc__TokenPath", Path.Combine(credentials, "catalog.token"));
}

var gateway = builder.AddProject<Projects.Kun_Gateway_Standalone>("gateway")
    .WithReference(catalog)
    .WithReference(order);

builder.Build().Run();
