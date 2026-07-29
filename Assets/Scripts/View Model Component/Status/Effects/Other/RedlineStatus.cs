using UnityEngine;

/// <summary>
/// Redline: the unit fights past its limits — outgoing physical damage +33%.
/// (Loss-of-control AI behavior is a separate, later feature.)
/// </summary>
public class RedlineStatus : StatusEffect
{
    [Tooltip("Multiplier applied to outgoing physical damage")]
    public float damageMultiplier = 1.33f;

    private Unit owner;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        this.Subscribe<TweakDamageEvent>(OnTweakDamage);
    }

    private void OnDisable()
    {
        this.Unsubscribe<TweakDamageEvent>(OnTweakDamage);
    }

    private void OnTweakDamage(TweakDamageEvent e)
    {
        if (e.Attacker != owner || !e.IsPhysical)
            return;

        // High sortOrder so the multiplier applies after additive tweaks
        e.Modifiers.Add(new MultValueModifier(100, damageMultiplier));
    }

    public bool IsBerserk()
    {
        return true;
    }
}
