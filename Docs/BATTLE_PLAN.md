# Battle System Plan — the working queue

**Date:** 2026-07-29 · Successor to `CODE_AUDIT.md` Phase 2/4 for everything battle-scoped.
Design intent for these systems lives in `GDD.md` (§3 gameplay, §5 slice battles).
Design stance: **FFT is the base, not the target.** Genre-floor competence first (fix the
lying systems), then original pillars that make this game its own.

Work strictly top-down: items in §1 block everything below them.

---

## 1. Fix the lying systems (in progress)

| # | Item | Status |
|---|---|---|
| 1.1 | **Statuses must affect combat.** Bulwark/Firewall/Redline/Doused/Static/Ghosted wired into `TweakDamageEvent` / `HitRateStatusCheckEvent`. | **done** (verified: Bulwark cut physical 14→9, restored on expiry, magic unaffected) |
| 1.2 | **Durations tick per-owner**, not on every unit's turn. | **done** |
| 1.3 | **Condition parenting bugs**: `GetComponent` vs `GetComponentInChildren` in the KO/Ghosted/Failsafe/Deadline family; Deadline must respect Failsafe (sibling status lookup). | **done** |
| 1.4 | **Control needs a miss chance**: per-ability `accuracy` in AbilityData (hard control 60–70%, soft debuffs 80–90%). RES-as-stat axis deferred until gear. | **done** (53 hostile inflictions tuned) |
| 1.5 | **AI runs on fossils**: attack patterns reference abilities that don't exist (`Water`, `Cure`, `Prominence`). Rebuild per-job patterns so enemies cast their actual kits. `SmartComputerPlayer` stays dead until a deliberate AI project. | **done** (patterns rebuilt; `SmartComputerPlayer` superseded by `TacticalComputerPlayer` and safe to delete) |
| 1.5b | **Difficulty modes** (2026-07-30): Easy = pattern AI, no scaling. Hard = `TacticalComputerPlayer` (scores every ability×move×target with Predict + hit chance; kill priority, focus fire, heal/status logic) + enemy HP ×1.3 and damage ×1.2 — tougher, deliberately beatable. Switch via `Tactics RPG → Difficulty` until the options UI exists; setting lives in PlayerPrefs (`DifficultySettings`). | **done** |
| 1.5c | **AI act-then-move** (hit-and-run): the tactical AI attacks from its current tile and *then* spends the move retreating whenever staying put scores ≥95% of the best move-then-attack plan and a safer tile exists. Safety = max-min Manhattan distance to **all** living foes (1.5d upgrades this to expected damage). Verified in-editor: staged Marksman shot Kamau then retreated, min distance to any hero 4→7. Easy AI unchanged. | **done** (2026-07-31) |
| 1.5d | **AI threat map** (foundation): per-tile expected damage from enemies that could reach+hit it next turn. Consumed by the hit-and-run retreat leg, a global safer-ground bias on all plans, and 1.5e/1.5g. | **done** (2026-07-31: verified — tile beside a hero 76.8 expected damage vs 0.0 in cover; kiting retreat now drops threat 81.3→52.5) |
| 1.5e | **AI self-preservation**: below 30% HP with no kill available, units retreat toward an allied healer with usable heals (danger-weighted approach), else the safest tile — with a parting shot when one is worth taking from the current tile. Healers weigh destination danger 3x and, when idle, drift toward the most wounded ally. | **done** (2026-07-31: verified — wounded Scav fled 76.8→0.0 threat with no healer; approached a spawned healer 4→2 when one existed) |
| 1.5f | **AI team focus fire + target value**: deterministic focus nomination (every teammate computes the same kill-first pick — no shared state needed) with an assist bonus; damage values scale by role (healer 1.35 > caster 1.2 > striker 1.0 > tank 0.9), wounded-ness, and kill feasibility. Never forces reach: units attack the best reachable target and their movement drifts toward the focus (post-strike converge leg + a gentle per-tile pull). | **done** (2026-07-31: verified — two enemies independently aimed at the healer; with focus unreachable the shooter hit the reachable caster while closing 15→12 on the focus) |
| 1.5g | **AI support discipline**: heal value scales with target criticality (stabilize the dying before topping off); healers refuse non-support casts that would break the emergency heal/revive MP reserve while allies are hurt or down; idle healers seek corpses they can revive, then the most wounded ally. Buff-while-closing was already emergent from scoring. | **done** (2026-07-31: verified — medic chose Field Surgery on the 15% ally over the 60% one; with a corpse present it planned the revive itself, closing 3→1) |
| 1.6 | Line-of-sight / arc for ranged attacks; high-ground combat bonus. | **done** (2026-07-31: `LineOfSight` blocks Constant ranges > 1 behind terrain, walls truncate Line volleys, `ElevationRules` grants ±15% damage and ±10 hit at ≥2 height difference; Infinite ranges bypass by design) |
| 1.7 | **KO decay & salvage**: after three skipped activations a fallen unit decays into board remains — a memory-core (restores half of the collector's missing HP/MP) or salvage (scrip scaled by level) — and is fully removed from battle (fixes the latent zombie-turn bug where decayed corpses re-entered the turn order). Walkers pass over KO'd units (can't end on them); ending a move on remains collects them. Pillar 4 (Scav-specific salvage play) builds on this. | **done** (2026-07-31: verified — corpse passable, Salvage spawned at 70 scrip, units 6→5 with tile freed, collection paid 5000→5070) |
| 1.8 | **Authored battle setup**: `BattleDefinition` ScriptableObject (level ref, per-unit spawn entries with position/facing/level, victory type, reinforcement waves) consumed by `InitBattleState` — game flow's `PendingBattle` first, scene `testBattle` second, writ-style random generation (the GDD §4.5.3 repeatable-contract path) when neither. `BattleClock` defines rounds atop the CTR scheduler (1 + turns/startingUnits); `BattleEvents` spawns waves on their round; `SurviveRoundsVictoryCondition` + `ReachZoneVictoryCondition` join DefeatAll/DefeatTarget (escort waits for M2's guest-control rules); `BattleSpawner` places units with nearest-free-tile fallback. First authored battle: Toll Road Ambush (Resources/Battles). | **done** (2026-07-30: verified in play mode — 5 units at exact authored tiles, round flipped at 5 activations, wave landed 5→7 units tile-linked, SurviveRounds declared Hero victor past round 1, ReachZone flips only when a living hero stands in the zone, writ fallback still spawns 6 + leader rule; fixed double-registration of authored units found during probe) |
| 1.8b | **Real terrain** (requested 2026-07-31): boards are bare height blocks. Terrain types with gameplay meaning — grass/road (normal), water/rivers (blocks walkers, not flyers), trees/buildings (block movement and line of sight), bridges connecting separated areas — FFT-style maps where some regions connect and some don't. Wire the dormant `TileTraversalFlags`/`TileTraversalOverride` into Movement, extend `LevelData`+BoardCreator to paint terrain, visual skins/props per type. Feeds Pillar 3 (Sync coverage as another terrain layer). | queued |
| 1.9 | **Equipment actually equips** (audit §6): `Equipment.Equip()` has zero callers — items/features never affect units. Wire starting gear into UnitFactory, make StatModifierFeatures live, and connect the shop's purchases to inventory. | queued |
| 1.10 | **Elements + crits** (audit §8): Elements component is populated but never read; no critical hits. Both plug into the waiting `TweakDamageEvent` stage (Doused's fire-vulnerability upgrade rides along). | queued |
| 1.11 | **Behavior-control statuses** (audit §4): Swayed/Scrambled/Redline land but don't seize control — Driver override so Swayed units fight for the enemy, Scrambled act randomly, Redlined force-attack the nearest unit. | queued |
| 1.12 | **Scrip out of PlayerPrefs** (audit §6): Bank balance belongs in GameData (save file), not machine-wide prefs. | queued |
| 1.13 | **jpCost decision** (audit §5): unlock data carries JP prices that nothing spends — either implement JP-buys-abilities (FFT-style shopping in the job menu) or delete the field. Design call, then wire. | queued |

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
