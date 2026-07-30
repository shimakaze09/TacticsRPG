using UnityEngine;

/// <summary>
/// Shredded: the unit's plating is torn open — incoming physical damage
/// +25% while active. The armor-shred payload for weapons like the Pry
/// Hook (via StatusOnHit gear traits) and future rending abilities.
/// </summary>
public class ShreddedStatus : StatusEffect
{
    [Tooltip("Multiplier applied to incoming physical damage")]
    public float damageMultiplier = 1.25f;

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
