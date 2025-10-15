using UnityEngine;

/// <summary>
/// Event published when an item is equipped
/// </summary>
public class ItemEquippedEvent
{
    public GameObject Item { get; }

    public ItemEquippedEvent(GameObject item)
    {
        Item = item;
    }
}

/// <summary>
/// Event published when an item is unequipped
/// </summary>
public class ItemUnequippedEvent
{
    public GameObject Item { get; }

    public ItemUnequippedEvent(GameObject item)
    {
        Item = item;
    }
}
