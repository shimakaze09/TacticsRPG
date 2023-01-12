using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ability : MonoBehaviour
{
    public const string CanPerformCheck = "Ability.CanPerformCheck";
    public const string FailedNotification = "Ability.FailedNotification";
    public const string DidPerformNotification = "Ability.DidPerformNotification";

    public bool CanPerform()
    {
        var exc = new BaseException(true);
        this.PostNotification(CanPerformCheck, exc);
        return exc.toggle;
    }

    public void Perform(List<Tile> targets)
    {
        if (!CanPerform())
        {
            this.PostNotification(FailedNotification);
            return;
        }

        for (var i = 0; i < targets.Count; ++i)
            Perform(targets[i]);

        this.PostNotification(DidPerformNotification);
    }

    public bool IsTarget(Tile tile)
    {
        var obj = transform;
        for (var i = 0; i < obj.childCount; ++i)
        {
            var targeter = obj.GetChild(i).GetComponent<AbilityEffectTarget>();
            if (targeter.IsTarget(tile))
                return true;
        }

        return false;
    }

    private void Perform(Tile target)
    {
        for (var i = 0; i < transform.childCount; ++i)
        {
            var child = transform.GetChild(i);
            var effect = child.GetComponent<BaseAbilityEffect>();
            effect.Apply(target);
        }
    }
}