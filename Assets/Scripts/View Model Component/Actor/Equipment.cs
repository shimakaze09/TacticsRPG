using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Equipment : MonoBehaviour
{
    #region Notifications

    public const string EquippedNotification = "Equipment.EquippedNotification";
    public const string UnEquippedNotification = "Equipment.UnEquippedNotification";

    #endregion

    #region Fields / Properties

    public IList<Equippable> items => _items.AsReadOnly();
    private List<Equippable> _items = new();

    #endregion

    #region Public

    public void Equip(Equippable item, EquipSlots slots)
    {
        UnEquip(slots);

        _items.Add(item);
        item.transform.SetParent(transform);
        item.slots = slots;
        item.OnEquip();

        this.PostNotification(EquippedNotification, item);
    }

    public void UnEquip(Equippable item)
    {
        item.OnUnEquip();
        item.slots = EquipSlots.None;
        item.transform.SetParent(transform);
        _items.Remove(item);

        this.PostNotification(UnEquippedNotification, item);
    }

    public void UnEquip(EquipSlots slots)
    {
        for (var i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            if ((item.slots & slots) != EquipSlots.None)
                UnEquip(item);
        }
    }

    public Equippable GetItem(EquipSlots slots)
    {
        return _items.FirstOrDefault(item => (item.slots & slots) != EquipSlots.None);
    }

    #endregion
}