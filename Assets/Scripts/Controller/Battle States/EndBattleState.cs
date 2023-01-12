using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndBattleState : BattleState
{
    public override void Enter()
    {
        base.Enter();
        SceneManager.LoadScene(0);
    }
}