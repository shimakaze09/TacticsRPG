using UnityEngine;
using System.Collections;

public class PerformAbilityState : BattleState
{
    public override void Enter()
    {
        base.Enter();
        turn.hasUnitActed = true;
        if (turn.hasUnitMoved)
            turn.lockMove = true;
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        // TODO play animations, etc
        yield return null;
        ApplyAbility();

        if (IsBattleOver())
            owner.ChangeState<CutSceneState>();
        else if (!UnitHasControl())
            owner.ChangeState<SelectUnitState>();
        else if (turn.hasUnitMoved)
            owner.ChangeState<EndFacingState>();
        else
            owner.ChangeState<CommandSelectionState>();
    }

    private void ApplyAbility()
    {
        var effects = turn.ability.GetComponentsInChildren<BaseAbilityEffect>();
        foreach (var target in turn.targets)
        foreach (var effect in effects)
        {
            var targeter = effect.GetComponent<AbilityEffectTarget>();
            if (targeter.IsTarget(target))
            {
                var rate = effect.GetComponent<HitRate>();
                var chance = rate.Calculate(target);
                if (Random.Range(0, 101) > chance)
                    // A Miss!
                    // TODO: Add animations, etc.
                    continue;
                effect.Apply(target);
            }
        }
    }

    private bool UnitHasControl()
    {
        // TODO: Add confuse status effect, etc.
        return turn.actor.GetComponentInChildren<KnockOutStatusEffect>() == null;
    }
}