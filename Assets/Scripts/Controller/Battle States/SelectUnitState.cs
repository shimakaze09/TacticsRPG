using UnityEngine;
using System.Collections;

public class SelectUnitState : BattleState
{
    private int index = -1;

    public override void Enter()
    {
        base.Enter();
        StartCoroutine(ChangeCurrentUnit());
    }

    private IEnumerator ChangeCurrentUnit()
    {
        index = (index + 1) % units.Count;
        turn.Change(units[index]);
        yield return null;
        owner.ChangeState<CommandSelectionState>();
    }
}