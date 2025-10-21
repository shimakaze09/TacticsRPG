using UnityEngine;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Data-driven FFT Ability Creator - Generates ability prefabs from JSON definition files
/// Reads ability data from Assets/Resources/AbilityData/{ClassName}.json
/// Creates prefabs under Assets/Resources/Abilities/{ClassName}/{AbilityName}.prefab
/// </summary>
public static class FFTAbilityCreator
{
    private const string AbilitiesPath = "Assets/Resources/Abilities";
    private const string AbilityDataPath = "Assets/Resources/AbilityData";

    #region JSON Data Structures

    [System.Serializable]
    public class AbilityDataFile
    {
        public string job;
        public List<AbilityData> abilities;
    }

    [System.Serializable]
    public class AbilityData
    {
        public string name;
        public RangeData range;
        public AreaData area;
        public PowerData power;
        public int mpCost;
        public List<EffectData> effects;
    }

    [System.Serializable]
    public class RangeData
    {
        public string type; // Constant, Line, Self
        public int value;
        public int height;
    }

    [System.Serializable]
    public class AreaData
    {
        public string type; // Unit, Full, Line, Cross, Diamond, etc.
    }

    [System.Serializable]
    public class PowerData
    {
        public string type; // Physical, Magical
        public int value;
    }

    [System.Serializable]
    public class EffectData
    {
        public string type; // Damage, Heal, Inflict, Revive
        public string hitRate; // A, B, C, D, S
        public string target; // Enemy, Ally, Self, etc.
        public string status; // For Inflict effects
    }

    #endregion

    #region Main Menu Integration

    [MenuItem("Tactics RPG/Create FFT Abilities/Generate from JSON")]
    public static void GenerateAbilitiesFromJSON()
    {
        Debug.Log("Starting JSON-based ability generation...");
        
        EnsureDirectoriesExist();
        
        // Find all JSON files in AbilityData folder
        string[] jsonFiles = Directory.GetFiles(AbilityDataPath, "*.json", SearchOption.TopDirectoryOnly);
        
        if (jsonFiles.Length == 0)
        {
            Debug.LogWarning($"No JSON files found in {AbilityDataPath}. Please create JSON files for each job class.");
            return;
        }

        int totalAbilitiesCreated = 0;
        int totalJobsProcessed = 0;

        foreach (string jsonFile in jsonFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(jsonFile);
            Debug.Log($"Processing job: {fileName}");
            
            try
            {
                AbilityDataFile abilityData = LoadJsonData<AbilityDataFile>(jsonFile);
                if (abilityData != null && abilityData.abilities != null)
                {
                    int abilitiesCreated = GenerateAbilitiesForJob(abilityData);
                    totalAbilitiesCreated += abilitiesCreated;
                    totalJobsProcessed++;
                    Debug.Log($"Created {abilitiesCreated} abilities for {fileName}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to process {fileName}: {e.Message}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"JSON-based generation complete! Created {totalAbilitiesCreated} abilities across {totalJobsProcessed} jobs.");
    }

    #endregion

    #region Directory Management

    private static void EnsureDirectoriesExist()
    {
        // Ensure base directories exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        if (!AssetDatabase.IsValidFolder(AbilitiesPath))
            AssetDatabase.CreateFolder("Assets/Resources", "Abilities");

        if (!AssetDatabase.IsValidFolder(AbilityDataPath))
            AssetDatabase.CreateFolder("Assets/Resources", "AbilityData");
    }

    private static void DeleteExistingAbilityPrefabs(string classPath)
    {
        if (Directory.Exists(classPath))
        {
            string[] prefabFiles = Directory.GetFiles(classPath, "*.prefab", SearchOption.TopDirectoryOnly);
            foreach (string prefabFile in prefabFiles)
            {
                AssetDatabase.DeleteAsset(prefabFile);
            }
        }
    }

    #endregion

    #region Ability Generation

    private static int GenerateAbilitiesForJob(AbilityDataFile jobData)
    {
        string className = jobData.job;
        string classPath = Path.Combine(AbilitiesPath, className);
        
        // Delete existing prefabs for this class
        DeleteExistingAbilityPrefabs(classPath);
        
        // Ensure class directory exists
        if (!AssetDatabase.IsValidFolder(classPath))
        {
            AssetDatabase.CreateFolder(AbilitiesPath, className);
        }

        int abilitiesCreated = 0;

        foreach (AbilityData abilityData in jobData.abilities)
        {
            try
            {
                CreateRootAbilityObject(abilityData, classPath);
                abilitiesCreated++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create ability '{abilityData.name}': {e.Message}");
            }
        }

        return abilitiesCreated;
    }

    private static void CreateRootAbilityObject(AbilityData data, string classPath)
    {
        // Create root GameObject
        GameObject abilityRoot = new GameObject(data.name);
        
        // Add Ability component
        AddComponent<Ability>(abilityRoot);

        // Add Power component based on type
        if (data.power != null)
        {
            if (data.power.type == "Physical")
            {
                var powerComponent = AddComponent<PhysicalAbilityPower>(abilityRoot);
                powerComponent.level = data.power.value;
            }
            else if (data.power.type == "Magical")
            {
                var powerComponent = AddComponent<MagicalAbilityPower>(abilityRoot);
                powerComponent.level = data.power.value;
            }
        }

        // Add Range component
        if (data.range != null)
        {
            AddRangeComponent(abilityRoot, data.range);
        }

        // Add Area component
        if (data.area != null)
        {
            AddAreaComponent(abilityRoot, data.area);
        }

        // Add MP Cost component
        if (data.mpCost > 0)
        {
            var mpCostComponent = AddComponent<AbilityMagicCost>(abilityRoot);
            mpCostComponent.amount = data.mpCost;
        }

        // Create effect child objects
        if (data.effects != null)
        {
            foreach (EffectData effectData in data.effects)
            {
                CreateEffectChild(abilityRoot, effectData);
            }
        }

        // Save as prefab
        string prefabPath = Path.Combine(classPath, $"{data.name}.prefab");
        PrefabUtility.SaveAsPrefabAsset(abilityRoot, prefabPath);
        
        // Clean up temporary GameObject
        Object.DestroyImmediate(abilityRoot);
    }

    private static void CreateEffectChild(GameObject root, EffectData effectData)
    {
        GameObject effectChild = new GameObject($"Effect_{effectData.type}");
        effectChild.transform.SetParent(root.transform);

        // Add Hit Rate component
        AddHitRateComponent(effectChild, effectData.hitRate);

        // Add Effect component
        AddEffectComponent(effectChild, effectData);

        // Add Target component
        AddTargetComponent(effectChild, effectData.target);
    }

    #endregion

    #region Component Helpers

    private static void AddRangeComponent(GameObject go, RangeData rangeData)
    {
        switch (rangeData.type)
        {
            case "Constant":
                var constantRange = AddComponent<ConstantAbilityRange>(go);
                constantRange.horizontal = rangeData.value;
                constantRange.vertical = rangeData.height;
                break;
            case "Line":
                var lineRange = AddComponent<LineAbilityRange>(go);
                lineRange.horizontal = rangeData.value;
                break;
            case "Self":
                AddComponent<SelfAbilityRange>(go);
                break;
            default:
                Debug.LogWarning($"Unknown range type: {rangeData.type}");
                break;
        }
    }

    private static void AddAreaComponent(GameObject go, AreaData areaData)
    {
        switch (areaData.type)
        {
            case "Unit":
                AddComponent<UnitAbilityArea>(go);
                break;
            case "Full":
                AddComponent<FullAbilityArea>(go);
                break;
            case "Specify":
                AddComponent<SpecifyAbilityArea>(go);
                break;
            default:
                Debug.LogWarning($"Unknown area type: {areaData.type}");
                break;
        }
    }

    private static void AddHitRateComponent(GameObject go, string hitRate)
    {
        switch (hitRate)
        {
            case "A":
                AddComponent<ATypeHitRate>(go);
                break;
            case "S":
                AddComponent<STypeHitRate>(go);
                break;
            case "Full":
                AddComponent<FullTypeHitRate>(go);
                break;
            default:
                Debug.LogWarning($"Unknown hit rate: {hitRate}");
                break;
        }
    }

    private static void AddEffectComponent(GameObject go, EffectData effectData)
    {
        switch (effectData.type)
        {
            case "Damage":
                AddComponent<DamageAbilityEffect>(go);
                break;
            case "Heal":
                AddComponent<HealAbilityEffect>(go);
                break;
            case "Inflict":
                var inflictEffect = AddComponent<InflictAbilityEffect>(go);
                if (!string.IsNullOrEmpty(effectData.status))
                {
                    inflictEffect.statusName = effectData.status;
                }
                break;
            case "Revive":
                AddComponent<ReviveAbilityEffect>(go);
                break;
            default:
                Debug.LogWarning($"Unknown effect type: {effectData.type}");
                break;
        }
    }

    private static void AddTargetComponent(GameObject go, string targetType)
    {
        switch (targetType)
        {
            case "Enemy":
                AddComponent<EnemyAbilityEffectTarget>(go);
                break;
            case "AbsorbDamage":
                AddComponent<AbsorbDamageAbilityEffectTarget>(go);
                break;
            case "KOd":
                AddComponent<KOdAbilityEffectTarget>(go);
                break;
            case "Undead":
                AddComponent<UndeadAbilityEffectTarget>(go);
                break;
            default:
                AddComponent<DefaultAbilityEffectTarget>(go);
                break;
        }
    }

    private static T AddComponent<T>(GameObject go) where T : Component
    {
        return go.AddComponent<T>();
    }

    #endregion

    #region JSON Loading

    private static T LoadJsonData<T>(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"JSON file not found: {path}");
            return default(T);
        }

        try
        {
            string jsonContent = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(jsonContent);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load JSON from {path}: {e.Message}");
            return default(T);
        }
    }

    #endregion
}

#endif