# Roadmap

**Updated:** 2026-08-10  
**Goal:** turn the existing battle framework into one complete, repeatable
player journey before expanding content.

Design intent belongs in [GDD.md](GDD.md). Battle-specific detail belongs in
[BATTLE_PLAN.md](BATTLE_PLAN.md). Current assessment and evidence belong in
[PROJECT_REVIEW.md](PROJECT_REVIEW.md).

## M1 — Playable vertical slice spine

Exit condition: a fresh player build can complete
**Title → Hub → Battle → Results → Hub**, repeat the loop for a second battle,
then save, quit, and continue without losing rewards.

Work in this order:

1. [#2 — Build starts on a non-interactive title scene](https://github.com/shimakaze09/TacticsRPG/issues/2)
2. [#4 — World state does not load or present a hub scene](https://github.com/shimakaze09/TacticsRPG/issues/4)
3. [#8 — Make UIManager ownership safe across repeated battle loads](https://github.com/shimakaze09/TacticsRPG/issues/8)
4. [#5 — Consolidate post-battle orchestration and reward ownership](https://github.com/shimakaze09/TacticsRPG/issues/5)
5. [#3 — Persist scrip and item rewards](https://github.com/shimakaze09/TacticsRPG/issues/3)
6. [#9 — Add a usable continuation path after battle results](https://github.com/shimakaze09/TacticsRPG/issues/9)

Do not add another meta-flow singleton or a second reward pipeline. Keep
`GameFlowController` as the orchestration authority and use one save-backed
reward transaction.

## M1.1 — Reliability and collaboration

- [#7 — Automate generated content, compile checks, and battle probes](https://github.com/shimakaze09/TacticsRPG/issues/7)
- Add runtime/editor/test assembly definitions after the vertical-slice branch
  is stable.
- Add focused tests around persistence, rewards, stat derivation, and repeated
  scene transitions.

## M2 — Usable meta game

- [#6 — Replace placeholder ShopState transactions](https://github.com/shimakaze09/TacticsRPG/issues/6)
- Contract briefing and party/deployment selection
- Job menu canvas with Cert spending and job switching
- Settings UI, including difficulty
- First two authored battles connected to the normal flow
- Turn-order bar, forecast explanations, status tooltips, and damage feedback

Exit condition: two authored contracts are playable from the hub with real
growth, inventory, and shop state.

## M3 — Chapter 1 content

- Six authored battles and story scenes from `GDD.md` §5
- Quest board and story flags, including Choice A
- Repeatable writs and one short side story
- First production art/audio batches
- Input System controls and save slots
- External playtest and balance pass

## Post-slice

- Acts 2–3 and ending branches
- Remaining side, hidden, character, and relic quests
- Timeline warfare expansion, Grit reactions, Sync terrain, and deeper salvage
- Archive screen, controller support, and region-map presentation

## Technical debt to interleave carefully

- Namespaces and assembly definitions
- Consolidate duplicate `SerializableDictionary` implementations
- Split oversized coordinators such as `JobManager`, `UIManager`, and
  `TacticalComputerPlayer`
- Repair Tweener/EasingControl and pooled-object scene lifecycles
- Replace static input-event lifetime assumptions during the Input System pass

Completed historical work is preserved in Git history and
[CODE_AUDIT.md](CODE_AUDIT.md); it is intentionally not duplicated in this
queue.
