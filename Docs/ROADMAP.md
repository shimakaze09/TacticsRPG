# Roadmap — the master work queue

**Updated:** 2026-07-30. Ordered top-down; **design authority is `GDD.md`** —
this file is the execution order for its production plan (§11). Battle-scoped
detail lives in `BATTLE_PLAN.md`, structure/audit history in
`PROJECT_REVIEW.md` + `CODE_AUDIT.md`, world/content rules in `WORLD.md`.
Statuses here mirror BATTLE_PLAN — when in doubt, BATTLE_PLAN §1 is the
authority for battle items.

**Milestone mapping (GDD §11):** §1–§2 below = **M0** (systems hardening).
**M1** (playable loop) = meta UI minimal + initiative bar + forecast/popups +
first two authored battles. **M2** (slice content) = 6 battles, story scenes,
shop, Cert-buys-abilities, **quest board + story flags + writ generator +
Choice A + first side story**, art/audio first pass. **M3** (polish) =
controls rework, mix, tuning, save slots. Post-slice: branching Acts 2–3 and
the four endings, side/hidden/character quests, relic hunts, pillars —
full taxonomy in GDD §4.3/§4.5.

## 1. Documentation passes (in progress)

File-level summaries: **done** (every script). Method comments remain
(~1,316 of 1,592 methods), in three passes — each lands as its own verified,
pushed commit:

1. Battle core + ability pipeline (states, controllers, ability components, modifiers)
2. Actor/status/AI/persistence
3. UI/animation/audio/items/tools

Convention (see `CLAUDE.md`): file summary + method comments, nothing noisier.
Files touched by feature work get their method comments in the same change.

## 2. Battle fixes — finish BATTLE_PLAN §1

Done so far: **1.1–1.4** (status honesty, per-owner durations, condition
parenting, control accuracy) · **1.5/1.5b** (AI patterns rebuilt, Easy/Hard
difficulties with the tactical AI) · **1.5c** (hit-and-run) · **1.6**
(line-of-sight + high ground).

Remaining, in strict order:

- ~~1.5d~~ AI threat map — **done** (2026-07-31)
- ~~1.5e~~ AI self-preservation — **done** (2026-07-31)
- ~~1.5f~~ AI team focus fire + target value model — **done** (2026-07-31)
- ~~1.5g~~ AI support discipline — **done** (2026-07-31)
- ~~1.7~~ KO decay & salvage pickup — **done** (2026-07-31)
- ~~1.8~~ Authored battle setup: BattleDefinition assets, BattleClock rounds, reinforcement waves, SurviveRounds victory — **done** (2026-07-30)
- ~~1.8b~~ Real terrain: TerrainType/TerrainRules, terrain-aware movement/LoS/spawning, BoardCreator painting, Coldwater Crossing map — **done** (2026-07-30)
- ~~1.9~~ Equipment actually equips: GearCatalog + per-job loadouts, recalc keeps gear bonuses, shop→PartyInventory — **done** (2026-07-30)
- ~~1.9b~~ Gear behavior model: weapon reach/arc/shape/damage profile + composable GearTraits (recoil, self-slow, resists) — **done** (2026-07-30)
- ~~1.9c~~ Gear traits wave 2: flank bonus, status-on-hit, lifesteal, rifle dead zone; attack-facing fix; full family map in GDD §3.3 — **done** (2026-07-30)
- ~~1.10~~ Elements + crits: battle-wide affinity law, element gear traits live, deterministic-forecast crits, conditional damage traits — **done** (2026-07-30)
- ~~HRD~~ Architecture hardening (user-approved detour): `EventSubscriptions` symmetric-cleanup bag on the risky subscribers, typed `StatusRegistry` (reflection infliction gone), the 50-check `BattleProbeRunner` regression suite (menu + headless/CI entry), and `ARCHITECTURE.md` as the how-to-build authority — **done** (2026-07-30; suite green 50/50). Remaining debt stays in §"tech debt": namespaces/asmdefs → UTF migration, SerializableDictionary dedup, tweener/pool lifecycle.
- **1.11** Behavior-control statuses seize control: Swayed/Scrambled/Redline (audit §4) ← **next**
- **1.12** Scrip moves from PlayerPrefs into GameData (audit §6)
- **1.13** jpCost design decision: JP-buys-abilities or delete the field (audit §5)

## 3. Original pillars — BATTLE_PLAN §2 (design + build, in order)

1. **Timeline warfare**: initiative bar UI, delay/push/reorder abilities, AI awareness
2. **Grit reactions**: build-and-spend reaction resource, reaction window after abilities
3. **Sync terrain**: network coverage as casting geography
4. **In-battle salvage**: Scav grabs decayed remains (builds on 1.7)

## 4. Battle polish — BATTLE_PLAN §3

Damage popups (first — highest feel-per-effort), confirm-screen damage preview,
AoE previews, camera follow, minimal ability/hit/death animation.

## 5. Tuning — BATTLE_PLAN §4

Status durations + control accuracy bands, MP economy, CTR/SPD economy,
AoE-vs-single-target premium. Blocked on play-testing the honesty fixes.

## 6. Meta game

- Job-change menu **scene UI** (controller code exists, no canvas)
- Title screen (StartMenu has no canvas — game currently dead-ends there)
- Difficulty option in-game (editor menu exists; PlayerPrefs-backed)
- Shop flow integration, then world map / roster / save slots (audit Phase 4)

## 7. Tech debt (interleave when touching the area)

- Namespaces (279 files in global namespace); folder renames ("Data Persistance",
  "Exceptions" → Modifiers)
- Consolidate the two SerializableDictionary implementations
- EasingControl/Tweener lifecycle bugs; GameObjectPoolController scene-survival bugs;
  Point.GetHashCode collisions; static input events across scene loads
- First edit-mode tests around Stats/modifiers/JobProgressData/hit-rate math
