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

### Changed

- F1: tests use Shouldly instead of FluentAssertions across the suite (license policy — FA 8+ went commercial).
