using UnityEngine;

/// <summary>
/// Marks an item as consumable and applies its features once when used.
/// </summary>
public class Consumable : MonoBehaviour
{
    public void Consume(GameObject target)
    {
        var features = GetComponentsInChildren<Feature>();
        foreach (var feature in features)
            feature.Apply(target);
    }
}