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
        owner.round = owner.gameObject.AddComponent<TurnOrderController>().Round();
        yield return null;
        owner.ChangeState<CutSceneState>();
    }

    private void SpawnTestUnits()
    {
        var jobs = new string[] { "Rogue", "Warrior", "Wizard" };
        for (var i = 0; i < jobs.Length; ++i)
        {
            var instance = Instantiate(owner.heroPrefab) as GameObject;

            var s = instance.AddComponent<Stats>();
            s[StatTypes.LVL] = 1;

            var jobPrefab = Resources.Load<GameObject>("Jobs/" + jobs[i]);
            var jobInstance = Instantiate(jobPrefab) as GameObject;
            jobInstance.transform.SetParent(instance.transform);

            var job = jobInstance.GetComponent<Job>();
            job.Employ();
            job.LoadDefaultStats();

            var p = new Point((int)levelData.tiles[i].x, (int)levelData.tiles[i].z);

            var unit = instance.GetComponent<Unit>();
            unit.Place(board.GetTile(p));
            unit.Match();

            instance.AddComponent<WalkMovement>();

            units.Add(unit);

            var rank = instance.AddComponent<Rank>();
            rank.Init(10);

            instance.AddComponent<Health>();
            instance.AddComponent<Mana>();

            instance.name = jobs[i];
        }
    }
}