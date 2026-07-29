# TacticsRPG — project conventions

Unity 6 (6000.5.5f1) tactics RPG set in an original world ("The Long Autumn").
Key docs: `Docs/ROADMAP.md` (**the master work queue — start here**),
`Docs/WORLD.md` (setting, roster, naming), `Docs/BATTLE_PLAN.md` (battle
work queue), `Docs/PROJECT_REVIEW.md` + `Docs/CODE_AUDIT.md` (history).

## Documentation convention (mandatory)

Every script must be understandable from its comments alone:

- **Every file** starts with a `/// <summary>` on its top-level type (one type
  per file — the class summary IS the file summary). Say what the system does
  and how it fits in, not just what the class is named.
- **Every method** carries a comment: `/// <summary>` for public/protected
  members; at minimum a `//` line for private helpers and Unity lifecycle
  methods. State purpose ("why/what for"), not a restatement of the code.
- **Nothing else.** Inside method bodies, comment only where genuinely needed
  (a non-obvious constraint or gotcha). No line-by-line narration, no comments
  that restate the code.
- New code is written this way from the start; touched legacy code gets its
  comments added in the same change.

## Content pipeline

- Content lives as JSON in `Assets/Resources/{JobData,AbilityData,CatalogData}`;
  generated assets (gitignored) are rebuilt via `Tactics RPG → Generate Content
  → Abilities / Catalogs / Jobs` (in that order) after any JSON change.
- Stable ids never change after shipping; display names rename freely
  (see WORLD.md §5 for pipeline invariants, §3 for naming conventions).
- Damage/heal/status conventions and numeric ceilings: WORLD.md §4b and
  `StatLimits.cs`.

## Workflow

- Commit in split, logical batches (never one giant commit); push to
  `origin/main` — the project works directly on main.
- Verify script changes with a headless compile before handing back;
  content changes additionally need regeneration + an in-editor check
  (Unity MCP is configured via `.mcp.json`).
- **Docs stay synced, always**: completing any queue item updates its status
  row in ROADMAP.md *and* BATTLE_PLAN.md (and any other doc that mentions it)
  in the same commit as the work. Docs must never disagree with reality or
  each other.
- **README describes the project only** — what the game is. No doc-directory
  listings, no workflow notes (those live here and in Docs/).
