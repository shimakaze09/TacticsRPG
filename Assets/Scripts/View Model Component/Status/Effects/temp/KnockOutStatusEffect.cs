using UnityEngine;

public class KnockOutStatusEffect : StatusEffect
{
    private Unit owner;
    private Stats stats;

    private void Awake()
    {
        owner = GetComponentInParent<Unit>();
        stats = owner.GetComponent<Stats>();
    }

    private void OnEnable()
    {
        owner.transform.localScale = new Vector3(0.75f, 0.1f, 0.75f);
        this.SubscribeToSender<TurnCheckEvent>(OnTurnCheck, owner);
        this.SubscribeToSender<StatWillChangeEvent>(OnStatCounterWillChange, stats);
    }

    private void OnDisable()
    {
        owner.transform.localScale = Vector3.one;
        this.UnsubscribeFromSender<TurnCheckEvent>(OnTurnCheck, owner);
        if (stats != null)
            this.UnsubscribeFromSender<StatWillChangeEvent>(OnStatCounterWillChange, stats);
    }

    private void OnTurnCheck(TurnCheckEvent e)
    {
        // Don't allow a KO'd unit to take turns
        if (e.Exception.defaultToggle)
            e.Exception.FlipToggle();
    }

    private void OnStatCounterWillChange(StatWillChangeEvent e)
    {
        if (e.StatType != StatTypes.CTR)
            return;

        // Don't allow a KO'd unit to increment the turn order counter
        if (e.Exception.toValue > e.Exception.fromValue)
            e.Exception.FlipToggle();
    }
}