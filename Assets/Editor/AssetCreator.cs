using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AssetCreator
{
    [MenuItem("Assets/Create/Conversation Data")]
    public static void CreateConversationData()
    {
        ScriptableObjectUtility.CreateAsset<ConversationData>();
    }

    [MenuItem("Assets/Create/Unit Recipe")]
    public static void CreateUnitRecipe()
    {
        ScriptableObjectUtility.CreateAsset<UnitRecipe>();
    }

    [MenuItem("Assets/Create/Ability Catalog Recipe")]
    public static void CreateAbilityCatalogRecipe()
    {
        ScriptableObjectUtility.CreateAsset<AbilityCatalogRecipe>();
    }
}