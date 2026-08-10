# Architecture & Extension Guidelines

**Updated:** 2026-08-10. The rules for adding anything to this codebase.
Design authority for *what* to build is `GDD.md`; this document is the
authority for *how*. The verdict that produced it: keep the event-composed
architecture, harden the conventions around it — no DI-framework migration.
Current implementation status belongs in `PROJECT_REVIEW.md`; operational work
lives only in classified GitHub issues. `ROADMAP.md` defines the phase gates
and label contract.

## 1. The shape of the codebase

- **Content pipeline:** JSON under `Resources/*Data` → editor generators
  (`Tactics RPG → Generate Content`, order: Abilities → Catalogs → Jobs) →
  generated prefabs/assets (gitignored). Stable `id` slugs never change
  once shipped; display names rename freely.
- **Battle runtime:** units are MonoBehaviour compositions assembled by
  `UnitFactory` in dependency order; abilities are prefab trees (root
  `Ability` + power/range/area components, effect children); battle flow is
  the `BattleController` state machine; systems talk through the typed
  event bus (`GameEventBus` via `this.Publish` / `Subscribe`).
- **Code-defined law tables:** `TerrainRules`, `GearCatalog`, `StatLimits`,
  `CriticalHit`, `StatusRegistry`, `ElementRelationship` — design constants
  currently live in code.
- **Meta/persistence:** `GameFlowController` states + `GameData` save file.
  The meta layer is not yet integrated end to end; persistent objects must not
  own scene-local dependencies. See issues #3, #5, and #8.

## 2. Event bus rules

1. Events are **typed classes** in `EventArgs/`. Never stringly-typed.
2. **Prefer sender-scoped** (`SubscribeToSender`) over global `Subscribe`.
   Global subscriptions fan out to every listener in the scene.
3. Every handler of a globally-subscribed event **must open with an
   identity guard** (`if (e.Target != owner) return;`). A missing guard is
   a double-apply bug waiting to happen.
4. **Battle-wide laws live as single components on the BattleController**
   (`ElevationRules`, `ElementRules` pattern) — never once-per-unit, so a
   rule applies exactly once per calculation.
5. **Subscription lifetime:** hold one `EventSubscriptions` bag, subscribe
   through it in `OnEnable`, call `Clear()` in `OnDisable`. No
   hand-maintained mirror lists. Required for all new code.

## 3. Lifecycle rules (spawn-frame safety)

- Units must be fully usable **the same frame they spawn** (reinforcements,
  AI planning before `Start`). Therefore: components may cache their *own*
  GameObject's parts in `Awake`, but **cross-object and parent-chain
  lookups must be lazy** (resolve at first use, `??=` pattern).
- **Ability prefabs are parented after instantiation** — inside ability
  components, `GetComponentInParent` in `Awake`/`OnEnable` sees the wrong
  hierarchy. Resolve at query time (see `WeaponAbilityRange.Refresh`).
- `UnitFactory.Create` order is load-bearing (comment in the factory says
  why for each constraint). New unit parts get added there, not ad hoc.

## 4. Stats law

- `JobManager.RecalculateStats` is the **single authority** for derived
  stats. It rebuilds from job history + worn gear. Any new persistent
  bonus source must be folded into that computation. On job-carrying
  units, `StatModifierFeature` equip/unequip now *routes through* that
  same recomputation instead of writing deltas — one path, so cap
  clipping and baseline changes while equipped can never drift (the 1.9
  root-cause bug, closed for good by the #57 review).
- `StatLimits` caps (WORLD.md §4b) are enforced at the write points.
  Nothing may bypass them.
- The growth formula is settled: **ProgressionModel v2** (WORLD.md §4b;
  issues [#52](https://github.com/shimakaze09/TacticsRPG/issues/52) /
  [#54](https://github.com/shimakaze09/TacticsRPG/issues/54)) — current-job
  dominance, zero-growth unlocks, bounded cross-job carryover, character-level
  growth on the current job's profile. Job and gear tuning may now proceed
  against it. Formula, content, encounter, and reward changes must feed the
  automated balance report in
  [#58](https://github.com/shimakaze09/TacticsRPG/issues/58).

### Balance report (#58, v1)

- `Tactics RPG → Generate Balance Report` in-editor, or headless from the
  same clean-checkout path as the probes:
  `Unity -batchmode -nographics -projectPath . -executeMethod BalanceReportGenerator.RunHeadless`
  (exit 0 = clean). Writes `BalanceReport/report.{json,md}` at the repo root
  (gitignored) straight from the content JSON plus
  ProgressionModel/StatLimits/GearCatalog — no play mode, no generated
  assets, deterministic output.
- Target bands and tolerances are versioned constants in
  `Assets/Editor/BalanceReport/BalanceConfig.cs`; changing a band is a design
  decision and bumps its `Version` in the same edit.
- **Hard invariants fail CI** (exit 1): broken or misnamed ability-unlock
  references, job ↔ catalog ↔ ability file mismatches (WORLD §5), a primary
  stat saturating its 999 cap before level 90 single-job, zero-power heals,
  and >10× band outliers. Band drift (level-1 HP band, 2–6 turns-to-KO,
  zero-MP free power, dominated options) surfaces as **warnings — review
  prompts, never failures**.

## 5. Damage pipeline placement

Stages: attack/defense/power stat events → formula
`max(ATK×power/100 − DEF/2, 1)` → `TweakDamageEvent` → clamp.

- **Power scaling** (weapon damage profile) → `GetPowerEvent` modifiers.
- **Deterministic conditional rules** (elements, flank, execute, resists,
  elevation, statuses) → `TweakDamageEvent`. These SHOW in forecasts.
- **Randomness** (variance, crits) → `OnApply` only. `Predict` must stay
  deterministic — the AI and the forecast UI depend on it. Never put a
  random modifier in the Tweak stage.

## 6. Extension recipes

Each addition is data + one hook + one probe + one doc row. If a new idea
doesn't fit an existing hook, add the *hook* as its own reviewed piece
first — don't special-case content into engine code.

- **Gear:** a `GearCatalog` entry (stats, reach/arc/shape/profile, traits).
  No code for new items.
- **Gear trait:** add the enum value with a doc comment; implement in the
  right hook — `WeaponTraitRunner` (on-hit / conditional attacker-side),
  `GearDefenseFeature` (incoming damage), or a new Feature for wearer
  passives; add a `BattleProbeRunner` probe; add the GDD §3.3 row.
- **Status:** the `XStatus` class + one `StatusRegistry.Register<T>()`
  line (unknown names fail loudly at first use — no reflection).
- **Terrain:** `TerrainType` value + `TerrainRules` row + block prefab
  named after the skin + probe.
- **Victory objective:** `VictoryType` value + condition class +
  `InitBattleState.AddVictoryCondition` case + probe.
- **Battle-wide combat rule:** one component on the BattleController,
  added in `InitBattleState.Init`, subscribed via `EventSubscriptions`.

## 7. Persistence

Everything player-persistent goes in `GameData` through
`DataPersistenceManager`. **No new PlayerPrefs.** Currency, roster, and
equipment migration are tracked in issues
[#3](https://github.com/shimakaze09/TacticsRPG/issues/3),
[#60](https://github.com/shimakaze09/TacticsRPG/issues/60), and
[#61](https://github.com/shimakaze09/TacticsRPG/issues/61).

## 8. Verification workflow (the law of the land)

1. **Headless compile** of the sync worktree — zero `error CS`, no new
   warnings.
2. **Battle probes** — `Tactics RPG → Run Battle Probes` in-editor, or
   headless/CI:
   `Unity -batchmode -nographics -projectPath . -executeMethod BattleProbeMenu.RunHeadless`
   (exit code 0 = all passed). **Every new system lands with probes in
   `BattleProbeRunner`** — the suite is the accumulated proof of every
   invariant, and it must stay green.
3. Zero new console errors in a play session.
4. Update only the documents whose authority changed. Publish work on a focused
   branch and open a reviewable pull request; do not push feature work directly
   to `main`.

### Content generation and CI (issue #7)

- **One-shot generation**: `Tactics RPG → Generate Content → All (Validated)`
  cross-validates the content JSON (`ContentValidator` — ids, catalog and
  unlock references, unlock levels, JP curves), then runs Abilities →
  Catalogs → Jobs in order. Headless form:
  `Unity -batchmode -nographics -projectPath . -executeMethod ContentGenerationMenu.RunHeadless`
  (exit 0 only when validation and all three generators succeed). The
  individual generator menu items remain for targeted regeneration.
- **CI** (`.github/workflows/ci.yml`): every PR and push to main starts from
  a clean checkout (no generated content), regenerates via the validated
  entry, runs the battle probes, and enforces the balance report's hard
  invariants (`BalanceReportGenerator.RunHeadless`); logs and the balance
  report upload as artifacts. Pushes to main additionally produce a Windows
  build artifact via `CIBuild.Build`, which regenerates content before
  building. Requires the `UNITY_EMAIL` / `UNITY_PASSWORD` repository secrets
  — a Unity Personal account **without two-step verification** (headless
  login cannot answer a code prompt; Unity discontinued manual .alf/.ulf
  activation for Personal, so a dedicated 2FA-free CI account is the
  supported free-tier path). The workflow activates on start and returns the
  seat when done; the Library folder is cached keyed on `packages-lock.json`
  + editor version.

## 9. Known debt tracking

Architecture and tooling debt is tracked in the
[`group: tooling-architecture`](https://github.com/shimakaze09/TacticsRPG/issues?q=is%3Aissue+is%3Aopen+label%3A%22group%3A+tooling-architecture%22)
issue view; persistence and repeated-scene-load ownership defects use their
own primary groups. This document records the engineering rules those fixes
must preserve, not a second debt queue. A DI container remains out of scope
for the event-composed battle layer.
