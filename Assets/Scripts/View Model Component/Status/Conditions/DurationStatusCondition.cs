using UnityEngine;
using System.Collections;

public class DurationStatusCondition : StatusCondition
{
    public int duration = 10;

    private void OnEnable()
    {
        this.AddObserver(OnNewTurn, TurnOrderController.TurnBeganNotification);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnNewTurn, TurnOrderController.TurnBeganNotification);
    }

    private void OnNewTurn(object sender, object args)
    {
        duration--;
        if (duration <= 0)
            Remove();
    }
}