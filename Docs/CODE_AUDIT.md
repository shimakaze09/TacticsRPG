# TacticsRPG Code Audit

**Date:** 2026-07-28 · **Branch:** `claude/project-fft-comparison-9dia0i` (audited at commit `d7c36a2`)
**Method:** Full-source review across six areas — battle core, ability/status/stats pipeline, job system + JSON data, meta-game flow/UI/persistence, AI, and build/content tooling. Every finding below was verified against the actual code path (file:line cited); nothing is speculative.

---

## 1. Executive summary

The **in-battle architecture is genuinely strong** — the state machine, turn economy, ability composition model (range/area/target/power/hit-rate/effect as components), and the 27-job FFT data set are well beyond tutorial scope. However, the project is currently in a **non-runnable state end-to-end**, and several core combat rules are silently broken:

1. **The game cannot boot from a fresh clone.** Build settings reference a scene that doesn't exist; the generated job/ability assets were deleted and gitignored with no documented regeneration step; player builds fail to compile outright.
2. **Damage ignores defense entirely** — every attack resolves as `damage = attack` because the defense event handler checks the wrong unit.
3. **Status infliction is 100% dead** — all 124 `Inflict` entries in the JSON resolve to no-ops due to a class-name mismatch, and would expire instantly (duration 0) even if fixed.
4. **Persistence largely doesn't work** — job progress is never registered for save, loaded EXP is never applied to spawned units, `currentJob` can't survive serialization, and "New Game" immediately overwrites the previous save with stale in-memory state.
5. **The event bus is unreachable** — nothing ever registers `GameEventBus` in the `ServiceLocator`, so the first gold change throws; behind that NRE hides a self-recursive purchase event that would drain all gold on one click.
6. **The meta-game layer is scaffolding** — ~45 TODOs across the five GameFlow states, and all seven `GameSystem/UI` panel controllers are **0-byte files**.

Severity legend: **P0** = breaks the game/build now · **P1** = wrong behavior in live code paths · **P2** = latent trap / dead code / quality issue.

---

## 2. Critical findings (P0)

| # | Finding | Where |
|---|---------|-------|
| C1 | `using UnityEditor;` sits **above** the `#if UNITY_EDITOR` guard in a non-Editor folder → **player builds fail to compile** | `Assets/Scripts/Tools/DictionaryDrawer.cs:3` (guard at :7) |
| C2 | `EditorBuildSettings` lists only `Assets/Scenes/SampleScene.unity`, **which does not exist**. `SceneManager.LoadSceneAsync("Battle")` returns null → NRE at `BaseGameFlowState.cs:88`; `EndBattleState`'s `LoadScene(0)` targets the phantom scene | `ProjectSettings/EditorBuildSettings.asset:8-10` |
| C3 | Commit `d7c36a2` deleted `Resources/Abilities`, `Jobs`, `Ability Catalog Recipes` and gitignored them — they are **generated artifacts** (via `Assets/Editor/FFTJobCreator.cs` / `FFTAbilityCreator.cs` / `FFTAbilityCatalogCreator.cs`) but the generation step is documented nowhere (README is one line). Until the three editor menu items are run: `JobManager.LoadAllJobs` finds nothing (`JobManager.cs:183`), `InitializeWithDefaultJob` errors "No jobs available!" (`JobManager.cs:156`), every catalog load fails (`UnitFactory.cs:157`), and all ~250 JSON-defined abilities are dead content | `.gitignore`, `Assets/Resources/*.meta` (orphaned) |
| C4 | **Defense is never applied to damage.** `BaseAbilityPower.OnGetDefense` gates on `IsMyUnit(e.Target)`, but `IsMyUnit` compares against the *attacker* (the component's own parent unit) — false for every real defender. Sole subscriber to `GetDefenseStatEvent`, so `defense = 0` game-wide and the formula collapses to `damage = attack` | `Assets/Scripts/View Model Component/Ability/Power/BaseAbilityPower.cs:51-57,67-72` |
| C5 | **All JSON-driven status infliction is a no-op.** `InflictAbilityEffect` resolves `Type.GetType("Confuse")` but classes are named `ConfuseStatus` etc. → "Invalid Status Type" for all 124 `Inflict` entries. Even with matching names, the generator never sets `duration` (no field in `EffectData`), so statuses would be removed on the next turn tick | `InflictAbilityEffect.cs:16`, `Assets/Editor/FFTAbilityCreator.cs:332-338`, `DurationStatusCondition.cs:15-19` |
| C6 | **`GameEventBus` is never registered in the `ServiceLocator`** (zero `Register` calls repo-wide) → `Bank.gold` setter NREs on any gold change; the whole purchase pipeline dies after gold was already written to PlayerPrefs | `Bank.cs:23`, `ServiceLocator.cs:77-88` |
| C7 | **Soft-lock when a CPU unit ends the battle.** `BattleState.AddListeners` subscribes input only for human drivers; `CutSceneState` entered on a CPU turn can never receive `OnFire` → the outro conversation freezes on its first line and `EndBattleState` is never reached | `BattleState.cs:31-38,48`, `CutSceneState.cs:46-50`, `PerformAbilityState.cs:20-21` |
| C8 | **KO'd units keep taking full turns.** `TurnOrderController.CanTakeTurn` publishes `TurnCheckEvent` but *nothing subscribes to it anywhere* — a 0-HP unit accumulates CTR and gets a fully controllable turn | `TurnOrderController.cs:68-74`, `EventArgs/TurnOrderEvents.cs:19` |

---

## 3. Battle core

### Bugs (P1 unless noted)

- **EXP division-by-zero for uniform-level parties**: `(LVL - min) / (max - min)` → NaN weights when all members share a level (the common case). Currently dead code (see below) but real if wired. `ExperienceManager.cs:31,39`
- **Level-up detection can never trigger**: `PostBattleController.cs:222` calls `rank.DidLevelUp()` with no argument; `Rank.DidLevelUp(int recentExpGained = 0)` returns false unconditionally for `<= 0`. `Rank.cs:97-104`
- **`Resources.Load` with file extension always null**: `Resources.Load<LevelData>($"Levels/Level_{level}.asset")` — Resources paths must not include extensions. `EndBattleState.cs:24`
- **Post-battle index confusion**: `currentUnitIndex` is set against `unitsWithNewJobs` but consumed against `resultsData.playerUnits` — Next/Prev opens the wrong unit's job menu whenever the lists differ. `PostBattleController.cs:274-277,319-356`
- **Battle results dropped / paths contradictory**: `EndBattleState` only calls `EndBattleWithResults` when `GameStateManager.Instance != null`, but inside it `GameFlowController` takes priority and its branch discards `resultsData` (`NotifyBattleEnded()` carries no payload). With only GameFlowController present, the broken legacy path runs instead. `EndBattleState.cs:14-17`, `BattleControllerExtensions.cs:29-37`
- **Victory re-derived divergently**: `EndBattleState.CheckVictoryCondition` uses "any Hero HP > 0" instead of `BaseVictoryCondition.Victor`; with `DefeatTargetVictoryCondition` + `MinHP = 10` (set in `InitBattleState.cs:64-68`) the two checks disagree. `EndBattleState.cs:33-53`
- **Defeated-enemy count fabricated**: uses `HP <= 0` (never true with `MinHP = 10`) then `return count > 0 ? count : 3;` — hard-coded 3-enemy reward fallback. `BattleControllerExtensions.cs:119,127`
- **Locked menu entries confirmable**: when every entry is locked, `selection` stays on a locked entry and neither `CommandSelectionState.Confirm` nor `ActionSelectionState.Confirm` re-checks `IsLocked`; `PerformAbilityState` still sets `hasUnitActed = true` — a wasted action. `AbilityMenuPanelController.cs:84-158`
- **`FlyMovement` mixes world/local space**: `duration = (y - jumper.position.y) * 0.5f` (local target vs world position) can go negative on tall tiles; the landing duration at :37 is computed then ignored. `FlyMovement.cs:16,37-38`
- **`TileTraversalOverride` is a no-op**: `[SerializeField] private readonly` — Unity never serializes readonly fields, so inspector values are discarded and the hard-coded default (allow all) always applies. `TileTraversalOverride.cs:6`
- **NRE on non-targeter ability children**: `Ability.IsTarget` and `ConfirmAbilityTargetState.UpdateHitSuccessIndicator` dereference `GetComponent<AbilityEffectTarget>()` on every child without null checks. `Ability.cs:29-34`, `ConfirmAbilityTargetState.cs:90-101`

### Incomplete / dead (P2)

- `PerformAbilityState.Animate` is a TODO — abilities resolve with zero presentation. `PerformAbilityState.cs:16`
- `InitBattleState.SpawnTestUnits` — hard-coded recipes, random levels 9–11, units dropped on random tiles ignoring traversal; victory condition hard-codes "last spawned enemy, MinHP 10". `InitBattleState.cs:25-69`
- `ExperienceManager` has **no callers**; `PostBattleController.AwardRewards` gives *full* EXP to every unit; `GameFlow/PostBattleState` awards are all TODOs — **three competing, unreconciled reward implementations**.
- `KnockBack` movement never used (and inherits `range => stats[MOV]`, wrong for knockback). `CameraRig.follow` never assigned by code. Charge-time action statuses (`ChargingStatus`/`JumpingStatus`/`PerformingStatus`) are applied by nothing.

### Design risks

- `StateMachine.Transition` silently drops re-entrant transitions (`_inTransition` guard, no queue, no warning) — any future synchronous transition from `Enter()` is swallowed. `StateMachine.cs:27-28`
- States/controllers accumulate: `GetState<T>` AddComponents forever; `InitBattleState` adds a new `TurnOrderController` + victory condition per run — any battle-restart-without-scene-reload stacks duplicates, and `IsBattleOver` reads the *first* `BaseVictoryCondition` (stale `Victor` instantly ends the next battle).
- Pathfinding scratch state lives on `Tile.prev/distance` globally — the path `MoveSequenceState` walks is whatever the *last* `Board.Search` left behind; any search inserted between selection and traversal reroutes the unit silently.
- `Point.GetHashCode` is `x ^ y` — (1,2)/(2,1) collide and the whole diagonal hashes to 0; degrades the `Dictionary<Point, Tile>` hit in every BFS expansion. `Model/Point.cs:73`
- Static C# events (`InputController.moveEvent/fireEvent`, `ConversationController.completeEvent`) outlive scene loads; correctness depends on every state's Exit pairing. `Turn` is shared mutable state and `ActionSelectionState.category` is a `static int` — none of it resets across battles.

---

## 4. Ability / status / stats pipeline

### Bugs

- **(C4, C5 above are the headliners.)**
- **MP clamps against MHP, not MMP**: `Mana.cs:55` — a mage with MHP < MMP can never fill MP; MHP > MMP overfills.
- **`Destroy(this)` orphans status conditions → NRE**: `Status.Add` puts the condition on a *child* GameObject, but KO/Invisible/Reraise/Jumping/Critical/Chicken/Doom/Oil look it up with same-object `GetComponent` and fall back to `Destroy(this)` — the condition child survives, still subscribed, and its later `Remove()` NREs in `Status.Remove` (`Status.cs:25`). (`CountdownEffect.cs:35` uses `GetComponentInChildren` correctly, proving the others wrong.)
- **Doom can never respect Reraise/Undead**: looks for `ReraiseStatus`/`UndeadStatus` on its *own* GameObject; each status lives on its own sibling child. Always KOs. `DoomStatus.cs:40,56`
- **Crystallization bypasses the status system**: `KOStatus` `AddComponent`s `CrystalStatus`/`TreasureStatus` directly on the unit root — no `Status.Add`, no events, no condition; `turnsRemaining` is never decremented (permanent). `KOStatus.cs:83-85`
- **Min/Max modifiers have swapped semantics**: `MaxValueModifier` implements a floor, `MinValueModifier` a ceiling; current callers only work by exploiting the inversion. `Exceptions/Modifiers/{Max,Min}ValueModifier.cs:14`
- **0% chances still hit**: `Random.Range(0, 101)` + `roll <= chance` — a guaranteed-miss connects ~1% of the time. `HitRate.cs:32-34`
- **`WeaponAbilityPower` double-NRE**: dereferences the unequipped `Primary` slot before the unarmed fallback can run, and `UnarmedPower` uses the obsolete `Job` component while units get `JobManager`. `WeaponAbilityPower.cs:18-32`
- **The entire write-time buff/debuff family is inert**: Protect/Shell/Berserk/Defending/Charging/Toad/StatModifier hook `StatWillChangeEvent` (fires only on *writes*), but combat *reads* stats raw (`ATypeHitRate.cs:23-24`, `PhysicalAbilityPower.cs:7-12`, `STypeHitRate.cs:24`). Only the CTR hooks (Haste/Slow/Stop/Sleep freeze) work, because `TurnOrderController.cs:21` writes through the exception-enabled indexer. Worse: if a write does occur mid-status (level-up), the multiplier is permanently baked into the stored stat.
- **`DurationStatusCondition` ticks on *every* unit's turn** (global subscribe) while `CountdownCondition` correctly scopes to the owner — "duration 10" expires in ~1¼ rounds on an 8-unit map. `DurationStatusCondition.cs:7`
- **`StatComparisonCondition` un/subscribe imbalance** (`Init` subscribes, `OnDisable` unsubscribes, no `OnEnable`) — and this class is what removes KO on revival (`AutoStatusController.cs:23-24`); a deactivate/reactivate cycle makes a unit unrevivable. `StatComparisonCondition.cs:37,49`
- **Invisibility stripped by heals**: removes on *any* HP change, including regen. `InvisibleStatus.cs:36`
- **`AbsorbDamage` pair**: effect leaks a global (null-sender) subscription that `OnDisable` never removes; the "target" variant extends `BaseAbilityEffect` instead of `AbilityEffectTarget` yet is wired as a targeter by the generator → NRE if any JSON ever uses it. `AbsorbDamageAbilityEffect.cs:34-47`, `FFTAbilityCreator.cs:355-357`
- **Hard-coded tuning ignoring inspector fields**: Haste doubles CT despite `ctMultiplier = 1.5f`; Protect/Shell hard-code 1.5, and Shell targets `RES` instead of `MDF`. `HasteStatus.cs:11,37`, `ProtectStatus.cs:10,36`, `ShellStatus.cs:10,36`
- **`PetrifyStatus.cs:69`** passes `currentHP` as the *sortOrder* argument of `ClampValueModifier` — works today, but modifier ordering now depends on the unit's HP.

### Status-effect reachability (key insight)

Of ~34 status classes, **only `KOStatus` is reachable at runtime** (via `AutoStatusController`). Eighteen (Confuse, Slow, Sleep, Charm, Haste, Silence, Protect, Poison, Stop, Blind, Shell, Reraise, Regen, Petrify, Berserk, Invisible, Float, Reflect) are referenced by JSON but blocked by C5. **~35 status names in the JSON have no class at all** (AttackDown, DefenseDown, all `Steal*`/`Destroy*`/`*Break`, CureAll, Quick, Old, CTMax, Teleport, Recruit, …). Fourteen classes are fully orphaned (Charging, Defending, Jumping, Performing, Critical, Chicken, Doom, Immobilize, Oil, Toad, Undead, Vampire, Atheist, Faith).

### Dead extension points (P2)

`AutomaticHitCheckEvent`, `AutomaticMissCheckEvent`, `HitRateStatusCheckEvent`, `TweakDamageEvent`, and `TurnCheckEvent` are all **published into the void — zero subscribers**. These are precisely the hooks Blind/Sleep-auto-hit/elemental damage/KO-turn-skip need. Also unreferenced: `ReactionEffect` (empty base, zero subclasses), `BehaviorOverrideEffect`, `PeriodicEffect`, `StatModifierEffect`, `CountdownEffect`, `TransformEffect`, `CountdownCondition`, `DamageRemovalCondition`, `AttackedStatusCondition` (exact duplicate of `DamageRemovalCondition`), `EsunaAbilityEffect` (generator never emits it), `Consumable.Consume`, the `Undead` marker (never added → `UndeadAbilityEffectTarget` can never match), `Alliance.confused` (never set by Charm/Confuse → no targeting effect).

### Design risks

- Unstable `List.Sort` on modifiers with near-universal `sortOrder = 0` → Add-vs-Mult order nondeterministic.
- Periodic/status code writes HP with `SetValue(..., false)` (exceptions disabled), bypassing Petrify invulnerability and Health clamps; each caller re-implements clamping.
- `Predict()` re-runs the full event pipeline inside `OnApply` — stat events publish twice per hit.
- The bus catches and logs all handler exceptions — pipeline bugs surface only as console spam while combat silently corrupts.
- Dual job systems: obsolete `Job` still referenced by `WeaponAbilityPower`; units actually carry `JobManager`.

---

## 5. Job system & JSON data

### Bugs

- **JP table off-by-one**: `jpRequirements` has 8 entries but only 7 level-ups; `GetJobLevelForJP` returns a phantom **level 9** at 2500 JP (mastery actually lands at 1900). `AddJobPoints` compares unclamped values, so an 8→9 "level up" event fires. `JobDefinition.cs:203-215`, `JobProgressData.cs:157-167,216`
- **MOV/JMP/EVD accumulate on every recalculation**: `stats[MOV] + movementBonus` with no base reset — recalc runs on Start, every job switch, every job level-up, and load → permanent stat drift (+4 MOV per recalc for a Squire). `JobManager.cs:470-472`
- **Level-based JP is dead**: `UnitFactory` adds `JobManager` *before* `Rank`; `JobManager.Awake` caches `rank = null` and `OnEnable` skips the stat-event subscription — `OnCharacterLevelChanged` never fires. `UnitFactory.cs:39-40,116`, `JobManager.cs:87,104-111`
- **"Unique" jobs unlock for everyone instantly**: `CanUnlock` skips the character check when `allowedCharacterNames` is empty — and all six unique jobs have empty lists, empty prerequisites, `minimumCharacterLevel: 1`. First job level-up auto-unlocks all six. `JobDefinition.cs:184` + JSON
- **`InstantiatePrefab` null-fallback**: returns `new GameObject(name)` when a prefab is missing, so the downstream `!= null` warnings are unreachable and missing abilities silently become junk objects lacking the `Ability` component. `UnitFactory.cs:82-94,172,255`
- **Save round-trip loses the current job**: `JobProgressData.currentJob` is a ScriptableObject reference serialized by `JsonUtility` as an `instanceID` — meaningless next session. Persist the job *name* instead. `JobProgressData.cs:31`, `JobManager.cs:600-605`
- `JobManager.LoadData` dereferences `data.abilityMemoryData` without the null check `jobProgressData` gets. `JobManager.cs:590,608`
- Mid-battle job level-up is an unintended **full heal** — `RecalculateStats` restores HP/MP to max every recalc. `JobManager.cs:476-477`

### Data consistency (27 JobData / 28 AbilityData / 28 CatalogData files)

Schema is clean: all files parse; `baseStats` length 7 matches `Job.statOrder` and multiplier application order; `jpRequirements` identical (100…2500) everywhere; enum values all covered by the generator switches. But **name cross-references are badly broken**:

- **All unlocks reference nonexistent abilities** for: Bard (4/4), Deathknight (8/8), Fell Knight (7/7), Nightblade (7/7), Rune Knight (7/7) — the AbilityData files define completely different ability sets (e.g. Bard unlocks `Angel Song…` but data defines `Battle Chant, Bravery Song…`). Dancer (2 missing), Ark Knight (2 unlocks vs 10 unrelated abilities).
- **Arithmetician**: `Height Prime`/`CT Prime` exist in AbilityData but not its Catalog; Catalog's `Level Multiple 6–11` have **no AbilityData definition** (→ junk GameObjects per the null-fallback bug).
- **Minor orphans** (defined + cataloged, never unlockable): Assassin, Celebrant, Geomancer, Mystic, Samurai, Summoner, Time Mage, White Mage (2 each); **Mime** has 1 unlock vs 9+ defined abilities.
- **UnitRecipes**: `abilityCatalog` fields reference catalogs that exist nowhere (`DemoCatalog`, `Enemy Rogue`…) — currently harmless only because the field is dead code (`AddAbilityCatalog(recipe.abilityCatalog)` is never called; the job's catalog is used). `element` is empty in all six recipes → `Enum.TryParse("")` fails and **no `Elements` component is added** (NRE risk for unguarded `GetComponent<Elements>()`).
- **`jpCost` is parsed but never read by any runtime code** — FFT's buy-abilities-with-JP loop is unimplemented; abilities auto-learn at job level.

### Prerequisite-tree fidelity

Verified acyclic. WotL-accurate for Archer, Knight, Monk, Geomancer, Dragoon, Ninja, Samurai, Dancer, Bard, Arithmetician, Mime. Deviations: Black Mage requires Chemist L2 (FFT: Squire 2), Thief requires Archer L3 (FFT: Squire 2), Summoner requires Time Mage L3 (WotL: 2). No field exists for Bard/Dancer's gender restriction.

---

## 6. Meta-game flow, UI & persistence

### Persistence bugs

- **Job progress never saved**: `JobManager` implements `IDataPersistence` but never calls `DataPersistenceManager.Register(this)`; the one-shot scene scan in `Start()` runs before factory spawns. JP/job levels/unlocked jobs/learned abilities silently lost every quit. `JobManager.cs:36,104-111`, `DataPersistenceManager.cs:32-37`
- **Loaded EXP never applied**: `LoadGame()` pushes data only to objects registered at call time (once, in `Start`); battle units spawn later and self-register for *save* only — progression is saved but never restored. `DataPersistenceManager.cs:36,69-70`, `Unit.cs:36-39`
- **"New Game" overwrites the old save with stale state**: `NewGame()` replaces `gameData` without resetting live objects; `WorldState` then auto-saves unconditionally, pulling the *previous* session's in-memory values into the fresh file. `DataPersistenceManager.cs:48-51`, `WorldState.cs:205-215`
- **Corrupt saves are silently destroyed**: load failure → null → `NewGame()` → quit-save overwrites the (possibly recoverable) file. Writes go `FileMode.Create` straight to the final path — no temp+rename, no backup. `FileDataHandler.cs:37-40,58`
- **`SerializableDictionary.OnAfterDeserialize` throws** on key/value count mismatch (loop runs anyway) and on duplicate keys — inside `JsonUtility.FromJson`. `Serializable Types/SerializableDictionary.cs:26-31`
- **Save data keyed by `gameObject.name`**: two "Enemy Warrior"s overwrite each other; *enemy* EXP pollutes the player save (every `Unit` persists, not just party). `Unit.cs:24-31`
- **Gold lives in PlayerPrefs**, outside `GameData`: survives New Game, not per-slot, desyncs from the JSON save; hard-coded 5000 default; the insufficient-funds dialog's confirm button grants a free +5000 (`FakeBuyGold`). `Bank.cs:44-52`, `ItemShop.cs:96-99`
- **`PruneNullEntries` can't detect destroyed components**: interface-typed `== null` bypasses Unity's overloaded equality → destroyed implementers stay registered; `SaveData` then throws `MissingReferenceException` mid-loop (partial save). `DataPersistenceManager.cs:100-102`

### Event bus / shop

- **(C6 above)** plus: `Bank.cs:23` calls `Publish(this, new GoldChangedEvent(...))` with **swapped arguments** — publishes a `Bank`-typed event with the event as sender; `BankView`'s `GoldChangedEvent` subscription would never fire even after C6 is fixed.
- **Self-recursive purchase event**: `ItemCell` publishes `ItemPurchasedEvent` as a *request*; `ItemShop.Purchase` re-publishes the same type as a *confirmation* → synchronous re-entry buys repeatedly until gold < price. Currently masked by C6's NRE. `ItemCell.cs:27`, `ItemShop.cs:25,58-66,104`
- **Bus re-entrancy protection corrupts itself on nested publishes**: `_invoking` is a HashSet; a nested publish of the same event type removes the list in its `finally` while the outer invocation is still iterating → skipped/double-invoked handlers. The "thread-safe" claim in the header is false (no locks). Also, `CleanupDestroyedObjects` prunes by destroyed *sender* only — global (null-sender) subscriptions from destroyed components are never cleaned. `GameEventBus.cs:8,143,186,225`

### Lifecycle / scene management

- **Pool controller hands out destroyed objects**: static `pools` survive scene unloads while the scene-placed instance (never `DontDestroyOnLoad`) and its pooled children are destroyed → `MissingReferenceException` on next `Dequeue`; `AddEntry` refuses to rebuild (returns false on existing key); `ClearEntry` has zero callers. `GameObjectPoolController.cs:8-14,32,48-49,94-110`
- **State-exit doesn't cancel scene loads**: transition away mid-`LoadSceneAsync` still activates the scene and calls `OnSceneReady()` on the already-exited state — re-subscribing events after its own unsubscribe ran (permanent leak) with `PendingBattleLevel` already nulled. `BaseGameFlowState.cs:55-58,98-107`, `BattleFlowState.cs:91,132-138`
- **Tweener/EasingControl lifecycle**: disable→enable permanently freezes a tween (`Resume` restores `previousPlayState = Paused` and early-returns); `Stop()` never destroys the component (interrupting paths leak a dead `Tweener` per interruption); disabling only the component leaves the coroutine ticking the *Reversing* branch. `EasingControl.cs:103-107,169-188,191-223`, `Tweener.cs:7-12`
- **UIManager**: subscribes to `GameFlowController.OnFlowStateChanged` only if the instance already exists at `OnEnable` — silent permanent skip on unlucky script order. Moot today: **no scene contains a UIManager**. `UIManager.cs:149-155`

### What's actually in the scenes (state of wiring)

- `StartMenu.unity`: camera + light + GameFlowController **only**. No canvas, no buttons, no DataPersistenceManager → the game enters `TitleState` and **dead-ends** (`HandleNewGame/HandleLoadGame` are reachable only via editor ContextMenu).
- `Battle.unity`: has DataPersistenceManager + BattleController + battle UI, but **no UIManager, no GameStateManager, no PostBattleController, no JobMenuPanelController** → `GameStateManager.Instance` is always null, so `EndBattleState` always takes the broken legacy branch; the real EXP-award code never executes.
- All seven `GameSystem/UI` files (`BattleResultsPanelController`, `LoadingScreenController`, `PartyMenuPanelController`, `ScreenFader`, `ShopPanelController`, `TitleMenuPanelController`, `WorldMenuPanelController`) are **0-byte empty files**.
- ~45 TODOs across the five GameFlow states; every `Show*/Hide*UI` and `Subscribe*/Unsubscribe*` is a log-only stub; `PostBattleState` hard-codes victory=true/500 EXP/100 JP/1000 gold and all four `Award*` methods are no-ops; `ShopState` transactions are placeholder booleans, disconnected from the functional `ItemShop`/`Bank` stack. Nothing implements `IGameFlowEventListener`.

### Design risks

- **Two competing meta-flow singletons** (`GameFlowController` vs legacy `GameStateManager`) with divergent state enums and shop rules; call sites split between them; one path always drops rewards. Collapse to one before building the world map.
- **Three disconnected reward implementations** (see §3) — pick a canonical one first.
- **Name-string identity everywhere** (units, jobs, abilities) — renaming content orphans saves; duplicate party names unsupportable. Needs stable IDs before party management.
- Save timing: three uncoordinated writers of one file (auto-save on World entry, quit-save, post-battle save) with no dirty tracking, slots, or confirmation.

---

## 7. AI

- **Live bug**: `ComputerPlayer.GetMoveOptions` calls `GetComponentInChildren<Movement>()` on *itself* (the BattleController's GameObject), not the actor — movement-disabling statuses are checked against the wrong object (NRE or arbitrary unit depending on hierarchy). Line 168 does it correctly. `ComputerPlayer.cs:165,168`
- **`SmartComputerPlayer` (632 lines) is dead code**: nothing references it, and it *cannot* be wired — `BattleController.cpu` is typed `ComputerPlayer` and it doesn't inherit from it. If ever adopted, it has real bugs:
  - Never restores `unit.dir` after evaluating all 4 directions (every unit ends facing West). `SmartComputerPlayer.cs:159,170`
  - `GetAlliesInRange`/`GetEnemiesInRange` pass `(t1,t2) => distance(t1,t2) <= range` to `Board.Search` — adjacent-tile distance is always 1, so the flood-fill covers the whole board ("enemies in range 1" = every enemy on the map). `SmartComputerPlayer.cs:439,463`
  - Scoring ignores `direction` entirely → 4× redundant work, facing chosen by unstable sort. Status/heal scoring reads effects with `GetComponent` on the ability root, but the generator puts effects on *children* → always 0.
  - `FindPathToTarget` compares *normalized* directions — a 1-tile step ties with a max-range move.
  - `TacticalEvaluator` is an empty placeholder class.
- **Fragilities in the live AI**: `AttackOption.IsAbilityAngleBased` assumes every ability child has a `HitRate` (NRE otherwise); `IsAbilityTargetMatch` dereferences `tile.content` without a null check in the non-Tile branch; `PlanDirectionIndependent` computes marks/`isCasterMatch` only at the first move tile that reaches a fire tile (stale for others).
- Only 3 attack-pattern prefabs; no AI awareness of charge-time, reactions, terrain height, status curing, or retreat.

---

## 8. FFT gap analysis (mechanics not yet represented)

| Area | Status |
|------|--------|
| **Charge-time (CT) casting / Jump** | Absent. Turn loop grants one move+act per activation; `TurnOrderController.Round` adds a full round of CTR at once (no per-clocktick simulation), so slow spells/Jump cannot be scheduled without reworking `Round()`. Charging/Jumping/Performing statuses exist but nothing applies them. |
| **Reaction / Support / Movement passives** | Absent end-to-end: `ReactionEffect` has zero subclasses, no resolution point in the state flow after `ApplyAbility`, `AbilityMemory`'s R/S/M equip slots have no callers and no UI, and no JSON entry is marked as a passive (no `type` field exists). |
| **Brave / Faith** | No BRV/FTH in `StatTypes`; formulas omit them; `FaithStatus`/`AtheistStatus` are empty shells; `ChickenStatus` tracks a private bravery field connected to nothing. |
| **Elements** | `Elements` component is populated but **never read**; no weak/half/absorb/cancel; `TweakDamageEvent` (the intended hook) has zero subscribers; Fire/Fira/Firaga are element-less generic damage. |
| **Formulas** | Tutorial-style `(ATK − DEF/2) × power/100` (and DEF is currently 0 per C4), not FFT's per-ability formulas (PA×WP, Faith×MA×Q…); heals are flat `power ± 10%`. |
| **Evasion model** | Single EVD stat with facing divisors; no shield/accessory/class/weapon channels; Blind/Confuse/Sleep hit-rate interactions unimplemented (their event hooks exist but are unsubscribed). |
| **Height rules** | Symmetric `|Δh| ≤ JMP` pathfinding (no jump-up vs drop-down asymmetry, no fall damage); no ally pass-through; targeting ignores height by default (`vertical = int.MaxValue`) and there is no arc/obstruction check for bows/Jump. |
| **KO lifecycle** | Half-built: 3-count crystal/treasure exists but the counter ticks on the corpse's own turns — which only happens because of bug C8; corpses still block pathing; crystal pickup (`OnUnitStepOn`) is never called by movement. |
| **JP economy** | Auto-learn at job level instead of FFT's spend-JP-per-ability (`jpCost` parsed, never used); no JP spillover to same-job party members; `jpBattleMultiplier` declared "not implemented"; per-action JP absent (JP only via a placeholder 100/battle and a dead level-up path). |
| **Character-level growth** | Stats derive purely from job levels × multipliers — character level contributes nothing (a L99 and L1 unit with equal job levels are identical). |
| **Equipment** | Framework exists (`Equipment`/`Equippable`/`EquipSlots` incl. dual-wield) but `Equip()` has zero call sites, no item database (shop sells random-stat POCOs), no inventory, no equipment persistence. |
| **Meta loop** | No world map, no party roster as a persisted concept (no recruit/dismiss/bench), no shop stock progression, no save slots/save points, no mid-battle suspend save. Battle earnings (`goldGained`) are never deposited — battle rewards and shop spending literally use two different currencies. |
| **Presentation** | Units are Unity primitive meshes; no sprites/animations/VFX; `PerformAbilityState.Animate` is a TODO; camera never moves. |

---

## 9. Recommended remediation order

**Phase 0 — make it runnable (small, high-leverage):** *(all items below fixed on this branch, 2026-07-28)*
1. ~~Fix `DictionaryDrawer.cs` guard (C1); fix build-settings scene list (C2).~~ **Done** — `using UnityEditor;` moved inside the guard; build settings now list StartMenu (index 0), Battle (index 1), BoardCreator (disabled). Note: the legacy `LoadScene(0)` fallback in `EndBattleState` now lands on the title screen instead of a phantom scene.
2. ~~Document the JSON→prefab generation step (C3).~~ **Done** — README now documents the three `Tactics RPG → Create FFT …` menu items and the required order. (Committing/generating at load time remains an option for later.)
3. ~~Fix `IsMyUnit` defense check (C4); map JSON status names → `{Name}Status` and add a `duration` field to the generator (C5).~~ **Done** — `OnGetBaseDefense` now gates on `e.Attacker`; `InflictAbilityEffect.ResolveStatusType` tries both `"X"` and `"XStatus"`; `FFTAbilityCreator.EffectData` gained a `duration` field with a 3-turn default. **Requires re-running the ability generator in Unity for prefabs to pick up durations.**
4. ~~Register `GameEventBus` in the `ServiceLocator`; fix the swapped `Publish` args; split `ItemPurchasedEvent` into request/confirmation events (C6 + shop recursion).~~ **Done** — `ServiceBootstrap` registers the bus at startup; `Bank` publishes via the standard extension (correct arg order); `ItemCell` now publishes `ItemPurchaseRequestedEvent`, `ItemPurchasedEvent` is confirmation-only.
5. ~~Subscribe input in cutscenes regardless of driver (C7). Wire a `TurnCheckEvent` subscriber into `KOStatus` (C8).~~ **Done** — `CutSceneState.AddListeners` subscribes unconditionally; `KOStatus` now denies turns via `TurnCheckEvent`, ticking its death counter and resetting CTR on each denied activation (FFT-style).

**Phase 1 — one architecture, not two:** pick `GameFlowController` or `GameStateManager` (recommendation: keep `GameFlowController`, port the results payload into `NotifyBattleEnded`), delete the loser, and pick one reward implementation. Fix persistence registration/apply-on-load, stop keying saves by name, move gold into `GameData`.

**Phase 2 — combat correctness:** convert the write-time status family to read-time modifiers (subscribe the stat *query* events the combat code should publish, or route combat reads through the exception pipeline); fix status condition parenting (`GetComponentInChildren`); fix Mana clamp, hit-roll bounds, JP off-by-one, MOV/JMP/EVD drift, component-order in `UnitFactory`.

**Phase 3 — content integrity:** reconcile the 6 jobs whose unlocks reference nonexistent abilities; fill `allowedCharacterNames` for unique jobs; remove or implement the ~35 class-less status names in JSON.

**Phase 4 — FFT depth (design work):** clocktick-based turn simulation → charge-time casting; reaction/support/movement passives (a `type` field in AbilityData + a reaction window after `ApplyAbility`); Brave/Faith; elemental reads; equipment/inventory; then the meta loop (world map, roster, shops) on the surviving architecture.
