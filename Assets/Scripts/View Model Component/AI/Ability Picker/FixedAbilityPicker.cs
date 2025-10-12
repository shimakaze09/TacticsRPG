public class FixedAbilityPicker : BaseAbilityPicker
{
    public string ability;
    public Targets target;

    public override void Pick(PlanOfAttack plan)
    {
        plan.target = target;
        plan.ability = Find(ability);

        if (plan.ability == null || !plan.ability.CanPerform())
        {
            plan.ability = Default();
            plan.target = Targets.Foe;
        }
    }
}