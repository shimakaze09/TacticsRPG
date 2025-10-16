/// <summary>
/// UI context indicates what type of UI should be visible
/// Used by UIManager to show/hide UI categories
/// </summary>
public enum UIContext
{
    /// <summary>
    /// No specific context (hide all UI)
    /// </summary>
    None,

    /// <summary>
    /// Battle UI (ability menu, stat panel, battle messages, turn order)
    /// Shown during tactical battles
    /// </summary>
    Battle,

    /// <summary>
    /// Post-battle UI (results, rewards, menus)
    /// Shown after battle victory/defeat
    /// </summary>
    PostBattle,

    /// <summary>
    /// Shop UI (shop panel, inventory, equipment comparison)
    /// Shown when buying/selling items
    /// </summary>
    Shop,

    /// <summary>
    /// World UI (world map, party status, mini map)
    /// Shown during world exploration
    /// </summary>
    World,

    /// <summary>
    /// Global UI (pause menu, settings, dialog)
    /// Can be shown in any context
    /// </summary>
    Global
}
