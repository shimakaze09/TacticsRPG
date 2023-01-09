using UnityEngine;
using System.Collections;

public class FullTypeHitRate : HitRate
{
    public override int Calculate(Tile target)
    {
        var defender = target.content.GetComponent<Unit>();
        return Final(AutomaticMiss(defender) ? 100 : 0);
    }
}