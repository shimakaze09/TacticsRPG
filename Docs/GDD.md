# The Long Autumn — Game Design Document

**Version 1.0 · 2026-07-31 · Design authority for the whole game.**
Setting detail lives in `WORLD.md`; battle-system status and queue in
`BATTLE_PLAN.md`; execution order in `ROADMAP.md`. Where documents disagree,
this one states intent, WORLD.md owns lore, BATTLE_PLAN owns implementation truth.

**Locked platform/scope decisions:** PC with mouse+keyboard first (controller
post-slice) · 2D sprites on a 3D grid · vertical slice before full campaign ·
all three story seeds as one arc.

---

## 1. Vision & pillars

**One sentence:** FFT's tactical soul in an original post-collapse world —
battles where terrain, turn order, and information are weapons, inside a story
about who gets to write history.

**The player fantasy:** you run a small mercenary charter that is always one
bad contract from dissolving, in a world where "magic" is the liturgy of dying
machines and the map itself is propaganda. You out-think bigger forces, keep
your people alive, and eventually decide what the truth costs.

**Identity anchors (what makes this game itself, not an FFT clone):**

1. **Timeline warfare** — turn order is a visible, manipulable battlefield.
   The initiative bar is a first-class UI element; abilities push, delay, and
   reorder activations; the Clockhand job is its face. (Pillar 1)
2. **Grit reactions** — deterministic build-and-spend reactions instead of
   luck-based counters. Tanks are engines, not lottery tickets. (Pillar 2)
3. **Sync terrain** — network coverage as casting geography: Protocol casters
   are strong where the old world still listens and weak in dead zones, so maps
   are casting terrain, not just walking terrain. (Pillar 3)
4. **The disputed archive** — every chapter opens with the official Church
   record of events, then the player plays what actually happened. The framing
   device *is* the theme.
5. **Honest, legible tactics** — hit chances, damage forecasts, and turn order
   are always shown; statuses always do what they say (already true in the
   engine); numbers stay small and readable (caps: 999 per hit, ~5k boss HP —
   WORLD.md §4b is law).

**Anti-goals:** no number inflation, no gacha-style randomness in progression,
no Japanese-fantasy aesthetic drift (WORLD.md §3 register), no open-world scope.

**References:** FFT (turn economy, job feel), Tactics Ogre (tone), Into the
Breach (legibility, telegraphing), Disco Elysium (institutional unreliability
of records — tone only).

## 2. Game structure & core loop

```
Title ─► Charter Hub ─► Contract Briefing ─► Battle ─► Results & Growth ─► Story beat ─► Hub
              │                                                              
              ├── Roster & Job menu (Cert spending, job switching)            
              ├── Shop (gear, consumables)                                    
              └── Archive (replay story records; disputed entries marked)     
```

- **Charter Hub** is the meta-home: a single screen styled as the charter's
  ledger table. No world-map walking in the slice; contracts are chosen from a
  **board carrying every quest type** — main contracts, short/long side
  stories, repeatable writs, character quests, relic hunts (hidden quests
  never appear until discovered) — see §4.5. Post-slice: a region map with
  branch contracts.
- **Campaign shape:** 3 acts across 5 chapters, ~24–30 battles total. The
  vertical slice is **Chapter 1 complete** (6 battles + its story scenes).
- **Session shape:** a battle is 15–25 minutes; a chapter is an evening.

## 3. Gameplay systems

### 3.1 Battle (status: largely shipped — see BATTLE_PLAN §1)

Shipped and verified: CTR turn economy; composition-based abilities; honest
statuses (per-owner durations, real combat effects, tuned accuracy); damage
`max(ATK×power/100 − DEF/2, 1)` with global caps; line of sight + high ground
(±15% dmg, ±10 hit at ≥2 height); KO decay into memory-cores/salvage with
pass-over corpses; two difficulties — Easy (pattern AI) and Hard (tactical AI
with threat map, hit-and-run, self-preservation, team focus fire, support
discipline); authored battle setup (1.8: BattleDefinition assets, battle
rounds via BattleClock, reinforcement waves, objective types defeat-all /
defeat-target / survive-N-rounds / reach-zone); real terrain (1.8b:
Field/Road/Water/Obstacle/Building/Bridge with per-type pass/stop/sight
law — rivers split maps, bridges reconnect them, trees and buildings block
line of sight; units pass through allies but a standing foe is a wall, so
holding a chokepoint is a real tactic); equipment (1.9: per-job starting
loadouts from GearCatalog,
gear bonuses that survive stat recalculation, shop purchases into
PartyInventory); the gear behavior model (1.9b: weapon reach / fire arc /
attack shape / damage profile plus composable traits on any item — §3.3);
elements and crits (1.10: battle-wide affinity advantage, unit affinities
on the cast, gear element traits live, Doused ignites under fire, and
critical hits that roll only at application so forecasts stay honest).

Planned (queue order): control-seizing statuses (1.11), then the pillars. The
**escort** objective (civilian unit with Guest alliance) lands with the
market-rescue battle in M2 — it needs guest-unit control rules, not just a
victory check.

### 3.2 Jobs & growth (shipped, design ratified here)

23-job roster and tree per WORLD.md §2. Character EXP/levels come from Rank;
job levels come from Cert (JP) earned per activation; stats derive from job
level history. **Design call (resolves BATTLE_PLAN 1.13): Cert buys abilities.**
Job levels *unlock* abilities for purchase; the player spends banked Cert in
the Job menu to learn them (jpCost field becomes live data). Rationale: Cert
is the world's certification currency — buying your license fits; it also adds
a meaningful post-battle decision beyond job switching.

### 3.3 Equipment & economy (design for BATTLE_PLAN 1.9)

- **Slots:** primary (weapon), body (armor), trinket (accessory). Head/shield
  fold into body/trinket to keep the slice lean.
- **Starting gear:** every job's recipe includes a default weapon + body item
  (flat ATK/MAT and DEF/MDF contributions via the existing
  StatModifierFeature). Weapon examples: Warden mace, Marksman slug-thrower,
  Burner focus-coil.
- **Shop tiers:** slice has one shop with 2 tiers (chapter-start, mid-chapter
  restock). Scrip sources: contract pay (battle results), salvage pickups,
  and **writs** (§4.5) — the repeatable valve for players who want to farm
  scrip/Cert/levels instead of pushing forward underleveled.
- **No random drops** in the slice; loot is deterministic (salvage + fixed
  battle rewards). **Legendary tier** gear exists only as relic-hunt rewards
  (§4.5.6): one copy ever, unique passive via the Feature system, above shop
  gear but not outside the stat caps.

**Gear behavior model** (shipped with 1.9): every item is defined along
composable axes, so new gear is data, not new systems —

| Axis | What it does | Examples |
|---|---|---|
| Stats | flat bonuses via StatModifierFeature | +7 ATK mace, +5 DEF plate |
| Reach | basic-attack range (weapons) | dagger 1, pry hook 2, slug-thrower 5 |
| Fire arc | Direct is blocked by standing units and cover; Arcing lobs over both | slug-thrower vs recurve lath |
| Shape | attack footprint: Target (one tile), Line (spray to full reach), Sweep (target + both flanking tiles) | drip-torch line, grief-edge sweep |
| Damage profile | power scale vs coverage: precision >100%, wide <100% | dagger 110%, sweep blades 85%, drip-torch 75% |
| Traits | composable behaviors on ANY gear slot | see below |

**Shipped traits:** **Recoil** (attacker takes X% of each hit it deals —
Two-Head Blade), **WindedAfterStrike** (swinging Throttles the attacker —
Pit Cleaver), **PhysicalResist/Weakness** (armor shapes incoming physical
damage — Rattan Jacket), **FlankBonus** (+X% striking from behind — every
knife), **StatusOnHit** (X% chance to inflict a status — Static Knife
statics, Drip-Torch douses), **Lifesteal** (Grief-Edge feeds its bearer),
plus **minimum range** as a weapon property (the Slug-Thrower can't fire
inside 2 — get inside the gun). Weapon on-hit and conditional traits run
from the Attack ability; defensive traits ride the item as Features and
hook the same TweakDamage stage statuses use. Attackers turn to face their
target when acting, so facing rules and visuals always agree.

**The full design space** — gear behavior families and when they land
(each is data + one hook, never a new system):

| Family | Examples | Lands with |
|---|---|---|
| Conditional damage | execute (Pit Cleaver), opener (Wrapped Knuckles), terrain-conditional (Dowsing Staff on water), crit bonuses (Absolution Point); anti-armor/anti-caster still open | **1.10 shipped** (anti-* with 1.11's resource attacks) |
| Element interaction | affinity advantage law battle-wide, element resist/weak armor (rattan burns), Doused ignites under fire | **1.10 shipped** (branded weapons = content) |
| On-hit statuses, resource attack | more StatusOnHit gear, MP burn, armor shred (stacking DEF-down) | **1.11** (behavior statuses expand the status roster) |
| Timeline gear | CT-push weapons (knock the target's turn later), CT-cost or recharge modifiers, initiative trinkets | **Pillar 1** (timeline warfare) |
| Reaction gear | counter weapons, thorns armor, Grit-gain trinkets, reach weapons that deny counters | **Pillar 2** (Grit reactions) |
| Forced movement | knockback mauls, the Pry Hook PULLING its target a tile, repositioning shields | battle polish (#28 — needs push/pull resolution + AoE previews) |
| Mobility gear | +MOV boots, water-walking waders, nullgrav harness (fly traversal), jump kits | with terrain art pass / M2 shop tiers |
| Auras & wearer passives | the Charter Standard buffing adjacent allies (it IS a banner), regen mesh, status-immunity trinkets, one-time death protection (Failsafe exists) | M2 (needs aura recompute + trinket slot content) |
| Meta/economy | set bonuses (maker's marks), shop upgrade paths, cursed/bound relic gear (can't unequip, big drawback) | M2 shop rebuild + relic hunts |

**Anti-goals:** no durability/weapon breaking (friction without decisions),
no random affixes (loot stays deterministic, §above), nothing that breaks
the WORLD §4b caps.

### 3.4 Difficulty (shipped)

Easy = classic pattern AI, no scaling — the story difficulty. Hard = tactical
AI + enemies at 130% HP / 120% damage — the "feel hunted" difficulty.
Selection moves into Settings when the title screen lands (M1).

## 4. Storytelling

### 4.1 Frame

The whole game is presented as **Archive Entry 44-C, disputed** — a Church
record under review. Every chapter opens with a **chapter card**: the official
record in serif type on worn paper ("The Kestrel Charter burned the Coldwater
granary and fled south."), stamped and tidy. Then the player plays the truth.
At campaign end, the player's final choice decides which version enters the
archive.

### 4.2 Cast

| Character | Job (unique) | Role |
|---|---|---|
| **Rhen Calder** (player captain) | starts Drifter | Captain of the Kestrel Charter; pragmatist forced into historiography |
| **Aldric Vane** | Bannerlord | Rhen's oath-brother, co-founded Kestrel, left for the Ironquill Charter; the Delita-shaped mirror |
| **Captain Gide Marrow** | Oathbreaker | Ironquill's captain; treats contract law as a weapon; Act 2 betrayer |
| **Sister Vesper** | Knife of the Church | Cartographic Church redactor sent to "correct" the record — and the charter |
| **Ansel Rook** | Cipherguard | Church "observer" attached to the succession; Chapter 1 boss; later defector |
| **Deacon Wray** | Liturgist | Voice of the Church's founding order; Act 3 antagonist |
| **Corvus Rell** | Hollowed | What came back from a Severance zone; joins mid-game as living evidence |
| Widow Faye | (NPC) | Coldwater information broker; recurring neutral |
| Salome & Imre Voss | (NPCs) | The heirs; each holds half the fabricator boot-key |

### 4.3 Arc — branching structure and endings

The spine is fixed (succession war → redaction → Severance lie), but three
**choice points** branch the path and combine into **four endings**. Choices
are recorded as story flags in the save (PlayerProgress) from Chapter 1 on.

- **Act 1 — The Ledger of Coldwater Crossing (Ch. 1, the slice):** Foundry-Lord
  Voss dies; heirs Salome and Imre each hold half the boot-key to Coldwater's
  fabricator. The hungry Kestrel Charter signs with Salome; Ironquill (Marrow,
  with Vane in its colors) serves Imre. Skirmishes escalate to the fabricator
  hall, where Church observer Rook intervenes to seize the ledger "for
  neutrality."
  **Choice A (chapter close, in the slice):** hand the recovered ledger to
  **Salome**, to **Imre**, or **keep it**. Determines Act 2's employer, maps,
  and which heir survives; "keep it" starts Act 2 charterless and hunted —
  hard mode narratively, extra pay mechanically.
- **Act 2 — The Redaction (Ch. 2–3):** the war spreads; Marrow voids contract
  law at Vesper's direction; Kestrel obtains an un-redacted map.
  **Choice B (mid-Act 2): read the map or burn it.**
  *Read* = branded heretic, the truth-seeking path (Rell joins early, Vesper
  hunts you). *Burn* = stay legal, the company-man path (Church contracts,
  better pay, Vane's respect — and complicity). Both paths converge on the
  Vane confrontation duel that ends Act 2, but who stands beside you differs.
- **Act 3 — The Severance Lie (Ch. 4–5):** Rell's testimony + Rook's defection
  prove the Church's founding order *caused* the Severance. Wray moves to bury
  everyone who knows. Finale at the First Archive.
  **Choice C (finale):** what happens to the proof —
  **publish · suppress · seize** (use it as leverage to raise Kestrel into a
  great charter — the Delita option).

**Endings matrix (4):**

| Ending | Requires | Tone |
|---|---|---|
| **The True Archive** | read the map (B) + publish (C) | The world convulses on the truth; the archive entry the player has been reading is finally corrected. Costly, honest. |
| **The Quiet Autumn** | any path + suppress (C) | Peace bought with the lie; the game's opening chapter cards *were* this ending's world. Melancholy loop-closer. |
| **Charter Ascendant** | any path + seize (C) | Kestrel becomes a great power holding the Church by the throat; Rhen becomes what Marrow was. Dark mirror. |
| **The Good Servant** | burn the map (B) — Act 3 plays as Church retainers; C is offered by Wray as a *test* | Refuse or fail it and the record stands exactly as written; the player realizes they played the official version. Bleakest, and the shortest Act 3. |

Choice A colors all endings via epilogue cards (which heir rules Coldwater,
or neither) rather than forking whole ending scenes — branch cost stays sane.

### 4.4 Delivery

Chapter cards (new UI) · pre/post-battle conversations (ConversationController
exists) · 2–3 scripted mid-battle events per chapter (reinforcement + dialogue
triggers — needs a small battle-event hook in 1.8) · the Archive screen for
rereading records with truth/record toggles (post-slice).

### 4.5 Side content — quest taxonomy

Everything below appears on the Charter Hub's **contract board**, each type
with its own stamp/icon. All quest state lives in save-file story flags.

1. **Side stories — short (6–8 across the campaign):** one battle plus scenes;
   self-contained vignettes that flesh out the world (e.g. *The Last
   Ferryman* — an old man defends a crossing the Church already erased from
   the maps; *A Wager of Rust* — two Foundry gangs duel over a machine that
   turns out to be someone's grandmother's memory-core). Rewards: scrip, a
   rare item, sometimes a recruit.
2. **Side stories — long (2):** multi-battle chains with their own mini-arc:
   - ***The Hollow Road*** (3 battles): following Corvus Rell's back-trail
     into a Severance zone; horror-tinged; explains the Hollowed and seeds
     Act 3 evidence. Unlocks after Rell joins.
   - ***Faye's Ledger*** (4 battles): Widow Faye's information war — every
     job pays in secrets, and the finale reveals she has been selling to
     Vesper all along; the player chooses to cut her off or turn her double.
     Feeds an epilogue card.
3. **Writs — repeatable farming contracts:** parameterized skirmishes
   ("Clear the toll road", "Escort the grain convoy") that reuse the battle
   generator with randomized spawns; enemies scale to average party level;
   standard pay in scrip + Cert. Always available, deliberately unglamorous —
   the grind valve for players who want to out-level rather than out-think.
   (The old random test spawner literally becomes this system in 1.8.)
4. **Hidden quests (4–6):** never listed on the board; discovered by play —
   ending a battle on a rumor tile, keeping a "doomed" guest alive, revisiting
   a map after a story flag, reading the right archive entry. Rewards skew
   weird and lore-heavy (a dead charter's banner, a corrupted memory-core that
   whispers, the game's one joke quest).
5. **Character quests (6, one per unique cast member):** 1–2 battles each,
   unlocked by story progress + having the character deployed; deepens them
   and unlocks their **signature ability or gear** (e.g. Vane's quest ends
   with *Hold the Line* gaining its banner aura; Rook's post-defection quest
   unlocks *Old Codes*). Vesper's exists only on the read-the-map path;
   Marrow gets a posthumous one — you play his last clean contract as a
   flashback.
6. **Relic hunts — one-time legendary gear quests (6):** hard, condition-laden
   battles, each awarding a **named relic** — and the names are already
   canon: the Relic Blade job "draws power from named old-world blades," and
   these are those blades and their peers made real equipment:
   - *Vigil* (blade) — hunt: a vault that only opens during an enemy turn
     (timeline-warfare puzzle)
   - *Hunger* (blade) — hunt: win without healing
   - *The Surveyor* (slug-thrower) — hunt: kill the target from maximum range
   - *First Lantern* (focus-coil) — hunt: a dead-zone battle where Protocol
     costs double
   - *Doorwarden's Plate* (armor) — hunt: hold a gate for 8 rounds
   - *Faraday Shroud* (armor) — hunt: survive a Wakener bombardment map
   Legendary tier sits above shop gear with a unique passive each (via the
   Feature system); one copy ever, marked in the archive when found.

**Slice scope for side content (M2):** one short side story (*The Last
Ferryman*), writs unlocked after battle 2 (proving the repeatable system),
one hidden quest seed (rumor tile in battle 4's rooftops), and Choice A at
chapter close writing its story flag. Everything else is post-slice content
on proven systems.

## 5. Vertical slice — Chapter 1 definition

Six battles, three biomes, every core system showcased once.

| # | Battle | Map/biome | Objective | Enemy roster | Teaches / showcases |
|---|---|---|---|---|---|
| 1 | Toll Road Ambush | Autumn road gulch | Defeat all | 2 Drifters, 2 Scavs (bandits) | Move/act/facing, camera; safe tutorial |
| 2 | The Coldwater Bridge | River + single bridge, parapets | Defeat all | Warden, 2 Marksmen, Ghostspeaker (Ironquill) | Water blocks walkers, chokepoint, LoS cover |
| 3 | Night Market Rescue | Market district, props | Escort 2 civilians to exit zone | Scavs + Wraith (opportunists) | Escort objective, guest units, alleys |
| 4 | Rooftop Signals | Rooftops, big height deltas | Defeat target (signal officer) | Marksmen + Skybreaker | High ground rules, vertical movement, ledge salvage |
| 5 | The Sunken Approach | Marsh, dead water | Survive 6 rounds then extract | Ghostspeakers + Wastewalker | Status defense, terrain slog, first Sync-terrain *flavor* (visual only) |
| 6 | The Fabricator Ledger | Fabricator hall interior | Defeat Ansel Rook | Rook (Cipherguard boss) + Warden/Mender guard | Boss with turn manipulation (Sever Tempo/Freeze-Frame); initiative bar literacy |

Story scenes: opening card + hub intro; pre-2 (Vane reveal at the bridge);
post-3 (Faye deal); pre-6 (Rook parley); post-6 (ledger seizure, chapter-close
card contradicting what the player just did).

**Slice explicitly defers:** Grit reactions, Sync terrain mechanics (visual
tease only in battle 5), controller support, Archive screen, world-map hub.

## 6. Controls & input (mouse + keyboard)

| Action | Mouse | Keyboard |
|---|---|---|
| Move cursor / inspect | hover | arrows / WASD (menu context) |
| Select / confirm | left click | Enter / Space |
| Cancel / back | right click | Esc |
| Camera pan | edge push / middle-drag | WASD (field context) |
| Camera rotate 90° | — | Q / E |
| Camera zoom | wheel | — |
| Cycle allied units | — | Tab |
| Command hotkeys | — | 1 Move · 2 Act · 3 Wait |
| Turn order peek | hover initiative bar | hold T |
| Unit details | click portrait / R-click unit | R |
| Speed up AI turns | — | hold Shift |

Hover always forecasts: movement range on hover-select, damage/hit% on
hover-target. **Engineering note:** the current InputController is
keyboard-axis only and fires static events; the M3 controls rework replaces it
with an action-map layer (Unity Input System) implementing this table — until
then the existing keyboard flow remains the dev-testing path.

## 7. UI/UX — screens and style

### 7.1 Style guide

- **Motif:** worn archive paper + signal glitch. Meta screens are the *ledger*
  (paper, stamps, marginalia); battle HUD is the *field instrument* (dark
  glass, teal phosphor). Chapter cards bridge the two.
- **Palette:** autumn ochres/umber + off-white paper for meta; deep slate with
  **teal Protocol glow** (#39C6C0 family) for battle; danger = ember orange;
  healing = pale gold. One accent per faction (Kestrel teal, Ironquill rust,
  Church bone-white).
- **Type:** humanist serif for archive/story text; condensed grotesque sans
  for HUD numbers and labels (TextMeshPro; two font assets total).
- **Iconography:** single-weight line icons; statuses get 24px glyphs with
  duration pips; jobs get crest-style badges.

### 7.2 Screen inventory (slice)

| Screen | Contents | Status |
|---|---|---|
| Title | logo, Continue/New/Settings/Quit; archive-stamp animation | new (M1) |
| Settings | difficulty (moves here from editor menu), volume sliders, resolution | new (M1) |
| Charter Hub | ledger table: roster strip, Contracts, Jobs, Shop, Save | new (M1, minimal) |
| Briefing | contract card: objective, pay, enemy intel silhouettes, deploy count | new (M2) |
| **Battle HUD** | initiative bar (top, portraits in activation order — Pillar 1 seed); active-unit card (HP/MP/CT, statuses); command menu (exists); **forecast panel** on target hover: damage range, hit%, kill flag (Predict already computes); floating damage/heal numbers; status popups | partial (menus exist; bar/forecast/popups are M1) |
| Results | existing panel + Cert earned per unit, level/job-level ups, salvage tally | exists, extend (M2) |
| Job menu | per unit: job tree, switch job, **buy abilities with Cert** (3.2) | code exists, canvas new (M1/M2) |
| Shop | buy/sell lists, gear compare vs equipped (1.9) | rebuild of demo shop (M2) |
| Save slots | 3 slots + autosave, chapter/playtime stamps | new (M3; single-file today) |

## 8. Art direction (2D sprites on 3D grid)

- **Characters:** ~48px-tall sprites at 3/4 view, billboarded; **4 facings**
  (mirror E/W); palette-swap friendly (faction accents). Core animation set
  per job: idle (2f), walk (4f), attack (4f), cast (4f), hit (1f), KO (2f) —
  ~17 frames per job sprite; slice needs 10 player jobs + 6 enemy variants +
  Rook = a bounded, costable sheet list.
- **Portraits:** bust portraits, painted-sketch over flat color, ~256px, for
  the 9 cast members + generic soldier faces per faction.
- **Terrain:** 3 slice tile kits (autumn road, Coldwater town, marsh) of
  12–16 tiles each + 8–10 props per kit (carts, parapets, stalls, antennae,
  reed clumps). Tiles remain simple extruded blocks with painted tops/sides —
  height stays readable, which is the FFT trick that matters.
- **VFX:** sprite-sheet effects (slash arcs, protocol glyphs, smoke) plus a
  teal scanline shader for anything "Protocol"; damage numbers use the HUD sans.
- **Production stance: placeholder-first.** Systems and battles ship on
  primitives/proxy sprites; art lands as batches replace proxies (M2). Options
  costed in M2: commission vs curated asset packs vs hybrid — decision then,
  not now.

## 9. Audio direction

- **Direction:** *tape-warped orchestra over analog synth pulses* — strings
  and brass recorded "off an old reel," with sequenced synth underneath for
  Protocol presence. Percussion: field drums + machine ticks (timeline motif).
- **Slice track list (9):** Title ("The Archive Opens") · Hub ledger theme ·
  Briefing sting · Battle A (field) · Battle B (urban) · Boss (Rook — clock
  ticks in 7/8) · Victory sting · Defeat sting · Story motif ("The Long
  Autumn," reprised in chapter cards).
- **SFX categories:** UI (paper/stamp/tick), movement (footfalls per biome),
  impacts (phys/protocol split), statuses (apply/expire), ambience per biome.
- **Implementation:** existing MusicPlayer/AudioSequence handles intro+loop
  music; SFX through a light one-shot pool (M2); mix pass in M3.
- **Sourcing:** licensed packs acceptable for SFX; music decision (commission
  vs licensed vs produced) deferred to M2 with the art call.

## 10. Technical gap map

| GDD feature | Existing system | Gap (queue item) |
|---|---|---|
| Authored battles, objectives | BattleDefinition + BattleSpawner + BattleClock + 4 victory types (**1.8 shipped**) | escort objective (M2, with market rescue) |
| Terrain types, bridges, water | TerrainType + TerrainRules; terrain-aware movement/LoS/spawning; BoardCreator painting (**1.8b shipped**) | biome art passes per map (M2) |
| Equipment | GearCatalog + per-job loadouts + PartyInventory, recalc-safe bonuses (**1.9 shipped**) | equip/compare UI in job menu (M2); gear art |
| Cert buys abilities | jpCost in data, AbilityMemory | **1.13** (per §3.2 call) |
| Initiative bar | TurnOrderController (no UI) | Pillar 1 seed (M1) |
| Forecast panel / damage popups | Predict(), HitRate.Calculate() | Battle polish (M1) |
| Charter Hub / Title / Settings | GameFlow states (stubs), no canvases | Meta UI (M1 minimal, M2 full) |
| Chapter cards / mid-battle events | ConversationController; BattleEvents hook (**1.8 shipped**, runs reinforcement waves) | story triggers on the hook + card UI (M2) |
| Quest board & story flags | PlayerProgress.cs (empty placeholder), GameData | QuestDefinition data + flag store + board UI (M2) |
| Repeatable writs | InitBattleState's writ spawner (**1.8**: the no-definition fallback path, documented as GDD §4.5.3) | level scaling + reward hookup (M2) |
| Branching story / endings | — | flag-gated contract availability + ending resolver (M2 flags, post-slice content) |
| Hidden quest triggers | battle-event hook (1.8) | rumor tiles + condition checks (M2 seed, post-slice full) |
| Legendary gear | Feature system (1.9) | legendary tier + unique passives (post-slice, quests one-time-flagged) |
| Scrip in save | Bank in PlayerPrefs | **1.12** |
| Save slots | single-file DataPersistenceManager | M3 |
| Controls table | keyboard-axis InputController | M3 rework (Input System) |
| Sprites/portraits/tiles | primitives | M2 art batches |
| Music/SFX | MusicPlayer/AudioSequence, no content | M2 audio batch |

## 11. Production plan

- **M0 — Systems hardening** *(current queue, in progress)*: BATTLE_PLAN
  1.8–1.13 + docs passes B–D + tech debt as touched. Exit: an authored battle
  can be defined in data and played clean on both difficulties.
- **M1 — Playable loop** : Title/Settings/Hub (minimal), initiative bar,
  forecast panel + damage popups, job menu canvas, battle 1 & 2 authored and
  playable end-to-end from Title. Exit: "new game → two battles → growth →
  save/continue" without the editor.
- **M2 — Slice content**: all 6 battles, all story scenes + chapter cards,
  shop, Cert-buys-abilities, **quest board + story flags + Choice A**, the
  writ generator, one short side story, one hidden-quest seed, first art
  batches (sprites/portraits/tiles for the three biomes), music/SFX first
  pass. Exit: Chapter 1 complete with proxy-free core cast.
- **M3 — Slice polish**: Input System controls rework, audio mix, difficulty
  tuning pass (BATTLE_PLAN §4), save slots, performance/UX pass, external
  playtest. Exit: shippable demo build.
- **Post-slice:** Acts 2–3 branching content and the four endings · remaining
  side stories (short set + *The Hollow Road* + *Faye's Ledger*) · hidden and
  character quests · relic hunts + legendary gear · Grit reactions (Pillar 2)
  · Sync terrain (Pillar 3) · in-battle salvage play (Pillar 4) · Archive
  screen · controller support · region-map hub.

**Top risks:** sprite production volume (mitigation: placeholder-first, 4
facings via mirroring, palette swaps) · scope creep from pillars (mitigation:
slice defers 2 of 4 by design) · solo bandwidth (mitigation: M-gates are
playable builds, each independently stoppable).
