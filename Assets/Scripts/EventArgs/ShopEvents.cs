/// <summary>
/// Event published when the player asks to buy an item (e.g. a shop cell's
/// buy button). The shop validates gold and either completes the purchase
/// (publishing ItemPurchasedEvent) or shows the insufficient-funds dialog.
/// </summary>
public class ItemPurchaseRequestedEvent
{
    public Item Item { get; }

    public ItemPurchaseRequestedEvent(Item item)
    {
        Item = item;
    }
}

/// <summary>
/// Event published after an item purchase has completed successfully.
/// Must never be republished by its own subscribers.
/// </summary>
public class ItemPurchasedEvent
{
    public Item Item { get; }

    public ItemPurchasedEvent(Item item)
    {
        Item = item;
    }
}
