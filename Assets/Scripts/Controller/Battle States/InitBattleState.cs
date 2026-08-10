using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Battle state: builds the board, picks the AI per difficulty, spawns units
/// (from an authored BattleDefinition when one is pending, else writ-style
/// random generation), installs the victory condition and event hooks, and
/// starts the round loop.
/// </summary>
public class InitBattleState : BattleState
{
    public override void Enter()
    {
        base.Enter();
        StartCoroutine(Init());
    }

    /// <summary>
    /// The battle to run: the game flow's pending contract first, the scene's
    /// dev/test override second, writ-style random generation when neither.
    /// </summary>
    private BattleDefinition Definition
    {
        get
        {
            if (GameFlowController.Instance != null && GameFlowController.Instance.PendingBattle != null)
                return GameFlowController.Instance.PendingBattle;
            return owner.testBattle;
        }
    }

    // Builds the battle in dependency order, then hands off to the cutscene
    private IEnumerator Init()
    {
        var definition = Definition;
        var level = definition != null && definition.level != null ? definition.level : levelData;

        board.Load(level);
        var p = new Point((int)level.tiles[0].x, (int)level.tiles[0].z);
        SelectTile(p);

        ConfigureAI();
        if (owner.GetComponent<ElevationRules>() == null)
            owner.gameObject.AddComponent<ElevationRules>();
        if (owner.GetComponent<ElementRules>() == null)
            owner.gameObject.AddComponent<ElementRules>();
        if (owner.GetComponent<StatusExpiryRules>() == null)
            owner.gameObject.AddComponent<StatusExpiryRules>();

        var clock = owner.gameObject.AddComponent<BattleClock>();

        var unitContainer = new GameObject("Units");
        unitContainer.transform.SetParent(owner.transform);

        if (definition != null)
            SpawnFromDefinition(definition, unitContainer.transform);
        else
            SpawnWritUnits(unitContainer.transform);

        clock.Configure(units.Count);
        AddVictoryCondition(definition);

        if (definition != null && definition.waves != null && definition.waves.Count > 0)
        {
            var events = owner.gameObject.AddComponent<BattleEvents>();
            events.Configure(owner, definition, unitContainer.transform);
        }

        owner.round = owner.gameObject.AddComponent<TurnOrderController>().Round();
        yield return null;
        owner.ChangeState<CutSceneState>();
    }

    /// <summary>
    /// Picks the CPU brain for this battle: the tactical AI on Hard,
    /// the classic pattern AI on Easy.
    /// </summary>
    private void ConfigureAI()
    {
        if (DifficultySettings.Current == Difficulty.Hard)
        {
            var tactical = owner.GetComponent<TacticalComputerPlayer>();
            owner.cpu = tactical != null ? tactical : owner.gameObject.AddComponent<TacticalComputerPlayer>();
        }
        else if (owner.cpu == null || owner.cpu is TacticalComputerPlayer)
        {
            // Find (or add) the plain ComputerPlayer — GetComponent would also
            // match the tactical subclass, so filter by exact type.
            ComputerPlayer basic = null;
            foreach (var candidate in owner.GetComponents<ComputerPlayer>())
            {
                if (candidate.GetType() == typeof(ComputerPlayer))
                {
                    basic = candidate;
                    break;
                }
            }

            owner.cpu = basic != null ? basic : owner.gameObject.AddComponent<ComputerPlayer>();
        }

        Debug.Log($"[InitBattleState] Difficulty: {DifficultySettings.Current}, AI: {owner.cpu.GetType().Name}");
    }

    // Spawns every authored unit at its designed position (BattleSpawner
    // registers each unit in owner.units itself)
    private void SpawnFromDefinition(BattleDefinition definition, Transform container)
    {
        foreach (var entry in definition.heroes)
            BattleSpawner.Spawn(owner, entry, container);

        foreach (var entry in definition.enemies)
            BattleSpawner.Spawn(owner, entry, container);

        if (units.Count > 0)
            SelectTile(units[0].tile.pos);

        Debug.Log($"[InitBattleState] Authored battle '{definition.battleName}': {units.Count} units");
    }

    /// <summary>
    /// Writ-style fallback: the repeatable-contract generator (GDD §4.5.3) —
    /// random placements, randomized levels. Also the dev path when the
    /// Battle scene is played directly with no definition assigned.
    /// </summary>
    private void SpawnWritUnits(Transform container)
    {
        var recipes = new[]
        {
            "Alaois",
            "Hania",
            "Kamau",
            "Enemy Rogue",
            "Enemy Warrior",
            "Enemy Wizard"
        };

        var locations = new List<Tile>(board.tiles.Values);
        foreach (var recipe in recipes)
        {
            var level = Random.Range(9, 12);
            var instance = UnitFactory.Create(recipe, level);
            instance.transform.SetParent(container);

            // Only tiles this unit's locomotion can stand on
            var mask = BattleSpawner.PlacementMask(instance);
            var standable = new List<int>();
            for (var i = 0; i < locations.Count; i++)
                if (locations[i].CanStop(mask))
                    standable.Add(i);

            var random = standable[Random.Range(0, standable.Count)];
            var randomTile = locations[random];
            locations.RemoveAt(random);

            var unit = instance.GetComponent<Unit>();
            unit.Place(randomTile);
            unit.dir = (Directions)Random.Range(0, 4);
            unit.Match();

            units.Add(unit);
        }

        SelectTile(units[0].tile.pos);
    }

    // Installs the definition's victory rule (writ battles keep the classic
    // defeat-the-leader rule)
    private void AddVictoryCondition(BattleDefinition definition)
    {
        if (definition == null)
        {
            var writCondition = owner.gameObject.AddComponent<DefeatTargetVictoryCondition>();
            var enemy = units[units.Count - 1];
            writCondition.target = enemy;
            var health = enemy.GetComponent<Health>();
            health.MinHP = 10;
            return;
        }

        switch (definition.victoryType)
        {
            case VictoryType.DefeatTarget:
            {
                var condition = owner.gameObject.AddComponent<DefeatTargetVictoryCondition>();
                var index = definition.heroes.Count + Mathf.Clamp(definition.targetEnemyIndex, 0, definition.enemies.Count - 1);
                condition.target = units[Mathf.Clamp(index, 0, units.Count - 1)];
                break;
            }
            case VictoryType.SurviveRounds:
            {
                var condition = owner.gameObject.AddComponent<SurviveRoundsVictoryCondition>();
                condition.rounds = definition.surviveRounds;
                break;
            }
            case VictoryType.ReachZone:
            {
                var condition = owner.gameObject.AddComponent<ReachZoneVictoryCondition>();
                condition.zone.AddRange(definition.zone);
                break;
            }
            default:
                owner.gameObject.AddComponent<DefeatAllEnemiesVictoryCondition>();
                break;
        }
    }
}
