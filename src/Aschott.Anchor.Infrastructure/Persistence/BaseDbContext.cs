using Aschott.Anchor.Application.MultiTenancy;
using Aschott.Anchor.Application.UnitOfWork;
using Aschott.Anchor.Domain.Auditing;
using Aschott.Anchor.Domain.Entities;
using Aschott.Anchor.Infrastructure.Persistence.Conventions;
using Microsoft.EntityFrameworkCore;

namespace Aschott.Anchor.Infrastructure.Persistence;

public abstract class BaseDbContext(DbContextOptions options, ICurrentTenant currentTenant)
    : DbContext(options), IApplicationDbContext, IUnitOfWork
{
    protected ICurrentTenant CurrentTenant => currentTenant;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyAuditConventions();
        modelBuilder.ApplyMultiTenantFilters(currentTenant);
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
