/// <summary>
/// Enumeration of all global game flow states in the FFT-style tactics game.
/// Used by GameFlowController to manage high-level transitions between different game modes.
/// </summary>
public enum GameFlowState
{
    /// <summary>
    /// No state active (initialization only)
    /// </summary>
    None = 0,

    /// <summary>
    /// Title screen - New Game, Load Game, Options
    /// </summary>
    Title = 1,

    /// <summary>
    /// World/Overworld - Player can navigate, access menus, start battles, visit shops
    /// </summary>
    World = 2,

    /// <summary>
    /// Active tactical battle - BattleController is managing the combat
    /// </summary>
    Battle = 3,

    /// <summary>
    /// Post-battle results - EXP/JP distribution, rewards, level ups
    /// </summary>
    PostBattle = 4,

    /// <summary>
    /// Shop interface - Buy/sell equipment and items
    /// </summary>
    Shop = 5
}
