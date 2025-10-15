public class SilenceStatusEffect : StatusEffect
{
    private Unit owner;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        if (owner)
            this.Subscribe<AbilityCanPerformCheckEvent>(OnCanPerformCheck);
    }

    private void OnDisable()
    {
        this.Unsubscribe<AbilityCanPerformCheckEvent>(OnCanPerformCheck);
    }

    private void OnCanPerformCheck(AbilityCanPerformCheckEvent e)
    {
        var unit = e.Ability.GetComponentInParent<Unit>();
        if (owner == unit && e.Ability.TryGetComponent(typeof(AbilityMagicCost), out _))
        {
            if (e.Exception.defaultToggle)
                e.Exception.FlipToggle();
        }
    }
}