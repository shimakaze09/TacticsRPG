using UnityEngine;

public class PeriodicEffect : MonoBehaviour
{
    [Tooltip("Number of turns between each tick. 1 = every turn of the owner.")]
    public int tickCooldown = 1;
    [Tooltip("Amount to modify. If isPercent is true, this is treated as fraction of max (0.05 = 5%).")]
    public float amount = 0.05f;
    [Tooltip("If true, modify relative to max value of the statType. If false, modify as integer.")]
    public bool isPercent = true;
    public StatTypes statType = StatTypes.HP;

    private int counter = 0;
    private Unit owner;
    private Stats stats;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        stats = GetComponentInParent<Stats>();
        if (owner != null)
            this.SubscribeToSender<TurnBeganEvent>(OnTurnBegan, owner);
    }

    private void OnDisable()
    {
        if (owner != null)
            this.UnsubscribeFromSender<TurnBeganEvent>(OnTurnBegan, owner);
    }

    private void OnTurnBegan(TurnBeganEvent e)
    {
        counter++;
        if (counter >= tickCooldown)
        {
            counter = 0;
            ApplyTick();
        }
    }

    private void ApplyTick()
    {
        if (stats == null)
            stats = GetComponentInParent<Stats>();
        if (stats == null)
            return;

        if (statType == StatTypes.HP || statType == StatTypes.MHP)
        {
            var max = stats[StatTypes.MHP];
            int delta;
            if (isPercent)
                delta = Mathf.FloorToInt(max * amount);
            else
                delta = Mathf.RoundToInt(amount);

            // allow negative amounts for poison
            var newHp = Mathf.Clamp(stats[StatTypes.HP] + delta, 0, max);
            stats.SetValue(StatTypes.HP, newHp, false);
        }
        else if (statType == StatTypes.MP || statType == StatTypes.MMP)
        {
            var max = stats[StatTypes.MMP];
            int delta;
            if (isPercent)
                delta = Mathf.FloorToInt(max * amount);
            else
                delta = Mathf.RoundToInt(amount);

            var newMp = Mathf.Clamp(stats[StatTypes.MP] + delta, 0, max);
            stats.SetValue(StatTypes.MP, newMp, false);
        }
        else
        {
            // statType other than HP/MP could be supported if desired
        }
    }
}