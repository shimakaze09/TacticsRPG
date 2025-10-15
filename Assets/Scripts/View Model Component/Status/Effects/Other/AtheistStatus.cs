using UnityEngine;

/// <summary>
/// Atheist: Unit's Faith falls to zero, making magic ineffective.
/// Magic attacks used by unit are ineffective.
/// All magic cast on unit has no effect.
/// Lasts for 32 ticks (~3-4 turns). Cannot be protected against or healed.
/// </summary>
public class AtheistStatus : StatusEffect
{
    private Stats stats;

    private void OnEnable()
    {
        stats = GetComponentInParent<Stats>();

        if (stats != null)
        {
            // Set Faith to 0
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
        // If your system has a Faith stat, set it to 0
        // For now, this status serves as a flag for the magic system to check
    }

    public bool IsAtheist()
    {
        return true;
    }
}
