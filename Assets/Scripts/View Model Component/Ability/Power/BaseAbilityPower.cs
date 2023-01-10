using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAbilityPower : MonoBehaviour
{
    protected abstract int GetBaseAttack();
    protected abstract int GetBaseDefense(Unit target);
    protected abstract int GetPower();

    private void OnEnable()
    {
        this.AddObserver(OnGetBaseAttack, BaseAbilityEffect.GetAttackNotification);
        this.AddObserver(OnGetBaseDefense, BaseAbilityEffect.GetDefenseNotification);
        this.AddObserver(OnGetPower, BaseAbilityEffect.GetPowerNotification);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnGetBaseAttack, BaseAbilityEffect.GetAttackNotification);
        this.RemoveObserver(OnGetBaseDefense, BaseAbilityEffect.GetDefenseNotification);
        this.RemoveObserver(OnGetPower, BaseAbilityEffect.GetPowerNotification);
    }

    private void OnGetBaseAttack(object sender, object args)
    {
        if (IsMyEffect(sender))
        {
            var info = args as Info<Unit, Unit, List<ValueModifier>>;
            info.arg2.Add(new AddValueModifier(0, GetBaseAttack()));
        }
    }

    private void OnGetBaseDefense(object sender, object args)
    {
        if (IsMyEffect(sender))
        {
            var info = args as Info<Unit, Unit, List<ValueModifier>>;
            info.arg2.Add(new AddValueModifier(0, GetBaseDefense(info.arg1)));
        }
    }

    private void OnGetPower(object sender, object args)
    {
        if (IsMyEffect(sender))
        {
            var info = args as Info<Unit, Unit, List<ValueModifier>>;
            info.arg2.Add(new AddValueModifier(0, GetPower()));
        }
    }

    private bool IsMyEffect(object sender)
    {
        var obj = sender as MonoBehaviour;
        return obj != null && obj.transform.parent == transform;
    }
}