/// <summary>
/// Types of menu UI panels that can be shown
/// Used by UIManager to display the correct panel
/// </summary>
public enum MenuType
{
    /// <summary>
    /// No menu active
    /// </summary>
    None,

    /// <summary>
    /// Job change menu (JobMenuPanelController)
    /// Shows available jobs, current job, JP progress
    /// </summary>
    JobChange,

    /// <summary>
    /// Party management menu
    /// Shows all party members, stats, formation
    /// </summary>
    Party,

    /// <summary>
    /// Equipment menu
    /// Change weapons, armor, accessories
    /// </summary>
    Equipment,

    /// <summary>
    /// Ability menu (setting support abilities, reactions, movement)
    /// Different from battle ability menu
    /// </summary>
    Abilities,

    /// <summary>
    /// Inventory menu
    /// View and use items
    /// </summary>
    Inventory,

    /// <summary>
    /// Save/Load game menu
    /// </summary>
    SaveLoad,

    /// <summary>
    /// Post-battle results screen
    /// Shows EXP, JP, items gained, level ups
    /// </summary>
    BattleResults
}
