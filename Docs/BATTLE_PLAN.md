# Battle runtime status

**Updated:** 2026-08-10  
**Scope:** implemented tactical-runtime capability, balance observations, and
the verification contract. Implementable work lives in GitHub Issues.

## Current capability

The repository implements a substantial battle sandbox:

- CTR/SPD turn order with move/action economy
- Height-aware grid movement, facing, line of sight, and terrain rules
- Composed abilities with range, area, targeting, hit, power, and effect parts
- Physical/magical damage, deterministic forecasts, elements, and criticals
- Status application, duration, expiry, and behavior-control effects
- Equipment loadouts, weapon shapes/arcs/ranges, and composable gear traits
- KO/remains behavior and salvage pickup
- Defeat-all, defeat-target, survive-rounds, and reach-zone objectives
- Authored `BattleDefinition` setup plus repeatable-writ fallback spawning
- Easy pattern AI and Hard tactical AI
- 23 jobs and 135 data-defined abilities

These systems are **implemented**, but the player-facing loop and several
combat/progression contracts are not integrated or verified. Do not describe
the full game as playable until the phase-1 gate in
[ROADMAP.md](ROADMAP.md) is met.

## Quantitative balance snapshot

The 2026-08-10 static pass against `3857aa4` found that tuning content before
the progression foundations are fixed would produce misleading results:

- spawn level currently changes Rank/EXP but not combat stats
- initialized job Grades accumulate full stat blocks across job history
- 25 of 76 damaging abilities cost zero MP, with free power reaching 360
- heals remain flat at 12–30 while MHP grows cumulatively
- 10% MMP regeneration eventually exceeds every current spell cost
- RES has no job/gear initialization path despite controlling status accuracy

Evidence, decisions, and acceptance criteria live in
[#52](https://github.com/shimakaze09/TacticsRPG/issues/52),
[#54](https://github.com/shimakaze09/TacticsRPG/issues/54),
[#55](https://github.com/shimakaze09/TacticsRPG/issues/55),
[#56](https://github.com/shimakaze09/TacticsRPG/issues/56),
[#57](https://github.com/shimakaze09/TacticsRPG/issues/57), and
[#58](https://github.com/shimakaze09/TacticsRPG/issues/58).

## Work tracking

- [Combat rules](https://github.com/shimakaze09/TacticsRPG/issues?q=is%3Aissue+is%3Aopen+label%3A%22group%3A+combat-rules%22)
- [Statuses](https://github.com/shimakaze09/TacticsRPG/issues?q=is%3Aissue+is%3Aopen+label%3A%22group%3A+status%22)
- [Balance](https://github.com/shimakaze09/TacticsRPG/issues?q=is%3Aissue+is%3Aopen+label%3A%22group%3A+balance%22)
- [AI](https://github.com/shimakaze09/TacticsRPG/issues?q=is%3Aissue+is%3Aopen+label%3A%22group%3A+ai%22)
- [Jobs](https://github.com/shimakaze09/TacticsRPG/issues?q=is%3Aissue+is%3Aopen+label%3A%22group%3A+jobs%22)
- [Equipment and economy](https://github.com/shimakaze09/TacticsRPG/issues?q=is%3Aissue+is%3Aopen+label%3A%22group%3A+equipment-economy%22)

The setting-native identity proposals for jobs, gear, story objectives, and
new post-slice jobs are tracked in issues #64–#68. Design details remain in the
issues until ratified; approved decisions then update GDD/WORLD.

## Verification contract

Every battle-system change includes:

1. A deterministic probe or focused Unity test.
2. A clean Unity compile.
3. No new errors during a representative play session.
4. Balance-report output when formulas, stats, abilities, gear, encounters, or
   rewards change.
5. Documentation changes only where design, setup, or public behavior changed.

The repository documents `BattleProbeMenu.RunHeadless`, but Unity was not
available for this review. Clean-checkout automation remains tracked in
[#7](https://github.com/shimakaze09/TacticsRPG/issues/7).
