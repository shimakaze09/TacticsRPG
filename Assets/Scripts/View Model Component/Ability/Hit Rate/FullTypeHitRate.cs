using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullTypeHitRate : HitRate
{
    public override int Calculate(Unit attacker, Unit target)
    {
        return Final(AutomaticMiss(attacker, target) ? 100 : 0);
    }
}