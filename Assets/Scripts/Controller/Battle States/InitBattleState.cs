using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InitBattleState : BattleState
{
    public override void Enter()
    {
        base.Enter();
        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        board.Load(levelData);
        var p = new Point((int)levelData.tiles[0].x, (int)levelData.tiles[0].z);
        SelectTile(p);
        SpawnTestUnits();
        AddVictoryCondition();
        owner.round = owner.gameObject.AddComponent<TurnOrderController>().Round();
        yield return null;
        owner.ChangeState<CutSceneState>();
    }

    private void SpawnTestUnits()
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

        var unitContainer = new GameObject("Units");
        unitContainer.transform.SetParent(owner.transform);

        var locations = new List<Tile>(board.tiles.Values);
        foreach (var recipe in recipes)
        {
            var level = Random.Range(9, 12);
            var instance = UnitFactory.Create(recipe, level);
            instance.transform.SetParent(unitContainer.transform);

            var random = Random.Range(0, locations.Count);
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

    private void AddVictoryCondition()
    {
        var vc = owner.gameObject.AddComponent<DefeatTargetVictoryCondition>();
        var enemy = units[units.Count - 1];
        vc.target = enemy;
        var health = enemy.GetComponent<Health>();
        health.MinHP = 10;
    }
}