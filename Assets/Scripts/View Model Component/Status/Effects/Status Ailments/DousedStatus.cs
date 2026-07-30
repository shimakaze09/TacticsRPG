using UnityEngine;

/// <summary>
/// Doused: soaked in flammable slag — all incoming damage +25%, and fire
/// damage +50% instead (the slag ignites).
/// </summary>
public class DousedStatus : StatusEffect
{
    [Tooltip("Multiplier applied to incoming damage")]
    public float damageMultiplier = 1.25f;

    [Tooltip("Multiplier applied to incoming FIRE damage instead")]
    public float fireDamageMultiplier = 1.5f;

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
        if (e.Target != owner)
            return;

        var multiplier = e.HasElement && e.Element == ElementTypes.Fire
            ? fireDamageMultiplier
            : damageMultiplier;

        // High sortOrder so the multiplier applies after additive tweaks
        e.Modifiers.Add(new MultValueModifier(100, multiplier));
    }
}
