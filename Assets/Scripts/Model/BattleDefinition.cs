using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An authored battle: which map, who spawns where, how the battle is won,
/// and what reinforcements arrive when. Replaces the random test spawner for
/// story contracts; battles without a definition fall back to writ-style
/// generation. Authored via Tactics RPG → Battle Definition assets.
/// </summary>
[CreateAssetMenu(fileName = "New Battle", menuName = "Tactics RPG/Battle Definition")]
public class BattleDefinition : ScriptableObject
{
    [Tooltip("Stable id for quest/story flags (never changes once shipped)")]
    public string id;

    [Tooltip("Display name shown in briefing/results")]
    public string battleName;

    [Tooltip("The board to load")]
    public LevelData level;

    [Tooltip("Player units and where they deploy")]
    public List<SpawnEntry> heroes = new List<SpawnEntry>();

    [Tooltip("Enemy units and where they start")]
    public List<SpawnEntry> enemies = new List<SpawnEntry>();

    [Tooltip("How this battle is won")]
    public VictoryType victoryType = VictoryType.DefeatAll;

    [Tooltip("SurviveRounds: rounds to hold out (a round ≈ every unit acting once)")]
    public int surviveRounds = 6;

    [Tooltip("DefeatTarget: index into the enemies list")]
    public int targetEnemyIndex;

    [Tooltip("ReachZone: board tiles a hero must end a turn on to win")]
    public List<Point> zone = new List<Point>();

    [Tooltip("Reinforcement waves triggered by battle round")]
    public List<ReinforcementWave> waves = new List<ReinforcementWave>();

    [Tooltip("Authored contract rewards (RewardPolicy settles and commits them)")]
    public ContractRewards rewards = new ContractRewards();
}

/// <summary>
/// A contract's authored pay: flat scrip amounts, per-participant EXP/Cert,
/// deterministic salvage, and the defeat consolation fraction. Flat by design
/// — pay never scales with kill count, so reinforcements cannot be farmed
/// (issue #59). RewardPolicy is the only reader.
/// </summary>
[Serializable]
public class ContractRewards
{
    [Tooltip("Scrip paid on victory")]
    public int basePay;

    [Tooltip("Extra scrip when every enemy is down at battle end (full clear)")]
    public int bonusPay;

    [Tooltip("EXP each participating player unit earns on victory")]
    public int expAward;

    [Tooltip("Cert each participating player unit earns on victory")]
    public int certAward;

    [Tooltip("GearCatalog ids granted to the party inventory on victory")]
    public List<string> salvage = new List<string>();

    [Tooltip("Percent of basePay still paid on defeat (0 = nothing)")]
    [Range(0, 100)]
    public int defeatPayPercent;
}

/// <summary>One unit to spawn: recipe name, level, board position, facing.</summary>
[Serializable]
public class SpawnEntry
{
    public string recipe;
    public int level = 1;
    public Point position;
    public Directions facing = Directions.South;
}

/// <summary>Extra spawns arriving at the start of a given battle round.</summary>
[Serializable]
public class ReinforcementWave
{
    [Tooltip("Battle round on which this wave arrives (see BattleClock)")]
    public int round = 2;

    public List<SpawnEntry> spawns = new List<SpawnEntry>();
}

/// <summary>Victory rules a definition can select (escort arrives with M2).</summary>
public enum VictoryType
{
    DefeatAll,
    DefeatTarget,
    SurviveRounds,
    ReachZone
}
