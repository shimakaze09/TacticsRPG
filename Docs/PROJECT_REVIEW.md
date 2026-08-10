# TacticsRPG project review

**Reviewed:** 2026-08-10
**Revision:** `main` at `3857aa4`
**Scope:** repository structure, runtime code, serialized scenes, data content,
architecture, player-facing functionality, and a static balance audit.

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

## Live implementation status

The classified [GitHub issue tracker](https://github.com/shimakaze09/TacticsRPG/issues)
owns every active defect and implementation task. Start with
[phase 0](https://github.com/shimakaze09/TacticsRPG/issues?q=is%3Aissue+is%3Aopen+label%3A%22phase%3A+0-stabilize%22),
then the [core-loop group](https://github.com/shimakaze09/TacticsRPG/issues?q=is%3Aissue+is%3Aopen+label%3A%22group%3A+core-flow%22).
Each issue is labelled by importance, primary feature group, delivery phase,
and relative weight so the tracker can be worked without recreating a queue in
this snapshot.

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
  `TacticalComputerPlayer` are extraction-sensitive as they evolve. Classified
  architecture work owns the concrete changes.
- Persistent singletons and scene-local dependencies are mixed, creating
  repeated-load hazards.
- Generated content is required but absent from clean clones until manual editor
  commands run.

A DI-framework migration or battle rewrite is not recommended. The immediate
problem is ownership and integration, not the event-composed battle core.

## Gameplay assessment

The current battle vocabulary is broad, but much of the ability roster remains
concentrated around direct damage and status infliction. Setting-native job,
gear, story-objective, and post-slice job proposals are captured in
[#64](https://github.com/shimakaze09/TacticsRPG/issues/64),
[#65](https://github.com/shimakaze09/TacticsRPG/issues/65),
[#66](https://github.com/shimakaze09/TacticsRPG/issues/66),
[#67](https://github.com/shimakaze09/TacticsRPG/issues/67), and
[#68](https://github.com/shimakaze09/TacticsRPG/issues/68). Their aim is to
preserve the tactics fundamentals while making the decisions, setting, jobs,
equipment, and story unmistakably this game's own.

Presentation remains a gameplay requirement. Its concrete work and acceptance
criteria live in the classified UI/UX and accessibility issues.

## Balance assessment

The static audit found structural problems that make per-ability tuning
premature:

- encounter `SpawnEntry.level` changes Rank/EXP but not combat stats
- every initialized job contributes its Grade-1 stat block, including jobs the
  unit does not actively use
- mastering the 17 non-unique jobs would push most primary stats toward the
  999 cap and erase job identity
- 25 of 76 damaging abilities cost zero MP; free power reaches 360 while the
  basic attack uses 150
- healing stays flat at 12–30 while HP grows cumulatively, while 10% MMP
  regeneration can exceed the highest current ability cost
- RES affects hostile status accuracy but has no job or gear growth path

Root fixes and an automated budget report are specified in
[#52](https://github.com/shimakaze09/TacticsRPG/issues/52),
[#54](https://github.com/shimakaze09/TacticsRPG/issues/54),
[#55](https://github.com/shimakaze09/TacticsRPG/issues/55),
[#56](https://github.com/shimakaze09/TacticsRPG/issues/56),
[#57](https://github.com/shimakaze09/TacticsRPG/issues/57), and
[#58](https://github.com/shimakaze09/TacticsRPG/issues/58). Work order is
defined only by the phase and label rules in [ROADMAP.md](ROADMAP.md).

## Verification limits

This review inspected source, data, serialized project configuration, and
scripted extracts from job and ability definitions. Unity was not available in
the review environment, so compilation and battle probes were not rerun.
Claims marked implemented are repository observations; the CI work in issue #7
should turn them into continuously verified claims.
