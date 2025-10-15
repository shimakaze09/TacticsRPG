using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    #region Fields / Properties

    public IList<Equippable> items => _items.AsReadOnly();
    private readonly List<Equippable> _items = new();

    #endregion

    #region Public

    public void Equip(Equippable item, EquipSlots slots)
    {
        UnEquip(slots);

        _items.Add(item);
        item.transform.SetParent(transform);
        item.slots = slots;
        item.OnEquip();

        this.Publish(new ItemEquippedEvent(item.gameObject));
    }

    public void UnEquip(Equippable item)
    {
        item.OnUnEquip();
        item.slots = EquipSlots.None;
        item.transform.SetParent(transform);
        _items.Remove(item);

        this.Publish(new ItemUnequippedEvent(item.gameObject));
    }

    public void UnEquip(EquipSlots slots)
    {
        for (var i = _items.Count - 1; i >= 0; i--)
        {
            var item = _items[i];
            if ((item.slots & slots) != EquipSlots.None)
                UnEquip(item);
        }
    }

    public Equippable GetItem(EquipSlots slots)
    {
        for (var i = _items.Count - 1; i >= 0; i--)
        {
            var item = _items[i];
            if ((item.slots & slots) != EquipSlots.None)
                return item;
        }

        return null;
    }

    #endregion
}