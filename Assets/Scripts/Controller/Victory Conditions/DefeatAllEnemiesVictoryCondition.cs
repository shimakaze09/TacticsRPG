/// <summary>
/// Victory when every enemy unit is defeated.
/// </summary>
public class DefeatAllEnemiesVictoryCondition : BaseVictoryCondition
{
    protected override void CheckForGameOver()
    {
        base.CheckForGameOver();
        if (Victor == Alliances.None && PartyDefeated(Alliances.Enemy))
            Victor = Alliances.Hero;
    }
}