using System.Collections.Generic;

public static class ElementRelationship
{
    public static readonly Dictionary<ElementTypes, (ElementTypes advantaged, ElementTypes restrained)>
        elementRestriction =
            new()
            {
                { ElementTypes.Earth, (ElementTypes.Lightning, ElementTypes.Air) },
                { ElementTypes.Water, (ElementTypes.Fire, ElementTypes.Lightning) },
                { ElementTypes.Air, (ElementTypes.Earth, ElementTypes.Ice) },
                { ElementTypes.Fire, (ElementTypes.Ice, ElementTypes.Water) },
                { ElementTypes.Lightning, (ElementTypes.Water, ElementTypes.Earth) },
                { ElementTypes.Ice, (ElementTypes.Air, ElementTypes.Fire) },
                { ElementTypes.Holy, (ElementTypes.Chaos, ElementTypes.Chaos) },
                { ElementTypes.Chaos, (ElementTypes.Holy, ElementTypes.Holy) }
            };
}