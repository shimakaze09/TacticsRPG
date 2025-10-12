using System.Collections;
using UnityEngine;

public class SelectUnitState : BattleState
{
    private Coroutine changeUnitRoutine;

    public override void Enter()
    {
        base.Enter();
        changeUnitRoutine = StartCoroutine(ChangeCurrentUnit());
    }

    public override void Exit()
    {
        if (changeUnitRoutine != null)
        {
            StopCoroutine(changeUnitRoutine);
            changeUnitRoutine = null;
        }

        base.Exit();
        statPanelController.HidePrimary();
    }

    private IEnumerator ChangeCurrentUnit()
    {
        owner.round.MoveNext();
        SelectTile(turn.actor.tile.pos);
        RefreshPrimaryStatPanel(pos);
        yield return null;
        owner.ChangeState<CommandSelectionState>();

        changeUnitRoutine = null;
    }
}