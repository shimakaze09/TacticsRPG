using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// The automated balance audit (issue #58, v1): turns the content JSON and the
/// live code models (ProgressionModel, StatLimits, GearCatalog) into a
/// deterministic report — job stat tables across grade paths, a basic-attack
/// damage/turns-to-KO matrix, an ability MP-efficiency and dominance audit,
/// gear pricing, and encounter composition — measured against the versioned
/// bands in BalanceConfig. Hard invariant violations (broken references, name
/// mismatches, early cap saturation, zero-power heals, absurd outliers) are
/// errors that fail the headless run; band drift is a warning that prompts
/// review. Never enters play mode and never touches generated assets; output
/// goes to BalanceReport/report.{json,md} at the repo root (gitignored).
/// Menu: Tactics RPG → Generate Balance Report. CI: -executeMethod
/// BalanceReportGenerator.RunHeadless (exit 0 = clean).
/// </summary>
public static class BalanceReportGenerator
{
    // statOrder indices (JobManager layout: MHP, MMP, ATK, DEF, MAT, MDF, SPD)
    private const int MHP = 0, MMP = 1, ATK = 2, DEF = 3, MAT = 4, MDF = 5, SPD = 6;

    #region Report schema (stable — serialized by JsonUtility)

    /// <summary>Root of report.json; field order is the schema contract.</summary>
    [System.Serializable]
    public class ReportDoc
    {
        public int configVersion;
        public int progressionModelVersion;
        public int randomSeed;
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
        public List<JobStatTable> jobStats = new List<JobStatTable>();
        public List<MatchupRow> damageMatrix = new List<MatchupRow>();
        public List<AbilityRow> abilities = new List<AbilityRow>();
        public List<ControlRow> controlRows = new List<ControlRow>();
        public List<GearRow> gear = new List<GearRow>();
        public List<EncounterSummary> encounters = new List<EncounterSummary>();
    }

    /// <summary>One job's stat curves plus its cap-saturation level.</summary>
    [System.Serializable]
    public class JobStatTable
    {
        public string jobId;
        public string jobName;
        public string category;
        public int capSaturationLevel; // 0 = never saturates by 99
        public List<StatRow> rows = new List<StatRow>();
    }

    /// <summary>Stats at one (grade path, level) sample point, no gear.</summary>
    [System.Serializable]
    public class StatRow
    {
        public string path;
        public int level;
        public int mhp, mmp, atk, def, mat, mdf, spd;
    }

    /// <summary>One attacker-vs-defender basic-attack sample.</summary>
    [System.Serializable]
    public class MatchupRow
    {
        public string attacker;
        public string defender;
        public int level;
        public int damage;
        public int turnsToKo;
        public bool inBand;
    }

    /// <summary>One ability's efficiency numbers and review flags.</summary>
    [System.Serializable]
    public class AbilityRow
    {
        public string job;
        public string id;
        public string name;
        public string powerType;
        public int power;
        public int mpCost;
        public int expectedDamage;
        public int expectedHeal;
        public float valuePerMp; // 0 when the ability is free
        public int coverage;
        public string flags;
    }

    /// <summary>One Inflict effect's accuracy data (RES contract pending #57).</summary>
    [System.Serializable]
    public class ControlRow
    {
        public string job;
        public string abilityId;
        public string status;
        public int accuracy; // 0 in JSON means the pipeline default of 100
        public int duration;
        public string hitRate;
    }

    /// <summary>One GearCatalog entry's price-per-stat-point efficiency.</summary>
    [System.Serializable]
    public class GearRow
    {
        public string id;
        public string name;
        public string slot;
        public int tier;
        public int price;
        public int statTotal;
        public float pricePerPoint; // 0 when the item grants no flat stats
        public string traits;
    }

    /// <summary>One battle definition's composition and level budget.</summary>
    [System.Serializable]
    public class EncounterSummary
    {
        public string id;
        public string battleName;
        public int victoryType;
        public int surviveRounds;
        public int heroCount;
        public int enemyCount; // includes wave spawns
        public float heroAvgLevel;
        public float enemyAvgLevel;
        public List<EncounterUnitRow> units = new List<EncounterUnitRow>();
    }

    /// <summary>One spawned unit slot inside an encounter.</summary>
    [System.Serializable]
    public class EncounterUnitRow
    {
        public string side;
        public string recipe;
        public int level;
    }

    #endregion

    #region Entry points

    /// <summary>
    /// In-editor entry: generates the report, writes both files, and surfaces
    /// hard failures as console errors (the editor keeps running).
    /// </summary>
    [MenuItem("Tactics RPG/Generate Balance Report")]
    public static void Generate()
    {
        ReportDoc doc = BuildAndWrite();
        foreach (string error in doc.errors)
            Debug.LogError($"[Balance] {error}");
        Debug.Log($"[Balance] Report written to BalanceReport/report.md — {doc.errors.Count} error(s), {doc.warnings.Count} warning(s).");
    }

    /// <summary>
    /// CI entry (same clean-checkout path as the battle probes):
    /// Unity -batchmode -nographics -projectPath . -executeMethod
    /// BalanceReportGenerator.RunHeadless — exits 1 when any hard invariant
    /// fails, 0 when the content is clean (warnings never fail the run).
    /// </summary>
    public static void RunHeadless()
    {
        try
        {
            ReportDoc doc = BuildAndWrite();
            foreach (string error in doc.errors)
                Debug.LogError($"[Balance] {error}");
            Debug.Log($"[Balance] {doc.errors.Count} error(s), {doc.warnings.Count} warning(s).");
            EditorApplication.Exit(doc.errors.Count == 0 ? 0 : 1);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Balance] Report generation threw: {e}");
            EditorApplication.Exit(1);
        }
    }

    #endregion

    #region Orchestration

    // Load content, run every section, write report.json + report.md.
    private static ReportDoc BuildAndWrite()
    {
        BalanceContent content = BalanceContent.Load();
        var doc = new ReportDoc
        {
            configVersion = BalanceConfig.Version,
            progressionModelVersion = ProgressionModel.Version,
            randomSeed = BalanceConfig.RandomSeed
        };

        try
        {
            doc.errors.AddRange(content.loadErrors);
            BuildJobStatTables(content, doc);
            BuildDamageMatrix(content, doc);
            BuildAbilityAudit(content, doc);
            BuildGearTable(doc);
            BuildEncounterSummaries(content, doc);
            ValidateReferences(content, doc);
        }
        finally
        {
            content.Release();
        }

        string outputDir = Path.Combine(Path.GetDirectoryName(Application.dataPath), "BalanceReport");
        Directory.CreateDirectory(outputDir);
        File.WriteAllText(Path.Combine(outputDir, "report.json"), JsonUtility.ToJson(doc, true));
        File.WriteAllText(Path.Combine(outputDir, "report.md"), RenderMarkdown(doc));
        return doc;
    }

    #endregion

    #region Stat model (ProgressionModel arithmetic, no gear)

    // Full stat block for one job at (level, grade) plus mastered cross-jobs,
    // floored at 1 and capped exactly like JobManager.RecalculateStats.
    private static int[] ComputeStats(JobDefinition job, int level, int grade,
        List<BalanceContent.JobEntry> crossJobs, bool applyCaps)
    {
        var stats = new int[7];
        for (int i = 0; i < 7; i++)
        {
            int value = ProgressionModel.CurrentJobContribution(job, i, grade)
                        + ProgressionModel.LevelGrowthBonus(job, i, level);

            if (crossJobs != null)
            {
                foreach (BalanceContent.JobEntry cross in crossJobs)
                    value += ProgressionModel.CrossJobContribution(cross.def, i, ProgressionModel.MaxGrade);
            }

            value = Mathf.Max(1, value);
            if (applyCaps)
            {
                value = i switch
                {
                    MHP => Mathf.Min(value, StatLimits.MaxHP),
                    MMP => Mathf.Min(value, StatLimits.MaxMP),
                    _ => Mathf.Min(value, StatLimits.MaxPrimaryStat)
                };
            }

            stats[i] = value;
        }

        return stats;
    }

    // The generally accessible roster (Basic + Common categories) — the pool
    // cross-training paths and roster medians draw from.
    private static List<BalanceContent.JobEntry> CommonRoster(BalanceContent content)
    {
        var roster = new List<BalanceContent.JobEntry>();
        foreach (BalanceContent.JobEntry entry in content.jobs)
        {
            string category = (entry.data.category ?? "").ToLowerInvariant();
            if (category == "basic" || category == "common")
                roster.Add(entry);
        }

        return roster;
    }

    // Per-job stat tables over the four grade paths, plus cap saturation.
    private static void BuildJobStatTables(BalanceContent content, ReportDoc doc)
    {
        List<BalanceContent.JobEntry> roster = CommonRoster(content);

        foreach (BalanceContent.JobEntry entry in content.jobs)
        {
            var table = new JobStatTable
            {
                jobId = entry.data.id,
                jobName = entry.data.jobName,
                category = entry.data.category,
                capSaturationLevel = FindCapSaturationLevel(entry.def)
            };

            var cross3 = new List<BalanceContent.JobEntry>();
            var crossAll = new List<BalanceContent.JobEntry>();
            foreach (BalanceContent.JobEntry other in roster)
            {
                if (other.data.id == entry.data.id)
                    continue;
                if (cross3.Count < BalanceConfig.CrossJobSampleCount)
                    cross3.Add(other);
                crossAll.Add(other);
            }

            foreach (int level in BalanceConfig.StatTableLevels)
            {
                AddStatRow(table, entry.def, "grade1", level, 1, null);
                AddStatRow(table, entry.def, "grade8", level, ProgressionModel.MaxGrade, null);
                AddStatRow(table, entry.def, "grade8+cross3", level, ProgressionModel.MaxGrade, cross3);
                AddStatRow(table, entry.def, "completionist", level, ProgressionModel.MaxGrade, crossAll);
            }

            doc.jobStats.Add(table);

            int hp1 = table.rows[0].mhp;
            if (hp1 < BalanceConfig.Level1HpMin || hp1 > BalanceConfig.Level1HpMax)
                doc.warnings.Add($"{entry.data.id}: level-1 HP {hp1} outside {BalanceConfig.Level1HpMin}–{BalanceConfig.Level1HpMax} band.");

            if (table.capSaturationLevel > 0 && table.capSaturationLevel < BalanceConfig.CapSaturationMinLevel)
                doc.errors.Add($"{entry.data.id}: primary stat hits {StatLimits.MaxPrimaryStat} at level {table.capSaturationLevel} (single job, grade 8) — before the level-{BalanceConfig.CapSaturationMinLevel} floor.");
        }
    }

    // One sample row for a table.
    private static void AddStatRow(JobStatTable table, JobDefinition def, string path,
        int level, int grade, List<BalanceContent.JobEntry> cross)
    {
        int[] s = ComputeStats(def, level, grade, cross, true);
        table.rows.Add(new StatRow
        {
            path = path, level = level,
            mhp = s[MHP], mmp = s[MMP], atk = s[ATK], def = s[DEF], mat = s[MAT], mdf = s[MDF], spd = s[SPD]
        });
    }

    // Lowest level (2–99) where any uncapped primary stat reaches the 999
    // ceiling on the single-job grade-8 path; 0 when it never happens.
    private static int FindCapSaturationLevel(JobDefinition def)
    {
        for (int level = 2; level <= 99; level++)
        {
            int[] s = ComputeStats(def, level, ProgressionModel.MaxGrade, null, false);
            for (int i = ATK; i <= SPD; i++)
            {
                if (s[i] >= StatLimits.MaxPrimaryStat)
                    return level;
            }
        }

        return 0;
    }

    #endregion

    #region Damage / turns-to-KO matrix

    // Every job's basic attack (power 100, no gear) against every job's
    // level-matched DEF and HP; grade 1 at level 1, mastered above.
    private static void BuildDamageMatrix(BalanceContent content, ReportDoc doc)
    {
        foreach (int level in BalanceConfig.MatrixLevels)
        {
            int grade = level <= 1 ? 1 : ProgressionModel.MaxGrade;
            int outOfBand = 0;

            foreach (BalanceContent.JobEntry attacker in content.jobs)
            {
                int[] attackerStats = ComputeStats(attacker.def, level, grade, null, true);
                foreach (BalanceContent.JobEntry defender in content.jobs)
                {
                    int[] defenderStats = ComputeStats(defender.def, level, grade, null, true);
                    int damage = BasicDamage(attackerStats[ATK], defenderStats[DEF]);
                    int ttk = TurnsToKo(defenderStats[MHP], damage);
                    bool inBand = ttk >= BalanceConfig.TurnsToKoMin && ttk <= BalanceConfig.TurnsToKoMax;
                    if (!inBand)
                        outOfBand++;

                    doc.damageMatrix.Add(new MatchupRow
                    {
                        attacker = attacker.data.id,
                        defender = defender.data.id,
                        level = level,
                        damage = damage,
                        turnsToKo = ttk,
                        inBand = inBand
                    });
                }
            }

            int total = content.jobs.Count * content.jobs.Count;
            if (outOfBand > 0)
                doc.warnings.Add($"Damage matrix level {level}: {outOfBand}/{total} matchups outside the {BalanceConfig.TurnsToKoMin}–{BalanceConfig.TurnsToKoMax} turns-to-KO band.");
        }
    }

    // The settled formula, integer math exactly as DamageAbilityEffect runs it.
    private static int BasicDamage(int attack, int defense)
    {
        int damage = attack * BalanceConfig.BasicAttackPower / 100 - defense / 2;
        return Mathf.Clamp(Mathf.Max(damage, 1), 1, StatLimits.MaxDamagePerHit);
    }

    // Basic attacks needed to empty an HP pool.
    private static int TurnsToKo(int hp, int damage)
    {
        return (hp + damage - 1) / damage;
    }

    #endregion

    #region Ability audit

    // Per ability: expected damage/heal at the Chapter 1 reference point,
    // MP efficiency, range+area coverage, and dominance/outlier flags.
    private static void BuildAbilityAudit(BalanceContent content, ReportDoc doc)
    {
        List<BalanceContent.JobEntry> roster = CommonRoster(content);
        int level = BalanceConfig.AbilityAuditLevel;
        int grade = BalanceConfig.AbilityAuditGrade;

        var rosterStats = new List<int[]>();
        foreach (BalanceContent.JobEntry entry in roster)
            rosterStats.Add(ComputeStats(entry.def, level, grade, null, true));

        int medianDef = Median(rosterStats, DEF);
        int medianMdf = Median(rosterStats, MDF);
        int medianAtk = Median(rosterStats, ATK);
        int medianMat = Median(rosterStats, MAT);

        var sortedJobs = new List<string>(content.abilityFiles.Keys);
        sortedJobs.Sort(string.CompareOrdinal);

        foreach (string jobName in sortedJobs)
        {
            AbilityAssetGenerator.AbilityDataFile file = content.abilityFiles[jobName];
            BalanceContent.JobEntry owner = FindJobByCatalogName(content, jobName);
            int[] attackerStats = owner != null
                ? ComputeStats(owner.def, level, grade, null, true)
                : null;

            var rows = new List<AbilityRow>();
            foreach (AbilityAssetGenerator.AbilityData ability in file.abilities)
            {
                int atkStat = attackerStats != null ? attackerStats[ATK] : medianAtk;
                int matStat = attackerStats != null ? attackerStats[MAT] : medianMat;
                rows.Add(AuditAbility(jobName, ability, atkStat, matStat, medianDef, medianMdf, doc));
            }

            FlagDominatedOptions(rows, doc);
            doc.abilities.AddRange(rows);
        }
    }

    // Numbers and validation for a single ability entry.
    private static AbilityRow AuditAbility(string jobName, AbilityAssetGenerator.AbilityData ability,
        int atkStat, int matStat, int medianDef, int medianMdf, ReportDoc doc)
    {
        bool magical = ability.power != null && ability.power.type == "Magical";
        int power = ability.power != null ? ability.power.value : 0;
        var flags = new List<string>();

        var row = new AbilityRow
        {
            job = jobName,
            id = ability.id,
            name = ability.name,
            powerType = ability.power != null && !string.IsNullOrEmpty(ability.power.type) ? ability.power.type : "None",
            power = power,
            mpCost = ability.mpCost,
            coverage = CoverageScore(ability)
        };

        if (power < 0)
            doc.errors.Add($"{ability.id}: negative power {power}.");

        bool hasDamage = false, hasHeal = false;
        foreach (AbilityAssetGenerator.EffectData effect in ability.effects ?? new List<AbilityAssetGenerator.EffectData>())
        {
            if (effect.type == "Damage") hasDamage = true;
            if (effect.type == "Heal") hasHeal = true;
            if (effect.type == "Inflict" && !string.IsNullOrEmpty(effect.status))
            {
                doc.controlRows.Add(new ControlRow
                {
                    job = jobName,
                    abilityId = ability.id,
                    status = effect.status,
                    accuracy = effect.accuracy,
                    duration = effect.duration,
                    hitRate = effect.hitRate
                });
            }
        }

        if (hasDamage)
        {
            int attack = magical ? matStat : atkStat;
            int defense = magical ? medianMdf : medianDef;
            int damage = attack * power / 100 - defense / 2;
            row.expectedDamage = Mathf.Clamp(Mathf.Max(damage, 1), 1, StatLimits.MaxDamagePerHit);

            if (power <= 0)
                doc.errors.Add($"{ability.id}: Damage effect with zero power.");
            if (row.expectedDamage > BalanceConfig.OutlierMultiplier * BalanceConfig.CapstoneDamageMax)
                doc.errors.Add($"{ability.id}: expected damage {row.expectedDamage} exceeds {BalanceConfig.OutlierMultiplier}× the capstone band top — data outlier.");
            if (ability.mpCost == 0 && power > BalanceConfig.FreeDamagePowerWarnThreshold)
            {
                flags.Add("free-power");
                doc.warnings.Add($"{ability.id}: zero-MP damage at power {power} (> {BalanceConfig.FreeDamagePowerWarnThreshold}) undercuts the MP economy.");
            }
        }

        if (hasHeal)
        {
            row.expectedHeal = power;
            if (power <= 0)
                doc.errors.Add($"{ability.id}: Heal effect with zero power — the heal does nothing.");
            else if (power > BalanceConfig.OutlierMultiplier * BalanceConfig.HealPowerMax)
                doc.errors.Add($"{ability.id}: heal power {power} exceeds {BalanceConfig.OutlierMultiplier}× the heal band top — data outlier.");
            else if (power < BalanceConfig.HealPowerMin || power > BalanceConfig.HealPowerMax)
            {
                flags.Add("heal-out-of-band");
                doc.warnings.Add($"{ability.id}: heal power {power} outside the {BalanceConfig.HealPowerMin}–{BalanceConfig.HealPowerMax} band.");
            }
        }

        if (ability.mpCost > 0)
        {
            int value = Mathf.Max(row.expectedDamage, row.expectedHeal);
            row.valuePerMp = Mathf.Round(value * 100f / ability.mpCost) / 100f;
        }

        row.flags = string.Join(",", flags);
        return row;
    }

    // A dominated option: costs MP yet a zero-cost same-type damage ability in
    // the same job matches or beats its power and coverage.
    private static void FlagDominatedOptions(List<AbilityRow> rows, ReportDoc doc)
    {
        for (int i = 0; i < rows.Count; i++)
        {
            AbilityRow candidate = rows[i];
            if (candidate.mpCost <= 0 || candidate.expectedDamage <= 0)
                continue;

            for (int j = 0; j < rows.Count; j++)
            {
                AbilityRow free = rows[j];
                if (i == j || free.mpCost != 0 || free.expectedDamage <= 0)
                    continue;
                if (free.powerType != candidate.powerType)
                    continue;

                if (free.power >= candidate.power && free.coverage >= candidate.coverage)
                {
                    candidate.flags = string.IsNullOrEmpty(candidate.flags)
                        ? $"dominated-by:{free.id}"
                        : candidate.flags + $",dominated-by:{free.id}";
                    doc.warnings.Add($"{candidate.id}: dominated by zero-cost {free.id} (power {free.power} ≥ {candidate.power}, coverage {free.coverage} ≥ {candidate.coverage}).");
                    break;
                }
            }
        }
    }

    // Coverage score = reach + splash: Constant/Line reach in tiles (Infinite
    // and battlefield areas count as 99), Specify splash as extra diamond
    // tiles (2r(r+1)), Weapon reach as 1 (actual reach is gear-dependent).
    private static int CoverageScore(AbilityAssetGenerator.AbilityData ability)
    {
        int score = 0;

        if (ability.range != null)
        {
            switch (ability.range.type)
            {
                case "Constant":
                case "Line":
                    score += ability.range.value;
                    break;
                case "Infinite":
                    score += 99;
                    break;
                case "Weapon":
                    score += 1;
                    break;
            }
        }

        if (ability.area != null)
        {
            switch (ability.area.type)
            {
                case "Specify":
                    int radius = Mathf.Max(ability.area.value, 1);
                    score += 2 * radius * (radius + 1);
                    break;
                case "Full":
                    score += 99;
                    break;
            }
        }

        return score;
    }

    // Median of one stat slot across precomputed roster stat blocks.
    private static int Median(List<int[]> statBlocks, int statIndex)
    {
        var values = new List<int>();
        foreach (int[] block in statBlocks)
            values.Add(block[statIndex]);
        values.Sort();
        return values.Count == 0 ? 0 : values[values.Count / 2];
    }

    // The job whose abilityCatalogName matches an ability file's job field
    // (null for shared files like Common).
    private static BalanceContent.JobEntry FindJobByCatalogName(BalanceContent content, string catalogName)
    {
        foreach (BalanceContent.JobEntry entry in content.jobs)
        {
            if (entry.data.abilityCatalogName == catalogName)
                return entry;
        }

        return null;
    }

    #endregion

    #region Gear table

    // Price-per-stat-point view of the GearCatalog (informational in v1;
    // trait valuation and upgrade deltas stay with #58).
    private static void BuildGearTable(ReportDoc doc)
    {
        var rows = new List<GearRow>();
        foreach (GearData item in GearCatalog.All)
        {
            int statTotal = item.amount1 + item.amount2;
            var traits = new List<string>();
            if (item.traits != null)
            {
                foreach (GearTraitData trait in item.traits)
                    traits.Add(string.IsNullOrEmpty(trait.tag) ? $"{trait.type}:{trait.value}" : $"{trait.type}:{trait.value}:{trait.tag}");
            }

            rows.Add(new GearRow
            {
                id = item.id,
                name = item.name,
                slot = item.slot.ToString(),
                tier = item.tier,
                price = item.price,
                statTotal = statTotal,
                pricePerPoint = statTotal > 0 ? Mathf.Round(item.price * 100f / statTotal) / 100f : 0f,
                traits = string.Join(",", traits)
            });
        }

        rows.Sort((a, b) => string.CompareOrdinal(a.id, b.id));
        doc.gear.AddRange(rows);
    }

    #endregion

    #region Encounters

    // Composition and level budget per battle definition, with a lopsided-
    // level warning; stat budgets and reward projections stay with #58.
    private static void BuildEncounterSummaries(BalanceContent content, ReportDoc doc)
    {
        foreach (BalanceContent.Encounter encounter in content.encounters)
        {
            var summary = new EncounterSummary
            {
                id = encounter.id,
                battleName = encounter.battleName,
                victoryType = encounter.victoryType,
                surviveRounds = encounter.surviveRounds
            };

            int heroLevels = 0, enemyLevels = 0;
            foreach (BalanceContent.EncounterUnit unit in encounter.units)
            {
                summary.units.Add(new EncounterUnitRow { side = unit.side, recipe = unit.recipe, level = unit.level });
                if (unit.side == "Hero")
                {
                    summary.heroCount++;
                    heroLevels += unit.level;
                }
                else
                {
                    summary.enemyCount++;
                    enemyLevels += unit.level;
                }
            }

            summary.heroAvgLevel = summary.heroCount > 0 ? Mathf.Round(heroLevels * 100f / summary.heroCount) / 100f : 0f;
            summary.enemyAvgLevel = summary.enemyCount > 0 ? Mathf.Round(enemyLevels * 100f / summary.enemyCount) / 100f : 0f;

            if (Mathf.Abs(summary.heroAvgLevel - summary.enemyAvgLevel) > BalanceConfig.EncounterLevelDeltaTolerance)
                doc.warnings.Add($"{encounter.id}: hero/enemy average level gap {summary.heroAvgLevel} vs {summary.enemyAvgLevel} exceeds ±{BalanceConfig.EncounterLevelDeltaTolerance}.");

            doc.encounters.Add(summary);
        }
    }

    #endregion

    #region Reference validation (WORLD §5 invariants)

    // Cross-file contract checks: unlock ids resolve, unlock display names
    // match, and job ↔ ability file ↔ catalog stay in lockstep.
    private static void ValidateReferences(BalanceContent content, ReportDoc doc)
    {
        var abilityNamesById = new Dictionary<string, string>();
        foreach (AbilityAssetGenerator.AbilityDataFile file in content.abilityFiles.Values)
        {
            foreach (AbilityAssetGenerator.AbilityData ability in file.abilities)
            {
                if (string.IsNullOrEmpty(ability.id))
                    doc.errors.Add($"Ability '{ability.name}' in file '{file.job}' has no stable id.");
                else if (abilityNamesById.ContainsKey(ability.id))
                    doc.errors.Add($"Duplicate ability id '{ability.id}'.");
                else
                    abilityNamesById.Add(ability.id, ability.name);
            }
        }

        foreach (BalanceContent.JobEntry entry in content.jobs)
        {
            string jobId = entry.data.id;
            string catalogName = entry.data.abilityCatalogName;

            if (!content.catalogFiles.ContainsKey(catalogName))
                doc.errors.Add($"{jobId}: abilityCatalogName '{catalogName}' has no CatalogData file.");
            if (!content.abilityFiles.ContainsKey(catalogName))
                doc.errors.Add($"{jobId}: abilityCatalogName '{catalogName}' has no AbilityData file.");

            var referenced = new HashSet<string>();
            foreach (JobAbilityUnlockData unlock in entry.data.abilityUnlocks ?? new JobAbilityUnlockData[0])
            {
                if (string.IsNullOrEmpty(unlock.abilityId))
                {
                    doc.errors.Add($"{jobId}: abilityUnlocks entry '{unlock.abilityName}' has no ability id.");
                    continue;
                }

                referenced.Add(unlock.abilityId);
                if (!abilityNamesById.TryGetValue(unlock.abilityId, out string realName))
                    doc.errors.Add($"{jobId}: abilityUnlocks references missing ability id '{unlock.abilityId}'.");
                else if (realName != unlock.abilityName)
                    doc.errors.Add($"{jobId}: unlock '{unlock.abilityId}' names it '{unlock.abilityName}' but AbilityData says '{realName}'.");
            }

            if (content.abilityFiles.TryGetValue(catalogName, out AbilityAssetGenerator.AbilityDataFile abilityFile) &&
                content.catalogFiles.TryGetValue(catalogName, out CatalogAssetGenerator.CatalogDataFile catalog))
            {
                var abilityNames = new HashSet<string>();
                foreach (AbilityAssetGenerator.AbilityData ability in abilityFile.abilities)
                {
                    abilityNames.Add(ability.name);
                    if (!referenced.Contains(ability.id))
                        doc.warnings.Add($"{jobId}: ability '{ability.id}' is never unlockable in the job's level 1–8 spread.");
                }

                var catalogEntries = new HashSet<string>();
                if (catalog.categories != null)
                {
                    foreach (CatalogAssetGenerator.CategoryData category in catalog.categories)
                        catalogEntries.UnionWith(category.entries ?? new string[0]);
                }

                if (!abilityNames.SetEquals(catalogEntries))
                    doc.errors.Add($"{jobId}: catalog '{catalogName}' entries do not match AbilityData names (catalogs are generated, never hand-edited).");
            }
        }

        foreach (string catalogName in content.catalogFiles.Keys)
        {
            if (catalogName != "Common" && FindJobByCatalogName(content, catalogName) == null)
                doc.warnings.Add($"Catalog '{catalogName}' is not referenced by any job.");
        }
    }

    #endregion

    #region Markdown rendering

    // Human summary of the same data (report.json is the machine contract).
    // Deterministic apart from the single generated-at header line.
    private static string RenderMarkdown(ReportDoc doc)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Balance report");
        sb.AppendLine();
        sb.AppendLine($"Generated: {System.DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
        sb.AppendLine($"Config v{doc.configVersion} · ProgressionModel v{doc.progressionModelVersion} · seed {doc.randomSeed} (no sampling in v1) · no gear unless stated");
        sb.AppendLine();
        sb.AppendLine($"**{doc.errors.Count} error(s)** (fail CI) · {doc.warnings.Count} warning(s) (review prompts)");
        sb.AppendLine();

        RenderIssueList(sb, "Errors", doc.errors);
        RenderIssueList(sb, "Warnings", doc.warnings);
        RenderJobStats(sb, doc);
        RenderMatrix(sb, doc);
        RenderAbilities(sb, doc);
        RenderControlRows(sb, doc);
        RenderGear(sb, doc);
        RenderEncounters(sb, doc);
        return sb.ToString();
    }

    // Bullet list of errors or warnings.
    private static void RenderIssueList(StringBuilder sb, string title, List<string> items)
    {
        sb.AppendLine($"## {title}");
        sb.AppendLine();
        if (items.Count == 0)
        {
            sb.AppendLine("None.");
        }
        else
        {
            foreach (string item in items)
                sb.AppendLine($"- {item}");
        }

        sb.AppendLine();
    }

    // Per-job stat tables (also the #54 golden tables).
    private static void RenderJobStats(StringBuilder sb, ReportDoc doc)
    {
        sb.AppendLine("## Job stat tables");
        sb.AppendLine();
        sb.AppendLine("Paths: grade1 (fresh certification), grade8 (mastered, single job), grade8+cross3 (+ first three other Basic/Common jobs mastered), completionist (all Basic/Common jobs mastered).");
        sb.AppendLine();

        foreach (JobStatTable table in doc.jobStats)
        {
            string saturation = table.capSaturationLevel > 0
                ? $"primary stat caps at level {table.capSaturationLevel}"
                : "no primary-stat cap saturation by 99";
            sb.AppendLine($"### {table.jobId} — {table.jobName} ({table.category}; {saturation})");
            sb.AppendLine();
            sb.AppendLine("| Path | Lvl | MHP | MMP | ATK | DEF | MAT | MDF | SPD |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|---|");
            foreach (StatRow row in table.rows)
                sb.AppendLine($"| {row.path} | {row.level} | {row.mhp} | {row.mmp} | {row.atk} | {row.def} | {row.mat} | {row.mdf} | {row.spd} |");
            sb.AppendLine();
        }
    }

    // Damage/TTK grids, one per sampled level.
    private static void RenderMatrix(StringBuilder sb, ReportDoc doc)
    {
        sb.AppendLine("## Damage / turns-to-KO matrix");
        sb.AppendLine();
        sb.AppendLine($"Basic attack at power {BalanceConfig.BasicAttackPower}, no gear; cells are damage/turns-to-KO, ! marks TTK outside {BalanceConfig.TurnsToKoMin}–{BalanceConfig.TurnsToKoMax}.");
        sb.AppendLine();

        foreach (int level in BalanceConfig.MatrixLevels)
        {
            var defenders = new List<string>();
            var byAttacker = new Dictionary<string, List<MatchupRow>>();
            foreach (MatchupRow row in doc.damageMatrix)
            {
                if (row.level != level)
                    continue;
                if (!byAttacker.TryGetValue(row.attacker, out List<MatchupRow> list))
                {
                    list = new List<MatchupRow>();
                    byAttacker.Add(row.attacker, list);
                    if (!defenders.Contains(row.attacker))
                        defenders.Add(row.attacker);
                }

                list.Add(row);
            }

            sb.AppendLine($"### Level {level} ({(level <= 1 ? "grade 1" : "grade 8")})");
            sb.AppendLine();
            sb.Append("| atk \\ def |");
            foreach (string defender in defenders)
                sb.Append($" {defender} |");
            sb.AppendLine();
            sb.Append("|---|");
            foreach (string _ in defenders)
                sb.Append("---|");
            sb.AppendLine();

            foreach (string attacker in defenders)
            {
                sb.Append($"| {attacker} |");
                foreach (MatchupRow row in byAttacker[attacker])
                    sb.Append($" {row.damage}/{row.turnsToKo}{(row.inBand ? "" : "!")} |");
                sb.AppendLine();
            }

            sb.AppendLine();
        }
    }

    // Ability efficiency table.
    private static void RenderAbilities(StringBuilder sb, ReportDoc doc)
    {
        sb.AppendLine("## Ability audit");
        sb.AppendLine();
        sb.AppendLine($"Evaluated at level {BalanceConfig.AbilityAuditLevel}, grade {BalanceConfig.AbilityAuditGrade}, versus the Basic/Common roster median defense. Coverage = reach + splash tiles.");
        sb.AppendLine();
        sb.AppendLine("| Job | Ability | Type | Power | MP | Exp. dmg | Exp. heal | Per MP | Coverage | Flags |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|");
        foreach (AbilityRow row in doc.abilities)
            sb.AppendLine($"| {row.job} | {row.name} | {row.powerType} | {row.power} | {row.mpCost} | {row.expectedDamage} | {row.expectedHeal} | {Fmt(row.valuePerMp)} | {row.coverage} | {row.flags} |");
        sb.AppendLine();
    }

    // Status accuracy rows from ability JSON only.
    private static void RenderControlRows(StringBuilder sb, ReportDoc doc)
    {
        sb.AppendLine("## Control effects (RES contract pending #57)");
        sb.AppendLine();
        sb.AppendLine("Accuracy is the raw JSON value before evade/resist (0 = pipeline default 100). Resist-side math is out of scope until #57 lands.");
        sb.AppendLine();
        sb.AppendLine("| Job | Ability | Status | Accuracy | Duration | Hit rate |");
        sb.AppendLine("|---|---|---|---|---|---|");
        foreach (ControlRow row in doc.controlRows)
            sb.AppendLine($"| {row.job} | {row.abilityId} | {row.status} | {(row.accuracy > 0 ? row.accuracy : 100)} | {row.duration} | {row.hitRate} |");
        sb.AppendLine();
    }

    // Gear pricing table.
    private static void RenderGear(StringBuilder sb, ReportDoc doc)
    {
        sb.AppendLine("## Gear catalog");
        sb.AppendLine();
        sb.AppendLine("| Id | Name | Slot | Tier | Price | Stat pts | Price/pt | Traits |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|");
        foreach (GearRow row in doc.gear)
            sb.AppendLine($"| {row.id} | {row.name} | {row.slot} | {row.tier} | {row.price} | {row.statTotal} | {Fmt(row.pricePerPoint)} | {row.traits} |");
        sb.AppendLine();
    }

    // Encounter composition tables.
    private static void RenderEncounters(StringBuilder sb, ReportDoc doc)
    {
        sb.AppendLine("## Encounters");
        sb.AppendLine();
        sb.AppendLine("Hard mode adds ×1.3 enemy MHP and ×1.2 enemy outgoing damage on top of these levels (DifficultySettings).");
        sb.AppendLine();

        foreach (EncounterSummary encounter in doc.encounters)
        {
            sb.AppendLine($"### {encounter.id} — {encounter.battleName}");
            sb.AppendLine();
            sb.AppendLine($"Victory type {encounter.victoryType}, survive rounds {encounter.surviveRounds}. Heroes {encounter.heroCount} (avg level {Fmt(encounter.heroAvgLevel)}) vs enemies {encounter.enemyCount} incl. waves (avg level {Fmt(encounter.enemyAvgLevel)}).");
            sb.AppendLine();
            sb.AppendLine("| Side | Recipe | Level |");
            sb.AppendLine("|---|---|---|");
            foreach (EncounterUnitRow unit in encounter.units)
                sb.AppendLine($"| {unit.side} | {unit.recipe} | {unit.level} |");
            sb.AppendLine();
        }
    }

    // Culture-invariant float formatting for tables.
    private static string Fmt(float value)
    {
        return value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
    }

    #endregion
}

#endif
