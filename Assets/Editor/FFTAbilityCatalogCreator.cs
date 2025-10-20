using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Editor utility to create Ability Catalog Recipe assets for all FFT jobs
/// These recipes define which abilities belong to each job's catalog
/// </summary>
public static class FFTAbilityCatalogCreator
{
    private const string CatalogRecipesPath = "Assets/Resources/Ability Catalog Recipes";

    [MenuItem("Tactics RPG/Create FFT Catalogs/Create All Ability Catalogs (Fresh)")]
    public static void CreateAllAbilityCatalogsFresh()
    {
        EnsureCatalogRecipesDirectoryExists();
        
        // Delete all existing catalog recipes first
        DeleteAllExistingCatalogs();
        
        // Create all ability catalog recipes
        CreateBasicJobCatalogs();
        CreateAdvancedJobCatalogs();
        CreateSpecialJobCatalogs();
        CreateUniqueJobCatalogs();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created ALL FFT ability catalog recipes (30+ catalogs)!");
    }

    private static void EnsureCatalogRecipesDirectoryExists()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        if (!AssetDatabase.IsValidFolder(CatalogRecipesPath))
            AssetDatabase.CreateFolder("Assets/Resources", "Ability Catalog Recipes");
    }

    private static void DeleteAllExistingCatalogs()
    {
        // Get all .asset files in the catalog recipes folder
        string[] catalogFiles = System.IO.Directory.GetFiles(CatalogRecipesPath, "*.asset", System.IO.SearchOption.TopDirectoryOnly);
        
        foreach (string file in catalogFiles)
        {
            string relativePath = "Assets" + file.Substring(Application.dataPath.Length);
            AssetDatabase.DeleteAsset(relativePath);
            Debug.Log($"Deleted existing catalog: {relativePath}");
        }
        
        Debug.Log($"Deleted {catalogFiles.Length} existing catalog recipes");
    }

    #region Basic Job Catalogs

    private static void CreateBasicJobCatalogs()
    {
        // Squire - Basic abilities
        CreateAbilityCatalogRecipe("DemoCatalog", new string[] {
            "Attack", "Defend", "Accumulate", "Stone Throw", 
            "Dash", "Focus", "Yell", "Cheer"
        });

        // Chemist - Item abilities
        CreateAbilityCatalogRecipe("Chemist", new string[] {
            "Potion", "Phoenix Down", "Antidote", "Eye Drop",
            "Echo Grass", "Remedy", "Auto Potion", "Throw Item"
        });

        // Knight - Fighter abilities
        CreateAbilityCatalogRecipe("Fighter Tech", new string[] {
            "First Aid", "Air Blast", "Earth Render", "Wind Slash",
            "Power Break", "Armor Break", "Mental Break", "Magic Break"
        });

        // Wizard - Black magic
        CreateAbilityCatalogRecipe("Black Magic", new string[] {
            "Fire", "Blizzard", "Thunder", "Flare",
            "Freeze", "Bolt", "Meteor", "Ultima"
        });

        // Priest - White magic
        CreateAbilityCatalogRecipe("White Magic", new string[] {
            "Cure", "Cura", "Curaga", "Holy",
            "Raise", "Reraise", "Esuna", "Dispel"
        });

        Debug.Log("Created basic job catalogs: DemoCatalog, Chemist, Fighter Tech, Black Magic, White Magic");
    }

    #endregion

    #region Advanced Job Catalogs

    private static void CreateAdvancedJobCatalogs()
    {
        // Archer abilities
        CreateAbilityCatalogRecipe("Archer", new string[] {
            "Aim", "Charge +", "Charge +2", "Charge +3",
            "Charge +4", "Charge +5", "Charge +6", "Charge +7"
        });

        // Monk abilities
        CreateAbilityCatalogRecipe("Monk", new string[] {
            "Chakra", "Counter", "Hamedo", "Return",
            "Two Hands", "Martial Arts", "Kick", "Earth Slash"
        });

        // Thief abilities
        CreateAbilityCatalogRecipe("Thief", new string[] {
            "Steal", "Mug", "Steal Heart", "Steal Weapon",
            "Steal Armor", "Steal Accessory", "Steal Gil", "Steal Item"
        });

        // Mystic abilities
        CreateAbilityCatalogRecipe("Mystic", new string[] {
            "Teleport", "Float", "Reflect", "Absorb",
            "Drain", "Osmose", "Demi", "Gravity"
        });

        // Time Mage abilities
        CreateAbilityCatalogRecipe("Time Magic", new string[] {
            "Haste", "Slow", "Stop", "Regen",
            "Reflect", "Float", "Teleport", "Quick"
        });

        // Summoner abilities
        CreateAbilityCatalogRecipe("Summon", new string[] {
            "Chocobo", "Moogle", "Golem", "Ramuh",
            "Ifrit", "Shiva", "Odin", "Bahamut"
        });

        // Geomancer abilities
        CreateAbilityCatalogRecipe("Geomancy", new string[] {
            "Earth", "Wind", "Water", "Fire",
            "Ice", "Lightning", "Poison", "Sleep"
        });

        // Dragoon abilities
        CreateAbilityCatalogRecipe("Jump", new string[] {
            "Jump", "High Jump", "Lancet", "Dragon",
            "Dragon Breath", "Dragon Crest", "Dragon Heart", "Dragon Soul"
        });

        Debug.Log("Created advanced job catalogs: Archer, Monk, Thief, Mystic, Time Magic, Summon, Geomancy, Jump");
    }

    #endregion

    #region Special Job Catalogs

    private static void CreateSpecialJobCatalogs()
    {
        // Oracle abilities
        CreateAbilityCatalogRecipe("Oracle", new string[] {
            "Pray Faith", "Pray", "Invitation", "Condemn",
            "Accumulate", "Yell", "Cheer", "Cry"
        });

        // Ninja abilities
        CreateAbilityCatalogRecipe("Ninja", new string[] {
            "Throw", "Art of War", "Art of War", "Art of War",
            "Art of War", "Art of War", "Art of War", "Art of War"
        });

        // Samurai abilities
        CreateAbilityCatalogRecipe("Samurai", new string[] {
            "Iaido", "Art of War", "Art of War", "Art of War",
            "Art of War", "Art of War", "Art of War", "Art of War"
        });

        // Lancer abilities
        CreateAbilityCatalogRecipe("Lancer", new string[] {
            "Lance", "Art of War", "Art of War", "Art of War",
            "Art of War", "Art of War", "Art of War", "Art of War"
        });

        // Mediator abilities
        CreateAbilityCatalogRecipe("Mediator", new string[] {
            "Talk Skill", "Art of War", "Art of War", "Art of War",
            "Art of War", "Art of War", "Art of War", "Art of War"
        });

        // Calculator abilities
        CreateAbilityCatalogRecipe("Calculator", new string[] {
            "Math Skill", "Art of War", "Art of War", "Art of War",
            "Art of War", "Art of War", "Art of War", "Art of War"
        });

        // Bard abilities
        CreateAbilityCatalogRecipe("Bard", new string[] {
            "Sing", "Art of War", "Art of War", "Art of War",
            "Art of War", "Art of War", "Art of War", "Art of War"
        });

        // Dancer abilities
        CreateAbilityCatalogRecipe("Dancer", new string[] {
            "Dance", "Art of War", "Art of War", "Art of War",
            "Art of War", "Art of War", "Art of War", "Art of War"
        });

        Debug.Log("Created special job catalogs: Oracle, Ninja, Samurai, Lancer, Mediator, Calculator, Bard, Dancer");
    }

    #endregion

    #region Unique Job Catalogs

    private static void CreateUniqueJobCatalogs()
    {
        // Mime abilities
        CreateAbilityCatalogRecipe("Mime", new string[] {
            "Mimic", "Art of War", "Art of War", "Art of War",
            "Art of War", "Art of War", "Art of War", "Art of War"
        });

        // Create empty catalogs for jobs that don't have specific abilities yet
        CreateEmptyAbilityCatalogRecipe("Priest");
        CreateEmptyAbilityCatalogRecipe("Archer");
        CreateEmptyAbilityCatalogRecipe("Monk");
        CreateEmptyAbilityCatalogRecipe("Thief");
        CreateEmptyAbilityCatalogRecipe("Mystic");
        CreateEmptyAbilityCatalogRecipe("Time Mage");
        CreateEmptyAbilityCatalogRecipe("Summoner");
        CreateEmptyAbilityCatalogRecipe("Geomancer");
        CreateEmptyAbilityCatalogRecipe("Dragoon");
        CreateEmptyAbilityCatalogRecipe("Oracle");
        CreateEmptyAbilityCatalogRecipe("Ninja");
        CreateEmptyAbilityCatalogRecipe("Samurai");
        CreateEmptyAbilityCatalogRecipe("Lancer");
        CreateEmptyAbilityCatalogRecipe("Mediator");
        CreateEmptyAbilityCatalogRecipe("Calculator");
        CreateEmptyAbilityCatalogRecipe("Bard");
        CreateEmptyAbilityCatalogRecipe("Dancer");
        CreateEmptyAbilityCatalogRecipe("Mime");

        Debug.Log("Created unique job catalogs: Mime + empty catalogs for all jobs");
    }

    #endregion

    #region Helper Methods

    private static void CreateAbilityCatalogRecipe(string catalogName, string[] abilityNames)
    {
        string assetPath = $"{CatalogRecipesPath}/{catalogName}.asset";
        
        var catalog = ScriptableObject.CreateInstance<AbilityCatalogRecipe>();
        
        // Create categories and entries
        var category = new AbilityCatalogRecipe.Category();
        category.name = "Abilities";
        category.entries = abilityNames;
        
        catalog.categories = new AbilityCatalogRecipe.Category[] { category };
        
        AssetDatabase.CreateAsset(catalog, assetPath);
        EditorUtility.SetDirty(catalog);
        
        Debug.Log($"Created ability catalog recipe: {catalogName} with {abilityNames.Length} abilities");
    }

    private static void CreateEmptyAbilityCatalogRecipe(string catalogName)
    {
        string assetPath = $"{CatalogRecipesPath}/{catalogName}.asset";
        
        var catalog = ScriptableObject.CreateInstance<AbilityCatalogRecipe>();
        
        // Create empty category
        var category = new AbilityCatalogRecipe.Category();
        category.name = "Abilities";
        category.entries = new string[0]; // Empty for now
        
        catalog.categories = new AbilityCatalogRecipe.Category[] { category };
        
        AssetDatabase.CreateAsset(catalog, assetPath);
        EditorUtility.SetDirty(catalog);
        
        Debug.Log($"Created empty ability catalog recipe: {catalogName}");
    }

    #endregion
}

#endif
