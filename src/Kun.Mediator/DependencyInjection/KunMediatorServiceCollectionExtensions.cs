using Microsoft.Extensions.DependencyInjection;

namespace Kun.Mediator.DependencyInjection;

/// <summary>
/// Extension methods for registering <see cref="KunMediator"/> with an
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class KunMediatorServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="KunMediator"/> and all handlers/behaviors configured via
    /// <paramref name="configure"/> to the service collection.
    /// </summary>
    /// <remarks>
    /// May be called more than once on the same service collection: the first call creates the
    /// <see cref="KunMediatorBuilder"/> and registers <see cref="KunMediator"/>, <see cref="ISender"/> and
    /// <see cref="IPublisher"/>; later calls configure the same builder, so registrations from several
    /// modules stack into one mediator. Registering a second handler for a request that already has one throws.
    /// A collection whose descriptors were copied from another one gets its own builder.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">
    /// A delegate that configures the <see cref="KunMediatorBuilder"/> with request handlers,
    /// pipeline behaviors, and notification handlers.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddKunMediator(
        this IServiceCollection services,
        Action<KunMediatorBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        // The marker may have been copied into another service collection with the rest of the descriptors;
        // only reuse a builder that belongs to this collection.
        var builder = services.LastOrDefault(d => d.ServiceType == typeof(MediatorRegistration) && !d.IsKeyedService)?.ImplementationInstance
            is MediatorRegistration existing && ReferenceEquals(existing.Builder.Services, services) ? existing.Builder : null;
        if (builder is null)
        {
            builder = new KunMediatorBuilder(services);
            var registered = builder;
            services.AddSingleton(new MediatorRegistration(registered));
            services.AddSingleton<KunMediator>(sp =>
                registered.Build(sp.GetRequiredService<IServiceScopeFactory>()));
            services.AddSingleton<ISender>(sp => sp.GetRequiredService<KunMediator>());
            services.AddSingleton<IPublisher>(sp => sp.GetRequiredService<KunMediator>());
        }

        configure(builder);
        return services;
    }

    /// <summary>Marks the builder that the first <see cref="AddKunMediator"/> call created for this service collection.</summary>
    private sealed class MediatorRegistration(KunMediatorBuilder builder)
    {
        public KunMediatorBuilder Builder { get; } = builder;
    }
}
