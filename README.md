# TacticsRPG

A turn-based tactics RPG built in Unity 6, set in "The Long Autumn" — an
original post-collapse world where magic is half-understood machine Protocol,
mercenary Charters replace nations, and a map-controlling Church rewrites
history. Grid battles with height and line-of-sight, a 23-job certification
tree with per-job ability kits, turn-order manipulation, and two difficulty
modes backed by distinct AI brains.

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

