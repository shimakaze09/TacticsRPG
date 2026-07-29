# The Long Autumn — World Bible & Naming Conventions

**Status:** v1 (2026-07-29) — established by the re-theme content sweep. This document is
the authority for names and flavor. Mechanics are unchanged from the FFT-style systems;
only expression (names, fiction, terminology) is original.

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

Not a renamed FFT roster: the tree is a **single root with three certification tracks**
and dual-prerequisite capstones, kits were designed per job from scratch, and the pure
FFT analogs (Mime, Arithmetician, Dancer, Assassin) were **cut**. IDs are frozen —
display names may change, ids may not.

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

Character names on unique jobs are **placeholders** until the story cast is finalized.
Cut jobs (`echo`, `actuary`, `fireband`, `silencer`) may return later only with
mechanics of their own (mimicry, formula targeting, etc. need engine support first).

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
  resolve to C# classes (`PoisonStatus` etc.) at runtime. Renaming them is a *code*
  change (class renames + display-name layer), scheduled as its own pass. Until then,
  status names are mechanical ids, not display text.
- **`Common/Attack`** — generic term, referenced by unit-recipe attack paths.
- **Code identifiers** (`jpCost`, `AddJobPoints`, `StatTypes`) — internal API, no
  player exposure. UI strings should say Cert/Grade/scrip when panels get built.
- **Actuary's formula names** (Level Prime, HP Multiple 4, …) — mechanically
  descriptive, kept as-is.

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
   renames touch display fields only. The 2026-07-29 re-slug (FFT names → Long Autumn
   names) was the final free id change, done before any shipped save existed.
