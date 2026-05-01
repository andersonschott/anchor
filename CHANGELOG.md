# Changelog

All notable changes to this project will be documented in this file. The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- M0: initial repository scaffold, governance files, build infrastructure, CI workflows.
- M0: 26 NuGet IDs reserved as `0.0.1-reserved` (`Aschott.Anchor` + 25 sub-packages).
- M0: SonarCloud project + Quality Gate + PR decoration; CI status checks `build-test` and `license-check` enforced via repository ruleset on `main` and `dev`.
- M0: Linear ↔ GitHub two-way issue sync (team `Aschott`).
- F1: bootstrap `Aschott.Anchor.Domain` skeleton + bootstrap façade `Aschott.Anchor` (ADR 0002 — Option 1).
- F1: `Entity<TKey>` with semantic equality (type + Id), virtual hash, equality operators (ASC-19).
- F1: `DomainEvent` abstract record (Id + OccurredAtUtc UTC) and `AggregateRoot<TKey>` with raise/get/clear domain-event semantics returning a read-only collection (ASC-20).
- F1: `ValueObject` abstract class with structural equality via `GetEqualityComponents()` template method (ASC-21).
- F1: `IMultiTenant` marker (`Guid? TenantId`) and `MultiTenantEntity<TKey> : AggregateRoot<TKey>` with `internal SetTenantId` hook for infra-driven assignment (ASC-22).
- F1: bootstrap `Aschott.Anchor.Contracts` with `IntegrationEvent` abstract record (Id, OccurredAtUtc, optional TenantId) and module-discovery markers `IPermissionDefinitionProvider`, `IFeatureDefinitionProvider`, `ISettingDefinitionProvider` (ASC-23).
- F1: bootstrap `Aschott.Anchor.Application` with CQRS markers (`ICommand<T>`, `IQuery<T>`, `ICommandHandler<C,T>`, `IQueryHandler<Q,T>` — all wrapping `Mediator.IRequest<Result<T>>`), `ICurrentTenant` contract (nullable Id + `Change` scope), and `IUnitOfWork` (ASC-24).
- F1: Mediator (martinothamar, Apache 2.0) 3.0.2 + FluentValidation 12 added to CPM as canonical Application-tier dependencies. Mediator.SourceGenerator 3.0.0-preview.46 was avoided due to a vulnerable transitive `Scriban` 6.2.0 — moved to stable 3.0.2 which dropped the dep.
- F1: non-generic `IAggregateRoot` marker on `Aschott.Anchor.Domain.Entities` (`GetDomainEvents` + `ClearDomainEvents`) implemented by `AggregateRoot<TKey>`, so infrastructure can enumerate tracked aggregates without pinning `TKey`.
- F1: `Aschott.Anchor.Application` first behaviors slice (part of ASC-25): `RequiresTenantAttribute`, `IDomainEventDispatcher` + `IDomainEventCollector` abstractions, `LoggingBehavior` (entry/exit/duration logs + ActivitySource tagging), `TenantContextBehavior` (gates `[RequiresTenant]` requests against `ICurrentTenant.Id`).
- F1: `Aschott.Anchor.Application` second behaviors slice (closes ASC-25): `ValidationBehavior` (FluentValidation; failures return `Result<T>.Fail` when response is `Result<T>`, otherwise throw `ValidationException`), `UnitOfWorkBehavior` (commits UoW after successful commands; bypasses queries; suppressed on exceptions), `DomainEventDispatchBehavior` (collects events from tracked aggregates after `next()` and forwards to `IDomainEventDispatcher`, clearing aggregates afterward).
- F1: `AddAnchorApplication(this IServiceCollection, params Assembly[])` extension registering the 5 pipeline behaviors in canonical order (Logging → TenantContext → Validation → UnitOfWork → DomainEventDispatch) and scanning supplied assemblies for FluentValidation validators. Mediator's own `AddMediator` is left to the consumer (Mediator 3.x emits the DI extension via source generator at the consumer assembly).
- F1: `IAuditedObject` marker on `Aschott.Anchor.Domain.Auditing` (`CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`) — adopted by infrastructure audit conventions (ASC-26).
- F1: bootstrap `Aschott.Anchor.Infrastructure` building block (closes ASC-26): `IApplicationDbContext` (commit-only surface), `IRepository<TEntity, TKey>`, `CurrentTenantAccessor` (AsyncLocal-backed `ICurrentTenant` for CLI/jobs/tests), `AuditConventions.ApplyAuditConventions()` ModelBuilder extension that registers shadow-or-explicit audit properties for `IAuditedObject` entities, `MultiTenantQueryFilters.ApplyMultiTenantFilters(currentTenant)` ModelBuilder extension that adds an `e.TenantId == currentTenant.Id || currentTenant.Id == null` filter to every `IMultiTenant` entity, and `BaseDbContext` that wires conventions in `OnModelCreating` and stamps audit + tenant fields on `SaveChangesAsync`. EF Core 10.0.7 added to CPM.

### Changed

- F1: bumped `Microsoft.Extensions.DependencyInjection.Abstractions` and `Microsoft.Extensions.Logging.Abstractions` from 10.0.0 to 10.0.7 (transitively required by EF Core 10.0.7).
- F1: tests use Shouldly instead of FluentAssertions across the suite (license policy — FA 8+ went commercial).
