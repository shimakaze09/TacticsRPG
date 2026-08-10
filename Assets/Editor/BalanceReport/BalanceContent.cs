using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

#if UNITY_EDITOR

/// <summary>
/// Read-only content snapshot for the balance report (issue #58): loads the
/// source-of-truth JSON under Assets/Resources/{JobData,AbilityData,
/// CatalogData} and the battle definition YAML under Assets/Resources/Battles
/// into plain DTOs, without touching (or requiring) the gitignored generated
/// assets. Job entries carry a transient in-memory JobDefinition so the report
/// runs the real ProgressionModel arithmetic instead of a copy of the formula;
/// call Release() when done so those instances don't leak into the editor.
/// </summary>
public class BalanceContent
{
    /// <summary>One job's JSON data plus a transient JobDefinition mirroring it.</summary>
    public class JobEntry
    {
        /// <summary>Raw JobData JSON (unlocks, catalog name, categories).</summary>
        public JobDataFile data;

        /// <summary>In-memory definition fed to ProgressionModel (never saved).</summary>
        public JobDefinition def;
    }

    /// <summary>One recipe/level slot parsed from a battle definition asset.</summary>
    public class EncounterUnit
    {
        /// <summary>Which list the unit came from: Hero, Enemy, or Wave.</summary>
        public string side;

        /// <summary>UnitRecipe name the battle spawns.</summary>
        public string recipe;

        /// <summary>Authored spawn level (drives ProgressionModel stats, #52).</summary>
        public int level;
    }

    /// <summary>One battle definition's identity, objective, and unit slots.</summary>
    public class Encounter
    {
        /// <summary>Stable battle id from the asset.</summary>
        public string id;

        /// <summary>Display name from the asset.</summary>
        public string battleName;

        /// <summary>Raw VictoryType enum index from the asset.</summary>
        public int victoryType;

        /// <summary>Rounds to hold for survive objectives.</summary>
        public int surviveRounds;

        /// <summary>All hero/enemy/wave slots in file order.</summary>
        public List<EncounterUnit> units = new List<EncounterUnit>();
    }

    /// <summary>Jobs sorted by stable id (deterministic report order).</summary>
    public List<JobEntry> jobs = new List<JobEntry>();

    /// <summary>Ability files keyed by their "job" field (includes "Common").</summary>
    public Dictionary<string, AbilityAssetGenerator.AbilityDataFile> abilityFiles =
        new Dictionary<string, AbilityAssetGenerator.AbilityDataFile>();

    /// <summary>Catalog files keyed by catalogName.</summary>
    public Dictionary<string, CatalogAssetGenerator.CatalogDataFile> catalogFiles =
        new Dictionary<string, CatalogAssetGenerator.CatalogDataFile>();

    /// <summary>Battle definitions sorted by file name.</summary>
    public List<Encounter> encounters = new List<Encounter>();

    /// <summary>Data-shape problems found while loading (reported as errors).</summary>
    public List<string> loadErrors = new List<string>();

    /// <summary>
    /// Loads every content source from disk. Always succeeds structurally;
    /// malformed files land in loadErrors so the report can fail the run
    /// instead of throwing halfway through.
    /// </summary>
    public static BalanceContent Load()
    {
        var content = new BalanceContent();
        content.LoadJobs();
        content.LoadAbilities();
        content.LoadCatalogs();
        content.LoadEncounters();

        // An absent or empty source directory must fail the run, never
        // produce a clean report — a renamed JobData folder yielding zero
        // jobs is exactly the checkout drift this audit exists to catch.
        if (content.jobs.Count == 0)
            content.loadErrors.Add("No JobData files loaded — Assets/Resources/JobData is missing or empty.");
        if (content.abilityFiles.Count == 0)
            content.loadErrors.Add("No AbilityData files loaded — Assets/Resources/AbilityData is missing or empty.");
        if (content.catalogFiles.Count == 0)
            content.loadErrors.Add("No CatalogData files loaded — Assets/Resources/CatalogData is missing or empty.");
        if (content.encounters.Count == 0)
            content.loadErrors.Add("No battle definitions loaded — Assets/Resources/Battles is missing or empty.");

        return content;
    }

    /// <summary>
    /// Destroys the transient JobDefinition instances created for the
    /// ProgressionModel calls (ScriptableObjects are not garbage collected).
    /// </summary>
    public void Release()
    {
        foreach (JobEntry entry in jobs)
        {
            if (entry.def != null)
                Object.DestroyImmediate(entry.def);
        }
    }

    // Parse every JobData JSON and mirror it into an in-memory JobDefinition.
    private void LoadJobs()
    {
        foreach (string path in SortedFiles("Assets/Resources/JobData", "*.json"))
        {
            JobDataFile data = ReadJson<JobDataFile>(path);
            if (data == null)
                continue;

            if (string.IsNullOrEmpty(data.id) || string.IsNullOrEmpty(data.abilityCatalogName))
            {
                loadErrors.Add($"Job file {Path.GetFileName(path)} is missing its id or abilityCatalogName.");
                continue;
            }

            if (data.baseStats == null || data.baseStats.Length != 7)
            {
                loadErrors.Add($"Job '{data.id}' baseStats must have exactly 7 entries (MHP, MMP, ATK, DEF, MAT, MDF, SPD).");
                continue;
            }

            var def = ScriptableObject.CreateInstance<JobDefinition>();
            def.hideFlags = HideFlags.HideAndDontSave;
            def.id = data.id;
            def.jobName = data.jobName;
            def.baseStats = data.baseStats;
            def.hpMultiplier = data.statMultipliers.hp;
            def.mpMultiplier = data.statMultipliers.mp;
            def.atkMultiplier = data.statMultipliers.atk;
            def.defMultiplier = data.statMultipliers.def;
            def.matMultiplier = data.statMultipliers.mat;
            def.mdfMultiplier = data.statMultipliers.mdf;
            def.spdMultiplier = data.statMultipliers.spd;

            jobs.Add(new JobEntry { data = data, def = def });
        }

        jobs.Sort((a, b) => string.CompareOrdinal(a.data.id, b.data.id));
    }

    // Parse every AbilityData JSON, keyed by its "job" field.
    private void LoadAbilities()
    {
        foreach (string path in SortedFiles("Assets/Resources/AbilityData", "*.json"))
        {
            AbilityAssetGenerator.AbilityDataFile data = ReadJson<AbilityAssetGenerator.AbilityDataFile>(path);
            if (data == null)
                continue;

            if (string.IsNullOrEmpty(data.job) || data.abilities == null)
                loadErrors.Add($"Ability file {Path.GetFileName(path)} is missing its job name or abilities list.");
            else if (abilityFiles.ContainsKey(data.job))
                loadErrors.Add($"Duplicate ability file for job '{data.job}' ({Path.GetFileName(path)}).");
            else
                abilityFiles.Add(data.job, data);
        }
    }

    // Parse every CatalogData JSON, keyed by catalogName.
    private void LoadCatalogs()
    {
        foreach (string path in SortedFiles("Assets/Resources/CatalogData", "*.json"))
        {
            CatalogAssetGenerator.CatalogDataFile data = ReadJson<CatalogAssetGenerator.CatalogDataFile>(path);
            if (data == null)
                continue;

            if (string.IsNullOrEmpty(data.catalogName))
                loadErrors.Add($"Catalog file {Path.GetFileName(path)} is missing its catalogName.");
            else if (catalogFiles.ContainsKey(data.catalogName))
                loadErrors.Add($"Duplicate catalog file for '{data.catalogName}' ({Path.GetFileName(path)}).");
            else
                catalogFiles.Add(data.catalogName, data);
        }
    }

    // Parse each battle definition's YAML text for recipe/level composition.
    // Text-level parsing on purpose: the generated asset pipeline is not
    // needed, and the fields read here (id, name, objective, unit slots) are
    // stable serialized names of BattleDefinition.
    private void LoadEncounters()
    {
        var recipeLine = new Regex(@"^\s*-\s*recipe:\s*(.+?)\s*$");
        var levelLine = new Regex(@"^\s*level:\s*(\d+)\s*$");
        var intField = new Regex(@"^\s{2}(\w+):\s*(-?\d+)\s*$");
        var textField = new Regex(@"^\s{2}(id|battleName):\s*(.+?)\s*$");

        foreach (string path in SortedFiles("Assets/Resources/Battles", "*.asset"))
        {
            var encounter = new Encounter();
            string side = "";
            EncounterUnit pending = null;

            foreach (string line in File.ReadAllLines(path))
            {
                if (line.StartsWith("  heroes:")) { side = "Hero"; continue; }
                if (line.StartsWith("  enemies:")) { side = "Enemy"; continue; }
                if (line.StartsWith("  waves:")) { side = "Wave"; continue; }

                Match text = textField.Match(line);
                if (text.Success)
                {
                    if (text.Groups[1].Value == "id") encounter.id = text.Groups[2].Value;
                    else encounter.battleName = text.Groups[2].Value;
                    continue;
                }

                Match number = intField.Match(line);
                if (number.Success)
                {
                    int value = int.Parse(number.Groups[2].Value);
                    if (number.Groups[1].Value == "victoryType") encounter.victoryType = value;
                    if (number.Groups[1].Value == "surviveRounds") encounter.surviveRounds = value;
                    continue;
                }

                Match recipe = recipeLine.Match(line);
                if (recipe.Success && side != "")
                {
                    pending = new EncounterUnit { side = side, recipe = recipe.Groups[1].Value, level = 1 };
                    encounter.units.Add(pending);
                    continue;
                }

                Match level = levelLine.Match(line);
                if (level.Success && pending != null)
                {
                    pending.level = int.Parse(level.Groups[1].Value);
                    pending = null;
                }
            }

            if (encounter.units.Count == 0)
                loadErrors.Add($"Battle '{Path.GetFileName(path)}' parsed no unit slots — check the asset layout.");
            else
                encounters.Add(encounter);
        }
    }

    // Deterministic file enumeration: OS listing order is not guaranteed.
    private static IEnumerable<string> SortedFiles(string directory, string pattern)
    {
        if (!Directory.Exists(directory))
            return new string[0];

        string[] files = Directory.GetFiles(directory, pattern, SearchOption.TopDirectoryOnly);
        System.Array.Sort(files, string.CompareOrdinal);
        return files;
    }

    // JsonUtility wrapper that reports malformed files instead of throwing.
    private T ReadJson<T>(string path) where T : class
    {
        try
        {
            return JsonUtility.FromJson<T>(File.ReadAllText(path));
        }
        catch (System.Exception e)
        {
            loadErrors.Add($"Failed to parse {path}: {e.Message}");
            return null;
        }
    }
}

#endif
