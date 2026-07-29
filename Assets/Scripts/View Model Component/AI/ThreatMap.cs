using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Per-tile danger estimate for AI planning: for a given unit, how much damage
/// its foes could deal to something standing on each tile next turn. Built
/// once per AI evaluation from every foe's movement reach and best damage
/// profile (Manhattan approximation, line of sight ignored for speed).
/// Infinite-range foes threaten the whole board — which is accurate.
/// </summary>
public class ThreatMap
{
    private readonly Dictionary<Tile, float> threat = new Dictionary<Tile, float>();

    /// <summary>Expected next-turn damage on this tile (0 when unthreatened or unknown).</summary>
    public float GetThreat(Tile tile)
    {
        return tile != null && threat.TryGetValue(tile, out var value) ? value : 0f;
    }

    /// <summary>
    /// Builds the map from the perspective of <paramref name="forUnit"/>:
    /// every living foe contributes its strongest ability's rough damage to
    /// all tiles within its movement reach plus attack range.
    /// </summary>
    public static ThreatMap Build(BattleController bc, Unit forUnit)
    {
        var map = new ThreatMap();
        if (bc == null || bc.board == null || forUnit == null)
            return map;

        var myAlliance = forUnit.GetComponent<Alliance>();
        if (myAlliance == null)
            return map;

        foreach (var tile in bc.board.tiles.Values)
            map.threat[tile] = 0f;

        foreach (var foe in bc.units)
        {
            if (foe == null || foe == forUnit || foe.tile == null)
                continue;

            var foeAlliance = foe.GetComponent<Alliance>();
            if (foeAlliance == null || !myAlliance.IsMatch(foeAlliance, Targets.Foe))
                continue;

            var stats = foe.GetComponent<Stats>();
            if (stats == null || stats[StatTypes.HP] <= 0)
                continue;

            EstimateOffense(foe, stats, out var damage, out var reach, out var everywhere);
            if (damage <= 0f)
                continue;

            if (everywhere)
            {
                foreach (var tile in bc.board.tiles.Values)
                    map.threat[tile] += damage;
                continue;
            }

            // Movement reach plus attack range, Manhattan-approximated
            var movePositions = MovePositions(foe, bc.board);
            foreach (var tile in bc.board.tiles.Values)
            {
                var closest = int.MaxValue;
                foreach (var p in movePositions)
                {
                    var d = Mathf.Abs(tile.pos.x - p.x) + Mathf.Abs(tile.pos.y - p.y);
                    if (d < closest)
                        closest = d;
                }

                if (closest <= reach)
                    map.threat[tile] += damage;
            }
        }

        return map;
    }

    // Every position the foe could stand on next turn (its own tile included)
    private static List<Point> MovePositions(Unit foe, Board board)
    {
        var positions = new List<Point> { foe.tile.pos };
        var movement = foe.GetComponent<Movement>();
        if (movement != null && movement.CanMove())
        {
            foreach (var tile in movement.GetTilesInRange(board))
                positions.Add(tile.pos);
        }

        return positions;
    }

    // Rough "how hard can this foe hit and from how far": the max over its
    // damage abilities of stat x power/100, and the longest attack reach
    private static void EstimateOffense(Unit foe, Stats stats, out float damage, out int reach, out bool everywhere)
    {
        damage = 0f;
        reach = 1;
        everywhere = false;

        foreach (var ability in foe.GetComponentsInChildren<Ability>())
        {
            var hasDamage = ability.GetComponentInChildren<DamageAbilityEffect>() != null;
            if (!hasDamage)
                continue;

            var estimate = 0f;
            var power = ability.GetComponent<BaseAbilityPower>();
            switch (power)
            {
                case PhysicalAbilityPower physical:
                    estimate = stats[StatTypes.ATK] * physical.level / 100f;
                    break;
                case MagicalAbilityPower magical:
                    estimate = stats[StatTypes.MAT] * magical.level / 100f;
                    break;
                case WeaponAbilityPower _:
                    estimate = stats[StatTypes.ATK];
                    break;
            }

            estimate = Mathf.Min(estimate, StatLimits.MaxDamagePerHit);
            if (estimate > damage)
                damage = estimate;

            var range = ability.GetComponent<AbilityRange>();
            if (range != null)
            {
                if (!range.positionOriented)
                {
                    everywhere = true;
                    continue;
                }

                var abilityReach = range.horizontal;
                var area = ability.GetComponent<SpecifyAbilityArea>();
                if (area != null)
                    abilityReach += area.horizontal;

                if (abilityReach > reach)
                    reach = abilityReach;
            }
        }
    }
}
