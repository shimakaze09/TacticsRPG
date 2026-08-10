# Battle system plan

**Updated:** 2026-08-10  
**Scope:** the tactical runtime only. Player-flow and meta-game priorities live
in [ROADMAP.md](ROADMAP.md).

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
- Easy pattern AI and Hard tactical AI with threat, retreat, focus, and support
- 23 jobs and 135 data-defined abilities

These systems are **implemented**, but the normal player flow that reaches and
leaves them is incomplete. Do not describe the whole game as playable until the
vertical-slice exit condition in `ROADMAP.md` is met.

## Immediate battle-adjacent work

1. Move scrip and inventory into canonical save data as part of the one-time
   reward transaction ([issue #3](https://github.com/shimakaze09/TacticsRPG/issues/3)).
2. Make `jpCost`/Cert purchasing real in the job menu, or remove the unused
   field after an explicit design change. The current GDD chooses Cert purchase.
3. Add an escort objective only when Guest-unit control and failure rules are
   defined together.
4. Connect the first two authored battles to the hub/contract flow.

## Tactical depth order

1. **Timeline warfare:** initiative UI, delay/push/reorder abilities, and AI
   awareness.
2. **Grit reactions:** deterministic build-and-spend reaction windows.
3. **Sync terrain:** network coverage as casting geography.
4. **In-battle salvage:** Scav-specific interaction with remains.

Prioritize new interaction shapes over another large batch of damage/status
abilities: forced movement, hazards, delayed actions, reactions, deployables,
and objective interaction create more depth than additional names.

## Presentation and usability

- Visible turn order
- Movement and enemy-threat previews
- Damage/hit forecast explanations
- AoE previews
- Status icons, durations, and tooltips
- Camera framing, action feedback, and minimal VFX/audio
- Clear cancel/undo boundaries and AI-thinking feedback

## Verification contract

Every battle-system change should include:

1. A deterministic probe or focused Unity test.
2. A clean Unity compile.
3. No new errors during a representative play session.
4. Documentation changes only where design, setup, or public behavior changed.

The repository documents a headless entry point at
`BattleProbeMenu.RunHeadless`. The last historical documentation reports 71
passing probes, but this 2026-08-10 docs review could not rerun Unity. Automated
clean-checkout verification is tracked in
[issue #7](https://github.com/shimakaze09/TacticsRPG/issues/7).
