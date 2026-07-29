# TacticsRPG

A job-system tactics RPG built in Unity, set in an original post-collapse
world — see `Docs/WORLD.md` for the setting bible ("The Long Autumn"),
job roster, and naming conventions.

## Requirements

- **Unity 6.5 (6000.5.5f1)** or newer. The project was migrated from
  2022.3 LTS: URP 17.5, TextMeshPro via the built-in uGUI 2.0 package.
  On first open, Unity will re-serialize assets and regenerate
  `Packages/packages-lock.json` — commit those changes when prompted.

## Getting started (required after a fresh clone)

The job, ability, and catalog **prefab assets are not committed** — they are
generated from the JSON data in `Assets/Resources/{JobData,AbilityData,CatalogData}`
and the output folders (`Assets/Resources/Jobs`, `Assets/Resources/Abilities`,
`Assets/Resources/Ability Catalog Recipes`) are gitignored.

After cloning, open the project in Unity and run these editor menu items **in
this order** before entering play mode:

1. `Tactics RPG → Generate Content → Abilities`
2. `Tactics RPG → Generate Content → Catalogs`
3. `Tactics RPG → Generate Content → Jobs`

Without this step, `JobManager` finds no jobs ("No jobs available!"),
`UnitFactory` cannot load any ability prefabs, and battles will not function.

## Scenes

- `Assets/Scenes/StartMenu.unity` — entry point (build index 0), hosts `GameFlowController`.
- `Assets/Scenes/Battle.unity` — the battle scene, loaded by name.
- `Assets/Scenes/BoardCreator.unity` — editor-only board authoring tool (disabled in build settings).

## Project docs

- `Docs/ROADMAP.md` — **the master work queue** (start here); statuses mirror BATTLE_PLAN.
- `Docs/BATTLE_PLAN.md` — battle-system queue: fixes, original pillars, polish, tuning.
- `Docs/WORLD.md` — world bible: setting, job roster, naming conventions, combat/number law.
- `Docs/PROJECT_REVIEW.md` — architecture review and structure audit (historical + addenda).
- `Docs/CODE_AUDIT.md` — the 2026-07-28 line-level audit snapshot, with a current
  status header showing what's since been fixed.
