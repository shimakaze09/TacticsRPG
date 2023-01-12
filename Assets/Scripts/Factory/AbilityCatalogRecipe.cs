using UnityEngine;
using System.Collections;

public class AbilityCatalogRecipe : ScriptableObject
{
    [System.Serializable]
    public class Category
    {
        public string name;
        public string[] entries;
    }

    public Category[] categories;
}