# Roadmap — the master work queue

**Updated:** 2026-07-31. Ordered top-down; battle-scoped detail lives in
`BATTLE_PLAN.md`, structure/audit history in `PROJECT_REVIEW.md` + `CODE_AUDIT.md`,
world/content rules in `WORLD.md`. Statuses here mirror BATTLE_PLAN — when in
doubt, BATTLE_PLAN §1 is the authority for battle items.

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
- **1.5e** AI self-preservation (low-HP retreat toward healer/safety) ← **next**
- **1.5f** AI team focus fire + target value model
- **1.5g** AI support discipline (triage, MP reserve, buff openings, revive positioning)
- **1.7** KO decay pickup (memory-core/salvage), corpses stop blocking pathing
- **1.8** Real battle setup: spawn zones in LevelData, authored victory conditions
- **1.8b** Real terrain: types with gameplay meaning (water/trees/buildings/bridges), traversal + LoS integration, BoardCreator painting, visual skins
- **1.9** Equipment actually equips (audit §6 — `Equip()` has no callers)
- **1.10** Elements + critical hits via TweakDamage (audit §8)
- **1.11** Behavior-control statuses seize control: Swayed/Scrambled/Redline (audit §4)
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
