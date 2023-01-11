using UnityEngine;

public class Consumable : MonoBehaviour
{
    public void Consume(GameObject target)
    {
        var features = GetComponentsInChildren<Feature>();
        foreach (var feature in features)
            feature.Apply(target);
    }
}