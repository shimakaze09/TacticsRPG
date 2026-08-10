# TacticsRPG

TacticsRPG is a turn-based tactics RPG prototype built with Unity 6. It is set
in **The Long Autumn**, an original post-collapse world where magic is
half-understood machine Protocol, mercenary Charters replace nations, and a
map-controlling Church rewrites history.

## Project status

The repository contains a substantial tactical-battle framework, not yet a
complete player-facing RPG. The battle runtime includes height-aware grids,
CTR turns, movement and facing, statuses, terrain rules, equipment traits,
multiple objectives, Easy/Hard AI, 23 jobs, and 135 data-defined abilities.

The end-to-end loop is still under construction. A normal build currently
opens an unfinished title scene, and the Title → Hub → Battle → Results → Hub
journey is not yet complete. Track the current integration work in the
[GitHub issue tracker](https://github.com/shimakaze09/TacticsRPG/issues). The
[Roadmap](Docs/ROADMAP.md) explains the phase, priority, group, and weight
labels used to keep implementation focused.

## Requirements

- Unity **6000.5.5f1** (Unity 6.5) or a compatible newer editor
- Git LFS is not required by the current repository

The project was migrated from Unity 2022.3 LTS. On first open, Unity may
re-serialize assets and update `Packages/packages-lock.json`.

## Fresh-clone setup

Job, ability, and catalog assets are generated from JSON and are not committed.
Before entering Play mode, open the project in Unity and run these commands in
order:

1. `Tactics RPG → Generate Content → Abilities`
2. `Tactics RPG → Generate Content → Catalogs`
3. `Tactics RPG → Generate Content → Jobs`

Source data lives in:

- `Assets/Resources/AbilityData`
- `Assets/Resources/CatalogData`
- `Assets/Resources/JobData`

Generated output under `Assets/Resources/Abilities`,
`Assets/Resources/Ability Catalog Recipes`, and `Assets/Resources/Jobs` is
gitignored. Without generation, jobs and battle abilities cannot load.

## Running the project

- `Assets/Scenes/StartMenu.unity` is build index 0, but its player-facing UI is
  not complete yet. See [issue #2](https://github.com/shimakaze09/TacticsRPG/issues/2).
- `Assets/Scenes/Battle.unity` is the current development entry point for
  exercising the tactical runtime after generating content.
- `Assets/Scenes/BoardCreator.unity` is an editor-only board-authoring scene and
  is disabled in build settings.

## Verification

The repository includes an in-editor battle probe suite:

- Menu: `Tactics RPG → Run Battle Probes`
- Headless entry point: `BattleProbeMenu.RunHeadless`

Unity compilation and probes are not yet run automatically on GitHub. CI and
headless content generation are tracked in
[issue #7](https://github.com/shimakaze09/TacticsRPG/issues/7).

## Documentation

Start with the [documentation index](Docs/README.md). It separates current
implementation status from design intent, architecture rules, lore, and
historical audits.

## Reporting work

Use the repository issue forms for reproducible bugs, feature proposals, and
implementation tasks. Every unfinished issue is classified by importance,
feature group, delivery phase, and relative weight; the
[Roadmap](Docs/ROADMAP.md) defines the workflow.
