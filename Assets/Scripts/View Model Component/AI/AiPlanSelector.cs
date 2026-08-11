using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The tactical AI's decision stage: given the scored candidate list, picks
/// the plan the unit actually commits to. Applies the action threshold,
/// upgrades near-equal stationary attacks into hit-and-run (act, then
/// retreat), handles panic retreats toward a healer, and positions idle
/// healers and out-of-range attackers. Returns null when the unit should
/// simply advance on the nearest foe — the coordinator owns that fallback.
/// </summary>
public sealed class AiPlanSelector
{
    #region Tuning

    /// <summary>Candidates below this score are not worth acting on at all.</summary>
    public const float MinActScore = 4f;

    /// <summary>
    /// How close a stay-in-place attack must score to the overall best before
    /// the AI trades the move for a retreat instead (1.0 = must equal it).
    /// </summary>
    public const float HitAndRunTolerance = 0.95f;

    /// <summary>Below this HP fraction the unit retreats unless a kill is available.</summary>
    public const float PanicHealthFraction = 0.3f;

    /// <summary>
    /// Panic retreat: how strongly closing distance to a healer outweighs
    /// tile danger (higher = beeline to the healer).
    /// </summary>
    public const float HealSeekWeight = 3f;

    /// <summary>Idle healer positioning: pull toward the most wounded ally.</summary>
    public const float WoundedSeekWeight = 2f;

    #endregion

    private readonly AiTurnContext context;

    /// <summary>Binds the selection rules to one turn's snapshot.</summary>
    public AiPlanSelector(AiTurnContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Chooses this turn's plan from the candidate list: the highest-scoring
    /// action overall, upgraded to a hit-and-run whenever staying put attacks
    /// nearly as well and a safer tile is reachable; panic and idle
    /// positioning when nothing is worth doing. Null means "no plan here —
    /// advance on the nearest foe instead".
    /// </summary>
    public PlanOfAttack Select(List<AiPlanCandidate> candidates)
    {
        var poa = new PlanOfAttack();
        FindBest(candidates, out var best, out var bestStationary);

        // Self-preservation: badly wounded and no kill on the table -> fall
        // back toward a healer (or safety), landing a parting shot when one
        // is available from the current tile.
        if (IsPanicking() && (best == null || !best.KillsTarget))
        {
            var destination = PanicDestination();
            if (bestStationary != null && destination != context.Actor.tile)
            {
                FillPlan(poa, bestStationary);
                poa.actFirst = true;
                poa.postActMoveLocation = destination.pos;
                return poa;
            }

            poa.moveLocation = destination.pos;
            return poa;
        }

        var chosen = best;
        if (bestStationary != null && best != null &&
            bestStationary.Score >= best.Score * HitAndRunTolerance)
        {
            // Post-act move: converge on an out-of-reach focus target after
            // hitting whoever was close — otherwise kite to safety as usual.
            var destination = !bestStationary.HitsFocus && context.FocusTarget != null
                ? ConvergeTile(context.FocusTarget)
                : context.SafestMoveTile();

            if (destination != null &&
                (destination != context.Actor.tile &&
                 (!bestStationary.HitsFocus && context.FocusTarget != null ||
                  context.IsSafer(destination, context.Actor.tile))))
            {
                chosen = bestStationary;
                poa.actFirst = true;
                poa.postActMoveLocation = destination.pos;
            }
        }

        if (chosen != null)
        {
            FillPlan(poa, chosen);
            return poa;
        }

        if (context.IsActorHealer)
        {
            // An idle healer stays useful and alive: drift toward the most
            // wounded ally while avoiding dangerous ground.
            poa.actFirst = false;
            poa.moveLocation = HealerIdleTile().pos;
            return poa;
        }

        if (context.FocusTarget != null)
        {
            // Nothing in range: advance on the team's focus target
            // (danger-weighted) instead of just the nearest foe.
            poa.actFirst = false;
            poa.moveLocation = ConvergeTile(context.FocusTarget).pos;
            return poa;
        }

        return null;
    }

    // Scans candidates in generation order for the best plan overall and the
    // best that fires from the current tile (the hit-and-run candidate);
    // either comes back null below MinActScore. Strictly-greater comparisons
    // keep the earliest of tied candidates, matching enumeration order.
    private static void FindBest(List<AiPlanCandidate> candidates, out AiPlanCandidate best,
        out AiPlanCandidate bestStationary)
    {
        best = null;
        bestStationary = null;
        foreach (var candidate in candidates)
        {
            if (best == null || candidate.Score > best.Score)
                best = candidate;
            if (candidate.IsStationary && (bestStationary == null || candidate.Score > bestStationary.Score))
                bestStationary = candidate;
        }

        if (best != null && best.Score < MinActScore)
            best = null;
        if (bestStationary != null && bestStationary.Score < MinActScore)
            bestStationary = null;
    }

    // Copies a chosen candidate into the plan
    private static void FillPlan(PlanOfAttack poa, AiPlanCandidate candidate)
    {
        poa.ability = candidate.Ability;
        poa.target = Targets.Foe; // informational; targeting was resolved during scoring
        poa.moveLocation = candidate.MoveTile.pos;
        poa.fireLocation = candidate.FireTile.pos;
        poa.attackDirection = candidate.Direction;
    }

    // True when the actor is wounded enough to prioritize survival
    private bool IsPanicking()
    {
        var stats = context.Actor.GetComponent<Stats>();
        if (stats == null)
            return false;
        return stats[StatTypes.HP] <= stats[StatTypes.MHP] * PanicHealthFraction;
    }

    // A living allied healer that could actually help: has a usable
    // (affordable) heal ability. Null when none exists.
    private Unit FindHealerAlly()
    {
        foreach (var other in context.Bc.units)
        {
            if (other == null || other == context.Actor || other.tile == null)
                continue;

            var otherAlliance = other.GetComponent<Alliance>();
            if (otherAlliance == null || !context.Alliance.IsMatch(otherAlliance, Targets.Ally))
                continue;

            var stats = other.GetComponent<Stats>();
            if (stats == null || stats[StatTypes.HP] <= 0)
                continue;

            foreach (var ability in other.GetComponentsInChildren<Ability>())
            {
                if (ability.GetComponentInChildren<HealAbilityEffect>() != null && ability.CanPerform())
                    return other;
            }
        }

        return null;
    }

    // Where a panicking unit runs: toward a helpful healer (danger-weighted)
    // when one exists, otherwise simply the safest reachable tile
    private Tile PanicDestination()
    {
        var healer = FindHealerAlly();
        if (healer == null)
            return context.SafestMoveTile();

        var bestTile = context.Actor.tile;
        var bestCost = float.MaxValue;
        foreach (var tile in context.MoveOptions)
        {
            var distance = Mathf.Abs(tile.pos.x - healer.tile.pos.x) +
                           Mathf.Abs(tile.pos.y - healer.tile.pos.y);
            var cost = context.GetThreat(tile) + distance * HealSeekWeight;
            if (cost < bestCost)
            {
                bestCost = cost;
                bestTile = tile;
            }
        }

        return bestTile;
    }

    // Reachable tile that closes distance on a target while respecting
    // danger — how units drift toward an out-of-reach focus target
    private Tile ConvergeTile(Unit target)
    {
        var bestTile = context.Actor.tile;
        var bestCost = float.MaxValue;
        foreach (var tile in context.MoveOptions)
        {
            var distance = Mathf.Abs(tile.pos.x - target.tile.pos.x) +
                           Mathf.Abs(tile.pos.y - target.tile.pos.y);
            var cost = context.GetThreat(tile) * 0.5f + distance * WoundedSeekWeight;
            if (cost < bestCost)
            {
                bestCost = cost;
                bestTile = tile;
            }
        }

        return bestTile;
    }

    // Idle-healer positioning: toward a downed ally when this healer can
    // revive, else near the most wounded living ally, favoring low-danger
    // tiles; safest tile when nobody needs help
    private Tile HealerIdleTile()
    {
        var canRevive = false;
        foreach (var ability in context.Actor.GetComponentsInChildren<Ability>())
        {
            if (ability.GetComponentInChildren<ReviveAbilityEffect>() != null && ability.CanPerform())
            {
                canRevive = true;
                break;
            }
        }

        Unit wounded = null;
        Unit downed = null;
        var worstFraction = 1f;
        foreach (var other in context.Bc.units)
        {
            if (other == null || other == context.Actor || other.tile == null)
                continue;

            var otherAlliance = other.GetComponent<Alliance>();
            if (otherAlliance == null || !context.Alliance.IsMatch(otherAlliance, Targets.Ally))
                continue;

            var stats = other.GetComponent<Stats>();
            if (stats == null)
                continue;

            if (stats[StatTypes.HP] <= 0)
            {
                if (downed == null)
                    downed = other;
                continue;
            }

            var fraction = stats[StatTypes.HP] / (float)Mathf.Max(1, stats[StatTypes.MHP]);
            if (fraction < worstFraction)
            {
                worstFraction = fraction;
                wounded = other;
            }
        }

        // A corpse we can bring back outranks everything else
        if (canRevive && downed != null)
            wounded = downed;

        if (wounded == null)
            return context.SafestMoveTile();

        var bestTile = context.Actor.tile;
        var bestCost = float.MaxValue;
        foreach (var tile in context.MoveOptions)
        {
            var distance = Mathf.Abs(tile.pos.x - wounded.tile.pos.x) +
                           Mathf.Abs(tile.pos.y - wounded.tile.pos.y);
            var cost = context.GetThreat(tile) + distance * WoundedSeekWeight;
            if (cost < bestCost)
            {
                bestCost = cost;
                bestTile = tile;
            }
        }

        return bestTile;
    }
}
