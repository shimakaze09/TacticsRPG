using System.Collections.Generic;

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
            menuOptions = new List<string>();
        else
            menuOptions.Clear();

        menuTitle = "Action";
        menuOptions.Add("Attack");

        var catalog = turn.actor.GetComponentInChildren<AbilityCatalog>();
        for (var i = 0; i < catalog.CategoryCount(); i++)
            menuOptions.Add(catalog.GetCategory(i).name);

        abilityMenuPanelController.Show(menuTitle, menuOptions);
    }

    protected override void Confirm()
    {
        if (abilityMenuPanelController.selection == 0)
            Attack();
        else
            SetCategory(abilityMenuPanelController.selection - 1);
    }

    protected override void Cancel()
    {
        owner.ChangeState<CommandSelectionState>();
    }

    private void Attack()
    {
        turn.ability = turn.actor.GetComponentInChildren<Ability>();
        owner.ChangeState<AbilityTargetState>();
    }

    private void SetCategory(int index)
    {
        ActionSelectionState.category = index;
        owner.ChangeState<ActionSelectionState>();
    }
}