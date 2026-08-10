using UnityEngine;

/// <summary>
/// Which side this unit is on, with matching logic for targeting (self/ally/foe
/// checks, confusion flips). Confusion (Swayed) swaps who counts as ally and
/// foe rather than negating the whole relation — a blanket negation would
/// admit Neutral units into both sets and invert Self checks (issue #53).
/// </summary>
public class Alliance : MonoBehaviour
{
    public bool confused;
    public Alliances type;

    /// <summary>
    /// Whether `other` satisfies the requested relation from this unit's
    /// perspective. Neutral units are never allies or foes of anyone; Self is
    /// unaffected by confusion.
    /// </summary>
    public bool IsMatch(Alliance other, Targets targets)
    {
        if (other == null)
            return false;

        var effective = targets;
        if (confused && targets == Targets.Ally)
            effective = Targets.Foe;
        else if (confused && targets == Targets.Foe)
            effective = Targets.Ally;

        switch (effective)
        {
            case Targets.Self:
                return other == this;
            case Targets.Ally:
                // A unit is always its own ally (a Neutral medic may self-heal)
                if (other == this)
                    return true;
                return type == other.type && other.type != Alliances.Neutral;
            case Targets.Foe:
                return type != other.type && other.type != Alliances.Neutral;
            default:
                return false;
        }
    }
}
