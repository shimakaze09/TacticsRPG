using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class JobParser
{
    [MenuItem("Pre Production/Parse Jobs")]
    public static void Parse()
    {
        CreateDirectories();
        ParseStartingStats();
        ParseGrowthStats();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreateDirectories()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Jobs"))
            AssetDatabase.CreateFolder("Assets/Resources", "Jobs");
    }

    private static void ParseStartingStats()
    {
        var readPath = $"{Application.dataPath}/Settings/data/JobStartingStats.csv";
        var readText = File.ReadAllLines(readPath);
        for (var i = 1; i < readText.Length; i++)
            PartsStartingStats(readText[i]);
    }

    private static void PartsStartingStats(string line)
    {
        var elements = line.Split(',');
        var obj = GetOrCreate(elements[0]);
        var job = obj.GetComponent<Job>();
        for (var i = 1; i < Job.statOrder.Length + 1; i++)
            job.baseStats[i - 1] = Convert.ToInt32(elements[i]);

        var evade = GetFeature(obj, StatTypes.EVD);
        evade.amount = Convert.ToInt32(elements[8]);

        var res = GetFeature(obj, StatTypes.RES);
        res.amount = Convert.ToInt32(elements[9]);

        var move = GetFeature(obj, StatTypes.MOV);
        move.amount = Convert.ToInt32(elements[10]);

        var jump = GetFeature(obj, StatTypes.JMP);
        jump.amount = Convert.ToInt32(elements[11]);
    }

    private static void ParseGrowthStats()
    {
        var readPath = $"{Application.dataPath}/Settings/data/JobGrowthStats.csv";
        var readText = File.ReadAllLines(readPath);
        for (var i = 1; i < readText.Length; i++)
            ParseGrowthStats(readText[i]);
    }

    private static void ParseGrowthStats(string line)
    {
        var elements = line.Split(',');
        var obj = GetOrCreate(elements[0]);
        var job = obj.GetComponent<Job>();
        for (var i = 1; i < elements.Length; i++)
            job.growStats[i - 1] = Convert.ToSingle(elements[i]);
    }

    private static StatModifierFeature GetFeature(GameObject obj, StatTypes type)
    {
        var smf = obj.GetComponents<StatModifierFeature>();
        foreach (var statModifier in smf)
            if (statModifier.type == type)
                return statModifier;

        var feature = obj.AddComponent<StatModifierFeature>();
        feature.type = type;
        return feature;
    }

    private static GameObject GetOrCreate(string jobName)
    {
        var fullPath = $"Assets/Resources/Jobs/{jobName}.prefab";
        var obj = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
        if (obj == null)
            obj = Create(fullPath);
        return obj;
    }

    private static GameObject Create(string fullPath)
    {
        var instance = new GameObject("temp");
        instance.AddComponent<Job>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(instance, fullPath);
        Object.DestroyImmediate(instance);
        return prefab;
    }
}