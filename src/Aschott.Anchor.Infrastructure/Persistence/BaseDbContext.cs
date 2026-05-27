using Aschott.Anchor.Application.MultiTenancy;
using Aschott.Anchor.Application.UnitOfWork;
using Aschott.Anchor.Domain.Auditing;
using Aschott.Anchor.Domain.Entities;
using Aschott.Anchor.Infrastructure.Persistence.Conventions;
using Microsoft.EntityFrameworkCore;

namespace Aschott.Anchor.Infrastructure.Persistence;

public abstract class BaseDbContext(DbContextOptions options, ICurrentTenant currentTenant)
    : DbContext(options), IApplicationDbContext, IUnitOfWork, ITenantAwareDbContext
{
    protected ICurrentTenant CurrentTenant => currentTenant;

    /// <summary>
    /// Per-query view of <see cref="ICurrentTenant.Id"/>. Referenced by the
    /// multi-tenant global query filter — EF's funcletizer treats member access
    /// on a <see cref="DbContext"/> instance as a query parameter that is
    /// re-evaluated per query execution. Do not constant-fold this property at
    /// model build time; reading it must hit the live accessor every time the
    /// filter is evaluated.
    /// </summary>
    public Guid? CurrentTenantId => currentTenant.Id;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyAuditConventions();
        modelBuilder.ApplyMultiTenantFilters(this);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        ApplyTenantStamp();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditFields()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<IAuditedObject>())
        {
            if (entry.State == EntityState.Added)
                entry.Property(nameof(IAuditedObject.CreatedAt)).CurrentValue = now;
            if (entry.State == EntityState.Modified)
                entry.Property(nameof(IAuditedObject.UpdatedAt)).CurrentValue = now;
        }
    }

    private void ApplyTenantStamp()
    {
        foreach (var entry in ChangeTracker.Entries()
                     .Where(e => e.Entity is IMultiTenant && e.State == EntityState.Added))
        {
            if (entry.CurrentValues[nameof(IMultiTenant.TenantId)] is null && currentTenant.Id is not null)
                entry.CurrentValues[nameof(IMultiTenant.TenantId)] = currentTenant.Id;
        }
    }
}
