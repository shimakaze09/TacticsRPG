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
            this.AddObserver(OnStatWillChange, Stats.WillChangeNotification(statType), stats);
    }

    private void OnDisable()
    {
        if (stats != null)
            this.RemoveObserver(OnStatWillChange, Stats.WillChangeNotification(statType), stats);
    }

    private void OnStatWillChange(object sender, object args)
    {
        var exc = args as ValueChangeException;
        if (exc == null)
            return;

        if (mode == Mode.Multiply)
        {
            var m = new MultValueModifier(0, value);
            exc.AddModifier(m);
        }
        else
        {
            var a = new AddValueModifier(0, (int)Mathf.Round(value));
            exc.AddModifier(a);
        }
    }
}