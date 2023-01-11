public class DefeatTargetVictoryCondition : BaseVictoryCondition
{
    public Unit target;

    protected override void CheckForGameOver()
    {
        base.CheckForGameOver();
        if (Victor == Alliances.None && IsDefeated(target))
            Victor = Alliances.Hero;
    }
}