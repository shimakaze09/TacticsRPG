using UnityEngine;
using UnityEngine.SceneManagement;

public class EndBattleState : BattleState
{
    public override void Enter()
    {
        base.Enter();

        // Check victory condition
        bool victory = CheckVictoryCondition();

        // Use new GameStateManager integration if available
        if (GameStateManager.Instance != null)
        {
            // Transition to PostBattle state with results
            owner.EndBattleWithResults(victory);
        }
        else
        {
            // Fallback to old behavior (reload random level)
            Debug.LogWarning("[EndBattleState] GameStateManager not found, using legacy behavior");
            var level = Random.Range(0, 4);
            owner.levelData = Resources.Load<LevelData>($"Levels/Level_{level}.asset");
            SceneManager.LoadScene(0);
        }
    }

    /// <summary>
    /// Check if player won or lost
    /// (You can customize this based on your victory conditions)
    /// </summary>
    private bool CheckVictoryCondition()
    {
        // Simple check: If any player units are alive, it's a victory
        // You should customize this based on your actual victory conditions
        foreach (var unit in owner.units)
        {
            if (unit != null)
            {
                var alliance = unit.GetComponent<Alliance>();
                var health = unit.GetComponent<Health>();

                if (alliance != null && alliance.type == Alliances.Hero &&
                    health != null && health.HP > 0)
                {
                    return true; // At least one player unit alive = victory
                }
            }
        }

        return false; // No player units alive = defeat
    }
}