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
        FaceTargets();
        StartCoroutine(Animate());
    }

    // A unit strikes where it's looking: face the target area before the
    // blow, so visuals and facing-based rules (hit rates, flank bonuses)
    // agree. Self-targeted actions keep the current facing.
    private void FaceTargets()
    {
        if (turn.targets == null || turn.actor == null || turn.actor.tile == null)
            return;

        foreach (var tile in turn.targets)
        {
            if (tile == turn.actor.tile)
                continue;

            turn.actor.dir = turn.actor.tile.GetDirection(tile);
            turn.actor.Match();
            return;
        }
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