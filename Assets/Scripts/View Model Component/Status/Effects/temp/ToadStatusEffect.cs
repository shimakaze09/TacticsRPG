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
            this.AddObserver(OnAtkWillChange, Stats.WillChangeNotification(StatTypes.ATK), myStats);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnAtkWillChange, Stats.WillChangeNotification(StatTypes.ATK), myStats);
    }

    private void OnAtkWillChange(object sender, object args)
    {
        var exc = args as ValueChangeException;
        // Use a MinValueModifier to cap the final ATK value to maxAtkWhileToad.
        var m = new MinValueModifier(0, maxAtkWhileToad);
        exc.AddModifier(m);
    }
}