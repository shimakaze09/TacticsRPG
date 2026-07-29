using UnityEngine;

/// <summary>
/// Doused: soaked in flammable slag — all incoming damage +25%.
/// (Becomes a fire-specific vulnerability when the element system lands.)
/// </summary>
public class DousedStatus : StatusEffect
{
    [Tooltip("Multiplier applied to incoming damage")]
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
        if (e.Target != owner)
            return;

        // High sortOrder so the multiplier applies after additive tweaks
        e.Modifiers.Add(new MultValueModifier(100, damageMultiplier));
    }

    // Called by the (future) element system when a fire attack connects
    public void OnHitByFireAttack()
    {
        var cond = GetComponentInChildren<StatusCondition>();
        if (cond != null)
            cond.Remove();
        else
            Destroy(this);
    }
}
