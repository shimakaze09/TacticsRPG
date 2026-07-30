using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fills the shop with GearCatalog stock (tier 1 for the slice); icons cycle
/// through the serialized sprite set until gear gets real art.
/// </summary>
public class ItemGenerator : MonoBehaviour
{
    [SerializeField] private Sprite[] icons;

    private void Start()
    {
        var items = new List<Item>();
        var iconIndex = 0;
        foreach (var gear in GearCatalog.All)
        {
            if (gear.tier != 1)
                continue;

            items.Add(new Item
            {
                gearId = gear.id,
                name = gear.name,
                attack = gear.amount1,
                level = gear.tier,
                price = gear.price,
                sprite = icons != null && icons.Length > 0 ? icons[iconIndex++ % icons.Length] : null
            });
        }

        GetComponent<ItemShop>().Load(items);
    }
}
