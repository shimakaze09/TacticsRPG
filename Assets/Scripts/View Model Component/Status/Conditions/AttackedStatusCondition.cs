using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackedStatusCondition : StatusCondition
{
    private Stats stats;


    private void OnEnable()
    {
        stats = GetComponentInParent<Stats>();
        if (stats)
            this.AddObserver(OnHitNotification, Stats.DidChangeNotification(StatTypes.HP), stats);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnHitNotification, Stats.DidChangeNotification(StatTypes.HP), stats);
    }

    private void OnHitNotification(object sender, object args)
    {
    }
}