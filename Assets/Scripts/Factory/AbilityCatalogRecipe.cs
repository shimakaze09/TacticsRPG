using System;
using UnityEngine;

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