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
            this.SubscribeToSender<TurnBeganEvent>(OnTurnBegan, owner);
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
            this.UnsubscribeFromSender<TurnBeganEvent>(OnTurnBegan, owner);
        
        if (stats != null)
            this.UnsubscribeFromSender<StatDidChangeEvent>(OnStatChanged, stats);
    }

    private void OnTurnBegan(TurnBeganEvent e)
    {
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
            var cond = GetComponent<StatusCondition>();
            if (cond != null)
                cond.Remove();
            else
                Destroy(this);
        }
    }

    private void BecomePermaKO()
    {
        // Randomly choose crystal or treasure (50/50)
        bool isCrystal = Random.value > 0.5f;

        // Remove KO status
        var cond = GetComponent<StatusCondition>();
        if (cond != null)
            cond.Remove();

        // Add crystal or treasure status
        if (isCrystal)
            owner.gameObject.AddComponent<CrystalStatus>();
        else
            owner.gameObject.AddComponent<TreasureStatus>();
    }

    public int GetDeathCounter()
    {
        return currentCounter;
    }
}
