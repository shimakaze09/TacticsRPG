using System.Collections;

/// <summary>
/// Battle state: executes the confirmed ability against its targets, marks the
/// unit as having acted, then advances the turn.
/// </summary>
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
        turn.ability.Perform(turn.targets);
    }

    private bool UnitHasControl()
    {
        return turn.actor.GetComponentInChildren<KOStatus>() == null;
    }
}