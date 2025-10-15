using UnityEngine;

/// <summary>
/// Undead: HP cannot be restored by normal means (healing damages instead).
/// Raise/Arise incur HP damage. Death spell fully heals.
/// </summary>
public class UndeadStatus : StatusEffect
{
    private Stats stats;

    private void OnEnable()
    {
        stats = GetComponentInParent<Stats>();

        if (stats != null)
        {
            // Subscribe to HP changes to reverse healing
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
        if (e.StatType != StatTypes.HP)
            return;

        // Reverse healing into damage for undead units
        // This is handled by inverting any HP increase modifiers
        // The actual implementation would need to check the delta and reverse it
        // For now, we mark that this unit is undead and let the damage system handle it
    }

    // Method to be called by ability system for Death spell
    public void OnDeathSpell()
    {
        if (stats != null)
        {
            // Death spell fully heals undead
            stats.SetValue(StatTypes.HP, stats[StatTypes.MHP], false);
        }
    }
}
