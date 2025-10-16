/// <summary>
/// Game state enum for high-level game mode management
/// Determines which controller is active and what UI is shown
/// </summary>
public enum GameState
{
    /// <summary>
    /// Main menu (title screen, new game, load game)
    /// </summary>
    MainMenu,

    /// <summary>
    /// Tactical battle in progress
    /// BattleController active, Battle UI visible
    /// </summary>
    Battle,

    /// <summary>
    /// Post-battle results, rewards, job changes, shop
    /// PostBattleController active, Menu UI + Shop UI accessible
    /// </summary>
    PostBattle,

    /// <summary>
    /// Shopping for equipment/items
    /// ShopController active, Shop UI visible
    /// </summary>
    Shop,

    /// <summary>
    /// World map exploration (not yet implemented)
    /// WorldController active, World UI visible
    /// </summary>
    World,

    /// <summary>
    /// Story cutscenes and conversations
    /// ConversationController active, Dialog UI visible
    /// </summary>
    Cutscene
}
