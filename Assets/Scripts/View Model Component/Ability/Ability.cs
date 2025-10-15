using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    public bool CanPerform()
    {
        var exc = new BaseException(true);
        this.Publish(new AbilityCanPerformCheckEvent(this, exc));
        return exc.toggle;
    }

    public void Perform(List<Tile> targets)
    {
        if (!CanPerform())
        {
            this.Publish(new AbilityFailedEvent(this));
            return;
        }

        foreach (var tile in targets)
            Perform(tile);

        this.Publish(new AbilityDidPerformEvent(this));
    }

    public bool IsTarget(Tile tile)
    {
        var obj = transform;
        for (var i = 0; i < obj.childCount; i++)
        {
            var targeter = obj.GetChild(i).GetComponent<AbilityEffectTarget>();
            if (targeter.IsTarget(tile))
                return true;
        }

        return false;
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