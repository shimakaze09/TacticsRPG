/// <summary>
/// Composable gear behaviors beyond flat stats — any item can carry any mix
/// (GDD §3.3 gear behavior model). Weapon on-hit traits fire from the basic
/// attack; defensive traits shape damage the wearer takes. Element-tagged
/// traits are data-ready but their damage hook lands with elements (1.10).
/// </summary>
public enum GearTraitType
{
    /// <summary>Weapon: attacker takes value% of each hit it deals.</summary>
    Recoil,

    /// <summary>Weapon: swinging Throttles the attacker for value turns.</summary>
    WindedAfterStrike,

    /// <summary>Armor: incoming physical damage reduced by value%.</summary>
    PhysicalResist,

    /// <summary>Armor: incoming physical damage increased by value%.</summary>
    PhysicalWeakness,

    /// <summary>Armor: incoming damage of element `tag` reduced value% (1.10).</summary>
    ElementResist,

    /// <summary>Armor: incoming damage of element `tag` increased value% (1.10).</summary>
    ElementWeakness,

    /// <summary>Weapon: +value% damage when striking from behind.</summary>
    FlankBonus,

    /// <summary>Weapon: value% chance per hit to inflict status `tag` (duration turns).</summary>
    StatusOnHit,

    /// <summary>Weapon: attacker heals value% of the damage it deals.</summary>
    Lifesteal
}
