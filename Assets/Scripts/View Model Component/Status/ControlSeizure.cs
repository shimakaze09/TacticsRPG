using UnityEngine;

/// <summary>
/// Shared control-seizure logic for behavior statuses (Swayed, Scrambled,
/// Redline): while any of them is active the unit is driven by the AI via
/// Driver.special; control returns to the owner only when the LAST such
/// status ends. Statuses can't share a base class (different StatusEffect
/// branches), so they share this helper instead.
/// </summary>
public static class ControlSeizure
{
    /// <summary>
    /// Puts the AI in charge of the unit's turns. Statuses cache the driver
    /// and owner at enable time and hand them back to Release — the status
    /// object is already detached from the unit when its OnDisable runs
    /// (Status.Remove unparents before destroying), so parent lookups
    /// there find nothing.
    /// </summary>
    public static void Seize(Driver driver)
    {
        if (driver != null)
            driver.special = Drivers.Computer;
    }

    /// <summary>
    /// Ends this status's claim; the AI keeps control while another
    /// behavior status is still active on the unit.
    /// </summary>
    public static void Release(Driver driver, Unit owner, Component leaving)
    {
        if (driver == null)
            return;

        if (owner != null && HasOtherController(owner, leaving))
            return;

        driver.special = Drivers.None;
    }

    // Any other active behavior-control status on the unit?
    private static bool HasOtherController(Unit owner, Component leaving)
    {
        foreach (var candidate in owner.GetComponentsInChildren<StatusEffect>())
        {
            if (candidate == leaving)
                continue;
            if (candidate is SwayedStatus || candidate is ScrambledStatus || candidate is RedlineStatus)
                return true;
        }

        return false;
    }
}
