using UnityEngine;

/// <summary>
/// Immobilize: Unit cannot move. At end of AT, CT is decremented as if unit had moved.
/// Lasts for 24 ticks (~2-3 turns).
/// </summary>
public class ImmobilizeStatus : StatusEffect
{
    private Unit owner;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();

        if (owner)
        {
            // Prevent movement
            this.Subscribe<MovementCanMoveCheckEvent>(OnCanMoveCheck);
        }
    }

    private void OnDisable()
    {
        this.Unsubscribe<MovementCanMoveCheckEvent>(OnCanMoveCheck);
    }

    private void OnCanMoveCheck(MovementCanMoveCheckEvent e)
    {
        var unit = e.Movement.GetComponentInParent<Unit>();
        if (owner == unit && e.Exception.defaultToggle)
            e.Exception.FlipToggle();
    }
}
