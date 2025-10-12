using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

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
        obj.name = recipe.name;
        obj.AddComponent<Unit>();
        AddStats(obj);
        AddLocomotion(obj, recipe.locomotion);
        obj.AddComponent<Status>();
        obj.AddComponent<Equipment>();
        AddJob(obj, recipe.job);
        AddRank(obj, level);
        obj.AddComponent<Health>();
        obj.AddComponent<Mana>();
        AddAttack(obj, recipe.attack);
        AddAbilityCatalog(obj, recipe.abilityCatalog);
        AddAlliance(obj, recipe.alliance);
        AddAttackPattern(obj, recipe.strategy);
        AddElement(obj, recipe.element);
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
            return new GameObject(name);
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
        var instance = InstantiatePrefab("Jobs/" + name);
        instance.transform.SetParent(obj.transform);
        var job = instance.GetComponent<Job>();
        job.Employ();
        job.LoadDefaultStats();
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
        var instance = InstantiatePrefab("Abilities/" + name);
        instance.transform.SetParent(obj.transform);
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
                var abilityName = $"Abilities/{categoryName.name}/{entry}";
                var ability = InstantiatePrefab(abilityName);
                ability.transform.SetParent(category.transform);
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