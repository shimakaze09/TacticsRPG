using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ComputerPlayer : MonoBehaviour
{
    #region Fields

    private BattleController bc;
    private Unit actor => bc.turn.actor;
    private Alliance alliance => actor.GetComponent<Alliance>();
    private Unit nearestFoe;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        bc = GetComponent<BattleController>();
    }

    #endregion

    #region Public

    public PlanOfAttack Evaluate()
    {
        var poa = new PlanOfAttack();
        var pattern = actor.GetComponentInChildren<AttackPattern>();
        if (pattern)
            pattern.Pick(poa);
        else
            DefaultAttackPattern(poa);

        if (IsPositionIndependent(poa))
            PlanPositionIndependent(poa);
        else if (IsDirectionIndependent(poa))
            PlanDirectionIndependent(poa);
        else
            PlanDirectionDependent(poa);

        if (poa.ability == null)
            MoveTowardOpponent(poa);

        return poa;
    }

    #endregion

    #region Private

    private void DefaultAttackPattern(PlanOfAttack poa)
    {
        // Just get the first "Attack" ability
        poa.ability = actor.GetComponentInChildren<Ability>();
        poa.target = Targets.Foe;
    }

    private bool IsPositionIndependent(PlanOfAttack poa)
    {
        var range = poa.ability.GetComponent<AbilityRange>();
        return range.positionOriented == false;
    }

    private bool IsDirectionIndependent(PlanOfAttack poa)
    {
        var range = poa.ability.GetComponent<AbilityRange>();
        return !range.directionOriented;
    }

    private void PlanPositionIndependent(PlanOfAttack poa)
    {
        var moveOptions = GetMoveOptions();
        var tile = moveOptions[Random.Range(0, moveOptions.Count)];
        poa.moveLocation = poa.fireLocation = tile.pos;
    }

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

    private List<Tile> GetMoveOptions()
    {
        var options = actor.GetComponent<Movement>().GetTilesInRange(bc.board);
        options.Add(actor.tile);
        return options;
    }

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

    private void FindNearestFoe()
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

    private void MoveTowardOpponent(PlanOfAttack poa)
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

    public Directions DetermineEndFacingDirection()
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