using UnityEngine;

/// <summary>
/// Steeled: hard-won resistance after being controlled. Each stack adds
/// ControlBudget.SteeledResistancePerStack to the unit's effective RES
/// against further status attempts, so control chains hit diminishing
/// returns instead of locking a unit down indefinitely (issue #57).
/// Applied automatically by StatusRegistry.Inflict whenever a hard-control
/// status lands; hooks the same hit-rate status check as Static/Ghosted.
/// </summary>
public class SteeledStatus : StatusEffect
{
    [Tooltip("How many control applications this unit has recently absorbed")]
    public int stacks = 1;

    private Unit owner;

    // Subscribes while active — the status system removes the whole child
    // object when the duration condition expires
    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        this.Subscribe<HitRateStatusCheckEvent>(OnHitRateCheck);
    }

    private void OnDisable()
    {
        this.Unsubscribe<HitRateStatusCheckEvent>(OnHitRateCheck);
    }

    // Raises effective resistance when this unit is the one being targeted
    private void OnHitRateCheck(HitRateStatusCheckEvent e)
    {
        if (e.Target != owner)
            return;

        e.Args.HitRate += stacks * ControlBudget.SteeledResistancePerStack;
    }
}
