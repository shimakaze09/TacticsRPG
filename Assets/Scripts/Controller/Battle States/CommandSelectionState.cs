using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Battle state: top-level Move/Act/Wait menu for the active unit; hands CPU
/// units to the AI plan instead.
/// </summary>
public class CommandSelectionState : BaseAbilityMenuState
{
    public override void Enter()
    {
        base.Enter();
        statPanelController.ShowPrimary(turn.actor.gameObject);
        if (driver.Current == Drivers.Computer)
            StartCoroutine(ComputerTurn());
    }

    public override void Exit()
    {
        base.Exit();
        statPanelController.HidePrimary();
    }

    protected override void LoadMenu()
    {
        if (menuOptions == null)
        {
            menuTitle = "Commands";
            menuOptions = new List<string>(3)
            {
                "Move",
                "Action",
                "Wait"
            };
        }

        var unit = turn.actor;
        var canMove = unit.GetComponent<Movement>().CanMove();

        abilityMenuPanelController.Show(menuTitle, menuOptions);
        abilityMenuPanelController.SetLocked(0, turn.hasUnitMoved || !canMove);
        abilityMenuPanelController.SetLocked(1, turn.hasUnitActed);
    }

    protected override void Confirm()
    {
        switch (abilityMenuPanelController.selection)
        {
            case 0: // Move
                owner.ChangeState<MoveTargetState>();
                break;
            case 1: // Action
                owner.ChangeState<CategorySelectionState>();
                break;
            case 2: // Wait
                owner.ChangeState<EndFacingState>();
                break;
        }
    }

    protected override void Cancel()
    {
        if (turn.hasUnitMoved && !turn.lockMove)
        {
            turn.UndoMove();
            abilityMenuPanelController.SetLocked(0, false);
            SelectTile(turn.actor.tile.pos);
        }
        else
        {
            owner.ChangeState<ExploreState>();
        }
    }

    // Runs the AI plan across re-entries of this state: each leg (move, act)
    // transitions out and returns here until the turn is spent.
    private IEnumerator ComputerTurn()
    {
        if (turn.plan == null)
        {
            // A behavior status (Scrambled/Redline) dictates the turn
            // instead of the battle brain
            var dictator = turn.actor.GetComponentInChildren<ITurnPlanOverride>();
            turn.plan = dictator != null ? dictator.BuildPlan(owner, turn.actor) : owner.cpu.Evaluate();
            turn.ability = turn.plan.ability;
        }

        yield return new WaitForSeconds(1f);

        if (turn.plan.actFirst)
        {
            // Hit-and-run: strike from the current tile, then spend the move
            // on the retreat leg.
            if (turn.hasUnitActed == false && turn.plan.ability != null)
            {
                owner.ChangeState<AbilityTargetState>();
            }
            else if (turn.hasUnitMoved == false && turn.plan.postActMoveLocation != turn.actor.tile.pos)
            {
                turn.plan.moveLocation = turn.plan.postActMoveLocation;
                owner.ChangeState<MoveTargetState>();
            }
            else
            {
                owner.ChangeState<EndFacingState>();
            }

            yield break;
        }

        if (turn.hasUnitMoved == false && turn.plan.moveLocation != turn.actor.tile.pos)
            owner.ChangeState<MoveTargetState>();
        else if (turn.hasUnitActed == false && turn.plan.ability != null)
            owner.ChangeState<AbilityTargetState>();
        else
            owner.ChangeState<EndFacingState>();
    }
}