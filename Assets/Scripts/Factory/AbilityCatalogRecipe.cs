using System;
using UnityEngine;

/// <summary>
/// Asset describing an ability catalog: named categories each listing ability
/// prefab names to instantiate.
/// </summary>
public class AbilityCatalogRecipe : ScriptableObject
{
    public Category[] categories;

    [Serializable]
    public class Category
    {
        public string[] entries;
        public string name;
    }
}