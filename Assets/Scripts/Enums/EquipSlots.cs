using System;

/// <summary>
/// Equipment slot flags (primary/secondary/head/body/accessory); flags support
/// dual-wield style items.
/// </summary>
[Flags]
public enum EquipSlots
{
    None = 0,
    Primary = 1 << 0, // usually a weapon (sword etc)
    Secondary = 1 << 1, // usually a shield, but could be another sword (dual-wield) or occupied by two-handed weapon
    Head = 1 << 2, // helmet, hat, etc
    Body = 1 << 3, // body armor, robe, etc
    Accessory = 1 << 4 // ring, belt, etc
}