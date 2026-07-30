using UnityEngine;

/// <summary>
/// Swayed (charm): the unit fights for the other side — its alliance checks
/// invert (allies read as foes and vice versa) and the AI takes the wheel,
/// so the regular battle brain earnestly attacks the unit's own team and
/// helps its captors.
/// </summary>
public class SwayedStatus : StatusEffect
{
    private Alliance alliance;
    private Driver driver;
    private Unit owner;

    private void OnEnable()
    {
        alliance = GetComponentInParent<Alliance>();
        driver = GetComponentInParent<Driver>();
        owner = GetComponentInParent<Unit>();
        if (alliance != null)
            alliance.confused = true;

        ControlSeizure.Seize(driver);
    }

    private void OnDisable()
    {
        if (alliance != null)
            alliance.confused = false;

        ControlSeizure.Release(driver, owner, this);
    }
}
