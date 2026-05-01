using System.Linq.Expressions;
using System.Reflection;
using Aschott.Anchor.Application.MultiTenancy;
using Aschott.Anchor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aschott.Anchor.Infrastructure.Persistence.Conventions;

public static class MultiTenantQueryFilters
{
    private static readonly MethodInfo BuildFilterMethod =
        typeof(MultiTenantQueryFilters).GetMethod(
            nameof(BuildFilter),
            BindingFlags.Static | BindingFlags.NonPublic)!;

    /// <summary>
    /// Applies a query filter to every <see cref="IMultiTenant"/> entity:
    /// <c>e.TenantId == currentTenant.Id || currentTenant.Id == null</c>.
    /// The null-check provides a host-level bypass for system queries.
    /// The filter is constructed as a C#-compiled lambda closing over
    /// <paramref name="currentTenant"/> so EF Core parameterizes the value
    /// at query time instead of baking it in during model building.
    /// </summary>
    public static void ApplyMultiTenantFilters(this ModelBuilder modelBuilder, ICurrentTenant currentTenant)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentNullException.ThrowIfNull(currentTenant);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(e => typeof(IMultiTenant).IsAssignableFrom(e.ClrType)))
        {
            var lambda = (LambdaExpression)BuildFilterMethod
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(null, [currentTenant])!;

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    private static Expression<Func<TEntity, bool>> BuildFilter<TEntity>(ICurrentTenant currentTenant)
        where TEntity : class, IMultiTenant
        => e => e.TenantId == currentTenant.Id || currentTenant.Id == null;
}
