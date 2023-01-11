using UnityEngine;

public static class UnitFactory
{
    public static GameObject Create(string name, int level)
    {
        var recipe = Resources.Load<UnitRecipe>("Unit Recipes/" + name);
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
        return obj;
    }

    private static GameObject InstantiatePrefab(string name)
    {
        var prefab = Resources.Load<GameObject>(name);
        if (prefab == null)
        {
            Debug.LogError("No Prefab for name: " + name);
            return new GameObject(name);
        }

        var instance = Object.Instantiate(prefab);
        return instance;
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

        var recipe = Resources.Load<AbilityCatalogRecipe>("Ability Catalog Recipes/" + name);
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
                ability.name = entry;
                ability.transform.SetParent(category.transform);
            }
        }
    }
}