using UnityEngine;

/// <summary>
/// Oil: Unit becomes weak to fire attacks, doubling damage taken.
/// Removed after any fire attack hits. Otherwise lasts until end of battle.
/// </summary>
public class OilStatus : StatusEffect
{
    [Tooltip("Fire damage multiplier (2.0 = double damage)")]
    public float fireDamageMultiplier = 2.0f;

    private Unit owner;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
    }

    private void OnDisable()
    {
        // Cleanup if needed
    }

    // Called by damage system when hit by fire attack
    public void OnHitByFireAttack()
    {
        // Remove Oil after being hit by fire
        var cond = GetComponent<StatusCondition>();
        if (cond != null)
            cond.Remove();
        else
            Destroy(this);
    }

    public float GetFireDamageMultiplier()
    {
        return fireDamageMultiplier;
    }
}
