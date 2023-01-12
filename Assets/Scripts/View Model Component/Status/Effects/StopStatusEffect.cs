﻿using UnityEngine;
using System.Collections;

public class StopStatusEffect : StatusEffect
{
    private Stats myStats;

    private void OnEnable()
    {
        myStats = GetComponentInParent<Stats>();
        if (myStats)
            this.AddObserver(OnCounterWillChange, Stats.WillChangeNotification(StatTypes.CTR), myStats);
        this.AddObserver(OnAutomaticHitCheck, HitRate.AutomaticHitCheckNotification);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnCounterWillChange, Stats.WillChangeNotification(StatTypes.CTR), myStats);
        this.RemoveObserver(OnAutomaticHitCheck, HitRate.AutomaticHitCheckNotification);
    }

    private void OnCounterWillChange(object sender, object args)
    {
        var exc = args as ValueChangeException;
        exc.FlipToggle();
    }

    private void OnAutomaticHitCheck(object sender, object args)
    {
        var owner = GetComponentInParent<Unit>();
        var exc = args as MatchException;
        if (owner == exc.target)
            exc.FlipToggle();
    }
}