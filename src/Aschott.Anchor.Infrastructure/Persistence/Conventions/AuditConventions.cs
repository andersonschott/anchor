using Aschott.Anchor.Domain.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Aschott.Anchor.Infrastructure.Persistence.Conventions;

public static class AuditConventions
{
    public static void ApplyAuditConventions(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(e => typeof(IAuditedObject).IsAssignableFrom(e.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType, builder =>
            {
                builder.Property<DateTime>(nameof(IAuditedObject.CreatedAt)).IsRequired();
                builder.Property<DateTime?>(nameof(IAuditedObject.UpdatedAt));
                builder.Property<string?>(nameof(IAuditedObject.CreatedBy)).HasMaxLength(256);
                builder.Property<string?>(nameof(IAuditedObject.UpdatedBy)).HasMaxLength(256);
            });
        }
    }
}
