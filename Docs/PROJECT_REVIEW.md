# TacticsRPG — Project Review, Structure Audit & Originality Roadmap

**Date:** 2026-07-29 · **State reviewed:** `main` @ `9e817a7` + uncommitted Unity 6 fix batch
**Companion doc:** `Docs/CODE_AUDIT.md` (2026-07-28) — the line-level bug audit. This document does not repeat it; it verifies what changed since, reviews the *structure* of the codebase, and adds the piece no prior doc covers: how to keep the FFT *feeling* while making the game's content and world your own.

---

## 1. What this project is (full understanding)

A Final Fantasy Tactics-style tactical RPG in Unity 6 (6000.5.5f1, URP). ~280 C# scripts / ~19,300 lines. The foundation is the well-known Liquid Fire tactics tutorial architecture, substantially extended with an FFT-style job system, JSON-driven content, save/load, a smarter (unwired) AI, and a meta-game flow layer.

### How a battle actually runs

1. **Entry**: `StartMenu.unity` hosts `GameFlowController` (a `StateMachine`), which loads `Battle.unity`. (`Battle` can also be played directly.)
2. **Setup**: `InitBattleState` builds the board from `LevelData`, spawns units through `Factory/UnitFactory` (recipe → primitives + `Stats`, `Rank`, `Equipment`, `JobManager`, `Driver`, movement, AI), and installs a victory condition.
3. **Turn loop**: `TurnOrderController` runs FFT-style CTR ticks (`SPD` accumulates into `CTR`; act/move costs deduct). The active unit's `Driver` routes to either human input states (`SelectUnitState` → `CommandSelectionState` → `MoveTargetState`/`ActionSelectionState` → `AbilityTargetState` → `ConfirmAbilityTargetState` → `PerformAbilityState`) or `ComputerPlayer.Evaluate()`.
4. **Ability resolution**: an Ability prefab is a *composition* of small components — `AbilityRange` (constant/line/cone/self/infinite) + `AbilityArea` + `AbilityEffectTarget` filters + `HitRate` (A-type/S-type/full) + `BaseAbilityPower` (physical/magical/weapon) + `BaseAbilityEffect` (damage/heal/inflict/revive/absorb). Effects talk to stats through the event bus (`GetAttackStatEvent`, `GetPowerEvent`, …), so powers/statuses modify outcomes by subscribing, not by inheritance. This is the best part of the codebase.
5. **Stats pipeline**: `Stats` publishes `StatWillChangeEvent` (interceptable via `ValueModifier`s) and `StatDidChangeEvent`. `Health`/`Mana` clamp; `Rank` maps EXP↔level; statuses live as child GameObjects under a `Status` component with condition objects deciding expiry.
6. **Jobs**: `JobManager` (per unit) + `JobDefinition` ScriptableObjects (generated from `Assets/Resources/JobData/*.json` by editor tools) + `JobProgressData` (per-job JP/levels, FFT-style: stats = base + Σ(job levels × job multipliers)) + `AbilityMemory` (learned abilities). Post-battle, `PostBattleController` + `JobMenuPanelController` handle job switching.
7. **Persistence**: `DataPersistenceManager` scans `IDataPersistence` implementers, serializes `GameData` to JSON via `FileDataHandler`.
8. **Meta**: `GameFlowController` (Title→World→Battle→PostBattle→Shop states) is the intended spine; a second, older `GameStateManager` singleton coexists (see §3).

### Content pipeline (important to understand)

JSON in `Assets/Resources/{JobData,AbilityData,CatalogData}` → three editor menu commands (`Tactics RPG → Create FFT …`) → generated prefabs/ScriptableObjects in gitignored `Resources` folders. **The generated assets are not committed**; after any clone or JSON change you must re-run the generators (documented in README).

---

## 2. Delta since CODE_AUDIT.md (what's fixed, what isn't)

### Fixed since the audit snapshot

- **Phase 0 items C1–C8** (commit `59d9ebb`): build-breaking editor using, scene list, README generation docs, defense-check bug, status name resolution + duration, event bus registration + shop recursion, cutscene input, KO turn-skip.
- **Unity 6 migration** (commit `9e817a7` + this session): packages modernized to editor minimums, dead `vr` module removed, all `InstanceID`-era API errors/warnings resolved, obsolete `FindFirstObjectByType`/`FindObjectsSortMode` swapped, serialization-analyzer warnings addressed.
- **Legacy `Job` component deleted** (this session) — the audit's "dual job systems" risk is gone. `statOrder` now lives on `JobManager`; `WeaponAbilityPower.UnarmedPower` reads `JobManager.CurrentJob.baseStats` (was a guaranteed NRE).

### Fixed later the same day (2026-07-29 fix batch)

The following items from the table below were fixed after this review was written:
unarmed-attack NRE; hit-roll bounds; Mana clamp vs MMP; JP off-by-one; MOV/JMP/EVD drift;
UnitFactory component order and junk-GameObject fallback; unique-job unlock check;
mid-battle full-heal on recalc; persistence apply-on-register + JobManager registration +
New Game reset + atomic save writes with backup/quarantine + hero-only save gating;
GameStateManager deleted (GameFlowController is the single flow authority, results payload
ported into `NotifyBattleEnded(BattleResultsData)`); and **stable IDs** across JobData/
AbilityData/CatalogData JSON, generators, JobDefinition, JobProgressData (`currentJobId`),
and AbilityMemory. Job assets are now generated as `Jobs/{id}.asset`.

**Re-theme sweep executed the same evening** — the §5 blueprint is now live: all 27 jobs
and 240 abilities renamed to the "Long Autumn" palette (see `Docs/WORLD.md`), ids
re-slugged from the new names (final free id change), every unlock list rebuilt from
AbilityData (the 35 broken references and all orphans are gone), catalogs regenerated
from ability data (Arithmetician mismatch fixed), unique jobs assigned placeholder
owners, editor generators renamed (`Tactics RPG → Generate Content → …`), and the full
pipeline regenerated + verified in-editor (`Resources.Load` probes pass, 0 console
errors). Status identifiers in `effects[].status` are intentionally unchanged (coupled
to C# status classes — later pass, documented in WORLD.md §4).

### Verified still open — highest-impact items (as of the review; see note above)

| Priority | Finding | Ref |
|---|---|---|
| **P1** | `WeaponAbilityPower.PowerFromEquippedWeapon` still NREs for unarmed units: `Equipment.GetItem(Primary)` returns null (nothing ever calls `Equip()`), and `item.GetComponentsInChildren` runs before the unarmed fallback. Every basic attack by an unequipped unit throws. *(re-verified today)* | `WeaponAbilityPower.cs:24-25` |
| **P1** | Persistence: job progress never registered for save; loaded EXP never applied to spawned units; New Game overwrites saves with stale state; `currentJob` doesn't survive serialization. | audit §6 |
| **P1** | The entire write-time buff/debuff family (Protect/Shell/Berserk/…) is inert because combat reads stats raw. | audit §4 |
| **P1** | Status condition parenting bugs (`GetComponent` vs `GetComponentInChildren`) → orphaned conditions, Doom always KOs. | audit §4 |
| **P1** | Job system: JP off-by-one (phantom level 9), MOV/JMP/EVD drift on every recalc, component-order kills level-up JP, unique jobs unlock for everyone. | audit §5 |
| **P1** | Data integrity: 5 jobs' unlock lists reference abilities that don't exist; Arithmetician catalog mismatch; `jpCost` unused. | audit §5 |
| **P2** | Scene wiring: `Battle.unity` lacks `UIManager`/`GameStateManager`/`PostBattleController` → post-battle flow unreachable; `StartMenu.unity` has no canvas → title dead-ends. Seven `GameSystem/UI` controllers are 0-byte files. | audit §6 |
| **P2** | `SmartComputerPlayer` (632 lines) still dead code, still can't be wired (type mismatch with `BattleController.cpu`). | audit §7 |

The audit's Phase 1→4 remediation order remains the right plan. Nothing in the Unity 6 upgrade invalidated it.

---

## 3. Structure review (new findings)

These are architecture/organization issues, not line-level bugs:

1. **No namespaces** — 279 of 280 files sit in the global namespace (only `Utils.SerializableDictionary` is namespaced). At ~19K lines this is starting to hurt: name collisions are already real (see next item). Recommendation: introduce top-level namespaces mirroring folders (`TacticsRPG.Battle`, `TacticsRPG.Jobs`, `TacticsRPG.Persistence`, …) in one mechanical pass.
2. **Two different `SerializableDictionary` classes with the same name** — `Utils.SerializableDictionary` (Tools, 543 lines, full `IDictionary` + drawer) and the global one (`Data Persistance/Serializable Types`, extends `Dictionary`, used by `GameData`). Only the namespace accident makes this compile. Consolidate on one.
3. **Two competing meta-flow singletons** — `GameFlowController` vs `GameStateManager` (audit §6). This is the single most important structural decision to make before building more meta-game. Recommendation stands: keep `GameFlowController`, port the results payload, delete the other.
4. **Three reward implementations** (`ExperienceManager` [dead], `PostBattleController.AwardRewards`, `GameFlow/PostBattleState` stubs) — pick one.
5. **Folder naming** — "Data Persistance" (sp), "View Model Component" (it's not MVVM — these are gameplay components; consider `Components/` or `Battle/`), "Exceptions" (contains stat *modifiers*, not exceptions — tutorial jargon worth renaming to `Modifiers/`).
6. **String identity everywhere** — units, jobs, abilities are cross-referenced by display name in JSON, save data, and code (`FindJobByName("Squire")`). This blocks both save stability *and* the re-theming you want (renaming a job breaks saves and unlock trees). Fix once with stable IDs (§5.2) and both problems disappear.
7. **No assembly definitions** — fine at this size, but editor/runtime separation currently relies on magic folders. When compile times grow, split `Editor`, `Runtime`, `Tests` asmdefs.
8. **No tests at all** — the `test-framework` package is installed, zero test files exist. The event-driven combat pipeline is exactly the kind of code that regresses silently (the audit's C4/C5 lived undetected in the hot path). Even 20 play-mode-free tests around `Stats`/modifiers/`JobProgressData`/hit-rate math would pay for themselves immediately.
9. **Mixed event idioms** — the custom `GameEventBus` (typed pub/sub with senders) coexists with static C# events (`InputController.moveEvent`, `ConversationController.completeEvent`). Acceptable, but document which is for what; static events outlive scene loads and already caused audit findings.
10. **Editor tooling is FFT-branded** — `FFTJobCreator`, `FFTAbilityCreator`, `FFTAbilityCatalogCreator`, menu path `Tactics RPG → Create FFT …`. Harmless mechanically, but it hard-codes the source material into your pipeline names; rename alongside the content pass (§5).

---

## 4. FFT-likeness audit — where the project *is* FFT today

You said: FFT **feeling**, not FFT **content**. Here is exactly where the line currently sits.

### Mechanics (fine to keep — this is the "feeling")

CT/speed-based turn order, move+act turn economy, height/facing rules, job trees with prerequisites, per-job levels and JP, learned-ability persistence across jobs, stat growth via job multipliers, reaction/support/movement passive slots (planned), charge-time casting (planned), permadeath countdown on KO. Game *rules* are not protectable expression, and every tactics game shares this DNA (Tactics Ogre, Triangle Strategy, Fell Seal, Symphony of War). **Keep all of it.**

### Expression (currently verbatim FFT — must diverge)

- **All 20 generic job names** are the WotL roster verbatim: Squire, Chemist, Knight, Archer, Monk, White/Black/Time Mage, Summoner, Thief, Orator, Mystic, Geomancer, Dragoon, Samurai, Ninja, Arithmetician, Bard, Dancer, Mime. The 6 "unique" jobs (Ark Knight, Deathknight, Fell Knight, Nightblade, Rune Knight, Celebrant) are FFT/Ivalice character-class names.
- **Ability names**: `Fire/Fira`, `Blizzard`, `Bio`, `Drain`, Monk's exact WotL list (`Chakra`, `Aurablast`, `Wave Fist`, `Earth Slash`, `Purification`, `Pressure Point`), Arithmetician's `Height Prime`/`CT Prime`/`Level Multiple`, etc. — hundreds of Square Enix names in the JSON.
- **Status names**: Protect, Shell, Haste, Reraise, Esuna, Toad, Chicken, Faith/Innocent, Doom, Vampire, Float, Berserk, Oil — the FF status vocabulary, including FF-specific coinages (Reraise, Esuna).
- **System terms**: "JP", crystal/treasure KO outcomes, Brave/Faith (planned).
- **Prerequisite tree**: audit §5 verified it's largely WotL-accurate — i.e., the *data* is FFT's data.

**Practical rule of thumb** (not legal advice): numbers, formulas, and systems → yours to keep. Names, ability/status vocabulary, character/class identities, story, and the specific composition of the job tree → rewrite. Distinctive coined terms (Esuna, Reraise, Fira, Ivalice-specific class names) are the highest-risk, lowest-value things to keep — and renaming them is pure data work.

---

## 5. Re-theming blueprint — a western cyber/fantasy/endworld frame

You want western-flavored, possibly combining cyber + fantasy + end-of-world. The good news: your architecture makes re-theming almost entirely a *data* problem — the JSON files and generator are the single choke point for content names.

### 5.1 A setting frame that supports all three flavors (proposal — take or adapt)

> **Working title: "The Long Autumn."** Two centuries after an engineered catastrophe (the *Severance*) burned out the old world's networked civilization, its infrastructure keeps running on half-understood protocols people now treat as liturgy. "Magic" is *Protocol* — invoking surviving machine-systems through learned command-rites. Nation-states are gone; power sits with **Charters** (mercenary companies with legal personhood — your job-system frame), the **Cartographic Church** (controls the maps and the archives — history's gatekeeper, your FFT church-analog), and **Foundries** (city-states built around still-functioning fabricators). The player leads a minor charter caught between a succession war and a buried truth about what the Severance actually was.

This gives you: FFT's political intrigue + unreliable-history theme (chronicle frame: the game is presented as a *disputed archive entry*), a reason job "classes" are formal licensed trades (Charter certifications), and room for fantasy (Protocol-as-magic), cyber (old-world tech), and endworld (the ruins) in one coherent palette. If you'd rather go pure fantasy or pure post-apoc, the mapping below still works — swap the flavor column.

### 5.2 Make renaming safe *first* (engineering step)

Before touching names, decouple **identity from display**:

1. Add `id` (stable, never-changing slug: `"job.vanguard"`, `"abil.ember_1"`) alongside `name` (display) in JobData/AbilityData/CatalogData schemas.
2. Cross-reference everything (prerequisites, unlocks, catalogs, saves) by `id`; only UI reads `name`.
3. This simultaneously fixes audit §6's "name-string identity" P1 (saves keyed by display name) and makes future localization trivial.
4. Rename the editor tools (`FFTJobCreator` → `JobAssetGenerator`, menu `Tactics RPG → Generate Content`).

Then re-theming is a pure find-replace in display fields plus new prefab regeneration — zero code churn.

### 5.3 Job roster mapping (27 jobs, FFT structure preserved, names/flavor replaced)

| FFT job (current) | Proposed | Flavor hook |
|---|---|---|
| Squire | **Drifter** | Charterless freelance; basic fieldcraft |
| Chemist | **Sawbones** | Stim-injectors, salvage pharmacology; item mastery |
| Knight | **Warden** | Charter heavy infantry; break enemy gear (Rend → *Strip* protocols) |
| Archer | **Marksman** | Scoped slug-throwers; charge-shot = *aim cycles* |
| Monk | **Brawler** | Pit-fighter tradition; ki = *biofeedback* |
| White Mage | **Mender** | Field-protocol medic; heals via triage rites |
| Black Mage | **Burner** | Offensive protocol caster; elemental payloads |
| Time Mage | **Clockhand** | Manipulates scheduler protocols — haste/slow/stop |
| Summoner | **Wakener** | Boots dormant war-engines ("summons" = awakened machines) |
| Thief | **Scav** | Ruin-runner; steal → *strip parts* |
| Orator | **Broker** | Negotiator; invite/charm → *contract talk* |
| Mystic | **Ghostspeaker** | Interfaces with corrupted archives; debuffs as *hexes in the signal* |
| Geomancer | **Wastewalker** | Reads terrain memory; geomancy = *land-protocol* per tile type |
| Dragoon | **Skybreaker** | Jump-rig trooper (mag-lance + thruster legs) |
| Samurai | **Relic Blade** | Draws power from named old-world blades (fits western frame; drop the katana iconography) |
| Ninja | **Wraith** | Dual-wield infiltrator; smoke, mirrors, sabotage |
| Arithmetician | **Actuary** | Targets by number-patterns — keep this beloved weirdness, re-skin as *statistical targeting* |
| Bard | **Balladeer** | Broadcast songs over the dead network — party-wide buffs |
| Dancer | **Duelist-Dancer → "Fireband"** | Performance debuffer; or fold Bard/Dancer into one **Broadcaster** job, both-gender |
| Mime | **Echo** | Replays the last protocol it observed |
| Ark Knight *(unique)* | **Bannerlord** | Story character class |
| Deathknight *(unique)* | **Hollowed** | Fallen charter-captain; drain arts |
| Fell Knight *(unique)* | **Oathbreaker** | Gaffgarion-analog antagonist class |
| Nightblade *(unique)* | **Knife of the Church** | Cartographic Church assassin |
| Rune Knight *(unique)* | **Cipherguard** | Blade + protocol hybrid |
| Celebrant *(unique)* | **Liturgist** | Church war-cleric |

### 5.4 Vocabulary systems (rename once, apply everywhere)

- **Spell tiers**: replace `-a/-aga` suffixes with an escalation scheme of your own, e.g. protocol versions: *Ember → Ember.2 → Ember.Prime*, or intensity words: *Ember / Pyre / Firestorm*. Pick one scheme and generate all lines from it (Fire/Ice/Bolt → Ember/Rime/Arc).
- **Statuses**: Protect → *Bulwark*, Shell → *Firewall*, Haste → *Overclock*, Slow → *Throttle*, Stop → *Freeze-frame*, Reraise → *Failsafe*, Esuna → *Purge*, Regen → *Knit*, Poison → *Sepsis* or *Rads*, Doom → *Countdown*, Berserk → *Red-line*, Toad/Chicken → keep the comedy with setting-native forms: *Scrapped* (turned into a junk-drone), *Yellow-belly* (unchanged mechanic, western slang name).
- **KO lifecycle**: crystal/treasure → *core / salvage* (a fallen unit decays into a recoverable *memory-core* or lootable *salvage*). Same mechanic, native fiction.
- **JP** → **Cert** (certification points — Charters license trades); "Job level" → *Grade*. Brave/Faith (when you build them) → **Grit / Sync** (Sync = how well you resonate with Protocol — mechanically identical to Faith for casters).
- **Currency**: gold → *scrip* or *chits*.

### 5.5 Story seeds (FFT feeling, not FFT plot)

FFT's feel = political war told from the losing side + history falsified by an institution + a personal betrayal at the center. Three seeds that keep that shape in this world:

1. **The Disputed Entry** — frame the campaign as one archive entry the Cartographic Church marked *heretical*; you play the events as they "actually" happened. Chapter cards quote the official history, then the battle contradicts it (your Alazlam/Durai-papers analog, fully original).
2. **The Succession Audit** — a Foundry lord dies; two heirs each hold half a boot-key to the city's fabricator. Charters take sides. Your captain's oath-brother chooses the other side (Delita-shaped arc without Delita's story beats).
3. **The Severance Lie** — the catastrophe wasn't an accident; the Church's founding order caused it to end a war they were losing. Endgame pivots from succession politics to suppressing/revealing the proof.

### 5.6 Suggested order of work (merges with audit phases)

1. **Now** (with audit Phase 1): stable IDs in JSON schema + generator + saves (§5.2). Structural, unblocks everything.
2. **With audit Phase 3** (content integrity): do the rename pass *at the same time* as fixing the broken unlock cross-references — you must touch every JSON file anyway. One sweep: fix references by ID, apply new display names, delete or implement the 35 class-less statuses, regenerate.
3. **After that**: worldbuilding docs (`Docs/WORLD.md`: factions, timeline, naming conventions) so future content stays on-palette.
4. **Last**: rename FFT-flavored C# *class* names (`EsunaAbilityEffect` → `CleanseAbilityEffect`, `ToadStatus`, `ChickenStatus`, …). Zero player impact, do it opportunistically.

---

## 6. Consolidated verdict

- **Architecture**: genuinely good core (composition abilities, event-driven stats, clean state machines). The audit's Phase 1–2 fixes are the path to a *running* game; nothing structural needs a rewrite.
- **Biggest engineering risks**: dual meta-flow singletons, persistence correctness, string identity, zero tests.
- **Biggest content risk**: the data layer is currently a verbatim FFT clone — but it's concentrated in JSON + one generator, so the re-theme is cheap if you do the ID work first.
- **Recommended immediate next actions** (in order): fix the `WeaponAbilityPower` unarmed NRE → collapse to one flow singleton → persistence fixes → stable IDs → combined Phase-3 + re-theme content sweep.
