using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Easy-difficulty AI: follows the unit's AttackPattern, rates firing
/// positions by target marks and attack angle, and otherwise advances toward
/// the nearest foe. Degrades safely — a missing or malformed pattern falls
/// back to the unit's basic attack, and a turn with nothing worth doing
/// becomes a simple advance.
/// </summary>
public class ComputerPlayer : MonoBehaviour
{
    #region MonoBehaviour

    // Caches the battle this brain plans for (it lives on the controller)
    private void Awake()
    {
        bc = GetComponent<BattleController>();
    }

    #endregion

    #region Public

    /// <summary>
    /// Builds the acting unit's plan for this turn: pick an ability via the
    /// unit's attack pattern (basic attack when absent or empty), choose the
    /// best firing position for it, or advance when nothing is in reach.
    /// </summary>
    public virtual PlanOfAttack Evaluate()
    {
        var poa = new PlanOfAttack();
        var pattern = actor.GetComponentInChildren<AttackPattern>();
        if (pattern)
            pattern.Pick(poa);

        // Empty/malformed patterns leave the plan blank — fall back to the
        // basic attack instead of planning with a null ability
        if (poa.ability == null)
            DefaultAttackPattern(poa);

        if (poa.ability != null)
        {
            if (IsPositionIndependent(poa))
                PlanPositionIndependent(poa);
            else if (IsDirectionIndependent(poa))
                PlanDirectionIndependent(poa);
            else
                PlanDirectionDependent(poa);
        }

        if (poa.ability == null)
            MoveTowardOpponent(poa);

        return poa;
    }

    #endregion

    #region Fields

    protected BattleController bc;
    protected Unit actor => bc.turn.actor;
    protected Alliance alliance => actor.GetComponent<Alliance>();
    protected Unit nearestFoe;

    #endregion

    #region Private

    // Pattern-less (or pattern-failed) units swing their basic attack; the
    // resolver warns and degrades when even that is missing
    private void DefaultAttackPattern(PlanOfAttack poa)
    {
        poa.ability = BasicAttackResolver.Resolve(actor);
        poa.target = Targets.Foe;
    }

    // True for abilities whose fire options ignore the caster's position
    private bool IsPositionIndependent(PlanOfAttack poa)
    {
        var range = poa.ability.GetComponent<AbilityRange>();
        return range.positionOriented == false;
    }

    // True for abilities aimed at a tile rather than along a facing
    private bool IsDirectionIndependent(PlanOfAttack poa)
    {
        var range = poa.ability.GetComponent<AbilityRange>();
        return !range.directionOriented;
    }

    // Position doesn't matter: stand anywhere reachable and fire
    private void PlanPositionIndependent(PlanOfAttack poa)
    {
        var moveOptions = GetMoveOptions();
        var tile = moveOptions[Random.Range(0, moveOptions.Count)];
        poa.moveLocation = poa.fireLocation = tile.pos;
    }

    // Rates every (stand tile, aim tile) pair and keeps the best; restores
    // the actor's real tile before choosing
    private void PlanDirectionIndependent(PlanOfAttack poa)
    {
        var startTile = actor.tile;
        var map = new Dictionary<Tile, AttackOption>();
        var ar = poa.ability.GetComponent<AbilityRange>();
        var moveOptions = GetMoveOptions();

        foreach (var moveTile in moveOptions)
        {
            actor.Place(moveTile);
            var fireOptions = ar.GetTilesInRange(bc.board);

            foreach (var fireTile in fireOptions)
            {
                AttackOption ao = null;
                if (map.ContainsKey(fireTile))
                {
                    ao = map[fireTile];
                }
                else
                {
                    ao = new AttackOption();
                    map[fireTile] = ao;
                    ao.target = fireTile;
                    ao.direction = actor.dir;
                    RateFireLocation(poa, ao);
                }

                ao.AddMoveTarget(moveTile);
            }
        }

        actor.Place(startTile);
        var list = new List<AttackOption>(map.Values);
        PickBestOption(poa, list);
    }

    // Rates every (stand tile, facing) pair for directional abilities;
    // restores the actor's real tile and facing before choosing
    private void PlanDirectionDependent(PlanOfAttack poa)
    {
        var startTile = actor.tile;
        var startDirection = actor.dir;
        var list = new List<AttackOption>();
        var moveOptions = GetMoveOptions();

        foreach (var moveTile in moveOptions)
        {
            actor.Place(moveTile);

            for (var j = 0; j < 4; ++j)
            {
                actor.dir = (Directions)j;
                var ao = new AttackOption
                {
                    target = moveTile,
                    direction = actor.dir
                };
                RateFireLocation(poa, ao);
                ao.AddMoveTarget(moveTile);
                list.Add(ao);
            }
        }

        actor.Place(startTile);
        actor.dir = startDirection;
        PickBestOption(poa, list);
    }

    // True when the tile's occupant matches what the plan wants to hit
    private bool IsAbilityTargetMatch(PlanOfAttack poa, Tile tile)
    {
        var isMatch = false;
        if (poa.target == Targets.Tile)
        {
            isMatch = true;
        }
        else if (poa.target != Targets.None)
        {
            var other = tile.content.GetComponentInChildren<Alliance>();
            if (other != null && alliance.IsMatch(other, poa.target))
                isMatch = true;
        }

        return isMatch;
    }

    /// <summary>
    /// Every tile the acting unit may end its move on (its own tile
    /// included) — shared with the tactical brain via AiTurnContext.
    /// </summary>
    protected List<Tile> GetMoveOptions()
    {
        return AiTurnContext.ComputeMoveOptions(bc, actor);
    }

    // Counts favorable/unfavorable marks inside the ability's area for one
    // candidate fire location
    private void RateFireLocation(PlanOfAttack poa, AttackOption option)
    {
        var area = poa.ability.GetComponent<AbilityArea>();
        var tiles = area.GetTilesInArea(bc.board, option.target.pos);
        option.areaTargets = tiles;
        option.isCasterMatch = IsAbilityTargetMatch(poa, actor.tile);

        foreach (var tile in tiles)
        {
            if (actor.tile == tile || !poa.ability.IsTarget(tile))
                continue;

            var isMatch = IsAbilityTargetMatch(poa, tile);
            option.AddMark(tile, isMatch);
        }
    }

    // Keeps the highest-scoring options (ties broken by attack angle, then
    // randomly); clears the plan's ability when nothing scored at all
    private void PickBestOption(PlanOfAttack poa, List<AttackOption> list)
    {
        var bestScore = 1;
        var bestOptions = new List<AttackOption>();
        foreach (var option in list)
        {
            var score = option.GetScore(actor, poa.ability);
            if (score > bestScore)
            {
                bestScore = score;
                bestOptions.Clear();
                bestOptions.Add(option);
            }
            else if (score == bestScore)
            {
                bestOptions.Add(option);
            }
        }

        if (bestOptions.Count == 0)
        {
            poa.ability = null; // Clear ability as a sign not to perform it
            return;
        }

        var finalPicks = new List<AttackOption>();
        bestScore = 0;
        foreach (var option in bestOptions)
        {
            var score = option.bestAngleBasedScore;
            if (score > bestScore)
            {
                bestScore = score;
                finalPicks.Clear();
                finalPicks.Add(option);
            }
            else if (score == bestScore)
            {
                finalPicks.Add(option);
            }
        }

        var choice = finalPicks[Random.Range(0, finalPicks.Count)];
        poa.fireLocation = choice.target.pos;
        poa.attackDirection = choice.direction;
        poa.moveLocation = choice.bestMoveTile.pos;
    }

    // Board-search for the closest living foe, cached in nearestFoe
    protected void FindNearestFoe()
    {
        nearestFoe = null;
        bc.board.Search(actor.tile, delegate(Tile arg1, Tile arg2)
        {
            if (nearestFoe == null && arg2.content != null)
            {
                var other = arg2.content.GetComponentInChildren<Alliance>();
                if (other != null && alliance.IsMatch(other, Targets.Foe))
                {
                    var unit = other.GetComponent<Unit>();
                    var stats = unit.GetComponent<Stats>();
                    if (stats[StatTypes.HP] > 0)
                    {
                        nearestFoe = unit;
                        return true;
                    }
                }
            }

            return nearestFoe == null;
        });
    }

    // Fallback movement: walk the path toward the nearest foe as far as
    // reachable, or stay put when there is nowhere to go
    protected void MoveTowardOpponent(PlanOfAttack poa)
    {
        var moveOptions = GetMoveOptions();
        FindNearestFoe();
        if (nearestFoe != null)
        {
            var toCheck = nearestFoe.tile;
            while (toCheck != null)
            {
                if (moveOptions.Contains(toCheck))
                {
                    poa.moveLocation = toCheck.pos;
                    return;
                }

                toCheck = toCheck.prev;
            }
        }

        poa.moveLocation = actor.tile.pos;
    }

    /// <summary>
    /// End-of-turn facing: square up to the nearest foe when one exists,
    /// otherwise a random direction.
    /// </summary>
    public virtual Directions DetermineEndFacingDirection()
    {
        var dir = (Directions)Random.Range(0, 4);
        FindNearestFoe();
        if (nearestFoe != null)
        {
            var start = actor.dir;
            for (var i = 0; i < 4; i++)
            {
                actor.dir = (Directions)i;
                if (nearestFoe.GetFacing(actor) == Facings.Front)
                {
                    dir = actor.dir;
                    break;
                }
            }

            actor.dir = start;
        }

        return dir;
    }

    #endregion
}
