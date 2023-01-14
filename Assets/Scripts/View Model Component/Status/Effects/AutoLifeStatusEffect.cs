using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoLifeStatusEffect : StatusEffect
{
    private Unit owner;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        if (owner)
            this.AddObserver(OnTurnBegin, TurnOrderController.TurnCheckNotification, owner);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnTurnBegin, TurnOrderController.TurnCheckNotification, owner);
    }

    private void OnTurnBegin(object sender, object args)
    {
        var stats = GetComponent<Stats>();
        stats[StatTypes.HP] += Mathf.FloorToInt(stats[StatTypes.MHP] * 0.05f);
    }
}