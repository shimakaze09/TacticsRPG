using UnityEngine;

/// <summary>
/// Redline (berserk): the unit fights past its limits — outgoing physical
/// damage +33%, but the AI takes the wheel and it charges the NEAREST unit
/// on any side, swinging its basic attack whenever the target is in reach.
/// </summary>
public class RedlineStatus : StatusEffect, ITurnPlanOverride
{
    [Tooltip("Multiplier applied to outgoing physical damage")]
    public float damageMultiplier = 1.33f;

    private Unit owner;
    private Driver driver;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        driver = GetComponentInParent<Driver>();
        this.Subscribe<TweakDamageEvent>(OnTweakDamage);
        ControlSeizure.Seize(driver);
    }

    private void OnDisable()
    {
        this.Unsubscribe<TweakDamageEvent>(OnTweakDamage);
        ControlSeizure.Release(driver, owner, this);
    }

    private void OnTweakDamage(TweakDamageEvent e)
    {
        if (e.Attacker != owner || !e.IsPhysical)
            return;

        // High sortOrder so the multiplier applies after additive tweaks
        e.Modifiers.Add(new MultValueModifier(100, damageMultiplier));
    }

    /// <summary>Charge whoever is closest — friend or foe — and swing.</summary>
    public PlanOfAttack BuildPlan(BattleController bc, Unit actor)
    {
        var plan = new PlanOfAttack { target = Targets.None };
        plan.moveLocation = actor.tile.pos;
        plan.postActMoveLocation = actor.tile.pos;

        var prey = NearestUnit(bc, actor);
        if (prey == null)
            return plan;

        // Close in: the reachable tile nearest the prey
        var movement = actor.GetComponent<Movement>();
        var destination = actor.tile;
        if (movement != null)
        {
            var best = Distance(actor.tile.pos, prey.tile.pos);
            foreach (var tile in movement.GetTilesInRange(bc.board))
            {
                var d = Distance(tile.pos, prey.tile.pos);
                if (d < best)
                {
                    best = d;
                    destination = tile;
                }
            }
        }

        plan.moveLocation = destination.pos;
        plan.postActMoveLocation = destination.pos;

        // Swing when the prey is inside the weapon's reach from there
        var attack = FindBasicAttack(actor);
        if (attack != null)
        {
            var range = attack.GetComponent<AbilityRange>();
            if (range is WeaponAbilityRange weaponRange)
                weaponRange.Refresh();
            var reach = range != null ? range.horizontal : 1;
            if (Distance(destination.pos, prey.tile.pos) <= reach)
            {
                plan.ability = attack;
                plan.fireLocation = prey.tile.pos;
            }
        }

        return plan;
    }

    // The closest living unit that isn't the berserker itself
    private static Unit NearestUnit(BattleController bc, Unit actor)
    {
        Unit nearest = null;
        var best = int.MaxValue;
        foreach (var unit in bc.units)
        {
            if (unit == actor || unit.tile == null)
                continue;
            var stats = unit.GetComponent<Stats>();
            if (stats == null || stats[StatTypes.HP] <= 0)
                continue;

            var d = Distance(actor.tile.pos, unit.tile.pos);
            if (d < best)
            {
                best = d;
                nearest = unit;
            }
        }

        return nearest;
    }

    private static int Distance(Point a, Point b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private static Ability FindBasicAttack(Unit actor)
    {
        foreach (var ability in actor.GetComponentsInChildren<Ability>())
            if (ability.name == "Attack")
                return ability;
        return null;
    }
}
