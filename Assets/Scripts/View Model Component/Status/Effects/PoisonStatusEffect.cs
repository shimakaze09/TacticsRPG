using UnityEngine;
using System.Collections;

public class PoisonStatusEffect : StatusEffect
{
    private Unit owner;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        if (owner)
            this.AddObserver(OnNewTurn, TurnOrderController.TurnBeganNotification, owner);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnNewTurn, TurnOrderController.TurnBeganNotification, owner);
    }

    private void OnNewTurn(object sender, object args)
    {
        var s = GetComponentInParent<Stats>();
        var currentHP = s[StatTypes.HP];
        var maxHP = s[StatTypes.MHP];
        var reduce = Mathf.Min(currentHP, Mathf.FloorToInt(maxHP * 0.1f));
        s.SetValue(StatTypes.HP, currentHP - reduce, false);
    }
}