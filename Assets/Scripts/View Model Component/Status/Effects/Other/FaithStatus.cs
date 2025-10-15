using UnityEngine;

/// <summary>
/// Faith: Unit is treated as if they have maximum (100) Faith points.
/// Raises spellcasting ability but also raises effect of spells cast on that unit.
/// Opposite of Atheist. Lasts for 32 ticks (~3-4 turns).
/// </summary>
public class FaithStatus : StatusEffect
{
    [Tooltip("Faith value to set (100 = maximum in FFT)")]
    public int maxFaith = 100;

    private Stats stats;

    private void OnEnable()
    {
        stats = GetComponentInParent<Stats>();

        if (stats != null)
        {
            // Set Faith to maximum
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
        // If your system has a Faith stat, set it to max
        // For now, this status serves as a flag for the magic system
    }

    public int GetEffectiveFaith()
    {
        return maxFaith;
    }
}
