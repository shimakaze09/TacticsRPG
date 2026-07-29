using UnityEngine;

/// <summary>
/// KO (Knocked Out): Unit's HP reached 0. 
/// CT still increases; when it reaches 100, death counter drops by 1 (from 3).
/// If counter reaches 0 on next active turn, becomes Crystal or Treasure.
/// </summary>
public class KOStatus : StatusEffect
{
    [Tooltip("Number of turns KO'd before becoming crystal/treasure")]
    public int deathCounter = 3;

    private Unit owner;
    private Stats stats;
    private int currentCounter;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        stats = GetComponentInParent<Stats>();
        currentCounter = deathCounter;

        if (owner != null)
        {
            this.SubscribeToSender<TurnCheckEvent>(OnTurnCheck, owner);
        }

        if (stats != null)
        {
            // Subscribe to HP changes in case of revival
            this.SubscribeToSender<StatDidChangeEvent>(OnStatChanged, stats);
        }
    }

    private void OnDisable()
    {
        if (owner != null)
            this.UnsubscribeFromSender<TurnCheckEvent>(OnTurnCheck, owner);

        if (stats != null)
            this.UnsubscribeFromSender<StatDidChangeEvent>(OnStatChanged, stats);
    }

    private void OnTurnCheck(TurnCheckEvent e)
    {
        // A KO'd unit never acts. Each time it *would* have activated, the
        // death counter ticks down instead (and CT resets, as in FFT).
        if (!e.Exception.toggle)
            return;

        e.Exception.FlipToggle();

        if (stats != null)
            stats.SetValue(StatTypes.CTR, 0, false);

        currentCounter--;
        if (currentCounter <= 0)
        {
            // Become crystal or treasure
            BecomePermaKO();
        }
    }

    private void OnStatChanged(StatDidChangeEvent e)
    {
        if (e.StatType != StatTypes.HP)
            return;

        // If HP is restored above 0, remove KO status
        if (e.NewValue > 0)
        {
            var cond = GetComponentInChildren<StatusCondition>();
            if (cond != null)
                cond.Remove();
            else
                Destroy(this);
        }
    }

    // Converts the fallen unit into collectible remains and removes it from
    // battle entirely — it no longer occupies its tile, takes scheduler
    // ticks, or appears in AI planning.
    private void BecomePermaKO()
    {
        bool isCore = Random.value > 0.5f;
        RemainsPickup.Spawn(owner, isCore);

        var bc = FindAnyObjectByType<BattleController>();
        if (bc != null)
            bc.units.Remove(owner);

        if (owner.tile != null && owner.tile.content == owner.gameObject)
            owner.tile.content = null;

        Destroy(owner.gameObject);
    }

    public int GetDeathCounter()
    {
        return currentCounter;
    }
}
