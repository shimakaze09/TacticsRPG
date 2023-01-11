using UnityEngine;

public class Alliance : MonoBehaviour
{
    public bool confused;
    public Alliances type;

    public bool IsMatch(Alliance other, Targets targets)
    {
        var isMatch = false;
        switch (targets)
        {
            case Targets.Self:
                isMatch = other == this;
                break;
            case Targets.Ally:
                isMatch = type == other.type;
                break;
            case Targets.Foe:
                isMatch = type != other.type && other.type != Alliances.Neutral;
                break;
        }

        return confused ? !isMatch : isMatch;
    }
}