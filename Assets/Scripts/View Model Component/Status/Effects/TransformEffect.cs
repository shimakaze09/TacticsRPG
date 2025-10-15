using UnityEngine;

public class TransformEffect : MonoBehaviour
{
    [Tooltip("Optional cap on ATK while transformed. Set <= 0 to ignore.")]
    public int capAtk = 0;

    private Stats stats;

    private void OnEnable()
    {
        stats = GetComponentInParent<Stats>();
        if (capAtk > 0 && stats != null)
            this.AddObserver(OnAtkWillChange, Stats.WillChangeNotification(StatTypes.ATK), stats);
    }

    private void OnDisable()
    {
        if (capAtk > 0 && stats != null)
            this.RemoveObserver(OnAtkWillChange, Stats.WillChangeNotification(StatTypes.ATK), stats);
    }

    private void OnAtkWillChange(object sender, object args)
    {
        var exc = args as ValueChangeException;
        if (exc == null)
            return;

        if (capAtk > 0)
            exc.AddModifier(new MinValueModifier(0, capAtk));
    }

    // Visual/model swap hooks can be added here (e.g., enabling a "frog" child object, swapping animator, etc.)
}