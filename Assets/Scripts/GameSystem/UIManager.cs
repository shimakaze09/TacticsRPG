using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized UI management system
/// Manages all UI panels across different game contexts (Battle, PostBattle, Shop, etc.)
/// Singleton pattern for easy access from any controller
/// </summary>
public class UIManager : MonoBehaviour
{
    #region Singleton

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializePanels();
    }

    #endregion

    #region UI Panel References

    [Header("Battle UI")]
    [Tooltip("Ability selection menu during battles")]
    public AbilityMenuPanelController abilityMenuPanel;

    [Tooltip("Unit stat display panel")]
    public StatPanelController statPanel;

    [Tooltip("Battle messages (damage, status, etc.)")]
    public BattleMessageController battleMessageController;

    [Tooltip("Turn order display")]
    public TurnOrderController turnOrderController;

    [Header("Post-Battle/Menu UI")]
    [Tooltip("Job change menu (JobMenuPanelController)")]
    public JobMenuPanelController jobMenuPanel;

    [Tooltip("Battle results screen (EXP, JP, items, level ups)")]
    public GameObject battleResultsPanel;

    [Tooltip("Party management menu")]
    public GameObject partyMenuPanel;

    [Tooltip("Equipment change menu")]
    public GameObject equipmentMenuPanel;

    [Tooltip("Inventory menu")]
    public GameObject inventoryPanel;

    [Header("Shop UI")]
    [Tooltip("Shop buy/sell interface")]
    public GameObject shopPanel;

    [Tooltip("Equipment comparison panel for shops")]
    public GameObject equipmentComparisonPanel;

    [Header("Global UI")]
    [Tooltip("Pause menu (can be shown anytime)")]
    public GameObject pauseMenuPanel;

    [Tooltip("Settings menu")]
    public GameObject settingsPanel;

    [Tooltip("Dialog/conversation panel")]
    public DialogController dialogPanel;

    [Header("UI Containers")]
    [Tooltip("Parent container for Battle UI")]
    public CanvasGroup battleUIContainer;

    [Tooltip("Parent container for Post-Battle UI")]
    public CanvasGroup postBattleUIContainer;

    [Tooltip("Parent container for Shop UI")]
    public CanvasGroup shopUIContainer;

    [Tooltip("Parent container for Global UI")]
    public CanvasGroup globalUIContainer;

    #endregion

    #region State

    /// <summary>
    /// Current UI context (what UI is visible)
    /// </summary>
    public UIContext CurrentContext { get; private set; } = UIContext.None;

    /// <summary>
    /// Currently active menu type
    /// </summary>
    public MenuType CurrentMenu { get; private set; } = MenuType.None;

    /// <summary>
    /// Panel registry for quick lookup
    /// </summary>
    private Dictionary<Type, MonoBehaviour> panelRegistry = new Dictionary<Type, MonoBehaviour>();

    /// <summary>
    /// All active panels (for managing layers)
    /// </summary>
    private List<GameObject> activePanels = new List<GameObject>();

    #endregion

    #region Events

    /// <summary>
    /// Fired when UI context changes
    /// </summary>
    public event Action<UIContext, UIContext> OnContextChanged;

    /// <summary>
    /// Fired when menu is shown/hidden
    /// </summary>
    public event Action<MenuType> OnMenuChanged;

    #endregion

    #region Initialization

    private void InitializePanels()
    {
        // Register all panels for type-based lookup
        RegisterPanel(abilityMenuPanel);
        RegisterPanel(statPanel);
        RegisterPanel(battleMessageController);
        RegisterPanel(turnOrderController);
        RegisterPanel(jobMenuPanel);
        RegisterPanel(dialogPanel);

        // Hide all UI initially
        HideAllUI();
    }

    private void RegisterPanel<T>(T panel) where T : MonoBehaviour
    {
        if (panel != null)
        {
            panelRegistry[typeof(T)] = panel;
        }
    }

    #endregion

    #region Context Management

    /// <summary>
    /// Change UI context (Battle, PostBattle, Shop, etc.)
    /// </summary>
    public void SetContext(UIContext newContext)
    {
        if (CurrentContext == newContext)
            return;

        var oldContext = CurrentContext;
        CurrentContext = newContext;

        // Hide all context-specific UI
        HideContextUI(oldContext);

        // Show new context UI
        ShowContextUI(newContext);

        OnContextChanged?.Invoke(oldContext, newContext);
        Debug.Log($"[UIManager] Context changed: {oldContext} → {newContext}");
    }

    private void ShowContextUI(UIContext context)
    {
        switch (context)
        {
            case UIContext.Battle:
                ShowBattleUI();
                break;
            case UIContext.PostBattle:
                ShowPostBattleUI();
                break;
            case UIContext.Shop:
                ShowShopUI();
                break;
            case UIContext.Global:
                // Global UI shown on demand
                break;
            case UIContext.None:
                HideAllUI();
                break;
        }
    }

    private void HideContextUI(UIContext context)
    {
        switch (context)
        {
            case UIContext.Battle:
                HideBattleUI();
                break;
            case UIContext.PostBattle:
                HidePostBattleUI();
                break;
            case UIContext.Shop:
                HideShopUI();
                break;
        }
    }

    #endregion

    #region Battle UI

    /// <summary>
    /// Show all Battle UI elements
    /// </summary>
    public void ShowBattleUI()
    {
        SetCanvasGroupState(battleUIContainer, true);
        Debug.Log("[UIManager] Battle UI shown");
    }

    /// <summary>
    /// Hide all Battle UI elements
    /// </summary>
    public void HideBattleUI()
    {
        SetCanvasGroupState(battleUIContainer, false);

        // Also hide individual panels
        if (abilityMenuPanel != null && abilityMenuPanel.gameObject.activeSelf)
        {
            abilityMenuPanel.gameObject.SetActive(false);
        }

        Debug.Log("[UIManager] Battle UI hidden");
    }

    #endregion

    #region Post-Battle UI

    /// <summary>
    /// Show Post-Battle UI (results, menus)
    /// </summary>
    public void ShowPostBattleUI()
    {
        SetCanvasGroupState(postBattleUIContainer, true);
        Debug.Log("[UIManager] Post-Battle UI shown");
    }

    /// <summary>
    /// Hide Post-Battle UI
    /// </summary>
    public void HidePostBattleUI()
    {
        SetCanvasGroupState(postBattleUIContainer, false);
        HideMenu();
        Debug.Log("[UIManager] Post-Battle UI hidden");
    }

    #endregion

    #region Shop UI

    /// <summary>
    /// Show Shop UI
    /// </summary>
    public void ShowShopUI()
    {
        SetCanvasGroupState(shopUIContainer, true);
        ShowPanel(shopPanel);
        Debug.Log("[UIManager] Shop UI shown");
    }

    /// <summary>
    /// Hide Shop UI
    /// </summary>
    public void HideShopUI()
    {
        SetCanvasGroupState(shopUIContainer, false);
        HidePanel(shopPanel);
        Debug.Log("[UIManager] Shop UI hidden");
    }

    #endregion

    #region Menu Management

    /// <summary>
    /// Show specific menu panel
    /// </summary>
    public void ShowMenu(MenuType menuType)
    {
        if (CurrentMenu == menuType)
            return;

        // Hide current menu if any
        HideMenu();

        CurrentMenu = menuType;

        // Show requested menu
        switch (menuType)
        {
            case MenuType.JobChange:
                // JobMenuPanel is shown via JobMenuPanelController.Show()
                // This just marks it as active
                break;
            case MenuType.BattleResults:
                ShowPanel(battleResultsPanel);
                break;
            case MenuType.Party:
                ShowPanel(partyMenuPanel);
                break;
            case MenuType.Equipment:
                ShowPanel(equipmentMenuPanel);
                break;
            case MenuType.Inventory:
                ShowPanel(inventoryPanel);
                break;
            case MenuType.None:
                // No menu
                break;
        }

        OnMenuChanged?.Invoke(menuType);
        Debug.Log($"[UIManager] Menu shown: {menuType}");
    }

    /// <summary>
    /// Hide current menu
    /// </summary>
    public void HideMenu()
    {
        if (CurrentMenu == MenuType.None)
            return;

        switch (CurrentMenu)
        {
            case MenuType.JobChange:
                // JobMenuPanel handles its own hiding
                break;
            case MenuType.BattleResults:
                HidePanel(battleResultsPanel);
                break;
            case MenuType.Party:
                HidePanel(partyMenuPanel);
                break;
            case MenuType.Equipment:
                HidePanel(equipmentMenuPanel);
                break;
            case MenuType.Inventory:
                HidePanel(inventoryPanel);
                break;
        }

        CurrentMenu = MenuType.None;
        OnMenuChanged?.Invoke(MenuType.None);
    }

    #endregion

    #region Global UI

    /// <summary>
    /// Show pause menu
    /// </summary>
    public void ShowPauseMenu()
    {
        ShowPanel(pauseMenuPanel);
    }

    /// <summary>
    /// Hide pause menu
    /// </summary>
    public void HidePauseMenu()
    {
        HidePanel(pauseMenuPanel);
    }

    /// <summary>
    /// Show settings menu
    /// </summary>
    public void ShowSettings()
    {
        ShowPanel(settingsPanel);
    }

    /// <summary>
    /// Hide settings menu
    /// </summary>
    public void HideSettings()
    {
        HidePanel(settingsPanel);
    }

    #endregion

    #region Panel Access

    /// <summary>
    /// Get panel by type
    /// </summary>
    public T GetPanel<T>() where T : MonoBehaviour
    {
        if (panelRegistry.TryGetValue(typeof(T), out var panel))
        {
            return panel as T;
        }

        Debug.LogWarning($"[UIManager] Panel of type {typeof(T).Name} not found in registry");
        return null;
    }

    /// <summary>
    /// Check if panel is registered
    /// </summary>
    public bool HasPanel<T>() where T : MonoBehaviour
    {
        return panelRegistry.ContainsKey(typeof(T));
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Show a panel GameObject
    /// </summary>
    public void ShowPanel(GameObject panel)
    {
        if (panel == null)
            return;

        panel.SetActive(true);

        if (!activePanels.Contains(panel))
        {
            activePanels.Add(panel);
        }
    }

    /// <summary>
    /// Hide a panel GameObject
    /// </summary>
    public void HidePanel(GameObject panel)
    {
        if (panel == null)
            return;

        panel.SetActive(false);
        activePanels.Remove(panel);
    }

    /// <summary>
    /// Set CanvasGroup state (visible/invisible, interactable)
    /// </summary>
    private void SetCanvasGroupState(CanvasGroup canvasGroup, bool visible)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    /// <summary>
    /// Hide all UI
    /// </summary>
    public void HideAllUI()
    {
        HideBattleUI();
        HidePostBattleUI();
        HideShopUI();
        HideMenu();
    }

    #endregion

    #region Debug

    /// <summary>
    /// Get current UI state for debugging
    /// </summary>
    public string GetDebugInfo()
    {
        return $"Context: {CurrentContext}, Menu: {CurrentMenu}, Active Panels: {activePanels.Count}";
    }

    #endregion
}
