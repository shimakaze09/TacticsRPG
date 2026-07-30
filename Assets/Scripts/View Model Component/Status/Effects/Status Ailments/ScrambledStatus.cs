using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scrambled (confusion): the AI takes the wheel and the unit acts at
/// random — it wanders to a random reachable tile and may swing its basic
/// attack at a random nearby tile, friend, foe, or empty air. Cancelled
/// when the afflicted unit takes damage (base class).
/// </summary>
public class ScrambledStatus : DamageRemovableStatusEffect, ITurnPlanOverride
{
    private Driver driver;
    private Unit owner;

    private void OnEnable()
    {
        driver = GetComponentInParent<Driver>();
        owner = GetComponentInParent<Unit>();
        ControlSeizure.Seize(driver);
    }

    private void OnDisable()
    {
        ControlSeizure.Release(driver, owner, this);
    }

    /// <summary>Random wander, and a coin-flip swing at whatever is close.</summary>
    public PlanOfAttack BuildPlan(BattleController bc, Unit actor)
    {
        var plan = new PlanOfAttack { target = Targets.None };

        // Wander: any reachable tile (25% chance to stay put)
        var destination = actor.tile;
        var movement = actor.GetComponent<Movement>();
        if (movement != null && Random.value > 0.25f)
        {
            var reachable = movement.GetTilesInRange(bc.board);
            if (reachable.Count > 0)
                destination = reachable[Random.Range(0, reachable.Count)];
        }

        plan.moveLocation = destination.pos;
        plan.postActMoveLocation = destination.pos;

        // Coin flip: swing the basic attack at a random neighboring tile of
        // wherever it ends up — hitting anyone or nothing at all
        if (Random.value > 0.5f)
        {
            plan.ability = FindBasicAttack(actor);
            var directions = new List<Point>
                { new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0) };
            var swing = directions[Random.Range(0, directions.Count)];
            var target = bc.board.GetTile(destination.pos + swing);
            plan.fireLocation = target != null ? target.pos : destination.pos;
        }

        return plan;
    }

    // The unit's plain Attack ability, if it has one
    private static Ability FindBasicAttack(Unit actor)
    {
        foreach (var ability in actor.GetComponentsInChildren<Ability>())
            if (ability.name == "Attack")
                return ability;
        return null;
    }
}
