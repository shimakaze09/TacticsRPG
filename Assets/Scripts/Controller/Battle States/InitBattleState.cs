using System.Collections;
using UnityEngine;

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
        yield return null;
        owner.ChangeState<SelectUnitState>();
    }

    private void SpawnTestUnits()
    {
        var components = new[] { typeof(WalkMovement), typeof(FlyMovement), typeof(TeleportMovement) };
        for (var i = 0; i < 3; ++i)
        {
            var instance = Instantiate(owner.heroPrefab) as GameObject;
            var p = new Point((int)levelData.tiles[i].x, (int)levelData.tiles[i].z);
            var unit = instance.GetComponent<Unit>();
            unit.Place(board.GetTile(p));
            unit.Match();
            var m = instance.AddComponent(components[i]) as Movement;
            m.range = 5;
            m.jumpHeight = 1;
        }
    }
}