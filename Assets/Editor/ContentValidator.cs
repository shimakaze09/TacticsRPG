using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR

/// <summary>
/// Cross-reference validation for the content JSON (issue #7): checks ids and
/// every job/ability/catalog reference across the three source folders before
/// any asset is written, so a broken checkout fails loudly instead of
/// generating half-valid content. Enforces the WORLD.md §5 pipeline
/// invariants that are checkable from data alone.
/// </summary>
public static class ContentValidator
{
    private const string AbilityDataPath = "Assets/Resources/AbilityData";
    private const string CatalogDataPath = "Assets/Resources/CatalogData";
    private const string JobDataPath = "Assets/Resources/JobData";

    /// <summary>
    /// Runs every data check. Errors block generation; warnings flag drift
    /// worth a look (e.g. an ability no job unlocks) without failing CI.
    /// </summary>
    public static void Validate(List<string> errors, List<string> warnings)
    {
        // job/catalog name -> ability ids and display names defined for it
        var abilityIdsByJob = new Dictionary<string, HashSet<string>>();
        var abilityNamesByJob = new Dictionary<string, HashSet<string>>();
        var allAbilityIds = new HashSet<string>();
        var unlockedAbilityIds = new HashSet<string>();

        foreach (var file in JsonFiles(AbilityDataPath, errors))
        {
            var data = Load<AbilityAssetGenerator.AbilityDataFile>(file, errors);
            if (data == null)
                continue;

            if (string.IsNullOrEmpty(data.job))
            {
                errors.Add($"{Path.GetFileName(file)}: missing 'job' name");
                continue;
            }

            var ids = new HashSet<string>();
            var names = new HashSet<string>();
            abilityIdsByJob[data.job] = ids;
            abilityNamesByJob[data.job] = names;

            foreach (var ability in data.abilities ?? new List<AbilityAssetGenerator.AbilityData>())
            {
                if (string.IsNullOrEmpty(ability.id))
                    errors.Add($"{data.job}: ability '{ability.name}' has no stable id");
                else if (!allAbilityIds.Add(ability.id))
                    errors.Add($"{data.job}: duplicate ability id '{ability.id}'");
                else
                    ids.Add(ability.id);

                if (string.IsNullOrEmpty(ability.name))
                    errors.Add($"{data.job}: ability '{ability.id}' has no display name");
                else if (!names.Add(ability.name))
                    errors.Add($"{data.job}: duplicate ability name '{ability.name}'");
            }
        }

        // Catalogs are generated from AbilityData names (WORLD.md §5.2/§5.3):
        // every catalog must match a job file and list only its ability names
        var catalogNames = new HashSet<string>();
        foreach (var file in JsonFiles(CatalogDataPath, errors))
        {
            var data = Load<CatalogAssetGenerator.CatalogDataFile>(file, errors);
            if (data == null)
                continue;

            catalogNames.Add(data.catalogName);
            if (!abilityNamesByJob.TryGetValue(data.catalogName, out var jobNames))
            {
                errors.Add($"catalog '{data.catalogName}': no AbilityData file defines job '{data.catalogName}'");
                continue;
            }

            foreach (var category in data.categories ?? new CatalogAssetGenerator.CategoryData[0])
                foreach (var entry in category.entries ?? new string[0])
                    if (!jobNames.Contains(entry))
                        errors.Add($"catalog '{data.catalogName}': entry '{entry}' is not an ability of that job");
        }

        // Jobs: unique ids, resolvable catalog, resolvable unlock references,
        // in-range unlock levels, and a valid JP curve
        var jobIds = new HashSet<string>();
        foreach (var file in JsonFiles(JobDataPath, errors))
        {
            var data = Load<JobDataFile>(file, errors);
            if (data == null)
                continue;

            var jobLabel = string.IsNullOrEmpty(data.jobName) ? Path.GetFileName(file) : data.jobName;
            var id = string.IsNullOrEmpty(data.id) ? Slug(data.jobName) : data.id;
            if (!jobIds.Add(id))
                errors.Add($"{jobLabel}: duplicate job id '{id}'");

            if (string.IsNullOrEmpty(data.abilityCatalogName))
            {
                errors.Add($"{jobLabel}: missing abilityCatalogName");
            }
            else
            {
                if (!catalogNames.Contains(data.abilityCatalogName))
                    errors.Add($"{jobLabel}: abilityCatalogName '{data.abilityCatalogName}' has no CatalogData file");
                if (!abilityIdsByJob.ContainsKey(data.abilityCatalogName))
                    errors.Add($"{jobLabel}: abilityCatalogName '{data.abilityCatalogName}' has no AbilityData file");
            }

            var curveError = JobDefinition.ValidateJPCurve(data.jpRequirements);
            if (curveError != null)
                errors.Add($"{jobLabel}: {curveError}");

            abilityIdsByJob.TryGetValue(data.abilityCatalogName ?? "", out var ownIds);
            foreach (var unlock in data.abilityUnlocks ?? new JobAbilityUnlockData[0])
            {
                var abilityId = string.IsNullOrEmpty(unlock.abilityId)
                    ? $"{Slug(data.abilityCatalogName ?? data.jobName)}.{Slug(unlock.abilityName)}"
                    : unlock.abilityId;

                if (ownIds == null || !ownIds.Contains(abilityId))
                    errors.Add($"{jobLabel}: unlock '{unlock.abilityName}' resolves to id '{abilityId}' which its AbilityData does not define");
                else
                    unlockedAbilityIds.Add(abilityId);

                if (!JobDefinition.IsValidUnlockLevel(unlock.unlockAtJobLevel))
                    errors.Add($"{jobLabel}: unlock '{unlock.abilityName}' at job level {unlock.unlockAtJobLevel}, outside 1-{JobDefinition.MaxJobLevel}");

                if (unlock.jpCost < 0)
                    errors.Add($"{jobLabel}: unlock '{unlock.abilityName}' has negative jpCost {unlock.jpCost}");
            }
        }

        // WORLD.md §5.1: every defined ability should be unlockable somewhere.
        // Warning only — Common/Attack is reached via unit recipes, not unlocks.
        foreach (var id in allAbilityIds)
            if (!unlockedAbilityIds.Contains(id))
                warnings.Add($"ability '{id}' is not unlockable by any job (recipe-only?)");
    }

    // Enumerates a source folder, reporting a missing folder as an error —
    // a clean checkout must carry all three
    private static IEnumerable<string> JsonFiles(string path, List<string> errors)
    {
        if (!Directory.Exists(path))
        {
            errors.Add($"content folder missing: {path}");
            return new string[0];
        }

        return Directory.GetFiles(path, "*.json", SearchOption.TopDirectoryOnly);
    }

    // Parses one JSON file, converting parse failures into validation errors
    private static T Load<T>(string file, List<string> errors) where T : class
    {
        try
        {
            return JsonUtility.FromJson<T>(File.ReadAllText(file));
        }
        catch (System.Exception e)
        {
            errors.Add($"{Path.GetFileName(file)}: failed to parse — {e.Message}");
            return null;
        }
    }

    // Mirrors the generators' fallback slug for data that predates stable ids
    private static string Slug(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "";
        var s = System.Text.RegularExpressions.Regex.Replace(name.Trim().ToLowerInvariant(), "[^a-z0-9]+", "_");
        return s.Trim('_');
    }
}
#endif
