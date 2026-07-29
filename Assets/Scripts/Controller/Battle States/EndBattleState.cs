using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Battle state: decides victory or defeat and hands results to the game flow
/// (or returns to title when played standalone).
/// </summary>
public class EndBattleState : BattleState
{
    public override void Enter()
    {
        base.Enter();

        bool victory = CheckVictoryCondition();

        if (GameFlowController.Instance != null)
        {
            owner.EndBattleWithResults(victory);
        }
        else
        {
            // No flow controller (battle scene played directly in the editor):
            // just return to the title scene.
            Debug.LogWarning("[EndBattleState] GameFlowController not found, returning to title scene");
            SceneManager.LoadScene(0);
        }
    }

    /// <summary>
    /// Determines the battle outcome. Prefers the installed victory
    /// condition's verdict; falls back to "any hero still standing".
    /// </summary>
    private bool CheckVictoryCondition()
    {
        var condition = owner.GetComponent<BaseVictoryCondition>();
        if (condition != null && condition.Victor != Alliances.None)
            return condition.Victor == Alliances.Hero;

        foreach (var unit in owner.units)
        {
            if (unit != null)
            {
                var alliance = unit.GetComponent<Alliance>();
                var health = unit.GetComponent<Health>();

                if (alliance != null && alliance.type == Alliances.Hero &&
                    health != null && health.HP > health.MinHP)
                {
                    return true; // At least one player unit alive = victory
                }
            }
        }

        return false; // No player units alive = defeat
    }
}
