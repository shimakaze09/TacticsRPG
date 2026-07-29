# Battle System Plan — the working queue

**Date:** 2026-07-29 · Successor to `CODE_AUDIT.md` Phase 2/4 for everything battle-scoped.
Design stance: **FFT is the base, not the target.** Genre-floor competence first (fix the
lying systems), then original pillars that make this game its own.

Work strictly top-down: items in §1 block everything below them.

---

## 1. Fix the lying systems (in progress)

| # | Item | Status |
|---|---|---|
| 1.1 | **Statuses must affect combat.** Bulwark/Firewall/Redline/Doused/Static/Ghosted wired into `TweakDamageEvent` / `HitRateStatusCheckEvent`. (Turn-order statuses, DoTs, and hard disables already work.) | queued |
| 1.2 | **Durations tick per-owner**, not on every unit's turn. | queued |
| 1.3 | **Condition parenting bugs**: `GetComponent` vs `GetComponentInChildren` in the KO/Ghosted/Failsafe/Deadline family; Deadline must respect Failsafe (sibling status lookup). | queued |
| 1.4 | **Control needs a miss chance**: per-ability `accuracy` in AbilityData (hard control 60–70%, soft debuffs 80–90%). RES-as-stat axis deferred until gear. | queued |
| 1.5 | **AI runs on fossils**: attack patterns reference abilities that don't exist (`Water`, `Cure`, `Prominence`). Rebuild per-job patterns so enemies cast their actual kits. `SmartComputerPlayer` stays dead until a deliberate AI project. | **done** (patterns rebuilt; `SmartComputerPlayer` superseded by `TacticalComputerPlayer` and safe to delete) |
| 1.5b | **Difficulty modes** (2026-07-30): Easy = pattern AI, no scaling. Hard = `TacticalComputerPlayer` (scores every ability×move×target with Predict + hit chance; kill priority, focus fire, heal/status logic) + enemy HP ×1.3 and damage ×1.2 — tougher, deliberately beatable. Switch via `Tactics RPG → Difficulty` until the options UI exists; setting lives in PlayerPrefs (`DifficultySettings`). | **done** |
| 1.6 | Line-of-sight / arc for ranged attacks; high-ground combat bonus. | **done** (2026-07-31: `LineOfSight` blocks Constant ranges > 1 behind terrain, walls truncate Line volleys, `ElevationRules` grants ±15% damage and ±10 hit at ≥2 height difference; Infinite ranges bypass by design) |
| 1.7 | KO decay pickup (memory-core/salvage currently unreachable); corpses block pathing. | with §2 salvage |
| 1.8 | Real battle setup: spawn zones in LevelData, authored victory conditions (replace random test spawns). | before content push |

## 2. Original pillars (design + build, in order)

1. **Timeline warfare** — turn-order manipulation as a core axis. Visible initiative
   bar; delay/push/reorder abilities; Clockhand as the identity job. (CTR manipulation
   already works — this formalizes it into UI + more abilities + AI awareness.)
2. **Grit reactions** — units build Grit by dealing/taking hits, *spend* it to trigger
   equipped reactions (counter, brace, auto-stim). Deterministic, no Brave-style dice.
   Requires a reaction window after `BaseAbilityEffect.Apply`.
3. **Sync terrain** — network coverage as casting terrain: Protocol abilities scale up
   in high-Sync tiles (old-world infrastructure), down in dead zones. Physical jobs
   ignore it. Maps become casting geography; Wastewalker reads/alters it.
4. **In-battle salvage** — KO decay produces salvage the Scav can grab mid-fight
   (builds on 1.7).

## 3. Polish (any time after §1)

- Damage popups / floating text (highest feel-per-effort in the project).
- Initiative bar UI (doubles as pillar #1's foundation).
- Predicted damage in the confirm screen (`Predict()` already computes it).
- AoE previews for blasts and lines; camera follows the action.
- `PerformAbilityState.Animate` — minimal attack/hit/death tweens.

## 4. Tuning (blocked on §1; numbers are placeholders until then)

- Status durations (once per-owner) and control accuracy bands.
- MP economy: 10% MMP regen/turn vs costs 6–30; Colossus at 30 MP is once-per-battle — intended?
- CTR act/move costs and the SPD 4–8 spread (value of one Overclock turn).
- AoE vs single-target damage premium; MP-per-damage efficiency curves.
- EVD/facing bands (current EVD 4–15 unvalidated against A-type math).

Numeric law (see WORLD.md §4b): damage/heals cap at 999 per hit, HP wall 20,000
(bosses ~3–5k), primary stats cap 999. Tune under the caps.
