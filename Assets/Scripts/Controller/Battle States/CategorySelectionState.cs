using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CategorySelectionState : BaseAbilityMenuState
{
    public override void Enter()
    {
        base.Enter();
        statPanelController.ShowPrimary(turn.actor.gameObject);
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
            menuTitle = "Action";
            menuOptions = new List<string>(3)
            {
                "Attack",
                "White Magic",
                "Black Magic"
            };
        }

        abilityMenuPanelController.Show(menuTitle, menuOptions);
    }

    protected override void Confirm()
    {
        switch (abilityMenuPanelController.selection)
        {
            case 0:
                Attack();
                break;
            case 1:
                SetCategory(0);
                break;
            case 2:
                SetCategory(1);
                break;
        }
    }

    protected override void Cancel()
    {
        owner.ChangeState<CommandSelectionState>();
    }

    private void Attack()
    {
        turn.hasUnitActed = true;
        if (turn.hasUnitMoved)
            turn.lockMove = true;
        owner.ChangeState<CommandSelectionState>();
    }

    private void SetCategory(int index)
    {
        ActionSelectionState.category = index;
        owner.ChangeState<ActionSelectionState>();
    }
}