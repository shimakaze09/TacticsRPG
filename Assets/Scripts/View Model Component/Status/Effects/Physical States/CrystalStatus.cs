using UnityEngine;

/// <summary>
/// Crystal: Unit has been KO'd for 3 turns and crystallized.
/// Allows other units to step on tile to restore HP/MP or learn abilities.
/// Unit is permanently dead and cannot be targeted.
/// </summary>
public class CrystalStatus : StatusEffect
{
    [Tooltip("Duration before crystal disappears (turns)")]
    public int crystalDuration = 3;

    private Unit owner;
    private int turnsRemaining;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        turnsRemaining = crystalDuration;
    }

    private void OnDisable()
    {
        // Crystal disappears
    }

    // Called when a unit steps on the crystal tile
    public void OnUnitStepOn(Unit unit)
    {
        if (unit == null)
            return;

        var stats = unit.GetComponent<Stats>();
        if (stats != null)
        {
            // Restore full HP and MP
            stats.SetValue(StatTypes.HP, stats[StatTypes.MHP], false);
            stats.SetValue(StatTypes.MP, stats[StatTypes.MMP], false);
        }

        // Remove crystal after collection
        if (owner != null)
            Destroy(owner.gameObject);
    }

    public bool IsCrystal()
    {
        return true;
    }
}
