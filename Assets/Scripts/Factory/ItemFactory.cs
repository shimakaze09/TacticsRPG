using UnityEngine;

/// <summary>
/// Builds wearable item GameObjects from GearCatalog entries: an Equippable
/// plus its StatModifierFeatures, tagged with the gear id for save/compare.
/// </summary>
public static class ItemFactory
{
    /// <summary>Creates the item, or null for an unknown gear id.</summary>
    public static GameObject Create(string gearId)
    {
        var data = GearCatalog.Get(gearId);
        if (data == null)
        {
            Debug.LogError($"[ItemFactory] Unknown gear id '{gearId}'");
            return null;
        }

        var obj = new GameObject(data.name);
        obj.AddComponent<GearTag>().gearId = data.id;

        var equippable = obj.AddComponent<Equippable>();
        equippable.defaultSlots = data.slot;

        AddFeature(obj, data.stat1, data.amount1);
        if (data.amount2 != 0)
            AddFeature(obj, data.stat2, data.amount2);

        // Defensive traits activate with the item like any other feature;
        // weapon on-hit traits are run by the Attack ability instead
        if (HasDefensiveTrait(data))
            obj.AddComponent<GearDefenseFeature>();

        return obj;
    }

    private static bool HasDefensiveTrait(GearData data)
    {
        if (data.traits == null)
            return false;

        foreach (var trait in data.traits)
        {
            switch (trait.type)
            {
                case GearTraitType.PhysicalResist:
                case GearTraitType.PhysicalWeakness:
                case GearTraitType.ElementResist:
                case GearTraitType.ElementWeakness:
                    return true;
            }
        }

        return false;
    }

    private static void AddFeature(GameObject obj, StatTypes stat, int amount)
    {
        var feature = obj.AddComponent<StatModifierFeature>();
        feature.type = stat;
        feature.amount = amount;
    }
}
