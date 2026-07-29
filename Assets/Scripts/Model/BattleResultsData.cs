using System;

/// <summary>
/// Results payload passed from the battle to the post-battle flow.
/// </summary>
[Serializable]
public class BattleResultsData
{
    public bool victory;
    public int expGained;
    public int jpGained;
    public int goldGained;
    public string[] itemsGained;
    public Unit[] playerUnits; // For level up checks and job changes
}
