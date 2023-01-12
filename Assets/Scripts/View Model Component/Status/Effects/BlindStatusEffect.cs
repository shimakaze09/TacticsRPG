using UnityEngine;
using System.Collections;

public class BlindStatusEffect : StatusEffect
{
    private void OnEnable()
    {
        this.AddObserver(OnHitRateStatusCheck, HitRate.StatusCheckNotification);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnHitRateStatusCheck, HitRate.StatusCheckNotification);
    }

    private void OnHitRateStatusCheck(object sender, object args)
    {
        var info = args as Info<Unit, Unit, int>;
        var owner = GetComponentInParent<Unit>();
        if (owner == info.arg0)
            // The attacker is blind
            info.arg2 += 50;
        else if (owner == info.arg1)
            // The defender is blind
            info.arg2 -= 20;
    }
}