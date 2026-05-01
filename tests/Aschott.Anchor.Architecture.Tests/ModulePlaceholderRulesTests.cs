using Xunit;

namespace Aschott.Anchor.Architecture.Tests;

/// <summary>
/// Module-level architectural rules. These activate against module assemblies
/// from F3+ (Tenants, Identity, Auth, Permissions, Audit, Jobs, etc.).
/// In F1 the framework itself is the only code under test, so these rules are
/// recorded as skipped stubs to make their intent and trigger condition
/// auditable.
/// </summary>
public sealed class ModulePlaceholderRulesTests
{
    [Fact(Skip = "Activated when concrete modules introduce CQRS handlers (F3+)")]
    public void Command_and_query_handlers_are_internal_sealed()
    {
        // Rule: every type implementing ICommandHandler<,> or IQueryHandler<,> must be
        // declared `internal sealed` to keep handler discovery a DI-only concern and
        // forbid cross-module handler invocation.
    }

    [Fact(Skip = "Activated when concrete modules introduce commands/queries (F3+)")]
    public void Commands_and_queries_are_records_or_sealed()
    {
        // Rule: every concrete ICommand<>/IQuery<> implementation must be a record or
        // a sealed class — enforces immutability at the request boundary.
    }

    [Fact(Skip = "Activated when concrete modules introduce IEndpoint impls (F3+)")]
    public void Endpoints_live_under_an_Endpoints_namespace()
    {
        // Rule: every IEndpoint implementation must reside under a *.Endpoints.*
        // namespace within its module — keeps route mapping co-located.
    }

    [Fact(Skip = "Activated when concrete modules introduce tenant-scoped aggregates (F3+)")]
    public void Tenant_scoped_aggregates_inherit_MultiTenantEntity()
    {
        // Rule: aggregates that the module documents as tenant-scoped must inherit
        // MultiTenantEntity<TKey> rather than AggregateRoot<TKey> directly so the
        // BaseDbContext query filter applies. Module decides scope via convention
        // (e.g. attribute on the assembly or a marker namespace).
    }

    [Fact(Skip = "Activated when concrete modules raise domain events (F3+)")]
    public void Domain_events_are_records_inheriting_DomainEvent()
    {
        // Rule: every DomainEvent subclass must be a record and must live under the
        // module's Domain.*.Events namespace — enforces immutability of events
        // and keeps them findable by convention.
    }
}
