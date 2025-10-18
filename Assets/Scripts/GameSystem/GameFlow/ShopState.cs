using UnityEngine;

/// <summary>
/// Shop State - Handles the shop interface for buying and selling items/equipment.
/// 
/// RESPONSIBILITIES:
/// - Display shop UI
/// - Handle buy transactions
/// - Handle sell transactions
/// - Update player inventory and gold
/// - Show equipment comparisons
/// - Return to World state when done
/// 
/// INTEGRATION:
/// - Uses UIManager to display shop panels
/// - Works with inventory system
/// - Works with equipment system
/// </summary>
public class ShopState : BaseGameFlowState
{
    #region Properties

    public override GameFlowState StateType => GameFlowState.Shop;

    // Shop typically doesn't need a separate scene
    // It can overlay on top of the world scene
    protected override string SceneName => null;

    /// <summary>
    /// Current shop inventory (items available for purchase)
    /// </summary>
    private ShopInventory currentShop;

    #endregion

    #region State Lifecycle

    protected override void OnSceneReady()
    {
        Debug.Log("[ShopState] Shop state ready");

        // Load shop inventory
        LoadShopInventory();

        // Show shop UI
        ShowShopUI();

        // Subscribe to shop events
        SubscribeToShopEvents();
    }

    protected override void OnStateExit()
    {
        Debug.Log("[ShopState] Exiting shop state");

        // Unsubscribe from shop events
        UnsubscribeFromShopEvents();

        // Hide shop UI
        HideShopUI();

        // Clear shop data
        currentShop = null;
    }

    #endregion

    #region Shop Inventory

    /// <summary>
    /// Load the shop's inventory
    /// </summary>
    private void LoadShopInventory()
    {
        Debug.Log("[ShopState] Loading shop inventory");

        // TODO: Load shop inventory based on location/shop ID
        // For now, create a placeholder
        currentShop = new ShopInventory
        {
            shopName = "General Store", // Placeholder
            // Add items, equipment, etc.
        };

        Debug.Log($"[ShopState] Loaded shop: {currentShop.shopName}");
    }

    #endregion

    #region UI Management

    /// <summary>
    /// Show the shop UI
    /// </summary>
    private void ShowShopUI()
    {
        if (Controller.UIManager != null)
        {
            // TODO: Show shop panel
            // Controller.UIManager.ShowPanel<ShopPanel>();
            // Controller.UIManager.SetContext(UIContext.Shop);
            // shopPanel.DisplayShop(currentShop);
            Debug.Log("[ShopState] Shop UI shown");
        }
        else
        {
            Debug.LogWarning("[ShopState] UIManager not found");
        }
    }

    /// <summary>
    /// Hide the shop UI
    /// </summary>
    private void HideShopUI()
    {
        if (Controller.UIManager != null)
        {
            // TODO: Hide shop panel
            // Controller.UIManager.HidePanel<ShopPanel>();
            Debug.Log("[ShopState] Shop UI hidden");
        }
    }

    #endregion

    #region Event Handling

    /// <summary>
    /// Subscribe to shop UI events
    /// </summary>
    private void SubscribeToShopEvents()
    {
        // TODO: Subscribe to shop panel events
        // Example:
        // shopPanel.OnItemBought += HandleItemBought;
        // shopPanel.OnItemSold += HandleItemSold;
        // shopPanel.OnExitShop += HandleExitShop;
    }

    /// <summary>
    /// Unsubscribe from shop events
    /// </summary>
    private void UnsubscribeFromShopEvents()
    {
        // TODO: Unsubscribe from shop panel events
    }

    #endregion

    #region Shop Transactions

    /// <summary>
    /// Handle item purchase
    /// </summary>
    public void BuyItem(string itemId, int quantity, int cost)
    {
        Debug.Log($"[ShopState] Buying {quantity}x {itemId} for {cost} gold");

        // TODO: Validate player has enough gold
        // TODO: Validate shop has item in stock
        // TODO: Add item to player inventory
        // TODO: Deduct gold from player
        // TODO: Update shop UI

        // Placeholder logic:
        bool canAfford = true; // Check player gold
        if (canAfford)
        {
            Debug.Log($"[ShopState] Purchase successful: {itemId}");
            // Add to inventory
            // Deduct gold
        }
        else
        {
            Debug.Log("[ShopState] Not enough gold!");
            // Show error message
        }
    }

    /// <summary>
    /// Handle item sale
    /// </summary>
    public void SellItem(string itemId, int quantity, int value)
    {
        Debug.Log($"[ShopState] Selling {quantity}x {itemId} for {value} gold");

        // TODO: Validate player has item in inventory
        // TODO: Remove item from inventory
        // TODO: Add gold to player
        // TODO: Update shop UI

        // Placeholder logic:
        bool hasItem = true; // Check inventory
        if (hasItem)
        {
            Debug.Log($"[ShopState] Sale successful: {itemId}");
            // Remove from inventory
            // Add gold
        }
        else
        {
            Debug.Log("[ShopState] Item not in inventory!");
            // Show error message
        }
    }

    /// <summary>
    /// Buy equipment and auto-equip (optional feature)
    /// </summary>
    public void BuyAndEquipItem(string equipmentId, int cost)
    {
        Debug.Log($"[ShopState] Buying and equipping {equipmentId} for {cost} gold");

        // TODO: Buy the item
        // TODO: Auto-equip to selected unit
        // TODO: Return old equipment to inventory
    }

    #endregion

    #region Exit Shop

    /// <summary>
    /// Exit the shop and return to world
    /// </summary>
    public void ExitShop()
    {
        Debug.Log("[ShopState] Exiting shop");
        Controller.EnterWorld();
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// Data structure for shop inventory
    /// </summary>
    private class ShopInventory
    {
        public string shopName;
        // Add collections for:
        // - Available items
        // - Available equipment
        // - Buy/sell price modifiers
    }

    #endregion

    #region Debug/Testing

    /// <summary>
    /// For testing: Exit shop immediately
    /// </summary>
    [ContextMenu("Debug: Exit Shop")]
    private void DebugExitShop()
    {
        ExitShop();
    }

    /// <summary>
    /// For testing: Buy a test item
    /// </summary>
    [ContextMenu("Debug: Buy Test Item")]
    private void DebugBuyTestItem()
    {
        BuyItem("TestItem", 1, 100);
    }

    #endregion
}
