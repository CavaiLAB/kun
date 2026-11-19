using System.Linq.Expressions;
using Kun.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Kun.EntityFrameworkCore.MultiTenancy;

/// <summary>
/// <see cref="ModelBuilder"/> extensions that attach the Kun multi-tenant
/// global query filter to every <see cref="IMultiTenant"/> entity.
/// </summary>
public static class KunMultiTenancyModelBuilderExtensions
{
    /// <summary>
    /// Attaches the tenant predicate to the executing DbContext so a shared model never reads
    /// a previous context's tenant accessor. Call with <c>modelBuilder.ApplyKunMultiTenantFilters(this)</c>.
    /// </summary>
    public static ModelBuilder ApplyKunMultiTenantFilters<TContext>(this ModelBuilder modelBuilder, TContext context)
        where TContext : DbContext, IKunMultiTenantDbContext
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentNullException.ThrowIfNull(context);
        var currentTenantId = Expression.Property(
            Expression.Convert(Expression.Constant(context, typeof(TContext)), typeof(IKunMultiTenantDbContext)),
            nameof(IKunMultiTenantDbContext.TenantIdForModelCacheKey));
        return ApplyQueryFilters(modelBuilder, currentTenantId, softDelete: false);
    }

    /// <summary>
    /// The one place that builds Kun's global query filters. Every <see cref="IMultiTenant"/> entity gets
    /// <c>e.TenantId == currentTenantId</c>; with <paramref name="softDelete"/>, every <see cref="ISoftDelete"/>
    /// entity also gets <c>!e.IsDeleted</c>, merged into the same predicate because <c>HasQueryFilter</c>
    /// replaces any earlier filter. <see cref="KunDbContext"/> passes its own member access as
    /// <paramref name="currentTenantId"/> and turns soft delete on.
    /// </summary>
    internal static ModelBuilder ApplyQueryFilters(ModelBuilder modelBuilder, Expression currentTenantId, bool softDelete)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entity.ClrType;
            var isMultiTenant = typeof(IMultiTenant).IsAssignableFrom(clrType);
            var isSoftDelete = softDelete && typeof(ISoftDelete).IsAssignableFrom(clrType);
            if (!isMultiTenant && !isSoftDelete)
            {
                continue;
            }

            var parameter = Expression.Parameter(clrType, "e");
            Expression? predicate = null;
            if (isMultiTenant)
            {
                var tenantIdProp = Expression.Property(parameter, nameof(IMultiTenant.TenantId));
                predicate = Expression.Equal(tenantIdProp, currentTenantId);
            }

            if (isSoftDelete)
            {
                var notDeleted = Expression.Not(Expression.Property(parameter, nameof(ISoftDelete.IsDeleted)));
                predicate = predicate is null ? notDeleted : Expression.AndAlso(predicate, notDeleted);
            }

            modelBuilder.Entity(clrType).HasQueryFilter(Expression.Lambda(predicate!, parameter));
        }

        return modelBuilder;
    }
}
