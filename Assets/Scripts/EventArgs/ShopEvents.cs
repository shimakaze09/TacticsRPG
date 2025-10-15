/// <summary>
/// Event published when an item is purchased
/// </summary>
public class ItemPurchasedEvent
{
    public Item Item { get; }

    public ItemPurchasedEvent(Item item)
    {
        Item = item;
    }
}
