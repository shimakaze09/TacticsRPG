using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hard-difficulty AI. Where the Easy brain (ComputerPlayer) follows a scripted
/// attack pattern, this one scores every usable (ability × move tile × fire
/// tile/direction) combination using the game's own Predict() and hit-chance
/// math, then executes the best plan:
///  - kills are worth finishing (large bonus when predicted damage >= target HP)
///  - focus fire (wounded targets are worth proportionally more)
///  - heals/revives valued by HP actually restored, statuses by tactical worth
///    (silencing a caster is worth more; re-applying an active status is worth 0)
///  - hostile area effects that would catch its own allies score negative
///  - hit chance multiplies everything, so back/side attacks emerge naturally
/// </summary>
public class TacticalComputerPlayer : ComputerPlayer
{
    #region Tuning

    private const float KillBonus = 60f;
    private const float LowHealthFocusWeight = 12f;
    private const float AllyHitPenaltyFactor = 1.25f;
    private const float HealWeight = 1.1f;
    private const float ReviveValue = 80f;
    private const float CleansePerStatus = 15f;
    private const float MpCostWeight = 0.25f;
    private const float MinActScore = 4f;

    // How close a stay-in-place attack must score to the overall best before
    // the AI trades the move for a retreat instead (1.0 = must equal it)
    private const float HitAndRunTolerance = 0.95f;

    // How strongly expected incoming damage on the destination tile discounts
    // a plan — every plan mildly prefers safer ground when value is equal
    private const float ThreatPositionWeight = 0.15f;

    // Healers weigh destination danger this many times harder than others
    private const float HealerThreatMultiplier = 3f;

    // Below this HP fraction the unit retreats unless a kill is available
    private const float PanicHealthFraction = 0.3f;

    // Panic retreat: how strongly closing distance to a healer outweighs
    // tile danger (higher = beeline to the healer)
    private const float HealSeekWeight = 3f;

    // Idle healer positioning: pull toward the most wounded ally
    private const float WoundedSeekWeight = 2f;

    // Flat bonus for damaging the team's agreed focus target — makes the
    // whole team converge on one victim instead of spreading damage
    private const float FocusAssistBonus = 18f;

    // Tiny per-tile pull toward the focus target so positioning drifts that
    // way without distorting attack choices
    private const float FocusConvergeWeight = 0.3f;

    // Damage-value multipliers by target role: taking out support first
    private const float HealerTargetMultiplier = 1.35f;
    private const float CasterTargetMultiplier = 1.2f;
    private const float TankTargetMultiplier = 0.9f;

    // Danger estimate for the current evaluation, rebuilt each turn
    private ThreatMap threatMap;

    // Threat weight for this evaluation (amplified for healers)
    private float threatWeight = ThreatPositionWeight;

    // The team's agreed kill-first target for this evaluation. Nomination is
    // deterministic from battle state, so every teammate computes the same
    // answer — coordination without shared state.
    private Unit focusTarget;

    private static readonly HashSet<string> BuffStatuses = new HashSet<string>
    {
        "Bulwark", "Firewall", "Knit", "Overclock", "Failsafe", "Nullgrav", "Ghosted"
    };

    private static readonly Dictionary<string, float> StatusValues = new Dictionary<string, float>
    {
        // hostile control / debuffs
        { "Graycast", 34f }, { "Swayed", 32f }, { "Blackout", 30f }, { "FreezeFrame", 30f },
        { "Deadline", 26f }, { "Scrambled", 22f }, { "DeadAir", 20f },
        { "Pinned", 16f }, { "Static", 16f }, { "Throttle", 14f },
        { "Sepsis", 12f }, { "Doused", 12f }, { "Redline", 12f },
        { "Desync", 10f }, { "Thirst", 10f },
        // friendly buffs
        { "Bulwark", 16f }, { "Firewall", 14f }, { "Knit", 14f },
        { "Overclock", 22f }, { "Failsafe", 18f }, { "Nullgrav", 6f }, { "Ghosted", 10f }
    };

    #endregion

    #region Plan

    private class Option
    {
        public Ability ability;
        public Tile moveTile;
        public Tile fireTile;
        public Directions direction;
        public float score;

        /// <summary>True when this plan's predicted damage finishes a foe.</summary>
        public bool killsTarget;

        /// <summary>True when this plan damages the team's focus target.</summary>
        public bool hitsFocus;
    }

    /// <summary>
    /// Builds this turn's plan: the highest-scoring action overall, upgraded
    /// to a hit-and-run (act from the current tile, then retreat) whenever
    /// staying put attacks nearly as well and a safer tile is reachable.
    /// </summary>
    public override PlanOfAttack Evaluate()
    {
        var poa = new PlanOfAttack();
        threatMap = ThreatMap.Build(bc, actor);
        threatWeight = IsHealer(actor) ? ThreatPositionWeight * HealerThreatMultiplier : ThreatPositionWeight;
        focusTarget = NominateFocusTarget();
        FindBestActions(out var best, out var bestStationary);

        // Self-preservation: badly wounded and no kill on the table -> fall
        // back toward a healer (or safety), landing a parting shot when one
        // is available from the current tile.
        if (IsPanicking() && (best == null || !best.killsTarget))
        {
            var destination = PanicDestination();
            if (bestStationary != null && destination != actor.tile)
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
            bestStationary.score >= best.score * HitAndRunTolerance)
        {
            // Post-act move: converge on an out-of-reach focus target after
            // hitting whoever was close — otherwise kite to safety as usual.
            var destination = !bestStationary.hitsFocus && focusTarget != null
                ? ConvergeTile(focusTarget)
                : SafestMoveTile(GetMoveOptions());

            if (destination != null &&
                (destination != actor.tile &&
                 (!bestStationary.hitsFocus && focusTarget != null || IsSafer(destination, actor.tile))))
            {
                chosen = bestStationary;
                poa.actFirst = true;
                poa.postActMoveLocation = destination.pos;
            }
        }

        if (chosen != null)
        {
            FillPlan(poa, chosen);
        }
        else if (IsHealer(actor))
        {
            // An idle healer stays useful and alive: drift toward the most
            // wounded ally while avoiding dangerous ground.
            poa.actFirst = false;
            poa.moveLocation = HealerIdleTile().pos;
        }
        else if (focusTarget != null)
        {
            // Nothing in range: advance on the team's focus target
            // (danger-weighted) instead of just the nearest foe.
            poa.actFirst = false;
            poa.moveLocation = ConvergeTile(focusTarget).pos;
        }
        else
        {
            poa.actFirst = false;
            MoveTowardOpponent(poa);
        }

        return poa;
    }

    // Copies a scored option into the plan
    private static void FillPlan(PlanOfAttack poa, Option option)
    {
        poa.ability = option.ability;
        poa.target = Targets.Foe; // informational; targeting was resolved during scoring
        poa.moveLocation = option.moveTile.pos;
        poa.fireLocation = option.fireTile.pos;
        poa.attackDirection = option.direction;
    }

    /// <summary>
    /// Scores every usable (ability x move x target) combination. Returns the
    /// best plan overall and the best plan that fires from the current tile
    /// (the hit-and-run candidate); either may be null below MinActScore.
    /// </summary>
    private void FindBestActions(out Option best, out Option bestStationary)
    {
        var startTile = actor.tile;
        var startDir = actor.dir;
        best = null;
        bestStationary = null;

        var moveOptions = GetMoveOptions();
        var abilities = CollectUsableAbilities();

        foreach (var ability in abilities)
        {
            var range = ability.GetComponent<AbilityRange>();
            var area = ability.GetComponent<AbilityArea>();
            if (range == null || area == null)
                continue;

            if (!range.positionOriented)
            {
                // Infinite-style ranges: fire options don't depend on where we
                // stand, so score them once and pick the safest place to stand.
                var fireTiles = range.GetTilesInRange(bc.board);
                var safeTile = SafestMoveTile(moveOptions);
                foreach (var fireTile in fireTiles)
                    Consider(ability, area, safeTile, fireTile, startDir, startTile, ref best, ref bestStationary);
            }
            else if (range.directionOriented)
            {
                // Line/cone style: the fire "location" is our own tile; the
                // direction is what matters.
                foreach (var moveTile in moveOptions)
                {
                    actor.Place(moveTile);
                    for (var d = 0; d < 4; ++d)
                    {
                        actor.dir = (Directions)d;
                        Consider(ability, area, moveTile, moveTile, actor.dir, startTile, ref best, ref bestStationary);
                    }
                }

                actor.Place(startTile);
                actor.dir = startDir;
            }
            else
            {
                foreach (var moveTile in moveOptions)
                {
                    actor.Place(moveTile);
                    var fireTiles = range.GetTilesInRange(bc.board);
                    foreach (var fireTile in fireTiles)
                        Consider(ability, area, moveTile, fireTile, actor.dir, startTile, ref best, ref bestStationary);
                }

                actor.Place(startTile);
                actor.dir = startDir;
            }
        }

        actor.Place(startTile);
        actor.dir = startDir;

        if (best != null && best.score < MinActScore)
            best = null;
        if (bestStationary != null && bestStationary.score < MinActScore)
            bestStationary = null;
    }

    #endregion

    #region Scoring

    // Scores one candidate (ability from moveTile at fireTile/direction) and
    // updates the running best — plus the best that fires without moving,
    // which is the hit-and-run candidate.
    private void Consider(Ability ability, AbilityArea area, Tile moveTile, Tile fireTile, Directions direction,
        Tile startTile, ref Option best, ref Option bestStationary)
    {
        var areaTiles = area.GetTilesInArea(bc.board, fireTile.pos);

        // Cheap early-out: nothing to affect
        var hasContent = false;
        foreach (var t in areaTiles)
        {
            if (t.content != null)
            {
                hasContent = true;
                break;
            }
        }

        if (!hasContent)
            return;

        var score = 0f;
        var kills = false;
        var focusHit = false;
        foreach (var tile in areaTiles)
            score += ScoreTile(ability, tile, ref kills, ref focusHit);

        if (score <= 0f)
            return;

        var mpCost = ability.GetComponent<AbilityMagicCost>();
        if (mpCost != null)
            score -= mpCost.amount * MpCostWeight;

        // Prefer safer ground when plans are otherwise equal (healers weigh
        // danger several times harder — see threatWeight)
        if (threatMap != null)
            score -= threatMap.GetThreat(moveTile) * threatWeight;

        // Gentle pull toward the focus target: among near-equal firing
        // positions, end up closer to the team's kill-first pick
        if (focusTarget != null && focusTarget.tile != null)
        {
            var focusDistance = Mathf.Abs(moveTile.pos.x - focusTarget.tile.pos.x) +
                                Mathf.Abs(moveTile.pos.y - focusTarget.tile.pos.y);
            score -= focusDistance * FocusConvergeWeight;
        }

        Option candidate = null;
        if (best == null || score > best.score)
        {
            candidate = new Option
            {
                ability = ability,
                moveTile = moveTile,
                fireTile = fireTile,
                direction = direction,
                score = score,
                killsTarget = kills,
                hitsFocus = focusHit
            };
            best = candidate;
        }

        if (moveTile == startTile && (bestStationary == null || score > bestStationary.score))
        {
            bestStationary = candidate ?? new Option
            {
                ability = ability,
                moveTile = moveTile,
                fireTile = fireTile,
                direction = direction,
                score = score,
                killsTarget = kills,
                hitsFocus = focusHit
            };
        }
    }

    private float ScoreTile(Ability ability, Tile tile, ref bool kills, ref bool focusHit)
    {
        if (tile.content == null)
            return 0f;

        var defender = tile.content.GetComponentInChildren<Unit>();
        if (defender == null)
            return 0f;

        var defStats = defender.GetComponent<Stats>();
        var defAlliance = defender.GetComponent<Alliance>();
        if (defStats == null || defAlliance == null)
            return 0f;

        var isFoe = alliance.IsMatch(defAlliance, Targets.Foe);
        var isDown = defStats[StatTypes.HP] <= 0;

        var total = 0f;
        for (var i = 0; i < ability.transform.childCount; i++)
        {
            var child = ability.transform.GetChild(i);
            var effect = child.GetComponent<BaseAbilityEffect>();
            var targeter = child.GetComponent<AbilityEffectTarget>();
            var hitRate = child.GetComponent<HitRate>();
            if (effect == null || targeter == null || hitRate == null)
                continue;

            if (!targeter.IsTarget(tile))
                continue;

            var chance = Mathf.Clamp(hitRate.Calculate(tile), 0, 100) / 100f;
            if (chance <= 0f)
                continue;

            total += ScoreEffect(effect, tile, defender, defStats, isFoe, isDown, ref kills, ref focusHit) * chance;
        }

        return total;
    }

    private float ScoreEffect(BaseAbilityEffect effect, Tile tile, Unit defender, Stats defStats, bool isFoe, bool isDown, ref bool kills, ref bool focusHit)
    {
        switch (effect)
        {
            case DamageAbilityEffect _ when isDown:
                return 0f;

            case DamageAbilityEffect _:
            {
                var damage = -effect.Predict(tile);
                if (!isFoe)
                    return -damage * AllyHitPenaltyFactor;

                var hp = defStats[StatTypes.HP];
                if (damage >= hp)
                    kills = true;
                float value = Mathf.Min(damage, hp); // overkill isn't worth extra
                if (damage >= hp)
                    value += KillBonus;
                value += (1f - hp / (float)Mathf.Max(1, defStats[StatTypes.MHP])) * LowHealthFocusWeight;

                // Support dies first; damage on the team's focus target earns
                // the assist bonus that makes the team converge
                value *= RoleMultiplier(defender);
                if (defender == focusTarget)
                {
                    value += FocusAssistBonus;
                    focusHit = true;
                }

                return value;
            }

            case HealAbilityEffect _ when isDown:
                return 0f;

            case HealAbilityEffect _:
            {
                var missing = defStats[StatTypes.MHP] - defStats[StatTypes.HP];
                var healed = Mathf.Min(effect.Predict(tile), missing);
                return isFoe ? -healed : healed * HealWeight;
            }

            case ReviveAbilityEffect _:
                return isDown && !isFoe ? ReviveValue : 0f;

            case CleanseAbilityEffect _ when !isFoe && !isDown:
                return CountRemovableStatuses(defender) * CleansePerStatus;

            case InflictAbilityEffect inflict when !isDown:
                return ScoreStatus(inflict.statusName, defender, defStats, isFoe);

            default:
                return 0f;
        }
    }

    private float ScoreStatus(string statusName, Unit defender, Stats defStats, bool isFoe)
    {
        if (!StatusValues.TryGetValue(statusName, out var value))
            value = 10f;

        // Re-applying an active status is worthless
        var type = InflictAbilityEffect.ResolveStatusType(statusName);
        if (type != null && defender.GetComponentInChildren(type) != null)
            return 0f;

        // Silencing is far more valuable against actual casters
        if (statusName == "DeadAir" && defStats[StatTypes.MMP] >= 20)
            value *= 1.6f;

        var isBuff = BuffStatuses.Contains(statusName);
        if (isBuff)
            return isFoe ? 0f : value;

        return isFoe ? value : -value;
    }

    private int CountRemovableStatuses(Unit unit)
    {
        var status = unit.GetComponentInChildren<Status>();
        return status != null ? status.GetComponentsInChildren<DurationStatusCondition>().Length : 0;
    }

    #endregion

    #region Helpers

    private List<Ability> CollectUsableAbilities()
    {
        var result = new List<Ability>();
        foreach (var ability in actor.GetComponentsInChildren<Ability>())
        {
            if (ability.CanPerform())
                result.Add(ability);
        }

        return result;
    }

    /// <summary>
    /// Manhattan distance from a tile to the closest living foe, considering
    /// ALL foes — retreating from one enemy must not walk into another.
    /// (The 1.5d threat map will replace raw distance with expected damage.)
    /// </summary>
    private int DistanceToNearestFoe(Tile tile)
    {
        if (tile == null)
            return int.MaxValue;

        var closest = int.MaxValue;
        foreach (var other in bc.units)
        {
            if (other == null || other.tile == null)
                continue;

            var otherAlliance = other.GetComponent<Alliance>();
            if (otherAlliance == null || !alliance.IsMatch(otherAlliance, Targets.Foe))
                continue;

            var stats = other.GetComponent<Stats>();
            if (stats == null || stats[StatTypes.HP] <= 0)
                continue;

            var distance = Mathf.Abs(tile.pos.x - other.tile.pos.x) +
                           Mathf.Abs(tile.pos.y - other.tile.pos.y);
            closest = Mathf.Min(closest, distance);
        }

        return closest;
    }

    /// <summary>
    /// The team's kill-first pick: highest target value among living foes,
    /// where value = role weight (healer > caster > striker > tank) scaled by
    /// wounded-ness and by whether this team can realistically finish them.
    /// Deterministic from battle state, so every teammate agrees. Never
    /// forces reach — units out of range simply attack who they can and
    /// converge with their movement instead.
    /// </summary>
    private Unit NominateFocusTarget()
    {
        Unit best = null;
        var bestValue = 0f;

        var teamHit = Mathf.Max(1f, ThreatMap.EstimateDamage(actor));
        foreach (var mate in bc.units)
        {
            if (mate == null || mate == actor)
                continue;
            var mateAlliance = mate.GetComponent<Alliance>();
            if (mateAlliance == null || !alliance.IsMatch(mateAlliance, Targets.Ally))
                continue;
            teamHit = Mathf.Max(teamHit, ThreatMap.EstimateDamage(mate));
        }

        foreach (var foe in bc.units)
        {
            if (foe == null || foe.tile == null)
                continue;

            var foeAlliance = foe.GetComponent<Alliance>();
            if (foeAlliance == null || !alliance.IsMatch(foeAlliance, Targets.Foe))
                continue;

            var stats = foe.GetComponent<Stats>();
            if (stats == null || stats[StatTypes.HP] <= 0)
                continue;

            var woundedness = 1f - stats[StatTypes.HP] / (float)Mathf.Max(1, stats[StatTypes.MHP]);
            var feasibility = Mathf.Clamp(teamHit * 2f / Mathf.Max(1, stats[StatTypes.HP]), 0.3f, 1.5f);
            var value = RoleMultiplier(foe) * (0.6f + woundedness) * feasibility;

            if (value > bestValue)
            {
                bestValue = value;
                best = foe;
            }
        }

        return best;
    }

    /// <summary>Target priority by role: support first, armor last.</summary>
    private static float RoleMultiplier(Unit unit)
    {
        if (IsHealer(unit))
            return HealerTargetMultiplier;

        var stats = unit.GetComponent<Stats>();
        if (stats == null)
            return 1f;
        if (stats[StatTypes.MAT] > stats[StatTypes.ATK])
            return CasterTargetMultiplier;
        if (stats[StatTypes.DEF] >= stats[StatTypes.ATK])
            return TankTargetMultiplier;
        return 1f;
    }

    /// <summary>
    /// Reachable tile that closes distance on a target while respecting
    /// danger — how units drift toward an out-of-reach focus target.
    /// </summary>
    private Tile ConvergeTile(Unit target)
    {
        var moveOptions = GetMoveOptions();
        Tile bestTile = actor.tile;
        var bestCost = float.MaxValue;
        foreach (var tile in moveOptions)
        {
            var distance = Mathf.Abs(tile.pos.x - target.tile.pos.x) +
                           Mathf.Abs(tile.pos.y - target.tile.pos.y);
            var cost = (threatMap != null ? threatMap.GetThreat(tile) * 0.5f : 0f) +
                       distance * WoundedSeekWeight;
            if (cost < bestCost)
            {
                bestCost = cost;
                bestTile = tile;
            }
        }

        return bestTile;
    }

    /// <summary>True when the actor is wounded enough to prioritize survival.</summary>
    private bool IsPanicking()
    {
        var stats = actor.GetComponent<Stats>();
        if (stats == null)
            return false;
        return stats[StatTypes.HP] <= stats[StatTypes.MHP] * PanicHealthFraction;
    }

    /// <summary>True when the unit carries any heal or revive ability.</summary>
    private static bool IsHealer(Unit unit)
    {
        foreach (var ability in unit.GetComponentsInChildren<Ability>())
        {
            if (ability.GetComponentInChildren<HealAbilityEffect>() != null ||
                ability.GetComponentInChildren<ReviveAbilityEffect>() != null)
                return true;
        }

        return false;
    }

    /// <summary>
    /// A living allied healer that could actually help: has a usable
    /// (affordable) heal ability. Null when none exists.
    /// </summary>
    private Unit FindHealerAlly()
    {
        foreach (var other in bc.units)
        {
            if (other == null || other == actor || other.tile == null)
                continue;

            var otherAlliance = other.GetComponent<Alliance>();
            if (otherAlliance == null || !alliance.IsMatch(otherAlliance, Targets.Ally))
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

    /// <summary>
    /// Where a panicking unit runs: toward a helpful healer (danger-weighted)
    /// when one exists, otherwise simply the safest reachable tile.
    /// </summary>
    private Tile PanicDestination()
    {
        var moveOptions = GetMoveOptions();
        var healer = FindHealerAlly();
        if (healer == null)
            return SafestMoveTile(moveOptions);

        Tile bestTile = actor.tile;
        var bestCost = float.MaxValue;
        foreach (var tile in moveOptions)
        {
            var distance = Mathf.Abs(tile.pos.x - healer.tile.pos.x) +
                           Mathf.Abs(tile.pos.y - healer.tile.pos.y);
            var cost = (threatMap != null ? threatMap.GetThreat(tile) : 0f) +
                       distance * HealSeekWeight;
            if (cost < bestCost)
            {
                bestCost = cost;
                bestTile = tile;
            }
        }

        return bestTile;
    }

    /// <summary>
    /// Idle-healer positioning: near the most wounded living ally while
    /// favoring low-danger tiles; safest tile when nobody is hurt.
    /// </summary>
    private Tile HealerIdleTile()
    {
        Unit wounded = null;
        var worstFraction = 1f;
        foreach (var other in bc.units)
        {
            if (other == null || other == actor || other.tile == null)
                continue;

            var otherAlliance = other.GetComponent<Alliance>();
            if (otherAlliance == null || !alliance.IsMatch(otherAlliance, Targets.Ally))
                continue;

            var stats = other.GetComponent<Stats>();
            if (stats == null || stats[StatTypes.HP] <= 0)
                continue;

            var fraction = stats[StatTypes.HP] / (float)Mathf.Max(1, stats[StatTypes.MHP]);
            if (fraction < worstFraction)
            {
                worstFraction = fraction;
                wounded = other;
            }
        }

        var moveOptions = GetMoveOptions();
        if (wounded == null)
            return SafestMoveTile(moveOptions);

        Tile bestTile = actor.tile;
        var bestCost = float.MaxValue;
        foreach (var tile in moveOptions)
        {
            var distance = Mathf.Abs(tile.pos.x - wounded.tile.pos.x) +
                           Mathf.Abs(tile.pos.y - wounded.tile.pos.y);
            var cost = (threatMap != null ? threatMap.GetThreat(tile) : 0f) +
                       distance * WoundedSeekWeight;
            if (cost < bestCost)
            {
                bestCost = cost;
                bestTile = tile;
            }
        }

        return bestTile;
    }

    /// <summary>
    /// True when moving to <paramref name="candidate"/> is genuinely safer
    /// than staying on <paramref name="current"/>: lower expected damage,
    /// or equal damage but farther from every foe.
    /// </summary>
    private bool IsSafer(Tile candidate, Tile current)
    {
        var candidateThreat = threatMap != null ? threatMap.GetThreat(candidate) : 0f;
        var currentThreat = threatMap != null ? threatMap.GetThreat(current) : 0f;

        if (candidateThreat < currentThreat)
            return true;
        return Mathf.Approximately(candidateThreat, currentThreat) &&
               DistanceToNearestFoe(candidate) > DistanceToNearestFoe(current);
    }

    /// <summary>
    /// Reachable tile with the lowest expected incoming damage; ties broken
    /// by max-min distance to all living foes.
    /// </summary>
    private Tile SafestMoveTile(List<Tile> moveOptions)
    {
        Tile bestTile = actor.tile;
        var bestThreat = float.MaxValue;
        var bestDistance = int.MinValue;
        foreach (var tile in moveOptions)
        {
            var tileThreat = threatMap != null ? threatMap.GetThreat(tile) : 0f;
            var distance = DistanceToNearestFoe(tile);
            if (tileThreat < bestThreat ||
                (Mathf.Approximately(tileThreat, bestThreat) && distance > bestDistance))
            {
                bestThreat = tileThreat;
                bestDistance = distance;
                bestTile = tile;
            }
        }

        return bestTile;
    }

    #endregion
}
