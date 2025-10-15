using UnityEngine;

public abstract class StatModifierEffect : StatusEffect
{
    public enum Mode { Add, Multiply }
    public StatTypes statType = StatTypes.DEF;
    public Mode mode = Mode.Multiply;
    [Tooltip("Add uses raw value addition; Multiply uses multiplier (1.5 = +50%).")]
    public float value = 1.0f;

    private Stats stats;

    private void OnEnable()
    {
        stats = GetComponentInParent<Stats>();
        if (stats != null)
            this.SubscribeToSender<StatWillChangeEvent>(OnStatWillChange, stats);
    }

    private void OnDisable()
    {
        if (stats != null)
            this.UnsubscribeFromSender<StatWillChangeEvent>(OnStatWillChange, stats);
    }

    private void OnStatWillChange(StatWillChangeEvent e)
    {
        if (e.StatType != statType)
            return;

        if (mode == Mode.Multiply)
        {
            var m = new MultValueModifier(0, value);
            e.Exception.AddModifier(m);
        }
        else
        {
            var a = new AddValueModifier(0, (int)Mathf.Round(value));
            e.Exception.AddModifier(a);
        }
    }
}