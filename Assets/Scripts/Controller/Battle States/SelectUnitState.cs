using System.Collections;

public class SelectUnitState : BattleState
{
    public override void Enter()
    {
        base.Enter();
        StartCoroutine(ChangeCurrentUnit());
    }

    private IEnumerator ChangeCurrentUnit()
    {
        owner.round.MoveNext();
        SelectTile(turn.actor.tile.pos);
        yield return null;
        owner.ChangeState<CommandSelectionState>();
    }
}