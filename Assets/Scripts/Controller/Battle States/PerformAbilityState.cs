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
        // TODO apply ability effect, etc
        TemporaryAttackExample();

        if (turn.hasUnitMoved)
            owner.ChangeState<EndFacingState>();
        else
            owner.ChangeState<CommandSelectionState>();
    }

    private void TemporaryAttackExample()
    {
        foreach (var target in turn.targets)
        {
            var obj = target.content;
            var stats = obj != null ? obj.GetComponentInChildren<Stats>() : null;
            if (stats != null)
            {
                stats[StatTypes.HP] -= 50;
                if (stats[StatTypes.HP] <= 0)
                    Debug.Log("KO'd Uni!", obj);
            }
        }
    }
}