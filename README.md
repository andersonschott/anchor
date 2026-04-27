# Anchor

Modular monolith framework for .NET — DDD + CQRS + multi-tenancy + OpenIddict. Apache 2.0.

> **Status:** pre-alpha (M0 setup). Not ready for consumption. See [milestones](https://linear.app/aschott/project/anchor-2428366bab75) for v1.0 roadmap.

## What

Anchor is an opinionated framework for building SaaS systems on .NET 10. It packages decisions made over years of building modular monoliths — Clean Architecture, DDD, CQRS via source-gen Mediator, OpenIddict-backed auth, shared-DB multi-tenancy, OpenTelemetry observability — into reusable building blocks and `dotnet new` templates.

## Why

Existing .NET frameworks force a tradeoff:

- **ABP** is comprehensive but heavy, has commercial UI tier, and forces a 7-layer module structure.
- **Clean Architecture templates** scaffold once and rot.

Anchor sits between: smaller surface area than ABP, but ships canonical modules (Tenants, Identity, Auth, Permissions, Audit, Jobs, BlobStore, Notifications) and templates that stay alive.

## Status & roadmap

This is v0.x preview. Track progress: [Linear roadmap](https://linear.app/aschott/project/anchor-2428366bab75).

## Building blocks (planned for v1.0)

| Package                          | Purpose                                                        |
| -------------------------------- | -------------------------------------------------------------- |
| `Aschott.Anchor.Domain`          | Entity, AggregateRoot, ValueObject, MultiTenantEntity, DomainEvent |
| `Aschott.Anchor.Application`     | ICommand, IQuery, handlers, pipeline behaviors, IUnitOfWork    |
| `Aschott.Anchor.Contracts`       | IntegrationEvent, marker interfaces                            |
| `Aschott.Anchor.Infrastructure`  | BaseDbContext, IRepository, audit conventions, query filters   |
| `Aschott.Anchor.AspNetCore`      | IEndpoint, error middleware, tenant resolver chain             |

Modules: Tenants, Identity, Auth, Permissions, Audit, Jobs (Quartz default + Hangfire adapter), BlobStore, Features, Settings, Notifications, Localization.

## License

Apache 2.0. See [LICENSE](LICENSE).
