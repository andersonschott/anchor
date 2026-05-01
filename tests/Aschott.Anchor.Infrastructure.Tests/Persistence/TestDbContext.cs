using Aschott.Anchor.Application.MultiTenancy;
using Aschott.Anchor.Domain.Auditing;
using Aschott.Anchor.Domain.Entities;
using Aschott.Anchor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aschott.Anchor.Infrastructure.Tests.Persistence;

public sealed class TestCustomer : MultiTenantEntity<Guid>, IAuditedObject
{
    public string Name { get; private set; } = "";
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public string? UpdatedBy { get; private set; }

    private TestCustomer() { }

    public TestCustomer(Guid id, Guid? tenantId, string name) : base(id, tenantId)
    {
        Name = name;
    }

    public void Rename(string name) => Name = name;
}

public sealed class TestDbContext(DbContextOptions options, ICurrentTenant currentTenant)
    : BaseDbContext(options, currentTenant)
{
    public DbSet<TestCustomer> Customers => Set<TestCustomer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestCustomer>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Name).HasMaxLength(256).IsRequired();
        });
        base.OnModelCreating(modelBuilder);
    }
}
