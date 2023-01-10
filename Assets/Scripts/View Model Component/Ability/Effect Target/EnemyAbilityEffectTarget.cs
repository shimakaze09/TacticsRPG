using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyAbilityEffectTarget : AbilityEffectTarget
{
    private Alliance alliance;

    private void Start()
    {
        alliance = GetComponentInParent<Alliance>();
    }

    public override bool IsTarget(Tile tile)
    {
        if (tile == null || tile.content == null)
            return false;

        var other = tile.content.GetComponentInChildren<Alliance>();
        return alliance.IsMatch(other, Targets.Foe);
    }
}