using UnityEngine;

/// <summary>
/// Berserk: Unit's effective physical attack increased by 50%.
/// Cannot be controlled - attacks nearby enemies automatically.
/// Cannot use reaction or certain movement abilities.
/// Never naturally wears off - must be healed with Esuna or similar.
/// </summary>
public class BerserkStatus : StatusEffect
{
    [Tooltip("Physical attack multiplier (1.5 = +50%)")]
    public float attackMultiplier = 1.5f;

    private Unit owner;
    private Stats stats;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        stats = GetComponentInParent<Stats>();

        if (stats != null)
        {
            // Increase physical attack
            this.SubscribeToSender<StatWillChangeEvent>(OnStatWillChange, stats);
        }
    }

    private void OnDisable()
    {
        if (stats != null)
            this.UnsubscribeFromSender<StatWillChangeEvent>(OnStatWillChange, stats);
    }

    private void OnStatWillChange(StatWillChangeEvent e)
    {
        // Boost physical attack by 50%
        if (e.StatType == StatTypes.ATK)
        {
            var modifier = new MultValueModifier(0, attackMultiplier);
            e.Exception.AddModifier(modifier);
        }
    }

    public bool IsBerserk()
    {
        return true;
    }
}
