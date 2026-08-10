# TacticsRPG project review

**Reviewed:** 2026-08-10  
**Revision:** `main` at `6f7fb14`  
**Scope:** repository structure, runtime code, serialized scenes, data content,
architecture, and player-facing functionality.

## Executive assessment

TacticsRPG is a strong tactical-combat framework, but it is not yet a complete
playable RPG. Its best work is in the battle layer: composed abilities,
height-aware navigation, status and equipment interactions, authored
objectives, deterministic prediction, and two materially different AI modes.

The main product risk is the missing player-facing spine. The repository has
many independently promising systems, but the normal
Title → Hub → Battle → Results → Hub path does not yet work end to end.

| Area | Assessment |
|---|---:|
| Battle-system foundation | 8/10 |
| Combat architecture | 7/10 |
| Tactical gameplay depth | 6/10 |
| Code maintainability | 6/10 |
| AI architecture | 7/10 |
| Meta-game functionality | 2/10 |
| Testing/build reliability | 4/10 |
| Production readiness | 3/10 |

Repository snapshot: roughly 23,000 lines of C#, 23 jobs, 135 unique ability
IDs, three scenes, JSON-to-asset generation, persistence, AI, battle states,
status systems, editor tooling, and an in-editor probe suite.

## What is implemented well

- CTR/SPD turn order with move/action economy
- Grid movement with height, facing, terrain, and line of sight
- Compositional abilities rather than one subclass per spell
- Deterministic damage forecasting separated from random application effects
- Typed combat events and sender-scoped subscription support
- Status application/expiry and behavior-control statuses
- Equipment profiles and composable gear traits
- Multiple victory-condition components and authored battle definitions
- Stable job/ability IDs and JSON-driven content generation
- Easy pattern AI and Hard tactical AI separated by strategy
- Useful architecture guidance and accumulated battle probes

The ability system is the architectural center worth preserving. Range, area,
target filtering, hit rate, power, and effects are assembled as components,
which makes new content and cross-cutting rules much cheaper than subclass-heavy
designs.

## Current player-flow blockers

| Priority | Finding | Tracking |
|---|---|---|
| Blocker | Build starts on a title scene with no interactive UI | [#2](https://github.com/shimakaze09/TacticsRPG/issues/2) |
| Blocker | World state has no real scene or hub surface | [#4](https://github.com/shimakaze09/TacticsRPG/issues/4) |
| Blocker | Persistent `UIManager` owns battle-scene references | [#8](https://github.com/shimakaze09/TacticsRPG/issues/8) |
| High | Post-battle state/controller duplicate reward ownership | [#5](https://github.com/shimakaze09/TacticsRPG/issues/5) |
| High | Results flow lacks a wired continuation path | [#9](https://github.com/shimakaze09/TacticsRPG/issues/9) |
| High | Displayed scrip/items are not committed | [#3](https://github.com/shimakaze09/TacticsRPG/issues/3) |
| High | Shop APIs report success without real transactions | [#6](https://github.com/shimakaze09/TacticsRPG/issues/6) |
| High | Clean-clone generation, compile, and probes are not automated | [#7](https://github.com/shimakaze09/TacticsRPG/issues/7) |

## Architecture assessment

### Strengths

- Composition is used where behavior varies most: abilities, statuses, gear,
  and victory conditions.
- Battle-wide laws such as elevation and elements have explicit hooks.
- Stable data IDs separate identity from display names.
- Prediction is designed to stay deterministic for UI and AI consumers.
- `BattleDefinition` and content generators keep authored data out of state
  machines.

### Risks

- Almost all runtime types live in the global namespace.
- There are no assembly-definition boundaries for runtime, editor, and tests.
- Duplicate infrastructure remains, including two `SerializableDictionary`
  implementations.
- Several coordinators combine orchestration, state, query logic, and policy;
  `JobManager`, `UIManager`, `GameFlowController`, `PostBattleController`, and
  `TacticalComputerPlayer` need careful extraction as they evolve.
- Persistent singletons and scene-local dependencies are mixed, creating
  repeated-load hazards.
- Generated content is required but absent from clean clones until manual editor
  commands run.

A DI-framework migration or battle rewrite is not recommended. The immediate
problem is ownership and integration, not the event-composed battle core.

## Gameplay assessment

The current battle vocabulary is broad, but much of the ability roster remains
concentrated around direct damage and status infliction. The next useful depth
increase should be new interaction shapes:

- forced movement and displacement
- persistent hazards and terrain manipulation
- reactions, intercepts, and overwatch
- delayed/charged actions
- deployables or summoned objects
- objective interaction and ally coordination

Presentation is also a gameplay requirement. Strong rules will still feel like
a prototype without turn-order visibility, threat and AoE previews, forecast
explanations, status tooltips, action feedback, and clear cancel boundaries.

## Recommended order

1. Complete one vertical slice: Title → Hub → one Battle → Results → Hub.
2. Make persistence, UI, and scene ownership explicit.
3. Collapse post-battle logic to one idempotent reward transaction.
4. Prove the loop twice in one session and across save/load.
5. Automate clean-checkout content generation, compilation, and probes.
6. Build real shop/roster/job surfaces.
7. Expand tactical depth only after the existing systems are player-reachable.

## Verification limits

This review inspected source, data, and serialized project configuration. Unity
was not available in the review environment, so compilation and battle probes
were not rerun. Claims marked implemented are repository observations; the CI
work in issue #7 should turn them into continuously verified claims.
