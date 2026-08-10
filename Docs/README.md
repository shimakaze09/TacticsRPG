# TacticsRPG documentation

This index defines what each document owns. When two documents disagree, use
the authority and status rules below rather than whichever file was edited most
recently.

## Document map

| Document | Authority | Status |
|---|---|---|
| [README](../README.md) | Repository setup, entry points, and verification | Current |
| [PROJECT_REVIEW](PROJECT_REVIEW.md) | Current code, architecture, and gameplay assessment | Current snapshot |
| [ROADMAP](ROADMAP.md) | Ordered implementation priorities | Current queue |
| [BATTLE_PLAN](BATTLE_PLAN.md) | Battle-runtime capability and battle-specific work | Current queue |
| [ARCHITECTURE](ARCHITECTURE.md) | Engineering rules and extension patterns | Current |
| [GDD](GDD.md) | Product vision and intended game design | Design authority |
| [WORLD](WORLD.md) | Setting, roster, terminology, and naming conventions | Lore authority |
| [CODE_AUDIT](CODE_AUDIT.md) | Findings captured on 2026-07-28 | Historical archive |

## Status language

- **Implemented** means code or serialized content exists in the repository.
- **Integrated** means the player can reach and use it through the normal game
  flow.
- **Verified** means a named check was run against the stated revision.
- **Planned** means design intent only; it must not be described as shipped.
- **Historical** means the document preserves an earlier review and is not a
  live defect list.

An implemented battle system can still be unavailable in a player build. The
current project has many implemented tactical systems, but the normal
Title → Hub → Battle → Results → Hub loop is not yet integrated.

## Update rules

1. Change current implementation claims in `PROJECT_REVIEW.md`.
2. Change work order in `ROADMAP.md`; keep battle detail in `BATTLE_PLAN.md`.
3. Change intended behavior in `GDD.md`, not in a status document.
4. Change lore or display vocabulary in `WORLD.md`.
5. Update `README.md` whenever setup, generation, scenes, or verification changes.
6. Keep `CODE_AUDIT.md` immutable except for archive notes or links to current work.
7. Link active defects to GitHub issues instead of maintaining duplicate TODO
   descriptions in several documents.
