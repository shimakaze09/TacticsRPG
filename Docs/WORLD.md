# The Long Autumn — World Bible & Naming Conventions

**Status:** v1.1 (2026-08-10). This document is the authority for setting,
names, and flavor. The game's overall design lives in `GDD.md`; implementation
status belongs in `PROJECT_REVIEW.md` and `BATTLE_PLAN.md`; delivery rules live
in `ROADMAP.md` and executable work lives in GitHub Issues.
Lore entries do not imply that the corresponding feature is player-reachable.

---

## 1. The setting in one page

Two centuries ago an engineered catastrophe — the **Severance** — burned out the old
world's networked civilization. Its infrastructure survived it: fabricators, satellites,
war-engines, and buried systems still run on half-understood protocols that people now
invoke like liturgy. "Magic" is **Protocol** — learned command-rites that living minds
address to surviving machine-systems.

Power sits with three kinds of institutions:

- **Charters** — mercenary companies with legal personhood. A charter *certifies* its
  soldiers in formal trades (the job system). The player leads a minor charter.
- **The Cartographic Church** — controls the maps, the archives, and therefore history.
  Where the Church redraws a border or redacts a record, reality follows. (Antagonist
  institution; the unreliable-history engine of the story.)
- **Foundries** — city-states built around still-functioning fabricators.

**Story spine (seeds):** a Foundry succession war fought by proxy charters; the
protagonist's oath-brother on the other side; and beneath it, proof that the Severance
was not an accident — the Church's founding order caused it. The campaign is framed as
a **disputed archive entry**: chapter cards quote the official history, then the battle
contradicts it.

---

## 2. Job roster (23) — redesigned 2026-07-29

This is not a renamed legacy tactics roster: the tree is a **single root with
three certification tracks** and dual-prerequisite capstones. Kits are designed
around The Long Autumn's institutions and battlefield language. Jobs without a
setting-native decision loop were cut. IDs are frozen—display names may change,
ids may not.

### The tree

```
Drifter (root)
├── Field track:   Warden ─→ Brawler ─┐
│                  Warden+Marksman → Skybreaker ─┴→ Relic Blade (capstone)
├── Trade track:   Marksman ─→ Scav ─→ Wraith
│                  Sawbones+Scav → Broker ─→ Balladeer
└── Protocol:      Sawbones ─→ Mender ─→ Burner ─→ Clockhand
                   Mender ─→ Ghostspeaker ─→ Wastewalker
                   Burner+Ghostspeaker → Wakener (capstone)
```

### Mechanical identities

| Id | Job | Plays as |
|---|---|---|
| `drifter` | Drifter | Root generalist: thrown scrap, self-buffs, small heal |
| `warden` | Warden | Melee control tank: Strip (disarm/pin) + self shield-wall |
| `brawler` | Brawler | Fast striker: line quake, self-heal, revive, self-Overclock |
| `skybreaker` | Skybreaker | Vertical burst: 3-height jumps, fire lines, self-Nullgrav |
| `relic_blade` | Relic Blade | MP-fueled duelist capstone: named-blade arts, blast + freeze |
| `marksman` | Marksman | Longest ranges in the game; pin shots, piercing lines |
| `sawbones` | Sawbones | Triage medic: burst heals, Cleanse, sleep/toxin darts, revive |
| `mender` | Mender | Barrier medic: Knit/Bulwark/Firewall/Failsafe + area Mend line |
| `burner` | Burner | Artillery caster: Ember/Rime/Arc tiers, blast radii |
| `clockhand` | Clockhand | Tempo control: Overclock/Throttle/Freeze-Frame/Deadline (the CTR statuses that actually work) |
| `ghostspeaker` | Ghostspeaker | Pure affliction suite: Static→Graycast escalation |
| `wakener` | Wakener | Capstone: **infinite-range** engine strikes, huge MP costs |
| `scav` | Scav | Skirmisher: dirty tricks, scrap bombs, self-Ghosted |
| `wraith` | Wraith | Assassin: sleep/silence blades, Terminus nuke |
| `broker` | Broker | Mind control: Desync/Redline/Swayed, ally Rally |
| `balladeer` | Balladeer | **Board-wide broadcasts** (Full-area buffs/debuffs) |
| `wastewalker` | Wastewalker | Terrain mage: mixed phys/mag, snares, self-centered Tremor |
| `bannerlord` | Bannerlord | Unique — **Aldric Vane**: command auras |
| `hollowed` | Hollowed | Unique — **Corvus Rell**: drain blades, Deadline cuts |
| `oathbreaker` | Oathbreaker | Unique — **Captain Gide Marrow**: contract-law strikes |
| `knife_of_the_church` | Knife of the Church | Unique — **Sister Vesper**: Redact arts, steeple drops |
| `cipherguard` | Cipherguard | Unique — **Ansel Rook**: blade + protocol hybrid |
| `liturgist` | Liturgist | Unique — **Deacon Wray**: sermons that mend and unmake |

Character names on unique jobs are ratified by the current cast sheet in
GDD.md §4.2. Cut jobs (`echo`, `actuary`, `fireband`, `silencer`)
may return later only with mechanics of their own (mimicry, formula targeting, etc.
need engine support first). **Widow Faye** — once the Silencer's owner — survives the
cut as an NPC: Coldwater's information broker.

## 3. Naming conventions (use these for all new content)

- **Spell tiers**: protocol versions — `X`, `X.2`, `X.Prime` (e.g. Ember → Ember.2;
  Mend → Mend.2 → Mend.Prime). Never `-a`/`-aga` suffixes.
- **Elemental lines**: fire = **Ember**, cold = **Rime**, lightning = **Arc**.
- **Job verb signatures** (one verb family per job): Warden **Strip/Sap** ·
  Scav **Lift** · Knife of the Church **Redact** · Cipherguard **Sever** ·
  Echo **Playback:** · Oathbreaker contract-law terms · Wakener engine callsigns
  (Furnace, Glacier, Dynamo, Bedrock, Rampart, Prism, Colossus, Undertow, Courier,
  Nightingale) · Relic Blade named blades (Vigil, Cold Iron, Long Autumn, Rainfall,
  Gathering Storm, Quiet Hour, Hunger, Reaper's Due, First Forge, Scattered Light).
- **System terms** (display vocabulary; mechanics unchanged): JP → **Cert** ·
  job level → **Grade** · gold → **scrip** · Brave/Faith (future) → **Grit/Sync** ·
  KO decay: crystal → **memory-core**, treasure → **salvage**.
- **Register**: western, terse, worn. Avoid Japanese loanwords, FF coinages
  (Esuna/Reraise/Fira), and modern brand-like names.

## 4. What was deliberately NOT renamed (and why)

- **`effects[].status` identifiers in AbilityData** (Poison, Protect, Haste, …) — these
  resolve to C# classes (`PoisonStatus` etc.) at runtime. They are stable
  mechanical ids rather than display text; any player-facing terminology may
  be layered over them without changing saved/data identity.
- **`Common/Attack`** — generic term, referenced by unit-recipe attack paths.
- **Code identifiers** (`jpCost`, `AddJobPoints`, `StatTypes`) — internal API, no
  player exposure. UI strings should say Cert/Grade/scrip when panels get built.
- **Legacy formula-targeting names** (Level Prime, HP Multiple 4, …) may remain
  in historical or unused data. Actuary is not part of the active 23-job roster;
  formula targeting must earn a new implementation slot before returning. Any
  return is proposed and classified through the current-job review
  [#64](https://github.com/shimakaze09/TacticsRPG/issues/64) or the post-slice
  job exploration
  [#68](https://github.com/shimakaze09/TacticsRPG/issues/68) first.

## 4b. Damage & healing conventions (2026-07-29)

- **Damage** = `max(ATK × power/100 − DEF/2, 1)` (MAT/MDF for magical). Power is a
  percentage of the attacking stat: **100 ≈ one stat's worth of damage**. Gear and
  buffs that raise ATK/MAT scale through every ability automatically; DEF mitigates
  flat. Crits, elements, and situational multipliers plug into `TweakDamageEvent`
  after this base — never bake them into power values.
- **Healing** = flat `power ± 10%` (no stat scaling yet). Author heal powers as the
  intended HP restored.
- Baseline tuning targets at job level 1, no gear: basic attacks ~10–15, kit hits
  ~15–25, capstone nukes ~30–45, against HP pools of ~35–110 (2–6 hits to KO).
- **Progression model v2** (`ProgressionModel.cs`, 2026-08-10, issues #52/#54):
  combat stats are always recomputed from job history + character level + gear +
  difficulty, never stored. The **current job dominates**: its kit (base ×
  multiplier) once, **+0.5 kit per grade** earned beyond 1, **+0.25 kit per
  character level** above 1 (spawn/writ levels therefore materially change enemy
  stats). Every **other** unlocked job carries over at most **0.25 of its kit**,
  scaled linearly by training progress — unlocks alone grant nothing. Bands
  (tank/striker/caster/support) stay distinct all campaign, multiclassing
  broadens a build without saturating the 999 caps, and the 3,000–5,000 HP boss
  band comes from authored boss levels/gear rather than stacked job history.
  Party units and generated enemies share the model.
- **Control budget** (`ControlBudget.cs` + `ProgressionModel.ResistanceFor`,
  2026-08-10, issue #57): RES is derived, never authored — **15 base + 0.5 per
  level above 1 + 0.5 per point of the current job's MDF kit**, capped at **75**
  so max-accuracy control always keeps a real chance. Status hit chance is
  clamped to **5–95** outside auto-hit/miss exceptions. Hard-control statuses
  (sleep/stop/disable/immobilize/delayed-KO/charm/confusion/berserk) cap at
  **3 turns** per data-driven inflict, and each landed control grants the
  target a **Steeled** stack (+20 effective RES, 3 turns) so chains hit
  diminishing returns.
- **Tempo statuses** (`OverclockStatus`/`ThrottleStatus`, 2026-08-10, issue #19):
  Overclock multiplies every CT gain by **1.5** (50% more turns-per-clock) and
  Throttle by **0.5**; both read their configurable `ctMultiplier`, clamped to
  the `StatLimits` CT-gain range (**0.25–2.0**) so no data value can freeze a
  unit out of initiative or grant runaway turns. Stacked, they compose
  multiplicatively (×0.75).
- **Global ceilings** (`StatLimits.cs`, enforced in the stat pipeline, job
  recalculation, and the effect clamp): max HP **20,000** (late-game bosses are
  *designed* around 3,000–5,000 — the cap is a wall, not a target), max MP 9,999,
  primary stats 999, and **no single hit or heal ever exceeds 999**. Numbers stay
  small and readable; content tunes under the caps, never by raising them.

## 5. Content pipeline invariants (enforced by the sweep script)

1. Every `abilityUnlocks` entry references an existing AbilityData id — and every
   defined ability is unlockable somewhere in its job's level 1–8 spread.
2. Catalog entries are generated from AbilityData names (never hand-edited).
3. `AbilityData.job == CatalogData.catalogName == JobData.abilityCatalogName`
   (this string is also the generated prefab folder name).
4. Ids are derived from display names **once at creation** and never changed after —
   renames touch display fields only. The 2026-07-29 re-slug (legacy names → Long Autumn
   names) was the final free id change, done before any shipped save existed.
