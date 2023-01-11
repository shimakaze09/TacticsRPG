public class EndFacingState : BattleState
{
    private Directions startDir;

    public override void Enter()
    {
        base.Enter();
        startDir = turn.actor.dir;
        SelectTile(turn.actor.tile.pos);
        owner.facingIndicator.gameObject.SetActive(true);
        owner.facingIndicator.SetDirection(turn.actor.dir);
    }

    public override void Exit()
    {
        owner.facingIndicator.gameObject.SetActive(false);
        base.Exit();
    }

    protected override void OnMove(object sender, InfoEventArgs<Point> e)
    {
        turn.actor.dir = e.info.GetDirection();
        turn.actor.Match();
        owner.facingIndicator.SetDirection(turn.actor.dir);
    }

    protected override void OnFire(object sender, InfoEventArgs<int> e)
    {
        switch (e.info)
        {
            case 0:
                owner.ChangeState<SelectUnitState>();
                break;
            case 1:
                turn.actor.dir = startDir;
                turn.actor.Match();
                owner.ChangeState<CommandSelectionState>();
                break;
        }
    }
}