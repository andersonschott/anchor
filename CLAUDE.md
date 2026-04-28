# Anchor — Claude Operating Manual

> Guia para sessões Claude trabalhando neste repositório. Read this first before doing anything.

## What this project is

Anchor é um **framework modular monolith para .NET 10**, escrito greenfield. Inspirado em ABP (mais leve) e mappi-core (mesma arquitetura, sem ser consumidor). Apache 2.0, repo público, governance "benevolent dictator" até v1.0.

- **Repo:** `github.com/andersonschott/anchor`
- **Namespace NuGet:** `Aschott.Anchor.*` (vendor-prefixed; vira `Lifetime.*` em rename major bump quando migrar pro corporativo).
- **Estado atual:** M0 done (tag `v0.0.0`), F1 (Building blocks) em execução.

## Where docs live

| Path | What |
|---|---|
| Vault Obsidian: `1 - Projetos/Framework Próprio/` | Specs (Constitutional Doc / Greenfield Implementation), plans executáveis, log incremental |
| `docs/adr/` | Architectural Decision Records (Markdown, numerados) |
| `CHANGELOG.md` | Keep a Changelog format; entradas em `## [Unreleased]` |
| Linear project: `https://linear.app/aschott/project/anchor-2428366bab75` | Issues + milestones (M0..F6); team `Aschott`; bidirectional GitHub sync |

Specs canônicas estão fora deste repo (no vault Obsidian do Anderson). Quando uma decisão arquitetural cravar, **registre como ADR neste repo** (`docs/adr/NNNN-<title>.md`); não silently mude código.

## Stack and conventions

- **.NET 10** SDK pinado em `global.json` com `rollForward: latestFeature`.
- **Central Package Management** — todas as versões em `Directory.Packages.props`. Csprojs usam `<PackageReference Include="X" />` sem versão inline.
- **Build flags globais** (em `Directory.Build.props`): `TargetFramework=net10.0`, `Nullable=enable`, `ImplicitUsings=enable`, `TreatWarningsAsErrors=true`, `EnforceCodeStyleInBuild=true`, `AnalysisMode=All`. Significa: warnings = build break.
- **Solução em formato XML novo (`Anchor.slnx`)**, não `.sln`.
- **Source link** ligado (`Microsoft.SourceLink.GitHub`).

## Test discipline

- **TDD obrigatório** em F1+ — escreva teste failing, rode pra confirmar fail, implemente o mínimo, rode pra confirmar pass, commit. Não junte test+impl no mesmo commit a não ser que o plano diga.
- **xUnit** + **Shouldly** + **NSubstitute**. Coverlet (formato OpenCover) consumido por SonarCloud.
- **Shouldly, NÃO FluentAssertions.** FA 8.0+ virou comercial (Xceed); pin em 7.0.0 reforça o anti-pattern bait-and-switch que queremos evitar (mesma razão de não usar MediatR/Hangfire.Pro/AutoMapper v15+). API: `x.ShouldBe(y)`, `x.ShouldBeTrue()`, `Should.Throw<T>(...)`.
- **Coverage alvo:** ≥85% por pacote core (verificado via SonarCloud Quality Gate).
- **Architecture tests** (NetArchTest) em `tests/Aschott.Anchor.Architecture.Tests/` validam camadas (`Domain` não referencia `Application`, etc.).

## License policy (não negociável)

Allowlist de licenças (em `.github/workflows/ci.yml` job `license-check` + Spec 1):
- **Apache-2.0**
- **MIT**
- **BSD-3-Clause**
- **BSD-2-Clause**
- **MS-PL**
- **Microsoft.NET.Library**

**Pacotes vetados**: MediatR (BSL desde 12.0), Hangfire.Pro (commercial), Syncfusion (commercial), Telerik (commercial), AutoMapper v15+ (commercial), FluentAssertions v8+ (commercial). Substitutos canônicos:
- MediatR → **Mediator** (martinothamar, source-gen, Apache 2.0)
- Hangfire.Pro → **Quartz.NET** (Apache 2.0) como default; Hangfire core (LGPL via adapter) opcional
- AutoMapper → **Mapperly** (Apache 2.0) ou mapeamento manual
- FluentAssertions → **Shouldly** (BSD-2-Clause)

`license-check` no CI passa allowlist como **semicolon-separated** (não comma — `nuget-license` v4 mudou). Não reverter para vírgula.

## Git workflow (ESTRITO)

- `main` e `dev` são protegidos via **GitHub ruleset** (id `15679110`): required status checks `build-test` + `license-check` strict, no force push, no delete. Não dá pra pushar direto nem com bypass.
- **Todo trabalho via PR** de branch `feat/asc-NN-<short>` ou `chore/<short>` ou `fix/<short>` ou `docs/<short>` para `dev`. `dev` flui pra `main` via fast-forward merge no fim do milestone.
- **Branch base verification** (lição da PR #9 que silenciosamente reverteu PRs #6/#7/#8): antes de qualquer PR, rode:
  ```bash
  git fetch origin
  git checkout dev && git pull origin dev
  git checkout -b feat/asc-NN-short
  # ... do work ...
  git log --oneline origin/dev..HEAD
  # ↑ deve mostrar APENAS os commits da task atual; nada extra/inesperado
  ```
- **Squash-merge** com `--delete-branch` é o padrão. `gh pr merge <n> --squash --delete-branch`.
- **Conventional Commits**: `feat:`, `fix:`, `docs:`, `chore:`, `ci:`, `refactor:`, `test:`. Reference Linear issue no rodapé via `Closes ASC-NN`.

## gh CLI

- **Active account must be `andersonschott`**. Se aparecer `lftm-anderson-schott`, troque imediatamente:
  ```bash
  gh auth switch --user andersonschott
  ```
- Confirme com `gh api user --jq .login` antes de qualquer operação que mexa no repo.

## CI / Quality Gates

`.github/workflows/ci.yml` tem 3 jobs:
- `build-test` — restore, SonarCloud begin (skipped se sem `.csproj` em `src/`), build, test (XPlat coverage OpenCover), SonarCloud end.
- `license-check` — `nuget-license` com allowlist semicolon-separated.
- `pack-preview` — só em main/dev, packs preview em GitHub Packages (skipped se sem projetos).

`.github/workflows/release.yml` dispara em tag `v*`:
- `permissions: contents: write` (necessário para action-gh-release criar Release; sem isso 403).
- Step `Detect projects` + condicionais (`if: steps.projects.outputs.has_projects == 'true'`) em Restore/Build/Test/Pack/Push.
- **Routing por sufixo** (a implementar em F1.14): tags `-preview.*`/`-alpha.*` → GitHub Packages; sem sufixo → nuget.org.
- Create GitHub Release tem 2 variantes: com nupkg artifacts e tag-only.

**SonarCloud:**
- Project key: `andersonschott_anchor`, organization: `andersonschott` (não `aschott` — UI usa GitHub login como org default).
- Quality Gate "Sonar way" customizado: new code coverage ≥80%, duplications <3%, ratings A em maintainability/reliability/security, security hotspots reviewed 100%.
- `SONAR_TOKEN` está em GitHub secrets.
- **`SonarCloud Code Analysis` ainda NÃO está como required check** no ruleset porque até cobertura subir naturalmente em F1 ele falha (new code coverage = 0%). Adicionar como required em F1.12 (ASC-29).

**Important about `sonar-project.properties`:** **NÃO criar esse arquivo.** SonarScanner for .NET é incompatível com ele — pra projetos .NET, todas as configs vão em flags `/d:` no `SonarScanner begin` step do CI.

## Common commands

```bash
# Build / test local
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release

# Coverage local (após instalar tools opt-in)
dotnet tool restore   # instala dotnet-coverage + reportgenerator do .config/dotnet-tools.json
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings --results-directory ./coverage
dotnet reportgenerator -reports:./coverage/**/coverage.opencover.xml -targetdir:./coverage/report -reporttypes:Html
xdg-open ./coverage/report/index.html
```

## NuGet ID reservation

26 IDs `Aschott.Anchor.*` reservados como `0.0.1-reserved` em nuget.org (M0/ASC-14). Se precisar reservar mais (ex.: novo módulo): perfil de owner é `aschott`. Glob da API key `anchor-reserve` é `Aschott.Anchor*` (sem ponto antes do asterisco — cobre o ID raiz e qualquer sufixo).

## Project layout (target — F1+)

```
anchor/
├── src/
│   ├── Aschott.Anchor/                  # Bootstrap façade (option 1 do ADR 0002)
│   ├── Aschott.Anchor.Domain/           # DDD primitives
│   ├── Aschott.Anchor.Contracts/        # IntegrationEvent + module markers
│   ├── Aschott.Anchor.Application/      # CQRS (Mediator) + pipeline behaviors + IUnitOfWork
│   ├── Aschott.Anchor.Infrastructure/   # BaseDbContext + audit conventions + multi-tenant query filters
│   └── Aschott.Anchor.AspNetCore/       # IEndpoint discovery + tenant resolver chain + error middleware
└── tests/
    ├── Aschott.Anchor.Domain.Tests/
    ├── Aschott.Anchor.Application.Tests/
    ├── Aschott.Anchor.Infrastructure.Tests/  # com Testcontainers (Docker required)
    ├── Aschott.Anchor.AspNetCore.Tests/      # com WebApplicationFactory
    └── Aschott.Anchor.Architecture.Tests/    # NetArchTest, 8 rules
```

Modules (Tenants/Identity/Auth/Permissions/Audit/Jobs/etc.) entram em F3+, cada um nos 5 projetos canônicos (Domain/Application/Contracts/Infrastructure/Endpoints).

## Cross-project reminders

- Quando F1 estiver pronto (`v0.1.0-preview.1` em GitHub Packages), pode começar planejamento sério de **Health-System** rebuild — o gate ASC-5 → ASC-6 destrava H1.
- O `health-system` em `/home/aschott/WebstormProjects/health-system/` está em standby até F3 (Tenants + Identity + Auth) shipar.

## Anti-patterns (não fazer)

- ❌ Push direto em main/dev (vai falhar pelo ruleset de qualquer jeito).
- ❌ Commit `.sln` em vez de `.slnx`.
- ❌ Usar `MediatR`/`FluentAssertions 8+`/`AutoMapper v15+`/`Hangfire.Pro` (license violation).
- ❌ Inline `Version="X.Y.Z"` em `<PackageReference>` (CPM viola).
- ❌ Criar `sonar-project.properties` (incompatível com SonarScanner for .NET).
- ❌ `--no-verify` ou outro bypass de hooks.
- ❌ Pular TDD em F1+ porque "este é simples".
- ❌ Recriar branch a partir de commit stale sem `git pull origin dev` primeiro.
- ❌ Adicionar feature flag/backwards-compat shim sem necessidade real (Spec 1 não tolera).

## When unsure

Ler primeiro:
1. `1 - Projetos/Framework Próprio/log.md` no vault Obsidian — log incremental de decisões.
2. ADRs em `docs/adr/`.
3. Plan executável atual em `1 - Projetos/Framework Próprio/plans/`.
4. README do projeto no vault: `1 - Projetos/Framework Próprio/README.md`.

Se ainda assim não souber: parar e perguntar ao Anderson antes de fazer mudança arquitetural.
