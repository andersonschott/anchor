# 0002 — Forma do pacote raiz Aschott.Anchor

**Status:** Accepted (2026-04-28)
**Context:** Open question levantada em M0/ASC-14 ao reservar 26 IDs em nuget.org. Pacote raiz `Aschott.Anchor` foi reservado mas seu propósito não estava decidido.

## Decision

**Opção 1: Bootstrap façade.**

`Aschott.Anchor` expõe `IServiceCollection.AddAnchor(Action<AnchorBuilder>)` (a ser implementado em F1.7+) que pulls os 5 building blocks core (`Domain`, `Application`, `Contracts`, `Infrastructure`, `AspNetCore`) como `<ProjectReference>` fixos. Módulos opcionais (Tenants, Identity, Auth, Permissions, Audit, Jobs, etc.) ficam opt-in via extension methods (`builder.AddTenantsModule()`).

**Trade-offs:**
- Pro: DX premium — 1 referência inicial, descoberta de features.
- Pro: Ponto único de configuração via `AddAnchor(b => b.UseSqlServer().AddAuth())`.
- Con: Risco de puxar deps em projeto headless (worker service que não usa AspNetCore).

**Mitigações:**
- TFM `net10.0` puro (não `Microsoft.NET.Sdk.Web`).
- Endpoints AspNetCore opt-in via `builder.UseEndpoints()` em vez de auto-wire.
- Consumidores que querem ainda mais granularidade podem referenciar pacotes building block individualmente — `Aschott.Anchor` é caminho preferido, não obrigatório.

## Consequences

- Em F1.1 (esta task): csproj `src/Aschott.Anchor/Aschott.Anchor.csproj` criado com referência apenas a `Domain` (único existente agora). F1.6/7/9/10 adicionam Contracts/Application/Infrastructure/AspNetCore conforme cada um nasce.
- Em F1.7+ (após `AddAnchorApplication` existir): introduzir `AnchorBuilder` + extension methods `AddAnchor()` no projeto `Aschott.Anchor` que orquestram os AddAnchor* dos building blocks.
- ADR pode ser revisitada se experiência prática mostrar que façade vira tech debt; mas mudança quebraria consumidores publicados, então só revisitar antes de v1.0 stable.
