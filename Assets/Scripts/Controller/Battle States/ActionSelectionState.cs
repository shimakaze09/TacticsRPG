using System.Collections.Generic;

public class ActionSelectionState : BaseAbilityMenuState
{
    public static int category;
    private AbilityCatalog catalog;

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
        catalog = turn.actor.GetComponentInChildren<AbilityCatalog>();
        var container = catalog.GetCategory(category);
        menuTitle = container.name;

        var count = catalog.AbilityCount(container);
        if (menuOptions == null)
            menuOptions = new List<string>(count);
        else
            menuOptions.Clear();

        var locks = new bool[count];
        for (var i = 0; i < count; i++)
        {
            var ability = catalog.GetAbility(category, i);
            var cost = ability.GetComponent<AbilityMagicCost>();
            menuOptions.Add(cost ? $"{ability.name}: {cost.amount}" : ability.name);
            locks[i] = !ability.CanPerform();
        }

        abilityMenuPanelController.Show(menuTitle, menuOptions);
        for (var i = 0; i < count; i++)
            abilityMenuPanelController.SetLocked(i, locks[i]);
    }

    protected override void Confirm()
    {
        turn.ability = catalog.GetAbility(category, abilityMenuPanelController.selection);
        owner.ChangeState<AbilityTargetState>();
    }

    protected override void Cancel()
    {
        owner.ChangeState<CategorySelectionState>();
    }
}