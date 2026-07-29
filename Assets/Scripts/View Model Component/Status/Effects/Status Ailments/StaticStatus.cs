using UnityEngine;

/// <summary>
/// Static: vision full of interference — everything this unit attacks is
/// effectively +30 harder to hit. Hooks the hit-rate status check.
/// </summary>
public class StaticStatus : StatusEffect
{
    [Tooltip("Added to the target's effective evade/resist when this unit attacks")]
    public int hitPenalty = 30;

    private Unit owner;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        this.Subscribe<HitRateStatusCheckEvent>(OnHitRateCheck);
    }

    private void OnDisable()
    {
        this.Unsubscribe<HitRateStatusCheckEvent>(OnHitRateCheck);
    }

    private void OnHitRateCheck(HitRateStatusCheckEvent e)
    {
        if (e.Attacker != owner)
            return;

        e.Args.HitRate += hitPenalty;
    }

    public bool IsBlind()
    {
        return true;
    }
}
