using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The party's owned-but-unequipped gear, as GearCatalog ids (duplicates
/// allowed). Shop purchases land here. Persists via PlayerPrefs alongside
/// Bank for now — both migrate into GameData together (BATTLE_PLAN 1.12).
/// </summary>
public class PartyInventory
{
    private const string ItemsKey = "PartyInventory.items";
    private const char Separator = ';';

    private static PartyInventory _instance;
    public static PartyInventory Instance => _instance ??= new PartyInventory();

    private readonly List<string> _items;
    public IList<string> Items => _items.AsReadOnly();

    private PartyInventory()
    {
        _items = new List<string>();
        var saved = PlayerPrefs.GetString(ItemsKey, string.Empty);
        foreach (var id in saved.Split(Separator))
            if (!string.IsNullOrEmpty(id))
                _items.Add(id);
    }

    public void Add(string gearId)
    {
        if (GearCatalog.Get(gearId) == null)
        {
            Debug.LogError($"[PartyInventory] Refusing unknown gear id '{gearId}'");
            return;
        }

        _items.Add(gearId);
        Save();
    }

    public bool Remove(string gearId)
    {
        var removed = _items.Remove(gearId);
        if (removed)
            Save();
        return removed;
    }

    public void Clear()
    {
        _items.Clear();
        Save();
    }

    private void Save()
    {
        PlayerPrefs.SetString(ItemsKey, string.Join(Separator.ToString(), _items));
        PlayerPrefs.Save();
    }
}
