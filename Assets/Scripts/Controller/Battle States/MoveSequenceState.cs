using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSequenceState : BattleState
{
    public override void Enter()
    {
        base.Enter();
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        var m = owner.currentUnit.GetComponent<Movement>();
        yield return StartCoroutine(m.Traverse(owner.currentTile));
        owner.ChangeState<SelectUnitState>();
    }
}