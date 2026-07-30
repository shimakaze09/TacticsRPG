using System.Collections.Generic;

/// <summary>
/// One piece of gear: slot, up to two stat bonuses, shop price and tier.
/// Tier 0 = starting gear (not sold), 1 = chapter-start shop, 2 = mid-chapter
/// restock, 9 = legendary (relic hunts only, never sold).
/// </summary>
public class GearData
{
    public string id;
    public string name;
    public EquipSlots slot;
    public StatTypes stat1;
    public int amount1;
    public StatTypes stat2;
    public int amount2;
    public int price;
    public int tier;
}

/// <summary>
/// Every wearable item in the game and each job's starting loadout (GDD §3.3):
/// per-job weapon plus a role armor, flat stat bonuses via StatModifierFeature.
/// Code-defined for the slice; migrates to JSON when the shop is rebuilt (M2).
/// </summary>
public static class GearCatalog
{
    private static Dictionary<string, GearData> byId;
    private static Dictionary<string, string[]> startingGearByJob;

    /// <summary>All catalog entries (build views like shop lists from this).</summary>
    public static IEnumerable<GearData> All
    {
        get
        {
            EnsureBuilt();
            return byId.Values;
        }
    }

    public static GearData Get(string id)
    {
        EnsureBuilt();
        return id != null && byId.TryGetValue(id, out var data) ? data : null;
    }

    /// <summary>Gear ids a fresh unit of this job spawns wearing.</summary>
    public static string[] StartingGear(string jobId)
    {
        EnsureBuilt();
        return jobId != null && startingGearByJob.TryGetValue(jobId, out var gear)
            ? gear
            : System.Array.Empty<string>();
    }

    private static void EnsureBuilt()
    {
        if (byId != null)
            return;

        byId = new Dictionary<string, GearData>();
        startingGearByJob = new Dictionary<string, string[]>();

        // Role armors shared across jobs
        Armor("roadcoat", "Roadcoat", StatTypes.DEF, 3, StatTypes.MDF, 1, 250);
        Armor("scrap_plate", "Scrap Plate", StatTypes.DEF, 5, StatTypes.MDF, 0, 350);
        Armor("warded_vestment", "Warded Vestment", StatTypes.DEF, 2, StatTypes.MDF, 4, 300);
        Armor("charter_harness", "Charter Harness", StatTypes.DEF, 4, StatTypes.MDF, 2, 400);

        // Per-job weapons — physical lines
        Weapon("trail_knife", "Trail Knife", StatTypes.ATK, 5, 200);
        Weapon("linebreaker_mace", "Line-Breaker Mace", StatTypes.ATK, 7, 400);
        Weapon("wrapped_knuckles", "Wrapped Knuckles", StatTypes.ATK, 7, 350);
        Weapon("slug_thrower", "Slug-Thrower", StatTypes.ATK, 7, 450);
        Weapon("pry_hook", "Pry Hook", StatTypes.ATK, 5, 200);
        Weapon("charter_standard", "Charter Standard", StatTypes.ATK, 6, 400);
        Weapon("jumpjet_lance", "Jump-Jet Lance", StatTypes.ATK, 7, 450);
        Weapon("static_knife", "Static Knife", StatTypes.ATK, 7, 400);
        Weapon("grief_edge", "Grief-Edge", StatTypes.ATK, 8, 550);
        Weapon("broken_oath_blade", "Broken-Oath Blade", StatTypes.ATK, 8, 550);
        Weapon("sanctified_edge", "Sanctified Edge", StatTypes.ATK, 9, 700);
        Weapon("absolution_point", "Absolution Point", StatTypes.ATK, 8, 600);

        // Per-job weapons — caster lines
        Weapon("focus_coil", "Focus-Coil", StatTypes.MAT, 7, 400);
        Weapon("field_wand", "Field-Kit Wand", StatTypes.MAT, 6, 300);
        Weapon("sawbones_lantern", "Sawbones' Lantern", StatTypes.MAT, 5, 250);
        Weapon("litany_censer", "Litany Censer", StatTypes.MAT, 7, 400);
        Weapon("escapement_rod", "Escapement Rod", StatTypes.MAT, 6, 350);
        Weapon("speaker_totem", "Speaker Totem", StatTypes.MAT, 7, 400);
        Weapon("wirestring_guitar", "Wire-String Guitar", StatTypes.MAT, 5, 250);
        Weapon("ledger_seal", "Ledger Seal", StatTypes.MAT, 5, 250);
        Weapon("engine_key", "Engine Key", StatTypes.MAT, 8, 550);
        Weapon("dowsing_staff", "Dowsing Staff", StatTypes.MAT, 6, 300);

        // Hybrid
        Add(new GearData
        {
            id = "cipher_rod", name = "Cipher Rod", slot = EquipSlots.Primary,
            stat1 = StatTypes.ATK, amount1 = 4, stat2 = StatTypes.MAT, amount2 = 4,
            price = 450, tier = 1
        });

        // Starting loadouts (weapon, body) per job id
        Loadout("drifter", "trail_knife", "roadcoat");
        Loadout("warden", "linebreaker_mace", "scrap_plate");
        Loadout("brawler", "wrapped_knuckles", "roadcoat");
        Loadout("marksman", "slug_thrower", "roadcoat");
        Loadout("scav", "pry_hook", "roadcoat");
        Loadout("bannerlord", "charter_standard", "scrap_plate");
        Loadout("skybreaker", "jumpjet_lance", "scrap_plate");
        Loadout("wraith", "static_knife", "roadcoat");
        Loadout("hollowed", "grief_edge", "scrap_plate");
        Loadout("oathbreaker", "broken_oath_blade", "scrap_plate");
        Loadout("relic_blade", "sanctified_edge", "scrap_plate");
        Loadout("knife_of_the_church", "absolution_point", "roadcoat");
        Loadout("cipherguard", "cipher_rod", "charter_harness");
        Loadout("wastewalker", "dowsing_staff", "charter_harness");
        Loadout("burner", "focus_coil", "warded_vestment");
        Loadout("mender", "field_wand", "warded_vestment");
        Loadout("sawbones", "sawbones_lantern", "roadcoat");
        Loadout("liturgist", "litany_censer", "warded_vestment");
        Loadout("clockhand", "escapement_rod", "warded_vestment");
        Loadout("ghostspeaker", "speaker_totem", "warded_vestment");
        Loadout("balladeer", "wirestring_guitar", "warded_vestment");
        Loadout("broker", "ledger_seal", "warded_vestment");
        Loadout("wakener", "engine_key", "warded_vestment");
    }

    private static void Weapon(string id, string name, StatTypes stat, int amount, int price)
    {
        Add(new GearData
        {
            id = id, name = name, slot = EquipSlots.Primary,
            stat1 = stat, amount1 = amount, price = price, tier = 1
        });
    }

    private static void Armor(string id, string name, StatTypes stat1, int amount1, StatTypes stat2, int amount2,
        int price)
    {
        Add(new GearData
        {
            id = id, name = name, slot = EquipSlots.Body,
            stat1 = stat1, amount1 = amount1, stat2 = stat2, amount2 = amount2,
            price = price, tier = 1
        });
    }

    private static void Add(GearData data)
    {
        byId.Add(data.id, data);
    }

    private static void Loadout(string jobId, string weaponId, string bodyId)
    {
        startingGearByJob.Add(jobId, new[] { weaponId, bodyId });
    }
}
