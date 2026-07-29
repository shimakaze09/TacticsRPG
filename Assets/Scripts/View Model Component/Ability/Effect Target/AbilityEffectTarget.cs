using UnityEngine;

/// <summary>
/// Base filter deciding whether a tile is a legal recipient for one effect of
/// an ability.
/// </summary>
public abstract class AbilityEffectTarget : MonoBehaviour
{
    public abstract bool IsTarget(Tile tile);
}