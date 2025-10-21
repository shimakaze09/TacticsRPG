using UnityEngine;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Data-driven editor utility to create Ability Catalog Recipe assets for all FFT jobs
/// Reads catalog definitions from JSON files and generates Unity ScriptableObject assets
/// </summary>
public static class FFTAbilityCatalogCreator
{
    private const string CatalogRecipesPath = "Assets/Resources/Ability Catalog Recipes";
    private const string CatalogDataPath = "Assets/Resources/CatalogData";

    [MenuItem("Tactics RPG/Create FFT Catalogs/Generate from JSON")]
    public static void GenerateCatalogsFromJSON()
    {
        EnsureDirectoriesExist();
        DeleteExistingCatalogRecipes();
        
        // Get all JSON files in the CatalogData directory
        string[] jsonFiles = Directory.GetFiles(CatalogDataPath, "*.json", SearchOption.TopDirectoryOnly);
        
        int catalogsCreated = 0;
        foreach (string jsonFile in jsonFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(jsonFile);
            string relativePath = jsonFile.Replace(Application.dataPath, "Assets");
            
            try
            {
                CatalogDataFile catalogData = LoadJsonData<CatalogDataFile>(relativePath);
                CreateAbilityCatalogRecipe(catalogData);
                catalogsCreated++;
                Debug.Log($"Created catalog: {catalogData.catalogName} with {catalogData.categories[0].entries.Length} abilities");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create catalog from {fileName}: {e.Message}");
                Debug.LogError($"Attempted path: {relativePath}");
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Successfully created {catalogsCreated} ability catalog recipes from JSON files!");
    }

    private static void EnsureDirectoriesExist()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        if (!AssetDatabase.IsValidFolder(CatalogRecipesPath))
            AssetDatabase.CreateFolder("Assets/Resources", "Ability Catalog Recipes");
            
        if (!AssetDatabase.IsValidFolder(CatalogDataPath))
            AssetDatabase.CreateFolder("Assets/Resources", "CatalogData");
    }

    private static void DeleteExistingCatalogRecipes()
    {
        string[] catalogFiles = Directory.GetFiles(CatalogRecipesPath, "*.asset", SearchOption.TopDirectoryOnly);
        
        foreach (string file in catalogFiles)
        {
            string relativePath = "Assets" + file.Substring(Application.dataPath.Length);
            AssetDatabase.DeleteAsset(relativePath);
        }
        
        Debug.Log($"Deleted {catalogFiles.Length} existing catalog recipes");
    }

    private static void CreateAbilityCatalogRecipe(CatalogDataFile catalogData)
    {
        string assetPath = $"{CatalogRecipesPath}/{catalogData.catalogName}.asset";
        
        var catalog = ScriptableObject.CreateInstance<AbilityCatalogRecipe>();
        
        // Convert JSON categories to Unity categories
        var unityCategories = new List<AbilityCatalogRecipe.Category>();
        foreach (var jsonCategory in catalogData.categories)
        {
            var unityCategory = new AbilityCatalogRecipe.Category();
            unityCategory.name = jsonCategory.name;
            unityCategory.entries = jsonCategory.entries;
            unityCategories.Add(unityCategory);
        }
        
        catalog.categories = unityCategories.ToArray();
        
        AssetDatabase.CreateAsset(catalog, assetPath);
        EditorUtility.SetDirty(catalog);
    }

    private static T LoadJsonData<T>(string path)
    {
        string jsonContent = File.ReadAllText(path);
        return JsonUtility.FromJson<T>(jsonContent);
    }

    #region JSON Data Structures

    [System.Serializable]
    public class CatalogDataFile
    {
        public string catalogName;
        public string description;
        public CategoryData[] categories;
    }

    [System.Serializable]
    public class CategoryData
    {
        public string name;
        public string[] entries;
    }

    #endregion
}

#endif