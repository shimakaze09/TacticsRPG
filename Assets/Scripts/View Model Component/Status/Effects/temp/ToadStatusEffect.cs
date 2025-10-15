using UnityEngine;


public class ToadStatusEffect : StatusEffect
{
    private Stats myStats;

    // While toad is active, cap ATK to "maxAtkWhileToad" (FFT typically reduces to 1)
    public int maxAtkWhileToad = 1;

    private void OnEnable()
    {
        myStats = GetComponentInParent<Stats>();
        if (myStats)
            this.SubscribeToSender<StatWillChangeEvent>(OnAtkWillChange, myStats);
    }

    private void OnDisable()
    {
        if (myStats != null)
            this.UnsubscribeFromSender<StatWillChangeEvent>(OnAtkWillChange, myStats);
    }

    private void OnAtkWillChange(StatWillChangeEvent e)
    {
        if (e.StatType != StatTypes.ATK)
            return;

        // Use a MinValueModifier to cap the final ATK value to maxAtkWhileToad.
        var m = new MinValueModifier(0, maxAtkWhileToad);
        e.Exception.AddModifier(m);
    }
}