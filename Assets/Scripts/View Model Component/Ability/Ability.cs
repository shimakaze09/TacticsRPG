using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        foreach (var target in targets)
            Perform(target);

        this.PostNotification(DidPerformNotification);
    }

    private void Perform(Tile target)
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            var effect = child.GetComponent<BaseAbilityEffect>();
            effect.Apply(target);
        }
    }
}