using UnityEngine;
using System.Collections;

public class Consumable : MonoBehaviour
{
    public void Consume(GameObject target)
    {
        var features = GetComponentsInChildren<Feature>();
        for (var i = 0; i < features.Length; ++i)
            features[i].Apply(target);
    }
}