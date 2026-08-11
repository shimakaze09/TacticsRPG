using System;

/// <summary>
/// Results payload passed from the battle to the post-battle flow. Produced
/// only by RewardPolicy (forecast and settle share this shape) and consumed
/// by RewardPolicy.Commit exactly once — the committed flag makes re-entry a
/// no-op.
/// </summary>
[Serializable]
public class BattleResultsData
{
    public bool victory;
    public int policyVersion;
    public int expGained;
    public int jpGained;
    public int goldGained;
    public string[] itemsGained;
    public Unit[] playerUnits; // For level up checks and job changes

    /// <summary>Set by RewardPolicy.Commit; guards double payment.</summary>
    [NonSerialized]
    public bool committed;
}
