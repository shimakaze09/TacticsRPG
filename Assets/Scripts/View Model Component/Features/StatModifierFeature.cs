/// <summary>
/// Feature that adjusts a stat while active (weapon ATK bonuses, armor DEF).
/// </summary>
public class StatModifierFeature : Feature
{
    #region Fields / Properties

    public StatTypes type;
    public int amount;

    private Stats stats => _target.GetComponentInParent<Stats>();

    #endregion

    #region Protected

    protected override void OnApply()
    {
        stats[type] += amount;
    }

    protected override void OnRemove()
    {
        stats[type] -= amount;
    }

    #endregion
}