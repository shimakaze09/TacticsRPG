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
    }

    /// <summary>
    /// Builds this turn's plan: the highest-scoring action overall, upgraded
    /// to a hit-and-run (act from the current tile, then retreat) whenever
    /// staying put attacks nearly as well and a safer tile is reachable.
    /// </summary>
    public override PlanOfAttack Evaluate()
    {
        var poa = new PlanOfAttack();
        FindBestActions(out var best, out var bestStationary);

        var chosen = best;
        if (bestStationary != null && best != null &&
            bestStationary.score >= best.score * HitAndRunTolerance)
        {
            var retreat = SafestMoveTile(GetMoveOptions());
            if (retreat != null &&
                DistanceToNearestFoe(retreat) > DistanceToNearestFoe(actor.tile))
            {
                chosen = bestStationary;
                poa.actFirst = true;
                poa.postActMoveLocation = retreat.pos;
            }
        }

        if (chosen != null)
        {
            poa.ability = chosen.ability;
            poa.target = Targets.Foe; // informational; targeting was resolved during scoring
            poa.moveLocation = chosen.moveTile.pos;
            poa.fireLocation = chosen.fireTile.pos;
            poa.attackDirection = chosen.direction;
        }
        else
        {
            poa.actFirst = false;
            MoveTowardOpponent(poa);
        }

        return poa;
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
        foreach (var tile in areaTiles)
            score += ScoreTile(ability, tile);

        if (score <= 0f)
            return;

        var mpCost = ability.GetComponent<AbilityMagicCost>();
        if (mpCost != null)
            score -= mpCost.amount * MpCostWeight;

        Option candidate = null;
        if (best == null || score > best.score)
        {
            candidate = new Option
            {
                ability = ability,
                moveTile = moveTile,
                fireTile = fireTile,
                direction = direction,
                score = score
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
                score = score
            };
        }
    }

    private float ScoreTile(Ability ability, Tile tile)
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

            total += ScoreEffect(effect, tile, defender, defStats, isFoe, isDown) * chance;
        }

        return total;
    }

    private float ScoreEffect(BaseAbilityEffect effect, Tile tile, Unit defender, Stats defStats, bool isFoe, bool isDown)
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
                float value = Mathf.Min(damage, hp); // overkill isn't worth extra
                if (damage >= hp)
                    value += KillBonus;
                value += (1f - hp / (float)Mathf.Max(1, defStats[StatTypes.MHP])) * LowHealthFocusWeight;
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

    /// <summary>Reachable tile farthest from every living foe (max-min distance).</summary>
    private Tile SafestMoveTile(List<Tile> moveOptions)
    {
        Tile bestTile = actor.tile;
        var bestDistance = int.MinValue;
        foreach (var tile in moveOptions)
        {
            var distance = DistanceToNearestFoe(tile);
            if (distance > bestDistance)
            {
                bestDistance = distance;
                bestTile = tile;
            }
        }

        return bestTile;
    }

    #endregion
}
