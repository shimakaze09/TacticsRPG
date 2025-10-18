using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Editor utility to create complete FFT job definition assets (all 20 generic + special + unique jobs)
/// </summary>
public static class FFTJobCreator
{
    private const string JobsPath = "Assets/Resources/Jobs";

    [MenuItem("Tactics RPG/Create FFT Jobs/Create All Basic Jobs")]
    public static void CreateAllBasicJobs()
    {
        CreateSquireJob();
        CreateChemistJob();
        Debug.Log("Created basic FFT jobs: Squire, Chemist");
    }

    [MenuItem("Tactics RPG/Create FFT Jobs/Create All Common Jobs")]
    public static void CreateAllCommonJobs()
    {
        CreateKnightJob();
        CreateArcherJob();
        CreatePriestJob();
        CreateWizardJob();
        CreateMonkJob();
        CreateThiefJob();
        CreateMysticJob();
        CreateTimeMageJob();
        CreateSummonerJob();
        CreateGeomancerJob();
        CreateDragoonJob();
        CreateOraclJob();
        Debug.Log("Created common FFT jobs: Knight, Archer, Priest, Wizard, Monk, Thief, Mystic, Time Mage, Summoner, Geomancer, Dragoon, Oracl");
    }

    [MenuItem("Tactics RPG/Create FFT Jobs/Create All Special Jobs")]
    public static void CreateAllSpecialJobs()
    {
        CreateNinjaJob();
        CreateSamuraiJob();
        CreateLancerJob();
        CreateMediatorJob();
        CreateCalculatorJob();
        CreateBardJob();
        CreateDancerJob();
        CreateMimeJob();
        Debug.Log("Created special FFT jobs: Ninja, Samurai, Lancer, Mediator, Calculator, Bard, Dancer, Mime");
    }

    [MenuItem("Tactics RPG/Create FFT Jobs/Create All Unique Jobs")]
    public static void CreateAllUniqueJobs()
    {
        CreateDarkKnightJob();
        CreateHolyKnightJob();
        CreateHeavenKnightJob();
        CreateTempleKnightJob();
        CreateArcKnightJob();
        CreateHolySwordJob();
        CreatePrincessJob();
        CreateClericJob();
        CreateSkyPirateJob();
        CreateMachinistJob();
        Debug.Log("Created unique FFT jobs: Dark Knight, Holy Knight, Heaven Knight, Temple Knight, Arc Knight, Holy Sword, Princess, Cleric, Sky Pirate, Machinist");
    }

    [MenuItem("Tactics RPG/Create FFT Jobs/Create All Jobs (Fresh)")]
    public static void CreateAllJobsFresh()
    {
        EnsureJobsDirectoryExists();
        
        // Delete all existing job assets first
        DeleteAllExistingJobs();
        
        // Create all jobs fresh
        CreateAllBasicJobs();
        CreateAllCommonJobs();
        CreateAllSpecialJobs();
        CreateAllUniqueJobs();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created all FFT job definitions fresh (30 jobs total)!");
    }

    private static void DeleteAllExistingJobs()
    {
        // Get all .asset files in the Jobs folder
        string[] jobFiles = System.IO.Directory.GetFiles(JobsPath, "*.asset", System.IO.SearchOption.TopDirectoryOnly);
        
        foreach (string file in jobFiles)
        {
            string relativePath = "Assets" + file.Substring(Application.dataPath.Length);
            AssetDatabase.DeleteAsset(relativePath);
            Debug.Log($"Deleted existing job: {relativePath}");
        }
        
        Debug.Log($"Deleted {jobFiles.Length} existing job assets");
    }

    private static void EnsureJobsDirectoryExists()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        if (!AssetDatabase.IsValidFolder(JobsPath))
            AssetDatabase.CreateFolder("Assets/Resources", "Jobs");
    }

    // BASIC JOBS
    private static void CreateSquireJob()
    {
        var job = CreateJobAsset("Squire");
        job.jobName = "Squire";
        job.description = "Basic soldier class. Well-rounded stats and abilities.";
        job.category = JobCategory.Basic;
        job.minimumCharacterLevel = 1;

        // Balanced stat multipliers
        job.hpMultiplier = 1.3f;
        job.mpMultiplier = 0.9f;
        job.atkMultiplier = 1.0f;
        job.defMultiplier = 1.0f;
        job.matMultiplier = 0.8f;
        job.mdfMultiplier = 0.9f;
        job.spdMultiplier = 1.0f;

        // Base stats
        job.baseStats = new int[] { 50, 20, 5, 5, 3, 4, 5 }; // HP, MP, ATK, DEF, MAT, MDF, SPD

        // Movement
        job.movementBonus = 4;
        job.jumpBonus = 3;
        job.evadeBonus = 10;

        // No prerequisites (starting job)
        job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>();

        // Ability unlocks with proper JP costs
        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Attack", unlockAtJobLevel = 1, jpCost = 100 },
            new JobAbilityUnlock { abilityName = "Cure", unlockAtJobLevel = 2, jpCost = 200 },
            new JobAbilityUnlock { abilityName = "Water", unlockAtJobLevel = 3, jpCost = 300 },
            new JobAbilityUnlock { abilityName = "Defend", unlockAtJobLevel = 4, jpCost = 400 },
            new JobAbilityUnlock { abilityName = "Raise", unlockAtJobLevel = 5, jpCost = 500 },
            new JobAbilityUnlock { abilityName = "Blind", unlockAtJobLevel = 6, jpCost = 600 },
            new JobAbilityUnlock { abilityName = "Holy", unlockAtJobLevel = 7, jpCost = 700 },
            new JobAbilityUnlock { abilityName = "Esuna", unlockAtJobLevel = 8, jpCost = 800 }
        };

        // Set ability catalog
        job.abilityCatalogName = "DemoCatalog";

        EditorUtility.SetDirty(job);
    }

    private static void CreateChemistJob()
    {
        var job = CreateJobAsset("Chemist");
        job.jobName = "Chemist";
        job.description = "Item specialist. Can use items effectively and has potion-related abilities.";
        job.category = JobCategory.Basic;
        job.minimumCharacterLevel = 1;

        // Item-focused multipliers
        job.hpMultiplier = 1.1f;
        job.mpMultiplier = 1.2f;
        job.atkMultiplier = 0.8f;
        job.defMultiplier = 0.9f;
        job.matMultiplier = 1.0f;
        job.mdfMultiplier = 1.0f;
        job.spdMultiplier = 0.9f;

        job.baseStats = new int[] { 45, 30, 4, 4, 5, 5, 4 };

        job.movementBonus = 3;
        job.jumpBonus = 3;
        job.evadeBonus = 5;

        job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>();

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Cure", unlockAtJobLevel = 1, jpCost = 100 },
            new JobAbilityUnlock { abilityName = "Raise", unlockAtJobLevel = 2, jpCost = 200 },
            new JobAbilityUnlock { abilityName = "Drain", unlockAtJobLevel = 3, jpCost = 300 },
            new JobAbilityUnlock { abilityName = "Slience", unlockAtJobLevel = 4, jpCost = 400 },
            new JobAbilityUnlock { abilityName = "Holy", unlockAtJobLevel = 5, jpCost = 500 },
            new JobAbilityUnlock { abilityName = "Water", unlockAtJobLevel = 6, jpCost = 600 },
            new JobAbilityUnlock { abilityName = "Blind", unlockAtJobLevel = 7, jpCost = 700 },
            new JobAbilityUnlock { abilityName = "Esuna", unlockAtJobLevel = 8, jpCost = 800 }
        };

        // Set ability catalog
        job.abilityCatalogName = "DemoCatalog";

        EditorUtility.SetDirty(job);
    }

    // COMMON JOBS
    private static void CreateKnightJob()
    {
        var job = CreateJobAsset("Knight");
        var squire = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Squire.asset");

        job.jobName = "Knight";
        job.description = "Heavy armor specialist with high HP and physical attack. Uses swords and armor break abilities.";
        job.category = JobCategory.Common;
        job.minimumCharacterLevel = 1;

        // High physical stats
        job.hpMultiplier = 1.8f;
        job.mpMultiplier = 0.7f;
        job.atkMultiplier = 1.4f;
        job.defMultiplier = 1.5f;
        job.matMultiplier = 0.6f;
        job.mdfMultiplier = 0.8f;
        job.spdMultiplier = 0.8f;

        job.baseStats = new int[] { 60, 15, 7, 7, 3, 4, 4 };

        job.movementBonus = 3;
        job.jumpBonus = 3;
        job.evadeBonus = 5;

        // Prerequisites: Squire Lv2
        if (squire != null)
        {
            job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>
            {
                new JobPrerequisite { requiredJob = squire, requiredLevel = 2 }
            };
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Air Blast", unlockAtJobLevel = 1, jpCost = 100 },
            new JobAbilityUnlock { abilityName = "Attack", unlockAtJobLevel = 2, jpCost = 200 },
            new JobAbilityUnlock { abilityName = "Defend", unlockAtJobLevel = 3, jpCost = 300 },
            new JobAbilityUnlock { abilityName = "First Aid", unlockAtJobLevel = 4, jpCost = 400 },
            new JobAbilityUnlock { abilityName = "Rend Helm", unlockAtJobLevel = 5, jpCost = 500 },
            new JobAbilityUnlock { abilityName = "Rend Armor", unlockAtJobLevel = 6, jpCost = 600 },
            new JobAbilityUnlock { abilityName = "Rend Shield", unlockAtJobLevel = 7, jpCost = 700 },
            new JobAbilityUnlock { abilityName = "Rend Weapon", unlockAtJobLevel = 8, jpCost = 800 }
        };

        // Set ability catalog
        job.abilityCatalogName = "Fighter Tech";

        EditorUtility.SetDirty(job);
    }

    private static void CreateArcherJob()
    {
        var job = CreateJobAsset("Archer");
        var squire = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Squire.asset");

        job.jobName = "Archer";
        job.description = "Long-range specialist with bow attacks. High speed and range.";
        job.category = JobCategory.Common;
        job.minimumCharacterLevel = 1;

        // High speed, range focus
        job.hpMultiplier = 1.2f;
        job.mpMultiplier = 0.8f;
        job.atkMultiplier = 1.2f;
        job.defMultiplier = 0.9f;
        job.matMultiplier = 0.7f;
        job.mdfMultiplier = 0.8f;
        job.spdMultiplier = 1.3f;

        job.baseStats = new int[] { 48, 18, 6, 4, 3, 4, 6 };

        job.movementBonus = 3;
        job.jumpBonus = 4;
        job.evadeBonus = 15;

        if (squire != null)
        {
            job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>
            {
                new JobPrerequisite { requiredJob = squire, requiredLevel = 2 }
            };
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Arm Shot", unlockAtJobLevel = 1, jpCost = 100 },
            new JobAbilityUnlock { abilityName = "Leg Shot", unlockAtJobLevel = 2, jpCost = 200 },
            new JobAbilityUnlock { abilityName = "Seal Evil", unlockAtJobLevel = 3, jpCost = 300 },
            new JobAbilityUnlock { abilityName = "Blaze Gun", unlockAtJobLevel = 4, jpCost = 400 },
            new JobAbilityUnlock { abilityName = "Charge", unlockAtJobLevel = 5, jpCost = 500 },
            new JobAbilityUnlock { abilityName = "Charge+", unlockAtJobLevel = 6, jpCost = 600 },
            new JobAbilityUnlock { abilityName = "Charge++", unlockAtJobLevel = 7, jpCost = 700 },
            new JobAbilityUnlock { abilityName = "Charge+++", unlockAtJobLevel = 8, jpCost = 800 }
        };

        // Set ability catalog
        job.abilityCatalogName = "Archer";

        EditorUtility.SetDirty(job);
    }

    private static void CreatePriestJob()
    {
        var job = CreateJobAsset("Priest");
        var chemist = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Chemist.asset");

        job.jobName = "Priest";
        job.description = "Holy magic user. Specializes in healing and support magic.";
        job.category = JobCategory.Common;
        job.minimumCharacterLevel = 1;

        // Magic and support focus
        job.hpMultiplier = 1.0f;
        job.mpMultiplier = 1.5f;
        job.atkMultiplier = 0.7f;
        job.defMultiplier = 0.8f;
        job.matMultiplier = 1.3f;
        job.mdfMultiplier = 1.4f;
        job.spdMultiplier = 0.9f;

        job.baseStats = new int[] { 42, 40, 3, 4, 6, 7, 4 };

        job.movementBonus = 3;
        job.jumpBonus = 3;
        job.evadeBonus = 5;

        if (chemist != null)
        {
            job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>
            {
                new JobPrerequisite { requiredJob = chemist, requiredLevel = 2 }
            };
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Cure", unlockAtJobLevel = 1, jpCost = 100 },
            new JobAbilityUnlock { abilityName = "Cura", unlockAtJobLevel = 2, jpCost = 200 },
            new JobAbilityUnlock { abilityName = "Curaga", unlockAtJobLevel = 3, jpCost = 300 },
            new JobAbilityUnlock { abilityName = "Raise", unlockAtJobLevel = 4, jpCost = 400 },
            new JobAbilityUnlock { abilityName = "Arise", unlockAtJobLevel = 5, jpCost = 500 },
            new JobAbilityUnlock { abilityName = "Holy", unlockAtJobLevel = 6, jpCost = 600 },
            new JobAbilityUnlock { abilityName = "Esuna", unlockAtJobLevel = 7, jpCost = 700 },
            new JobAbilityUnlock { abilityName = "Protect", unlockAtJobLevel = 8, jpCost = 800 }
        };

        // Set ability catalog
        job.abilityCatalogName = "Priest";

        EditorUtility.SetDirty(job);
    }

    private static void CreateWizardJob()
    {
        var job = CreateJobAsset("Wizard");
        var chemist = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Chemist.asset");

        job.jobName = "Wizard";
        job.description = "Black magic user. Specializes in offensive elemental magic.";
        job.category = JobCategory.Common;
        job.minimumCharacterLevel = 1;

        // High magic attack
        job.hpMultiplier = 0.9f;
        job.mpMultiplier = 1.6f;
        job.atkMultiplier = 0.6f;
        job.defMultiplier = 0.7f;
        job.matMultiplier = 1.5f;
        job.mdfMultiplier = 1.2f;
        job.spdMultiplier = 0.9f;

        job.baseStats = new int[] { 40, 45, 3, 3, 7, 6, 4 };

        job.movementBonus = 3;
        job.jumpBonus = 3;
        job.evadeBonus = 5;

        if (chemist != null)
        {
            job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>
            {
                new JobPrerequisite { requiredJob = chemist, requiredLevel = 2 }
            };
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Fire", unlockAtJobLevel = 1, jpCost = 100 },
            new JobAbilityUnlock { abilityName = "Blizzard", unlockAtJobLevel = 2, jpCost = 200 },
            new JobAbilityUnlock { abilityName = "Thunder", unlockAtJobLevel = 3, jpCost = 300 },
            new JobAbilityUnlock { abilityName = "Fira", unlockAtJobLevel = 4, jpCost = 400 },
            new JobAbilityUnlock { abilityName = "Blizzara", unlockAtJobLevel = 5, jpCost = 500 },
            new JobAbilityUnlock { abilityName = "Thundara", unlockAtJobLevel = 6, jpCost = 600 },
            new JobAbilityUnlock { abilityName = "Firaga", unlockAtJobLevel = 7, jpCost = 700 },
            new JobAbilityUnlock { abilityName = "Blizzaga", unlockAtJobLevel = 8, jpCost = 800 }
        };

        // Set ability catalog
        job.abilityCatalogName = "Black Magic";

        EditorUtility.SetDirty(job);
    }

    // SPECIAL JOBS
    private static void CreateNinjaJob()
    {
        var job = CreateJobAsset("Ninja");
        var archer = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Archer.asset");

        job.jobName = "Ninja";
        job.description = "Swift assassin with dual-wield capability and high evasion.";
        job.category = JobCategory.Special;
        job.minimumCharacterLevel = 1;

        // Very high speed and evasion
        job.hpMultiplier = 1.3f;
        job.mpMultiplier = 0.9f;
        job.atkMultiplier = 1.3f;
        job.defMultiplier = 0.9f;
        job.matMultiplier = 0.8f;
        job.mdfMultiplier = 0.9f;
        job.spdMultiplier = 1.6f;

        job.baseStats = new int[] { 50, 20, 6, 4, 4, 4, 7 };

        job.movementBonus = 4;
        job.jumpBonus = 4;
        job.evadeBonus = 25;

        if (archer != null)
        {
            job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>
            {
                new JobPrerequisite { requiredJob = archer, requiredLevel = 4 }
            };
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Throw", unlockAtJobLevel = 1, jpCost = 100 },
            new JobAbilityUnlock { abilityName = "Two Hands", unlockAtJobLevel = 2, jpCost = 200 },
            new JobAbilityUnlock { abilityName = "Sunken State", unlockAtJobLevel = 3, jpCost = 300 },
            new JobAbilityUnlock { abilityName = "Dual Wield", unlockAtJobLevel = 4, jpCost = 400 },
            new JobAbilityUnlock { abilityName = "Shuriken", unlockAtJobLevel = 5, jpCost = 500 },
            new JobAbilityUnlock { abilityName = "Smoke Bomb", unlockAtJobLevel = 6, jpCost = 600 },
            new JobAbilityUnlock { abilityName = "Shadow Bind", unlockAtJobLevel = 7, jpCost = 700 },
            new JobAbilityUnlock { abilityName = "Shadow Stitch", unlockAtJobLevel = 8, jpCost = 800 }
        };

        // Set ability catalog
        job.abilityCatalogName = "Ninja";

        EditorUtility.SetDirty(job);
    }

    private static void CreateSamuraiJob()
    {
        var job = CreateJobAsset("Samurai");
        var knight = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Knight.asset");

        job.jobName = "Samurai";
        job.description = "Master swordsman with powerful katana techniques.";
        job.category = JobCategory.Special;
        job.minimumCharacterLevel = 1;

        // Balanced physical fighter
        job.hpMultiplier = 1.5f;
        job.mpMultiplier = 1.0f;
        job.atkMultiplier = 1.5f;
        job.defMultiplier = 1.2f;
        job.matMultiplier = 0.9f;
        job.mdfMultiplier = 1.0f;
        job.spdMultiplier = 1.1f;

        job.baseStats = new int[] { 55, 25, 7, 6, 4, 5, 5 };

        job.movementBonus = 3;
        job.jumpBonus = 3;
        job.evadeBonus = 10;

        if (knight != null)
        {
            job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>
            {
                new JobPrerequisite { requiredJob = knight, requiredLevel = 4 }
            };
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Draw Out", unlockAtJobLevel = 1, jpCost = 100 },
            new JobAbilityUnlock { abilityName = "Iaido", unlockAtJobLevel = 2, jpCost = 200 },
            new JobAbilityUnlock { abilityName = "Blade Grasp", unlockAtJobLevel = 3, jpCost = 300 },
            new JobAbilityUnlock { abilityName = "Bushido", unlockAtJobLevel = 4, jpCost = 400 },
            new JobAbilityUnlock { abilityName = "Seppuku", unlockAtJobLevel = 5, jpCost = 500 },
            new JobAbilityUnlock { abilityName = "Katana", unlockAtJobLevel = 6, jpCost = 600 },
            new JobAbilityUnlock { abilityName = "Wakizashi", unlockAtJobLevel = 7, jpCost = 700 },
            new JobAbilityUnlock { abilityName = "Masamune", unlockAtJobLevel = 8, jpCost = 800 }
        };

        // Set ability catalog
        job.abilityCatalogName = "Samurai";

        EditorUtility.SetDirty(job);
    }

    // COMMON JOBS (Additional)
    private static void CreateMonkJob()
    {
        var job = CreateJobAsset("Monk");
        var knight = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Knight.asset");

        job.jobName = "Monk";
        job.description = "Martial artist with powerful bare-handed attacks. High HP and physical attack.";
        job.category = JobCategory.Common;
        job.minimumCharacterLevel = 1;

        job.hpMultiplier = 1.7f;
        job.mpMultiplier = 0.8f;
        job.atkMultiplier = 1.5f;
        job.defMultiplier = 1.1f;
        job.matMultiplier = 0.7f;
        job.mdfMultiplier = 1.1f;
        job.spdMultiplier = 1.2f;

        job.baseStats = new int[] { 58, 18, 7, 5, 3, 5, 5 };

        job.movementBonus = 4;
        job.jumpBonus = 4;
        job.evadeBonus = 20;

        if (knight != null)
        {
            job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>
            {
                new JobPrerequisite { requiredJob = knight, requiredLevel = 3 }
            };
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Chakra", unlockAtJobLevel = 1, jpCost = 100 },
            new JobAbilityUnlock { abilityName = "Revive", unlockAtJobLevel = 2, jpCost = 200 },
            new JobAbilityUnlock { abilityName = "Counter", unlockAtJobLevel = 3, jpCost = 300 },
            new JobAbilityUnlock { abilityName = "Hamedo", unlockAtJobLevel = 4, jpCost = 400 },
            new JobAbilityUnlock { abilityName = "Return", unlockAtJobLevel = 5, jpCost = 500 },
            new JobAbilityUnlock { abilityName = "Two Hands", unlockAtJobLevel = 6, jpCost = 600 },
            new JobAbilityUnlock { abilityName = "Martial Arts", unlockAtJobLevel = 7, jpCost = 700 },
            new JobAbilityUnlock { abilityName = "Kick", unlockAtJobLevel = 8, jpCost = 800 }
        };

        // Set ability catalog
        job.abilityCatalogName = "Monk";

        EditorUtility.SetDirty(job);
    }

    private static void CreateThiefJob()
    {
        var job = CreateJobAsset("Thief");
        var archer = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Archer.asset");

        job.jobName = "Thief";
        job.description = "Swift and sneaky. Can steal items and has high speed.";
        job.category = JobCategory.Common;
        job.minimumCharacterLevel = 1;

        job.hpMultiplier = 1.1f;
        job.mpMultiplier = 0.8f;
        job.atkMultiplier = 1.1f;
        job.defMultiplier = 0.9f;
        job.matMultiplier = 0.7f;
        job.mdfMultiplier = 0.8f;
        job.spdMultiplier = 1.4f;

        job.baseStats = new int[] { 47, 18, 5, 4, 3, 4, 6 };

        job.movementBonus = 4;
        job.jumpBonus = 4;
        job.evadeBonus = 20;

        if (archer != null)
        {
            job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>
            {
                new JobPrerequisite { requiredJob = archer, requiredLevel = 3 }
            };
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Steal", unlockAtJobLevel = 1, jpCost = 100 },
            new JobAbilityUnlock { abilityName = "Steal Helmet", unlockAtJobLevel = 2, jpCost = 200 },
            new JobAbilityUnlock { abilityName = "Steal Armor", unlockAtJobLevel = 3, jpCost = 300 },
            new JobAbilityUnlock { abilityName = "Steal Accessory", unlockAtJobLevel = 4, jpCost = 400 },
            new JobAbilityUnlock { abilityName = "Steal Weapon", unlockAtJobLevel = 5, jpCost = 500 },
            new JobAbilityUnlock { abilityName = "Move-Find Item", unlockAtJobLevel = 6, jpCost = 600 },
            new JobAbilityUnlock { abilityName = "Steal Gil", unlockAtJobLevel = 7, jpCost = 700 },
            new JobAbilityUnlock { abilityName = "Steal Heart", unlockAtJobLevel = 8, jpCost = 800 }
        };

        // Set ability catalog
        job.abilityCatalogName = "Battle Tech";

        EditorUtility.SetDirty(job);
    }

    private static void CreateMysticJob()
    {
        var job = CreateJobAsset("Mystic");
        var priest = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Priest.asset");

        job.jobName = "Mystic";
        job.description = "Yin-yang magic user with both healing and offensive abilities.";
        job.category = JobCategory.Common;
        job.minimumCharacterLevel = 1;

        job.hpMultiplier = 1.0f;
        job.mpMultiplier = 1.4f;
        job.atkMultiplier = 0.7f;
        job.defMultiplier = 0.8f;
        job.matMultiplier = 1.3f;
        job.mdfMultiplier = 1.3f;
        job.spdMultiplier = 1.0f;

        job.baseStats = new int[] { 43, 38, 3, 4, 6, 6, 4 };

        job.movementBonus = 3;
        job.jumpBonus = 3;
        job.evadeBonus = 10;

        if (priest != null)
        {
            job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>
            {
                new JobPrerequisite { requiredJob = priest, requiredLevel = 3 }
            };
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Protect", unlockAtJobLevel = 1, jpCost = 100 },
            new JobAbilityUnlock { abilityName = "Shell", unlockAtJobLevel = 2, jpCost = 200 },
            new JobAbilityUnlock { abilityName = "Wall", unlockAtJobLevel = 3, jpCost = 300 },
            new JobAbilityUnlock { abilityName = "Reflect", unlockAtJobLevel = 4, jpCost = 400 },
            new JobAbilityUnlock { abilityName = "Reraise", unlockAtJobLevel = 5, jpCost = 500 },
            new JobAbilityUnlock { abilityName = "Haste", unlockAtJobLevel = 6, jpCost = 600 },
            new JobAbilityUnlock { abilityName = "Slow", unlockAtJobLevel = 7, jpCost = 700 },
            new JobAbilityUnlock { abilityName = "Regen", unlockAtJobLevel = 8, jpCost = 800 }
        };

        // Set ability catalog
        job.abilityCatalogName = "Mystic";

        EditorUtility.SetDirty(job);
    }

    private static void CreateTimeMageJob()
    {
        var job = CreateJobAsset("TimeMage");
        var wizard = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Wizard.asset");

        job.jobName = "Time Mage";
        job.description = "Master of time and gravity magic. Can manipulate speed and stop time.";
        job.category = JobCategory.Common;
        job.minimumCharacterLevel = 1;

        job.hpMultiplier = 0.9f;
        job.mpMultiplier = 1.5f;
        job.atkMultiplier = 0.6f;
        job.defMultiplier = 0.7f;
        job.matMultiplier = 1.4f;
        job.mdfMultiplier = 1.3f;
        job.spdMultiplier = 1.0f;

        job.baseStats = new int[] { 41, 42, 3, 3, 6, 6, 4 };

        job.movementBonus = 3;
        job.jumpBonus = 3;
        job.evadeBonus = 5;

        if (wizard != null)
        {
            job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>
            {
                new JobPrerequisite { requiredJob = wizard, requiredLevel = 3 }
            };
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Haste", unlockAtJobLevel = 1, jpCost = 100 },
            new JobAbilityUnlock { abilityName = "Slow", unlockAtJobLevel = 2, jpCost = 200 },
            new JobAbilityUnlock { abilityName = "Stop", unlockAtJobLevel = 3, jpCost = 300 },
            new JobAbilityUnlock { abilityName = "Don't Move", unlockAtJobLevel = 4, jpCost = 400 },
            new JobAbilityUnlock { abilityName = "Don't Act", unlockAtJobLevel = 5, jpCost = 500 },
            new JobAbilityUnlock { abilityName = "Float", unlockAtJobLevel = 6, jpCost = 600 },
            new JobAbilityUnlock { abilityName = "Meteor", unlockAtJobLevel = 7, jpCost = 700 },
            new JobAbilityUnlock { abilityName = "Quick", unlockAtJobLevel = 8, jpCost = 800 }
        };

        // Set ability catalog
        job.abilityCatalogName = "Time Mage";

        EditorUtility.SetDirty(job);
    }

    private static void CreateSummonerJob()
    {
        var job = CreateJobAsset("Summoner");
        var priest = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Priest.asset");
        var wizard = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Wizard.asset");

        job.jobName = "Summoner";
        job.description = "Calls forth powerful summon beasts to devastate enemies.";
        job.category = JobCategory.Common;
        job.minimumCharacterLevel = 1;

        job.hpMultiplier = 1.0f;
        job.mpMultiplier = 1.7f;
        job.atkMultiplier = 0.6f;
        job.defMultiplier = 0.8f;
        job.matMultiplier = 1.6f;
        job.mdfMultiplier = 1.4f;
        job.spdMultiplier = 0.9f;

        job.baseStats = new int[] { 43, 48, 3, 4, 7, 7, 4 };

        job.movementBonus = 3;
        job.jumpBonus = 3;
        job.evadeBonus = 5;

        job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>();
        if (priest != null && wizard != null)
        {
            job.prerequisites.Add(new JobPrerequisite { requiredJob = priest, requiredLevel = 3 });
            job.prerequisites.Add(new JobPrerequisite { requiredJob = wizard, requiredLevel = 3 });
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Ifrit", unlockAtJobLevel = 1, jpCost = 100 },
            new JobAbilityUnlock { abilityName = "Shiva", unlockAtJobLevel = 2, jpCost = 200 },
            new JobAbilityUnlock { abilityName = "Ramuh", unlockAtJobLevel = 3, jpCost = 300 },
            new JobAbilityUnlock { abilityName = "Titan", unlockAtJobLevel = 4, jpCost = 400 },
            new JobAbilityUnlock { abilityName = "Golem", unlockAtJobLevel = 5, jpCost = 500 },
            new JobAbilityUnlock { abilityName = "Bahamut", unlockAtJobLevel = 6, jpCost = 600 },
            new JobAbilityUnlock { abilityName = "Zodiac", unlockAtJobLevel = 7, jpCost = 700 },
            new JobAbilityUnlock { abilityName = "Ultima", unlockAtJobLevel = 8, jpCost = 800 }
        };

        // Set ability catalog
        job.abilityCatalogName = "Summoner";

        EditorUtility.SetDirty(job);
    }

    private static void CreateGeomancerJob()
    {
        var job = CreateJobAsset("Geomancer");
        var monk = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Monk.asset");

        job.jobName = "Geomancer";
        job.description = "Uses the power of the land to attack. Terrain-based abilities.";
        job.category = JobCategory.Common;
        job.minimumCharacterLevel = 1;

        job.hpMultiplier = 1.3f;
        job.mpMultiplier = 1.0f;
        job.atkMultiplier = 1.1f;
        job.defMultiplier = 1.0f;
        job.matMultiplier = 1.2f;
        job.mdfMultiplier = 1.1f;
        job.spdMultiplier = 1.0f;

        job.baseStats = new int[] { 50, 25, 5, 5, 5, 5, 4 };

        job.movementBonus = 3;
        job.jumpBonus = 4;
        job.evadeBonus = 10;

        if (monk != null)
        {
            job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>
            {
                new JobPrerequisite { requiredJob = monk, requiredLevel = 3 }
            };
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Pitfall", unlockAtJobLevel = 1, jpCost = 100 },
            new JobAbilityUnlock { abilityName = "Water Ball", unlockAtJobLevel = 2, jpCost = 200 },
            new JobAbilityUnlock { abilityName = "Hell Ivy", unlockAtJobLevel = 3, jpCost = 300 },
            new JobAbilityUnlock { abilityName = "Demon Fire", unlockAtJobLevel = 4, jpCost = 400 },
            new JobAbilityUnlock { abilityName = "Earth Slash", unlockAtJobLevel = 5, jpCost = 500 },
            new JobAbilityUnlock { abilityName = "Move on Lava", unlockAtJobLevel = 6, jpCost = 600 },
            new JobAbilityUnlock { abilityName = "Wind Slash", unlockAtJobLevel = 7, jpCost = 700 },
            new JobAbilityUnlock { abilityName = "Quake", unlockAtJobLevel = 8, jpCost = 800 }
        };

        // Set ability catalog
        job.abilityCatalogName = "Geomancer";

        EditorUtility.SetDirty(job);
    }

    private static void CreateDragoonJob()
    {
        var job = CreateJobAsset("Dragoon");
        var thief = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Thief.asset");

        job.jobName = "Dragoon";
        job.description = "Dragon knight with jump attacks. High vertical mobility.";
        job.category = JobCategory.Common;
        job.minimumCharacterLevel = 1;

        job.hpMultiplier = 1.6f;
        job.mpMultiplier = 0.8f;
        job.atkMultiplier = 1.4f;
        job.defMultiplier = 1.3f;
        job.matMultiplier = 0.7f;
        job.mdfMultiplier = 0.9f;
        job.spdMultiplier = 1.0f;

        job.baseStats = new int[] { 56, 18, 7, 6, 3, 4, 4 };

        job.movementBonus = 3;
        job.jumpBonus = 5;
        job.evadeBonus = 10;

        if (thief != null)
        {
            job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>
            {
                new JobPrerequisite { requiredJob = thief, requiredLevel = 4 }
            };
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Jump", unlockAtJobLevel = 1, jpCost = 100 },
            new JobAbilityUnlock { abilityName = "Lance", unlockAtJobLevel = 2, jpCost = 200 },
            new JobAbilityUnlock { abilityName = "Vertical Jump", unlockAtJobLevel = 3, jpCost = 300 },
            new JobAbilityUnlock { abilityName = "Ignore Height", unlockAtJobLevel = 4, jpCost = 400 },
            new JobAbilityUnlock { abilityName = "Dragon Breath", unlockAtJobLevel = 5, jpCost = 500 },
            new JobAbilityUnlock { abilityName = "Dragon Crest", unlockAtJobLevel = 6, jpCost = 600 },
            new JobAbilityUnlock { abilityName = "Dragon Spirit", unlockAtJobLevel = 7, jpCost = 700 },
            new JobAbilityUnlock { abilityName = "Dragon Soul", unlockAtJobLevel = 8, jpCost = 800 }
        };

        // Set ability catalog
        job.abilityCatalogName = "Dragoon";

        EditorUtility.SetDirty(job);
    }

    private static void CreateOraclJob()
    {
        var job = CreateJobAsset("Oracl");
        var mystic = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Mystic.asset");

        job.jobName = "Oracl";
        job.description = "Dark magic user with status-inflicting Yin-Yang magic.";
        job.category = JobCategory.Common;
        job.minimumCharacterLevel = 1;

        job.hpMultiplier = 1.0f;
        job.mpMultiplier = 1.4f;
        job.atkMultiplier = 0.7f;
        job.defMultiplier = 0.8f;
        job.matMultiplier = 1.3f;
        job.mdfMultiplier = 1.2f;
        job.spdMultiplier = 0.9f;

        job.baseStats = new int[] { 43, 40, 3, 4, 6, 6, 4 };

        job.movementBonus = 3;
        job.jumpBonus = 3;
        job.evadeBonus = 5;

        if (mystic != null)
        {
            job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>
            {
                new JobPrerequisite { requiredJob = mystic, requiredLevel = 4 }
            };
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Life Drain", unlockAtJobLevel = 1, jpCost = 100 },
            new JobAbilityUnlock { abilityName = "Spell Absorb", unlockAtJobLevel = 2, jpCost = 200 },
            new JobAbilityUnlock { abilityName = "Petrify", unlockAtJobLevel = 3, jpCost = 300 },
            new JobAbilityUnlock { abilityName = "Death Sentence", unlockAtJobLevel = 4, jpCost = 400 },
            new JobAbilityUnlock { abilityName = "Zombie", unlockAtJobLevel = 5, jpCost = 500 },
            new JobAbilityUnlock { abilityName = "Dark", unlockAtJobLevel = 6, jpCost = 600 },
            new JobAbilityUnlock { abilityName = "Darkra", unlockAtJobLevel = 7, jpCost = 700 },
            new JobAbilityUnlock { abilityName = "Darkga", unlockAtJobLevel = 8, jpCost = 800 }
        };

        // Set ability catalog
        job.abilityCatalogName = "Oracle";

        EditorUtility.SetDirty(job);
    }

    // SPECIAL JOBS (Additional)
    private static void CreateLancerJob()
    {
        var job = CreateJobAsset("Lancer");
        var dragoon = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Dragoon.asset");

        job.jobName = "Lancer";
        job.description = "Spear master with long-range physical attacks.";
        job.category = JobCategory.Special;
        job.minimumCharacterLevel = 1;

        job.hpMultiplier = 1.5f;
        job.mpMultiplier = 0.8f;
        job.atkMultiplier = 1.4f;
        job.defMultiplier = 1.2f;
        job.matMultiplier = 0.7f;
        job.mdfMultiplier = 0.9f;
        job.spdMultiplier = 1.1f;

        job.baseStats = new int[] { 54, 18, 7, 6, 3, 4, 5 };

        job.movementBonus = 3;
        job.jumpBonus = 4;
        job.evadeBonus = 10;

        if (dragoon != null)
        {
            job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>
            {
                new JobPrerequisite { requiredJob = dragoon, requiredLevel = 5 }
            };
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Spear Thrust", unlockAtJobLevel = 1 },
            new JobAbilityUnlock { abilityName = "Spear Throw", unlockAtJobLevel = 3 },
            new JobAbilityUnlock { abilityName = "Dragon Spirit", unlockAtJobLevel = 6 }
        };

        EditorUtility.SetDirty(job);
    }

    private static void CreateMediatorJob()
    {
        var job = CreateJobAsset("Mediator");
        var oracl = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Oracl.asset");

        job.jobName = "Mediator";
        job.description = "Diplomat who can charm and recruit enemies.";
        job.category = JobCategory.Special;
        job.minimumCharacterLevel = 1;

        job.hpMultiplier = 1.1f;
        job.mpMultiplier = 1.2f;
        job.atkMultiplier = 0.8f;
        job.defMultiplier = 0.9f;
        job.matMultiplier = 1.1f;
        job.mdfMultiplier = 1.2f;
        job.spdMultiplier = 1.0f;

        job.baseStats = new int[] { 46, 30, 4, 4, 5, 6, 4 };

        job.movementBonus = 3;
        job.jumpBonus = 3;
        job.evadeBonus = 10;

        if (oracl != null)
        {
            job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>
            {
                new JobPrerequisite { requiredJob = oracl, requiredLevel = 5 }
            };
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Negotiate", unlockAtJobLevel = 1 },
            new JobAbilityUnlock { abilityName = "Threaten", unlockAtJobLevel = 2 },
            new JobAbilityUnlock { abilityName = "Charm", unlockAtJobLevel = 4 },
            new JobAbilityUnlock { abilityName = "Invitation", unlockAtJobLevel = 6 }
        };

        EditorUtility.SetDirty(job);
    }

    private static void CreateCalculatorJob()
    {
        var job = CreateJobAsset("Calculator");
        var priest = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Priest.asset");
        var wizard = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Wizard.asset");

        job.jobName = "Calculator";
        job.description = "Mathematical mage with precise formula-based magic.";
        job.category = JobCategory.Special;
        job.minimumCharacterLevel = 1;

        job.hpMultiplier = 0.9f;
        job.mpMultiplier = 1.6f;
        job.atkMultiplier = 0.6f;
        job.defMultiplier = 0.7f;
        job.matMultiplier = 1.5f;
        job.mdfMultiplier = 1.4f;
        job.spdMultiplier = 0.8f;

        job.baseStats = new int[] { 40, 45, 3, 3, 7, 7, 3 };

        job.movementBonus = 3;
        job.jumpBonus = 3;
        job.evadeBonus = 5;

        job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>();
        if (priest != null && wizard != null)
        {
            job.prerequisites.Add(new JobPrerequisite { requiredJob = priest, requiredLevel = 4 });
            job.prerequisites.Add(new JobPrerequisite { requiredJob = wizard, requiredLevel = 4 });
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "CT Formula", unlockAtJobLevel = 1 },
            new JobAbilityUnlock { abilityName = "Height Formula", unlockAtJobLevel = 3 },
            new JobAbilityUnlock { abilityName = "Prime Number", unlockAtJobLevel = 5 },
            new JobAbilityUnlock { abilityName = "Short Charge", unlockAtJobLevel = 7 }
        };

        EditorUtility.SetDirty(job);
    }

    private static void CreateBardJob()
    {
        var job = CreateJobAsset("Bard");
        var summoner = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Summoner.asset");

        job.jobName = "Bard";
        job.description = "Male performer who sings songs to buff allies.";
        job.category = JobCategory.Special;
        job.minimumCharacterLevel = 1;

        job.hpMultiplier = 1.1f;
        job.mpMultiplier = 1.3f;
        job.atkMultiplier = 0.8f;
        job.defMultiplier = 0.9f;
        job.matMultiplier = 1.2f;
        job.mdfMultiplier = 1.2f;
        job.spdMultiplier = 1.1f;

        job.baseStats = new int[] { 46, 35, 4, 4, 5, 6, 5 };

        job.movementBonus = 3;
        job.jumpBonus = 3;
        job.evadeBonus = 10;

        if (summoner != null)
        {
            job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>
            {
                new JobPrerequisite { requiredJob = summoner, requiredLevel = 5 }
            };
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Angel Song", unlockAtJobLevel = 1 },
            new JobAbilityUnlock { abilityName = "Life Song", unlockAtJobLevel = 2 },
            new JobAbilityUnlock { abilityName = "Battle Song", unlockAtJobLevel = 3 },
            new JobAbilityUnlock { abilityName = "Last Song", unlockAtJobLevel = 5 }
        };

        EditorUtility.SetDirty(job);
    }

    private static void CreateDancerJob()
    {
        var job = CreateJobAsset("Dancer");
        var geomancer = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Geomancer.asset");

        job.jobName = "Dancer";
        job.description = "Female performer who uses dances to debuff enemies.";
        job.category = JobCategory.Special;
        job.minimumCharacterLevel = 1;

        job.hpMultiplier = 1.1f;
        job.mpMultiplier = 1.3f;
        job.atkMultiplier = 0.8f;
        job.defMultiplier = 0.9f;
        job.matMultiplier = 1.2f;
        job.mdfMultiplier = 1.2f;
        job.spdMultiplier = 1.2f;

        job.baseStats = new int[] { 46, 35, 4, 4, 5, 6, 5 };

        job.movementBonus = 4;
        job.jumpBonus = 3;
        job.evadeBonus = 15;

        if (geomancer != null)
        {
            job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>
            {
                new JobPrerequisite { requiredJob = geomancer, requiredLevel = 5 }
            };
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Witch Dance", unlockAtJobLevel = 1 },
            new JobAbilityUnlock { abilityName = "Slow Dance", unlockAtJobLevel = 2 },
            new JobAbilityUnlock { abilityName = "Polka Polka", unlockAtJobLevel = 3 },
            new JobAbilityUnlock { abilityName = "Nameless Dance", unlockAtJobLevel = 5 }
        };

        EditorUtility.SetDirty(job);
    }

    private static void CreateMimeJob()
    {
        var job = CreateJobAsset("Mime");
        var squire = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/Squire.asset");

        job.jobName = "Mime";
        job.description = "Master of all trades. Can mimic any action used on the battlefield.";
        job.category = JobCategory.Special;
        job.minimumCharacterLevel = 8;

        // Very high requirements - needs to master multiple jobs
        job.hpMultiplier = 1.4f;
        job.mpMultiplier = 1.4f;
        job.atkMultiplier = 1.3f;
        job.defMultiplier = 1.3f;
        job.matMultiplier = 1.3f;
        job.mdfMultiplier = 1.3f;
        job.spdMultiplier = 1.3f;

        job.baseStats = new int[] { 52, 35, 6, 6, 6, 6, 5 };

        job.movementBonus = 4;
        job.jumpBonus = 4;
        job.evadeBonus = 15;

        if (squire != null)
        {
            job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>
            {
                new JobPrerequisite { requiredJob = squire, requiredLevel = 8 }
            };
        }

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Mimic", unlockAtJobLevel = 1 }
        };

        EditorUtility.SetDirty(job);
    }

    // UNIQUE JOBS (Special character-specific)
    private static void CreateDarkKnightJob()
    {
        var job = CreateJobAsset("DarkKnight");

        job.jobName = "Dark Knight";
        job.description = "Gafgarion's unique job. Powerful dark sword techniques.";
        job.category = JobCategory.Unique;
        job.minimumCharacterLevel = 1;
        job.isUnique = true;

        job.hpMultiplier = 1.9f;
        job.mpMultiplier = 1.0f;
        job.atkMultiplier = 1.6f;
        job.defMultiplier = 1.5f;
        job.matMultiplier = 0.9f;
        job.mdfMultiplier = 1.1f;
        job.spdMultiplier = 1.2f;

        job.baseStats = new int[] { 62, 25, 8, 7, 4, 5, 5 };

        job.movementBonus = 4;
        job.jumpBonus = 3;
        job.evadeBonus = 15;

        job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>();

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Dark Sword", unlockAtJobLevel = 1 },
            new JobAbilityUnlock { abilityName = "Night Sword", unlockAtJobLevel = 2 },
            new JobAbilityUnlock { abilityName = "Dark Blade", unlockAtJobLevel = 4 }
        };

        EditorUtility.SetDirty(job);
    }

    private static void CreateHolyKnightJob()
    {
        var job = CreateJobAsset("HolyKnight");

        job.jobName = "Holy Knight";
        job.description = "Agrias's unique job. Holy sword techniques.";
        job.category = JobCategory.Unique;
        job.minimumCharacterLevel = 1;
        job.isUnique = true;

        job.hpMultiplier = 1.8f;
        job.mpMultiplier = 1.2f;
        job.atkMultiplier = 1.5f;
        job.defMultiplier = 1.4f;
        job.matMultiplier = 1.1f;
        job.mdfMultiplier = 1.3f;
        job.spdMultiplier = 1.1f;

        job.baseStats = new int[] { 60, 30, 8, 7, 5, 6, 5 };

        job.movementBonus = 4;
        job.jumpBonus = 3;
        job.evadeBonus = 12;

        job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>();

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Stasis Sword", unlockAtJobLevel = 1 },
            new JobAbilityUnlock { abilityName = "Split Punch", unlockAtJobLevel = 2 },
            new JobAbilityUnlock { abilityName = "Crush Punch", unlockAtJobLevel = 3 },
            new JobAbilityUnlock { abilityName = "Lightning Stab", unlockAtJobLevel = 4 },
            new JobAbilityUnlock { abilityName = "Hallowed Bolt", unlockAtJobLevel = 5 }
        };

        EditorUtility.SetDirty(job);
    }

    private static void CreateHeavenKnightJob()
    {
        var job = CreateJobAsset("HeavenKnight");

        job.jobName = "Heaven Knight";
        job.description = "Meliadoul's unique job. Armor-destroying divine techniques.";
        job.category = JobCategory.Unique;
        job.minimumCharacterLevel = 1;
        job.isUnique = true;

        job.hpMultiplier = 1.7f;
        job.mpMultiplier = 1.1f;
        job.atkMultiplier = 1.5f;
        job.defMultiplier = 1.4f;
        job.matMultiplier = 1.0f;
        job.mdfMultiplier = 1.2f;
        job.spdMultiplier = 1.1f;

        job.baseStats = new int[] { 58, 28, 7, 7, 4, 6, 5 };

        job.movementBonus = 3;
        job.jumpBonus = 3;
        job.evadeBonus = 10;

        job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>();

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Crush Helmet", unlockAtJobLevel = 1 },
            new JobAbilityUnlock { abilityName = "Crush Armor", unlockAtJobLevel = 2 },
            new JobAbilityUnlock { abilityName = "Destroy Shield", unlockAtJobLevel = 3 },
            new JobAbilityUnlock { abilityName = "Crush Weapon", unlockAtJobLevel = 4 }
        };

        EditorUtility.SetDirty(job);
    }

    private static void CreateTempleKnightJob()
    {
        var job = CreateJobAsset("TempleKnight");

        job.jobName = "Temple Knight";
        job.description = "Zalbag/Wiegraf's unique job. Divine knight abilities.";
        job.category = JobCategory.Unique;
        job.minimumCharacterLevel = 1;
        job.isUnique = true;

        job.hpMultiplier = 1.8f;
        job.mpMultiplier = 1.1f;
        job.atkMultiplier = 1.6f;
        job.defMultiplier = 1.5f;
        job.matMultiplier = 1.0f;
        job.mdfMultiplier = 1.2f;
        job.spdMultiplier = 1.1f;

        job.baseStats = new int[] { 60, 28, 8, 7, 4, 6, 5 };

        job.movementBonus = 4;
        job.jumpBonus = 3;
        job.evadeBonus = 12;

        job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>();

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Break", unlockAtJobLevel = 1 },
            new JobAbilityUnlock { abilityName = "Crush", unlockAtJobLevel = 2 },
            new JobAbilityUnlock { abilityName = "Destroy", unlockAtJobLevel = 3 }
        };

        EditorUtility.SetDirty(job);
    }

    private static void CreateArcKnightJob()
    {
        var job = CreateJobAsset("ArcKnight");

        job.jobName = "Arc Knight";
        job.description = "Construct 8's unique mechanical knight job.";
        job.category = JobCategory.Unique;
        job.minimumCharacterLevel = 1;
        job.isUnique = true;

        job.hpMultiplier = 2.0f;
        job.mpMultiplier = 0.5f;
        job.atkMultiplier = 1.7f;
        job.defMultiplier = 1.8f;
        job.matMultiplier = 0.5f;
        job.mdfMultiplier = 1.0f;
        job.spdMultiplier = 0.9f;

        job.baseStats = new int[] { 70, 10, 9, 9, 2, 5, 4 };

        job.movementBonus = 3;
        job.jumpBonus = 3;
        job.evadeBonus = 5;

        job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>();

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Self Destruct", unlockAtJobLevel = 1 },
            new JobAbilityUnlock { abilityName = "Repair", unlockAtJobLevel = 2 }
        };

        EditorUtility.SetDirty(job);
    }

    private static void CreateHolySwordJob()
    {
        var job = CreateJobAsset("HolySword");

        job.jobName = "Holy Sword";
        job.description = "Orlandu's legendary Sword Saint job.";
        job.category = JobCategory.Unique;
        job.minimumCharacterLevel = 1;
        job.isUnique = true;

        // Extremely powerful stats
        job.hpMultiplier = 2.0f;
        job.mpMultiplier = 1.3f;
        job.atkMultiplier = 1.8f;
        job.defMultiplier = 1.6f;
        job.matMultiplier = 1.2f;
        job.mdfMultiplier = 1.4f;
        job.spdMultiplier = 1.3f;

        job.baseStats = new int[] { 65, 32, 9, 8, 5, 7, 6 };

        job.movementBonus = 4;
        job.jumpBonus = 4;
        job.evadeBonus = 20;

        job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>();

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "All Swordskill", unlockAtJobLevel = 1 },
            new JobAbilityUnlock { abilityName = "Lightning Stab", unlockAtJobLevel = 1 },
            new JobAbilityUnlock { abilityName = "Dark Sword", unlockAtJobLevel = 1 }
        };

        EditorUtility.SetDirty(job);
    }

    private static void CreatePrincessJob()
    {
        var job = CreateJobAsset("Princess");

        job.jobName = "Princess";
        job.description = "Ovelia's unique royal job.";
        job.category = JobCategory.Unique;
        job.minimumCharacterLevel = 1;
        job.isUnique = true;

        job.hpMultiplier = 1.2f;
        job.mpMultiplier = 1.5f;
        job.atkMultiplier = 0.7f;
        job.defMultiplier = 1.0f;
        job.matMultiplier = 1.4f;
        job.mdfMultiplier = 1.5f;
        job.spdMultiplier = 1.0f;

        job.baseStats = new int[] { 48, 40, 3, 5, 6, 7, 4 };

        job.movementBonus = 3;
        job.jumpBonus = 3;
        job.evadeBonus = 10;

        job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>();

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Cure", unlockAtJobLevel = 1 },
            new JobAbilityUnlock { abilityName = "Protect", unlockAtJobLevel = 2 }
        };

        EditorUtility.SetDirty(job);
    }

    private static void CreateClericJob()
    {
        var job = CreateJobAsset("Cleric");

        job.jobName = "Cleric";
        job.description = "Alma's unique healer job.";
        job.category = JobCategory.Unique;
        job.minimumCharacterLevel = 1;
        job.isUnique = true;

        job.hpMultiplier = 1.1f;
        job.mpMultiplier = 1.6f;
        job.atkMultiplier = 0.7f;
        job.defMultiplier = 0.9f;
        job.matMultiplier = 1.5f;
        job.mdfMultiplier = 1.6f;
        job.spdMultiplier = 1.0f;

        job.baseStats = new int[] { 45, 45, 3, 4, 7, 8, 4 };

        job.movementBonus = 3;
        job.jumpBonus = 3;
        job.evadeBonus = 8;

        job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>();

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Mbarrier", unlockAtJobLevel = 1 },
            new JobAbilityUnlock { abilityName = "Aegis", unlockAtJobLevel = 2 },
            new JobAbilityUnlock { abilityName = "MBarrier", unlockAtJobLevel = 3 }
        };

        EditorUtility.SetDirty(job);
    }

    private static void CreateSkyPirateJob()
    {
        var job = CreateJobAsset("SkyPirate");

        job.jobName = "Sky Pirate";
        job.description = "Balthier's unique airship pirate job.";
        job.category = JobCategory.Unique;
        job.minimumCharacterLevel = 1;
        job.isUnique = true;

        job.hpMultiplier = 1.4f;
        job.mpMultiplier = 1.0f;
        job.atkMultiplier = 1.4f;
        job.defMultiplier = 1.2f;
        job.matMultiplier = 0.9f;
        job.mdfMultiplier = 1.0f;
        job.spdMultiplier = 1.3f;

        job.baseStats = new int[] { 52, 25, 7, 6, 4, 5, 6 };

        job.movementBonus = 4;
        job.jumpBonus = 4;
        job.evadeBonus = 18;

        job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>();

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Barrage", unlockAtJobLevel = 1 },
            new JobAbilityUnlock { abilityName = "Aim", unlockAtJobLevel = 2 },
            new JobAbilityUnlock { abilityName = "Seal Evil", unlockAtJobLevel = 3 }
        };

        EditorUtility.SetDirty(job);
    }

    private static void CreateMachinistJob()
    {
        var job = CreateJobAsset("Machinist");

        job.jobName = "Machinist";
        job.description = "Mustadio's unique engineer job with gun abilities.";
        job.category = JobCategory.Unique;
        job.minimumCharacterLevel = 1;
        job.isUnique = true;

        job.hpMultiplier = 1.3f;
        job.mpMultiplier = 1.0f;
        job.atkMultiplier = 1.3f;
        job.defMultiplier = 1.1f;
        job.matMultiplier = 1.0f;
        job.mdfMultiplier = 1.0f;
        job.spdMultiplier = 1.2f;

        job.baseStats = new int[] { 50, 25, 6, 5, 4, 5, 5 };

        job.movementBonus = 3;
        job.jumpBonus = 3;
        job.evadeBonus = 12;

        job.prerequisites = new System.Collections.Generic.List<JobPrerequisite>();

        job.abilityUnlocks = new System.Collections.Generic.List<JobAbilityUnlock>
        {
            new JobAbilityUnlock { abilityName = "Arm Shot", unlockAtJobLevel = 1 },
            new JobAbilityUnlock { abilityName = "Leg Shot", unlockAtJobLevel = 2 },
            new JobAbilityUnlock { abilityName = "Seal Evil", unlockAtJobLevel = 3 },
            new JobAbilityUnlock { abilityName = "Blaze Gun", unlockAtJobLevel = 4 }
        };

        EditorUtility.SetDirty(job);
    }

    private static JobDefinition CreateJobAsset(string name)
    {
        string assetPath = $"{JobsPath}/{name}.asset";
        
        // Always create new asset (fresh creation)
        var job = ScriptableObject.CreateInstance<JobDefinition>();
        AssetDatabase.CreateAsset(job, assetPath);
        return job;
    }

}
#endif
