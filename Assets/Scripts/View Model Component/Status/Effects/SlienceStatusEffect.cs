using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlienceStatusEffect : StatusEffect
{
    private Unit owner;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        if (owner)
            this.AddObserver(OnCanPerformCheck, Ability.CanPerformCheck);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnCanPerformCheck, Ability.CanPerformCheck);
    }

    private void OnCanPerformCheck(object sender, object args)
    {
        var unit = (sender as Ability).GetComponentInParent<Unit>();
        if (owner == unit)
        {
            var exc = args as BaseException;
            if (exc.defaultToggle)
                exc.FlipToggle();
        }
    }
}