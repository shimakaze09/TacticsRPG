using UnityEngine;

public class PoisonStatusEffect : StatusEffect
{
    private Unit owner;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        if (owner)
            this.SubscribeToSender<TurnBeganEvent>(OnNewTurn, owner);
    }

    private void OnDisable()
    {
        if (owner != null)
            this.UnsubscribeFromSender<TurnBeganEvent>(OnNewTurn, owner);
    }

    private void OnNewTurn(TurnBeganEvent e)
    {
        var s = GetComponentInParent<Stats>();
        var currentHP = s[StatTypes.HP];
        var maxHP = s[StatTypes.MHP];
        var reduce = Mathf.Min(currentHP, Mathf.FloorToInt(maxHP * 0.1f));
        s.SetValue(StatTypes.HP, currentHP - reduce, false);
    }
}