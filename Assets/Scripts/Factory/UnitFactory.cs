using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Builds a battle-ready unit from a UnitRecipe: model, stats, rank, job,
/// health/mana, attack ability, alliance, AI pattern, and difficulty modifiers,
/// in dependency order.
/// </summary>
public static class UnitFactory
{
    #region Cached Resources

    private static readonly Dictionary<string, GameObject> prefabCache = new();
    private static readonly Dictionary<string, UnitRecipe> unitRecipeCache = new();
    private static readonly Dictionary<string, AbilityCatalogRecipe> abilityCatalogCache = new();

    #endregion

    #region Public

    public static GameObject Create(string name, int level)
    {
        var recipe = LoadUnitRecipe(name);
        if (recipe == null)
        {
            Debug.LogError("No Unit Recipe for name: " + name);
            return null;
        }

        return Create(recipe, level);
    }

    public static GameObject Create(UnitRecipe recipe, int level)
    {
        var obj = InstantiatePrefab("Units/" + recipe.model);
        if (obj == null)
        {
            Debug.LogError("Missing unit model prefab: Units/" + recipe.model);
            return null;
        }

        obj.name = recipe.name;
        obj.AddComponent<Unit>();
        AddStats(obj);
        AddLocomotion(obj, recipe.locomotion);
        obj.AddComponent<Status>();
        obj.AddComponent<Equipment>();
        // Rank must exist before JobManager: JobManager.Awake caches the Rank
        // reference and its level-up JP subscription depends on it.
        AddRank(obj, level);
        AddJob(obj, recipe.job);
        EquipStartingGear(obj);
        obj.AddComponent<Health>();
        obj.AddComponent<Mana>();
        AddAttack(obj, recipe.attack);
        AddAlliance(obj, recipe.alliance);
        AddAttackPattern(obj, recipe.strategy);
        AddElement(obj, recipe.element);

        // Hard difficulty: enemies hit harder (HP boost happens inside
        // JobManager's stat recalculation once the Alliance exists)
        if (recipe.alliance == Alliances.Enemy && DifficultySettings.Current == Difficulty.Hard)
            obj.AddComponent<HardModeDamageModifier>();

        return obj;
    }

    #endregion

    #region Private

    private static UnitRecipe LoadUnitRecipe(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        if (!unitRecipeCache.TryGetValue(name, out var recipe) || recipe == null)
        {
            recipe = Resources.Load<UnitRecipe>("Unit Recipes/" + name);
            unitRecipeCache[name] = recipe;
        }

        return recipe;
    }

    private static AbilityCatalogRecipe LoadAbilityCatalogRecipe(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        if (!abilityCatalogCache.TryGetValue(name, out var recipe) || recipe == null)
        {
            recipe = Resources.Load<AbilityCatalogRecipe>("Ability Catalog Recipes/" + name);
            abilityCatalogCache[name] = recipe;
        }

        return recipe;
    }

    private static GameObject InstantiatePrefab(string name)
    {
        var prefab = LoadPrefab(name);
        if (prefab == null)
        {
            Debug.LogError("No Prefab for name: " + name);
            return null;
        }

        var instance = Object.Instantiate(prefab);
        instance.name = instance.name.Replace("(Clone)", "");
        return instance;
    }

    private static GameObject LoadPrefab(string path)
    {
        if (!prefabCache.TryGetValue(path, out var prefab) || prefab == null)
        {
            prefab = Resources.Load<GameObject>(path);
            prefabCache[path] = prefab;
        }

        return prefab;
    }

    private static void AddStats(GameObject obj)
    {
        var s = obj.AddComponent<Stats>();
        s.SetValue(StatTypes.LVL, 1, false);
    }

    private static void AddJob(GameObject obj, string name)
    {
        // Add JobManager component for FFT-style job system
        var jobManager = obj.AddComponent<JobManager>();
        
        // Recipes reference jobs by stable id; fall back to display name
        // for any legacy asset still using one.
        var jobDefinition = jobManager.FindJobById(name) ?? jobManager.FindJobByName(name);
        if (jobDefinition == null)
        {
            Debug.LogWarning($"JobDefinition '{name}' not found. JobManager will auto-initialize with the default job.");
            return;
        }
        
        // Initialize with the specified job
        jobManager.ProgressData.InitializeWithBasicJobs(jobDefinition);
        
        // Sync abilities with job progress
        jobManager.AbilityMemory.SyncLearnedAbilities(jobManager.ProgressData, jobManager.allJobs);
        
        // Calculate initial stats and fill HP/MP for the fresh unit
        jobManager.RecalculateStats(true);
        
        // Create ability catalog based on job's catalog name
        CreateJobAbilityCatalog(obj, jobDefinition.abilityCatalogName);
        
        Debug.Log($"Added JobManager to {obj.name} with job: {jobDefinition.jobName}");
    }

    private static void CreateJobAbilityCatalog(GameObject obj, string catalogName)
    {
        if (string.IsNullOrEmpty(catalogName))
        {
            Debug.LogWarning($"Job has no ability catalog specified for {obj.name}");
            return;
        }

        var main = new GameObject("Ability Catalog");
        main.transform.SetParent(obj.transform);
        main.AddComponent<AbilityCatalog>();

        var recipe = LoadAbilityCatalogRecipe(catalogName);
        if (recipe == null)
        {
            Debug.LogError($"No Ability Catalog Recipe Found: {catalogName}");
            return;
        }

        foreach (var categoryName in recipe.categories)
        {
            var category = new GameObject(categoryName.name);
            category.transform.SetParent(main.transform);

            foreach (var entry in categoryName.entries)
            {
                // Updated path structure for new data-driven system
                // Format: "Abilities/{JobName}/{AbilityName}"
                var abilityName = $"Abilities/{catalogName}/{entry}";
                var ability = InstantiatePrefab(abilityName);
                if (ability != null)
                {
                    ability.transform.SetParent(category.transform);
                }
                else
                {
                    Debug.LogWarning($"Ability not found: {abilityName}");
                }
            }
        }

        Debug.Log($"Created ability catalog '{catalogName}' for {obj.name}");
    }

    // Every job spawns wearing its default loadout (GDD §3.3); the feature
    // activation raises stats and RecalculateStats preserves the bonuses.
    // Keyed off the resolved job's stable id, not the recipe string, so
    // legacy display-name references still get their gear.
    private static void EquipStartingGear(GameObject obj)
    {
        var equipment = obj.GetComponent<Equipment>();
        var jobManager = obj.GetComponent<JobManager>();
        if (equipment == null || jobManager == null || jobManager.CurrentJob == null)
            return;

        foreach (var gearId in GearCatalog.StartingGear(jobManager.CurrentJob.id))
        {
            var item = ItemFactory.Create(gearId);
            if (item == null)
                continue;

            item.transform.SetParent(obj.transform);
            var equippable = item.GetComponent<Equippable>();
            equipment.Equip(equippable, equippable.defaultSlots);
        }
    }

    private static void AddLocomotion(GameObject obj, Locomotions type)
    {
        switch (type)
        {
            case Locomotions.Walk:
                obj.AddComponent<WalkMovement>();
                break;
            case Locomotions.Fly:
                obj.AddComponent<FlyMovement>();
                break;
            case Locomotions.Teleport:
                obj.AddComponent<TeleportMovement>();
                break;
        }
    }

    private static void AddAlliance(GameObject obj, Alliances type)
    {
        var alliance = obj.AddComponent<Alliance>();
        alliance.type = type;
    }

    private static void AddRank(GameObject obj, int level)
    {
        var rank = obj.AddComponent<Rank>();
        rank.Init(level);
    }

    private static void AddAttack(GameObject obj, string name)
    {
        // Handle the attack ability path
        // name format: "Common/Attack" -> "Abilities/Common/Attack"
        var abilityPath = "Abilities/" + name;
        var instance = InstantiatePrefab(abilityPath);
        if (instance != null)
        {
            instance.transform.SetParent(obj.transform);
        }
        else
        {
            Debug.LogWarning($"Attack ability not found: {abilityPath}");
        }
    }

    private static void AddAbilityCatalog(GameObject obj, string name)
    {
        var main = new GameObject("Ability Catalog");
        main.transform.SetParent(obj.transform);
        main.AddComponent<AbilityCatalog>();

        var recipe = LoadAbilityCatalogRecipe(name);
        if (recipe == null)
        {
            Debug.LogError("No Ability Catalog Recipe Found: " + name);
            return;
        }

        foreach (var categoryName in recipe.categories)
        {
            var category = new GameObject(categoryName.name);
            category.transform.SetParent(main.transform);

            foreach (var entry in categoryName.entries)
            {
                // Updated path structure for new data-driven system
                // Format: "Abilities/{JobName}/{AbilityName}"
                var abilityName = $"Abilities/{name}/{entry}";
                var ability = InstantiatePrefab(abilityName);
                if (ability != null)
                {
                    ability.transform.SetParent(category.transform);
                }
                else
                {
                    Debug.LogWarning($"Ability not found: {abilityName}");
                }
            }
        }
    }

    private static void AddAttackPattern(GameObject obj, string name)
    {
        var driver = obj.AddComponent<Driver>();
        if (string.IsNullOrEmpty(name))
        {
            driver.normal = Drivers.Human;
        }
        else
        {
            driver.normal = Drivers.Computer;
            var instance = InstantiatePrefab("Attack Pattern/" + name);
            if (instance != null)
                instance.transform.SetParent(obj.transform);
        }
    }

    private static void AddElement(GameObject obj, string name)
    {
        if (!Enum.TryParse(name, out ElementTypes elementType))
        {
            Debug.Log("Invalid input");
            return;
        }

        var element = obj.AddComponent<Elements>();
        element.types = elementType;
        (element.advantaged, element.restrained) = ElementRelationship.elementRestriction[elementType];
    }

    #endregion
}