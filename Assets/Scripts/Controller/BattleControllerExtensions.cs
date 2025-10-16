using UnityEngine;

/// <summary>
/// Battle state extension methods for GameStateManager integration
/// Add these methods to BattleController to support post-battle flow
/// </summary>
public static class BattleControllerExtensions
{
    /// <summary>
    /// End battle and transition to post-battle state
    /// Call this instead of directly changing to EndBattleState
    /// </summary>
    public static void EndBattleWithResults(this BattleController battle, bool victory)
    {
        // Gather battle results data
        var resultsData = new BattleResultsData
        {
            victory = victory,
            expGained = CalculateExpGained(battle, victory),
            jpGained = CalculateJPGained(battle, victory),
            goldGained = CalculateGoldGained(battle, victory),
            itemsGained = GatherItemsGained(battle),
            playerUnits = GetPlayerUnits(battle)
        };

        Debug.Log($"[BattleController] Battle ended. Victory: {victory}, EXP: {resultsData.expGained}, JP: {resultsData.jpGained}");

        // Notify GameStateManager
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnBattleEnded(victory, resultsData);
        }
        else
        {
            Debug.LogError("[BattleController] GameStateManager not found! Cannot transition to PostBattle.");
            
            // Fallback: Load main menu
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    #region Reward Calculation

    private static int CalculateExpGained(BattleController battle, bool victory)
    {
        if (!victory)
            return 0;

        // Simple calculation: 100 EXP per enemy defeated
        // You can make this more sophisticated based on enemy levels, etc.
        int enemyCount = CountDefeatedEnemies(battle);
        return enemyCount * 100;
    }

    private static int CalculateJPGained(BattleController battle, bool victory)
    {
        if (!victory)
            return 0;

        // Simple calculation: 50 JP per enemy defeated
        // FFT gave JP based on various factors (abilities used, damage dealt, etc.)
        int enemyCount = CountDefeatedEnemies(battle);
        return enemyCount * 50;
    }

    private static int CalculateGoldGained(BattleController battle, bool victory)
    {
        if (!victory)
            return 0;

        // Simple calculation: 500 gold base + 100 per enemy
        int enemyCount = CountDefeatedEnemies(battle);
        return 500 + (enemyCount * 100);
    }

    private static string[] GatherItemsGained(BattleController battle)
    {
        // For now, return empty array
        // You can implement item drops based on enemies defeated, treasure chests, etc.
        return new string[0];
    }

    private static Unit[] GetPlayerUnits(BattleController battle)
    {
        // Get all units that belong to the player
        var playerUnits = new System.Collections.Generic.List<Unit>();
        
        foreach (var unit in battle.units)
        {
            if (unit != null && unit.GetComponent<Alliance>()?.type == Alliances.Hero)
            {
                playerUnits.Add(unit);
            }
        }
        
        return playerUnits.ToArray();
    }

    private static int CountDefeatedEnemies(BattleController battle)
    {
        // Count defeated enemy units
        int count = 0;
        
        // This is a simplified version - you might need to track defeated enemies differently
        // For now, assume all non-player units were defeated if battle was won
        foreach (var unit in battle.units)
        {
            if (unit != null)
            {
                var alliance = unit.GetComponent<Alliance>();
                if (alliance != null && alliance.type != Alliances.Hero)
                {
                    var health = unit.GetComponent<Health>();
                    if (health == null || health.HP <= 0)
                    {
                        count++;
                    }
                }
            }
        }
        
        return count > 0 ? count : 3; // Default to 3 if counting failed
    }

    #endregion
}
