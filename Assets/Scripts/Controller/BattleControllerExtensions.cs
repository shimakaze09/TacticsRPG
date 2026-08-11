using UnityEngine;

/// <summary>
/// Battle-side bridge into the game flow: when a battle ends, settles the
/// reward payload through RewardPolicy and hands it to GameFlowController for
/// the post-battle flow (or falls back to the title scene when the battle
/// scene is played standalone).
/// </summary>
public static class BattleControllerExtensions
{
    /// <summary>
    /// End battle and transition to post-battle state
    /// Call this instead of directly changing to EndBattleState
    /// </summary>
    public static void EndBattleWithResults(this BattleController battle, bool victory)
    {
        BattleResultsData resultsData = RewardPolicy.Settle(battle, victory);

        Debug.Log($"[BattleController] Battle ended. Victory: {victory}, EXP: {resultsData.expGained}, Cert: {resultsData.jpGained}, scrip: {resultsData.goldGained}");

        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.NotifyBattleEnded(resultsData);
        }
        else
        {
            Debug.LogError("[BattleController] No GameFlowController found! Cannot transition to PostBattle.");

            // Fallback: Load main menu
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
}
