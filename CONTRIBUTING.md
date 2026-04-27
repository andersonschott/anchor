# Contributing to Anchor

Anchor is open source under Apache 2.0 but operates under a "benevolent dictator" governance model until v1.0. This means PRs are welcome but merge is at maintainer discretion. There is no SLA and no formal RFC process yet.

## Before opening a PR

1. **Open an issue first** for anything beyond a typo. State the problem before proposing a solution.
2. **Search existing issues** to avoid duplicates.
3. **Confirm the change aligns with project goals** by reading the [Constitutional Doc](https://linear.app/aschott/project/anchor-2428366bab75) (linked from project description).

## Branching and commits

- Branch from `dev`, not `main`.
- Naming: `feat/<linear-id>-<short>`, `fix/<linear-id>-<short>`, `docs/<short>`.
- Use [Conventional Commits](https://www.conventionalcommits.org/): `feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `chore:`.
- Reference Linear issue in commit body or footer: `Closes ANC-42`.

## Pull requests

- Target `dev` (not `main`).
- All checks green: build, test, arch tests, license check, SonarCloud Quality Gate.
- New code must have ≥80% line coverage on the diff (CI enforces via SonarCloud Quality Gate).
- Update `CHANGELOG.md` under `## Unreleased`.

## Reporting security issues

Do not open a public issue. See [SECURITY.md](SECURITY.md).
