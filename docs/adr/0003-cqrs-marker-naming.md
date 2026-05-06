# 0003 — Naming dos markers Cqrs (Aschott.Anchor.Application)

**Status:** Proposed (2026-05-06) — Accepted requer sign-off antes de `v0.2.0-preview.1`.
**Context:** Open question registrada na sessão F1 (log incremental, bloco [2026-05-01], Decisão/desvio #5; reaberta no closeout [2026-05-06]).

## Problem

`Aschott.Anchor.Application.Cqrs` expõe atualmente:

- `ICommand<TResponse>`
- `IQuery<TResponse>`
- `ICommandHandler<TCommand, TResponse>`
- `IQueryHandler<TQuery, TResponse>`

Todos os 4 wrapam `Mediator.IRequest<Result<TResponse>>` para impor a convenção canônica do Anchor: **command/query sempre retorna `FluentResults.Result<T>`, nunca lança exceção de fluxo de negócio**.

Mediator 3.x (martinothamar) expõe markers próprios com **mesmos nomes simples**, propósito diferente:

- `Mediator.ICommand<TResponse>`
- `Mediator.IQuery<TResponse>`

Quando um arquivo de consumer tem ambos `using Aschott.Anchor.Application.Cqrs;` e `using Mediator;` simultaneamente, referências a `ICommand<T>` / `IQuery<T>` ficam ambíguas e **falham compilação**. A ambiguidade é inevitável na prática porque:

- Os markers Anchor dependem de Mediator (extendem `IRequest<>`)
- Consumers tipicamente precisam `using Mediator;` para `IPublisher`, `IMediator`, `IRequestHandler<,>`, e para o source-gen de Mediator 3.x reconhecer suas implementações

Workarounds adotados em F1, ambos custosos:

- Tests qualificam com namespace completo: `Aschott.Anchor.Application.Cqrs.ICommand<Guid>` (verboso, repete em cada signature; ver `tests/Aschott.Anchor.Application.Tests/Behaviors/UnitOfWorkBehaviorTests.cs:11-12`).
- Código interno do Anchor usa qualifier relativo: `typeof(Cqrs.ICommand<>)` (só funciona dentro da árvore de namespace `Aschott.Anchor.Application`; ver `src/Aschott.Anchor.Application/Behaviors/UnitOfWorkBehavior.cs:15`).

Ambos vazam o ônus ergonômico para todo arquivo de handler/command/query do consumer. Quanto mais consumidores externos adotarem, maior o custo de reverter depois.

## Decision

**Renomear os markers com prefixo `Anchor`:**

| Atual (`v0.1.x`) | Proposto (`v0.2.0+`) |
|---|---|
| `ICommand<TResponse>` | `IAnchorCommand<TResponse>` |
| `IQuery<TResponse>` | `IAnchorQuery<TResponse>` |
| `ICommandHandler<TCommand, TResponse>` | `IAnchorCommandHandler<TCommand, TResponse>` |
| `IQueryHandler<TQuery, TResponse>` | `IAnchorQueryHandler<TQuery, TResponse>` |

Namespace permanece `Aschott.Anchor.Application.Cqrs`. Consumers podem continuar tendo `using Mediator;` e `using Aschott.Anchor.Application.Cqrs;` no mesmo arquivo sem ambiguidade.

### Alternativas consideradas

**A. Manter nomes atuais; documentar workaround.** Rejeitado — leak ergonômico em todo arquivo de consumer; piora à medida que adoção cresce.

**B. Eliminar markers Anchor; consumers usam `Mediator.ICommand<T>` direto.** Rejeitado — o valor agregado do Anchor é a convenção de retorno `Result<T>`. `Mediator.ICommand<T>` retorna `T` (lança exceção em falha). Consumers teriam que escrever `IRequest<Result<MyResponse>>` em todo lugar.

**C. Rebatizar para nomes não-`I`-prefixed (`Command<T>`, `Query<T>` como classes abstratas).** Rejeitado — abstract base classes forçam herança e descartam records; o contrato atual de interface preserva flexibilidade (consumer escolhe record/class/struct).

**D. Renomear só `Command`/`Query` (manter `ICommandHandler`/`IQueryHandler`).** Rejeitado — assimétrico. Par command+handler descasado é confuso; primeira coisa que quebra para consumers.

**E. Prefixo `IApp` ou `IApplication` em vez de `IAnchor`.** Rejeitado — `App`/`Application` são genéricos demais e podem colidir com tipos do consumer; `Anchor` reforça brand do framework.

**F. Mover para sub-namespace `Aschott.Anchor.Application.Cqrs.Markers` e instruir consumer a importar só esse.** Rejeitado — clash é por **nome de tipo** (resolvido em compile time com base no que está no escopo), não por namespace. Importar só Markers ainda traz `ICommand<T>` que colide com `Mediator.ICommand<T>` se Mediator também estiver no escopo.

**G. Type aliases (`using ICommand = Aschott.Anchor.Application.Cqrs.ICommand<...>`) padronizados em template.** Rejeitado — não é genérico (alias precisa fixar `TResponse`); ergonomia ruim.

## Consequences

### Breaking change
- `v0.1.0-preview.1` permanece publicada como está; não será removida nem deprecada formalmente (status preview implica churn explícito).
- Consumers que adotarem `0.1.x` terão que renomear referências ao migrar para `0.2.x`. Custo: trivial (sed pattern), mas é breaking.
- **Janela de oportunidade:** no momento desta ADR (2026-05-06), o único consumidor conhecido é o consumer test em `/tmp/anchor-consume-test`. Health-System ainda não adotou Anchor.Application. Renomear AGORA evita migração custosa depois — quando HS ou outro consumer real adotar, pega `0.2.x+` direto.

### Implementation scope (rastreio para PR de implementação separado)

Arquivos a alterar:

- **Renomear:** `src/Aschott.Anchor.Application/Cqrs/{ICommand,IQuery,ICommandHandler,IQueryHandler}.cs` → `IAnchor{Command,Query,CommandHandler,QueryHandler}.cs`
- **Atualizar tipo no Behavior:** `src/Aschott.Anchor.Application/Behaviors/UnitOfWorkBehavior.cs:15` — `typeof(Cqrs.ICommand<>)` → `typeof(Cqrs.IAnchorCommand<>)`
- **Tests a atualizar:**
  - `tests/Aschott.Anchor.Application.Tests/Cqrs/MarkerInterfaceTests.cs`
  - `tests/Aschott.Anchor.Application.Tests/Behaviors/UnitOfWorkBehaviorTests.cs` (perde a necessidade de namespace fully-qualified)
  - `tests/Aschott.Anchor.Architecture.Tests/LayerBoundariesTests.cs`
  - `tests/Aschott.Anchor.Architecture.Tests/ModulePlaceholderRulesTests.cs` (comentário das placeholder rules)
- **Docs:**
  - `docs/building-blocks.md` (tabela de blocks + exemplos do snippet `CreateCustomer`)
  - `CHANGELOG.md` (entrada `[Unreleased]` na seção `### Changed (BREAKING)`)

### Versionamento
- Land em `v0.2.0-preview.1` (próximo preview). Breaking change na superfície pública mesmo em pre-1.0; bumpar minor é a convenção limpa para evitar surpresa em quem fixou `0.1.*`.
- Implementação fica como issue separada no Linear (ASC-NN, criar quando esta ADR for aceita).

### Health-System implications
- HS atualmente não consome `Aschott.Anchor.Application` (ver `Refactor Auth Single-Tenant - Plano e Progresso.md`; HS usa MediatR/handlers próprios na implementação corrente).
- Quando HS adotar (pós-beta MVP, alinhado ao gate ASC-5 → ASC-6), pega direto `v0.2.x+`. Zero custo de migração para HS.
- Esta ADR é o único item Anchor-side que destrava adoção limpa pelo HS no futuro próximo. Daí a urgência de fechar agora, antes de qualquer outro consumer adotar `0.1.x`.

## Open after acceptance
- Criar Linear issue para a implementação (escopo cravado acima).
- Decidir se mantemos o `MediatorOptions`-style trick para resolver o clash semanticamente (Mediator não suporta), ou se escrevemos um analyzer/source-gen que detecte uso ambíguo. Consenso provisório: **não**, o rename resolve sem complexidade extra.

## References
- Mediator 3.x: https://github.com/martinothamar/Mediator
- Workarounds em F1: `src/Aschott.Anchor.Application/Behaviors/UnitOfWorkBehavior.cs:15`, `tests/Aschott.Anchor.Application.Tests/Behaviors/UnitOfWorkBehaviorTests.cs:11-12`.
- Log incremental Obsidian: `Framework Próprio/log.md` blocos `[2026-05-01]` (Decisão/desvio #5) e `[2026-05-06]` (Para o user retomar — open question pré-F2/F3).
