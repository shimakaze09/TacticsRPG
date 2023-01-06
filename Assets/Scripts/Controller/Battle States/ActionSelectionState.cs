using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionSelectionState : BaseAbilityMenuState
{
    public static int category;
    private string[] whiteMagicOptions = { "Cure", "Raise", "Holy" };
    private string[] blackMagicOptions = { "Fire", "Ice", "Lightning" };

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
        menuOptions ??= new List<string>(3);

        if (category == 0)
        {
            menuTitle = "White Magic";
            SetOptions(whiteMagicOptions);
        }
        else
        {
            menuTitle = "Black Magic";
            SetOptions(blackMagicOptions);
        }

        abilityMenuPanelController.Show(menuTitle, menuOptions);
    }

    protected override void Confirm()
    {
        turn.hasUnitActed = true;
        if (turn.hasUnitMoved)
            turn.lockMove = true;
        owner.ChangeState<CommandSelectionState>();
    }

    protected override void Cancel()
    {
        owner.ChangeState<CategorySelectionState>();
    }

    private void SetOptions(string[] options)
    {
        menuOptions.Clear();
        foreach (var option in options)
            menuOptions.Add(option);
    }
}