using System.Collections;
using UnityEngine;

/// <summary>
/// Battle state: advances the round loop until a unit's turn comes up, then
/// selects it and opens its command menu.
/// </summary>
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

    // Advances the scheduler to the next unit — or ends the battle when a
    // non-damage victory (e.g. survive-N-rounds) has been reached
    private IEnumerator ChangeCurrentUnit()
    {
        if (IsBattleOver())
        {
            owner.ChangeState<CutSceneState>();
            changeUnitRoutine = null;
            yield break;
        }

        owner.round.MoveNext();
        SelectTile(turn.actor.tile.pos);
        RefreshPrimaryStatPanel(pos);
        yield return null;
        owner.ChangeState<CommandSelectionState>();

        changeUnitRoutine = null;
    }
}