using UnityEngine;

/// <summary>
/// The unit is readying an ability. Cannot evade attacks.
/// Units attacking a charging unit have +50% physical attack.
/// </summary>
public class ChargingStatus : StatusEffect
{
    private Unit owner;
    private Stats stats;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        stats = GetComponentInParent<Stats>();

        if (stats != null)
        {
            // Subscribe to evasion checks - charging units cannot evade
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
        // Reduce evasion to 0 while charging
        if (e.StatType == StatTypes.EVD)
        {
            var modifier = new MultValueModifier(0, 0f);
            e.Exception.AddModifier(modifier);
        }
    }
}
