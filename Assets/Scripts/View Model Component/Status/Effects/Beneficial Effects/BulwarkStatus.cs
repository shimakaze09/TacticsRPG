using UnityEngine;

/// <summary>
/// Bulwark: physical damage taken by this unit is reduced by one third.
/// Hooks the TweakDamageEvent stage so it affects real combat resolution.
/// </summary>
public class BulwarkStatus : StatusEffect
{
    [Tooltip("Multiplier applied to incoming physical damage (0.667 = -1/3)")]
    public float damageMultiplier = 0.667f;

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
        if (e.Target != owner || !e.IsPhysical)
            return;

        // High sortOrder so the multiplier applies after additive tweaks
        e.Modifiers.Add(new MultValueModifier(100, damageMultiplier));
    }
}
