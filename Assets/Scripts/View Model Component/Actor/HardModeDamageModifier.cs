using UnityEngine;

/// <summary>
/// Added to enemy units when difficulty is Hard (see UnitFactory):
/// boosts this unit's outgoing damage by the difficulty multiplier.
/// </summary>
public class HardModeDamageModifier : MonoBehaviour
{
    private Unit owner;

    private void OnEnable()
    {
        owner = GetComponent<Unit>();
        this.Subscribe<TweakDamageEvent>(OnTweakDamage);
    }

    private void OnDisable()
    {
        this.Unsubscribe<TweakDamageEvent>(OnTweakDamage);
    }

    private void OnTweakDamage(TweakDamageEvent e)
    {
        if (e.Attacker != owner)
            return;

        // Before the status multipliers (sortOrder 100) — order is arbitrary
        // for pure multiplication but keeps the log readable
        e.Modifiers.Add(new MultValueModifier(95, DifficultySettings.EnemyDamageMultiplier));
    }
}
