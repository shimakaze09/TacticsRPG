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

        // Case-insensitive: generated prefab folders are Unity asset paths,
        // where two jobs differing only in case collide and the later file
        // silently erases the earlier one's output
        var seenAbilityJobs = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
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

            if (!seenAbilityJobs.Add(data.job))
            {
                errors.Add($"{Path.GetFileName(file)}: duplicate AbilityData file for job '{data.job}' — the later file would erase the earlier one's generated output");
                continue;
            }

            var ids = new HashSet<string>();
            var names = new HashSet<string>();
            // Ability names become '{name}.prefab' asset paths, so duplicate
            // detection must be case-insensitive ('Fire' vs 'fire' collide on
            // a case-insensitive filesystem); catalog membership below keeps
            // the exact set, since catalogs are generated from these names
            var namesCI = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
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
                else if (!namesCI.Add(ability.name))
                    errors.Add($"{data.job}: duplicate ability name '{ability.name}' (asset paths are case-insensitive)");
                else
                    names.Add(ability.name);
            }
        }

        // Catalogs are generated from AbilityData names (WORLD.md §5.2/§5.3):
        // every catalog must match a job file and list only its ability names.
        // Duplicate catalog names collide on the same generated asset path.
        var catalogNames = new HashSet<string>();
        var seenCatalogNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        foreach (var file in JsonFiles(CatalogDataPath, errors))
        {
            var data = Load<CatalogAssetGenerator.CatalogDataFile>(file, errors);
            if (data == null)
                continue;

            if (!seenCatalogNames.Add(data.catalogName ?? ""))
            {
                errors.Add($"{Path.GetFileName(file)}: duplicate CatalogData file for catalog '{data.catalogName}'");
                continue;
            }

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

        // Jobs, first pass: parse everything and collect resolved ids so
        // prerequisite references can be validated before any asset exists —
        // the generator only discovers a dangling prerequisite in its second
        // pass, after the whole job tree has been written
        var jobFiles = new List<(string label, JobDataFile data)>();
        var jobIds = new HashSet<string>();
        // Job ids become 'Jobs/{id}.asset' paths: duplicate detection must be
        // case-insensitive ('burner' vs 'Burner' collide on disk), while
        // prerequisite matching below stays exact against declared ids
        var jobIdsCI = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        foreach (var file in JsonFiles(JobDataPath, errors))
        {
            var data = Load<JobDataFile>(file, errors);
            if (data == null)
                continue;

            var jobLabel = string.IsNullOrEmpty(data.jobName) ? Path.GetFileName(file) : data.jobName;
            var id = string.IsNullOrEmpty(data.id) ? Slug(data.jobName) : data.id;
            // An empty resolved id would target 'Jobs/.asset' — hard error,
            // and only real ids may enter the reference sets
            if (string.IsNullOrEmpty(id))
                errors.Add($"{jobLabel}: job id resolves empty — 'id' or 'jobName' must be set");
            else if (!jobIdsCI.Add(id))
                errors.Add($"{jobLabel}: duplicate job id '{id}' (asset paths are case-insensitive)");
            else
                jobIds.Add(id);
            jobFiles.Add((jobLabel, data));
        }

        // Jobs, second pass: catalog references, unlock references and
        // levels, JP curves, and prerequisite targets/levels
        foreach (var (jobLabel, data) in jobFiles)
        {
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

            // Prerequisites resolve exactly like the generator's second pass:
            // requiredJobId, else the slug of requiredJobName, must name a job
            foreach (var prereq in data.prerequisites ?? new JobPrerequisiteData[0])
            {
                var requiredId = string.IsNullOrEmpty(prereq.requiredJobId)
                    ? Slug(prereq.requiredJobName)
                    : prereq.requiredJobId;

                if (!jobIds.Contains(requiredId))
                    errors.Add($"{jobLabel}: prerequisite '{prereq.requiredJobName}' resolves to job id '{requiredId}' which no JobData defines");

                if (!JobDefinition.IsValidUnlockLevel(prereq.requiredLevel))
                    errors.Add($"{jobLabel}: prerequisite '{prereq.requiredJobName}' requires level {prereq.requiredLevel}, outside 1-{JobDefinition.MaxJobLevel}");
            }
        }

        // WORLD.md §5.1: every defined ability should be unlockable somewhere.
        // Warning only — Common/Attack is reached via unit recipes, not unlocks.
        foreach (var id in allAbilityIds)
            if (!unlockedAbilityIds.Contains(id))
                warnings.Add($"ability '{id}' is not unlockable by any job (recipe-only?)");
    }

    // Enumerates a source folder. A missing folder or one with zero JSON
    // files is a hard error — each dataset is foundational, and generators
    // delete existing output before writing, so an emptied folder would
    // otherwise erase generated content while "succeeding"
    private static IEnumerable<string> JsonFiles(string path, List<string> errors)
    {
        if (!Directory.Exists(path))
        {
            errors.Add($"content folder missing: {path}");
            return new string[0];
        }

        var files = Directory.GetFiles(path, "*.json", SearchOption.TopDirectoryOnly);
        if (files.Length == 0)
            errors.Add($"content folder has no JSON files: {path}");
        return files;
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
