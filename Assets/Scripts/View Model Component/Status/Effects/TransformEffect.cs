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
            this.SubscribeToSender<StatWillChangeEvent>(OnAtkWillChange, stats);
    }

    private void OnDisable()
    {
        if (capAtk > 0 && stats != null)
            this.UnsubscribeFromSender<StatWillChangeEvent>(OnAtkWillChange, stats);
    }

    private void OnAtkWillChange(StatWillChangeEvent e)
    {
        if (e.StatType != StatTypes.ATK)
            return;

        if (capAtk > 0)
            e.Exception.AddModifier(new MinValueModifier(0, capAtk));
    }

    // Visual/model swap hooks can be added here (e.g., enabling a "frog" child object, swapping animator, etc.)
}