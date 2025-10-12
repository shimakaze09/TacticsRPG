using UnityEngine;
using UnityEngine.SceneManagement;

public class EndBattleState : BattleState
{
    public override void Enter()
    {
        base.Enter();
        var level = Random.Range(0, 4);
        owner.levelData = Resources.Load<LevelData>($"Levels/Level_{level}.asset");
        SceneManager.LoadScene(0);
    }
}