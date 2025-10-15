using UnityEngine;

/// <summary>
/// Treasure: Unit has been KO'd for 3 turns and became a treasure chest.
/// Grants item/weapon/equipment when another unit steps on the tile.
/// Unit is permanently dead and cannot be targeted.
/// </summary>
public class TreasureStatus : StatusEffect
{
    [Tooltip("Duration before treasure disappears (turns)")]
    public int treasureDuration = 5;

    private Unit owner;
    private int turnsRemaining;

    [Tooltip("Type of treasure to drop (item/weapon/equipment)")]
    public string treasureType = "item";

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        turnsRemaining = treasureDuration;
    }

    private void OnDisable()
    {
        // Treasure disappears or was collected
    }

    // Called when a unit steps on the treasure tile
    public void OnUnitStepOn(Unit unit)
    {
        if (unit == null)
            return;

        // Grant random item/equipment to the unit
        Debug.Log($"Unit {unit.name} collected treasure!");

        // Remove treasure after collection
        if (owner != null)
            Destroy(owner.gameObject);
    }

    public bool IsTreasure()
    {
        return true;
    }
}
