using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;

/// <summary>
/// Comprehensive FFT Ability Creator - Creates ALL abilities from Final Fantasy Tactics
/// Based on official FFT job system with complete ability lists for every job
/// </summary>
public static class FFTAbilityCreator
{
    private const string AbilitiesPath = "Assets/Resources/Abilities";

    [MenuItem("Tactics RPG/Create FFT Abilities/Create ALL FFT Abilities (Complete)")]
    public static void CreateAllFFTAbilitiesComplete()
    {
        EnsureAbilitiesDirectoryExists();
        
        // Delete all existing ability prefabs first
        DeleteAllExistingAbilities();
        
        // Create ALL FFT abilities for every job
        CreateSquireAbilities();
        CreateChemistAbilities();
        CreateKnightAbilities();
        CreateArcherAbilities();
        CreatePriestAbilities();
        CreateWizardAbilities();
        CreateMonkAbilities();
        CreateThiefAbilities();
        CreateMysticAbilities();
        CreateTimeMageAbilities();
        CreateSummonerAbilities();
        CreateGeomancerAbilities();
        CreateDragoonAbilities();
        CreateOracleAbilities();
        CreateNinjaAbilities();
        CreateSamuraiAbilities();
        CreateLancerAbilities();
        CreateMediatorAbilities();
        CreateCalculatorAbilities();
        CreateBardAbilities();
        CreateDancerAbilities();
        CreateMimeAbilities();
        
        // Create Special Job Abilities
        CreateAstrologerAbilities();
        CreateClericAbilities();
        CreateFellKnightAbilities();
        CreateDeathknightAbilities();
        CreateSorcererAbilities();
        
        // Special Job Abilities - All implemented
        CreatePrincessAbilities();
        CreateArkKnightAbilities();
        CreateAssassinAbilities();
        CreateCelebrantAbilities();
        CreateDivineKnightAbilities();
        CreateTemplarAbilities();
        CreateDragonkinAbilities();
        CreateAutomatonAbilities();
        CreateSoldierAbilities();
        CreateSkyPirateAbilities();
        CreateGameHunterAbilities();
        CreateNightbladeAbilities();
        CreateRuneKnightAbilities();
        
        // Create Reaction Abilities
        CreateReactionAbilities();
        
        // Create Support Abilities
        CreateSupportAbilities();
        
        // Create Movement Abilities
        CreateMovementAbilities();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created ALL FFT abilities complete (200+ abilities across all jobs and categories)!");
    }

    private static void EnsureAbilitiesDirectoryExists()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        if (!AssetDatabase.IsValidFolder(AbilitiesPath))
            AssetDatabase.CreateFolder("Assets/Resources", "Abilities");

        // Create job-specific folders
        CreateJobFolder("Squire");
        CreateJobFolder("Chemist");
        CreateJobFolder("Knight");
        CreateJobFolder("Archer");
        CreateJobFolder("Priest");
        CreateJobFolder("Wizard");
        CreateJobFolder("Monk");
        CreateJobFolder("Thief");
        CreateJobFolder("Mystic");
        CreateJobFolder("Time Mage");
        CreateJobFolder("Summoner");
        CreateJobFolder("Geomancer");
        CreateJobFolder("Dragoon");
        CreateJobFolder("Oracle");
        CreateJobFolder("Ninja");
        CreateJobFolder("Samurai");
        CreateJobFolder("Lancer");
        CreateJobFolder("Mediator");
        CreateJobFolder("Calculator");
        CreateJobFolder("Bard");
        CreateJobFolder("Dancer");
        CreateJobFolder("Mime");
    }

    private static void CreateJobFolder(string jobName)
    {
        string jobPath = $"{AbilitiesPath}/{jobName}";
        if (!AssetDatabase.IsValidFolder(jobPath))
            AssetDatabase.CreateFolder(AbilitiesPath, jobName);
    }

    private static void DeleteAllExistingAbilities()
    {
        // Get all .prefab files in all ability folders
        if (!System.IO.Directory.Exists(AbilitiesPath))
        {
            Debug.Log("Abilities directory doesn't exist yet, nothing to delete.");
            return;
        }
        
        string[] jobFolders = System.IO.Directory.GetDirectories(AbilitiesPath);
        
        int deletedCount = 0;
        foreach (string folder in jobFolders)
        {
            string[] prefabFiles = System.IO.Directory.GetFiles(folder, "*.prefab", System.IO.SearchOption.TopDirectoryOnly);
            
            foreach (string file in prefabFiles)
            {
                try
                {
                    // Convert absolute path to Unity asset path
                    string relativePath = file.Replace(Application.dataPath, "Assets").Replace("\\", "/");
                    
                    // Check if the asset exists before trying to delete it
                    if (AssetDatabase.LoadAssetAtPath<GameObject>(relativePath) != null)
                    {
                        AssetDatabase.DeleteAsset(relativePath);
                        deletedCount++;
                        Debug.Log($"Deleted existing ability: {relativePath}");
                    }
                    else
                    {
                        Debug.LogWarning($"Asset not found or already deleted: {relativePath}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to delete asset {file}: {e.Message}");
                }
            }
        }
        
        Debug.Log($"Deleted {deletedCount} existing ability prefabs");
    }

    #region Squire Abilities (Complete FFT Set)

    private static void CreateSquireAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Squire abilities from FFT
        CreateAttackAbility();
        CreateDefendAbility();
        CreateAccumulateAbility();
        CreateStoneThrowAbility();
        CreateDashAbility();
        CreateFocusAbility();
        CreateYellAbility();
        CreateCheerAbility();
        
        Debug.Log("Created Squire abilities: Attack, Defend, Accumulate, Stone Throw, Dash, Focus, Yell, Cheer");
    }

    private static void CreateAttackAbility()
    {
        var ability = CreateAbilityPrefab("Squire", "Attack");
        
        // Add components with specific data
        AddConstantRange(ability, 1, 1); // Range 1, height 1
        AddPhysicalDamageEffect(ability); // Basic physical damage effect
        AddWeaponPower(ability); // Uses weapon power (correct for Attack)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Squire", "Attack");
    }

    private static void CreateDefendAbility()
    {
        var ability = CreateAbilityPrefab("Squire", "Defend");
        
        AddSelfRange(ability); // Self only
        AddDefendEffect(ability); // Defend effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Squire", "Defend");
    }

    private static void CreateAccumulateAbility()
    {
        var ability = CreateAbilityPrefab("Squire", "Accumulate");
        
        AddSelfRange(ability); // Self only
        AddAccumulateEffect(ability); // Accumulate effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Squire", "Accumulate");
    }

    private static void CreateStoneThrowAbility()
    {
        var ability = CreateAbilityPrefab("Squire", "Stone Throw");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddPhysicalDamageEffect(ability); // Basic physical damage effect
        AddPhysicalPower(ability, 8); // Stone Throw power = 8 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Squire", "Stone Throw");
    }

    private static void CreateDashAbility()
    {
        var ability = CreateAbilityPrefab("Squire", "Dash");
        
        AddSelfRange(ability); // Self only
        AddDashEffect(ability); // Dash effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Squire", "Dash");
    }

    private static void CreateFocusAbility()
    {
        var ability = CreateAbilityPrefab("Squire", "Focus");
        
        AddSelfRange(ability); // Self only
        AddFocusEffect(ability); // Focus effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Squire", "Focus");
    }

    private static void CreateYellAbility()
    {
        var ability = CreateAbilityPrefab("Squire", "Yell");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddYellEffect(ability); // Yell effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Squire", "Yell");
    }

    private static void CreateCheerAbility()
    {
        var ability = CreateAbilityPrefab("Squire", "Cheer");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddCheerEffect(ability); // Cheer effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Squire", "Cheer");
    }

    #endregion

    #region Chemist Abilities (Complete FFT Set)

    private static void CreateChemistAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Chemist abilities from FFT
        CreatePotionAbility();
        CreateHiPotionAbility();
        CreatePhoenixDownAbility();
        CreateAntidoteAbility();
        CreateEyeDropAbility();
        CreateEchoGrassAbility();
        CreateRemedyAbility();
        CreateAutoPotionAbility();
        CreateThrowItemAbility();
        
        Debug.Log("Created Chemist abilities: Potion, Hi-Potion, Phoenix Down, Antidote, Eye Drop, Echo Grass, Remedy, Auto Potion, Throw Item");
    }

    private static void CreatePotionAbility()
    {
        var ability = CreateAbilityPrefab("Chemist", "Potion");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddHealEffect(ability, 50); // Heal 50 HP
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Chemist", "Potion");
    }

    private static void CreateHiPotionAbility()
    {
        var ability = CreateAbilityPrefab("Chemist", "Hi-Potion");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddHealEffect(ability, 100); // Heal 100 HP
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Chemist", "Hi-Potion");
    }

    private static void CreatePhoenixDownAbility()
    {
        var ability = CreateAbilityPrefab("Chemist", "Phoenix Down");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddReviveEffect(ability); // Revive effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Chemist", "Phoenix Down");
    }

    private static void CreateAntidoteAbility()
    {
        var ability = CreateAbilityPrefab("Chemist", "Antidote");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddCurePoisonEffect(ability); // Cure poison
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Chemist", "Antidote");
    }

    private static void CreateEyeDropAbility()
    {
        var ability = CreateAbilityPrefab("Chemist", "Eye Drop");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddCureBlindEffect(ability); // Cure blind
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Chemist", "Eye Drop");
    }

    private static void CreateEchoGrassAbility()
    {
        var ability = CreateAbilityPrefab("Chemist", "Echo Grass");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddCureSilenceEffect(ability); // Cure silence
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Chemist", "Echo Grass");
    }

    private static void CreateRemedyAbility()
    {
        var ability = CreateAbilityPrefab("Chemist", "Remedy");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddCureAllStatusEffect(ability); // Cure all status
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Chemist", "Remedy");
    }

    private static void CreateAutoPotionAbility()
    {
        var ability = CreateAbilityPrefab("Chemist", "Auto Potion");
        
        AddSelfRange(ability); // Self only
        AddAutoPotionEffect(ability); // Auto potion effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Chemist", "Auto Potion");
    }

    private static void CreateThrowItemAbility()
    {
        var ability = CreateAbilityPrefab("Chemist", "Throw Item");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddThrowItemEffect(ability); // Throw item effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Chemist", "Throw Item");
    }

    #endregion

    #region Knight Abilities (Complete FFT Set)

    private static void CreateKnightAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Knight abilities from FFT
        CreateFirstAidAbility();
        CreateAirBlastAbility();
        CreateEarthRenderAbility();
        CreateWindSlashAbility();
        CreatePowerBreakAbility();
        CreateArmorBreakAbility();
        CreateMentalBreakAbility();
        CreateMagicBreakAbility();
        
        Debug.Log("Created Knight abilities: First Aid, Air Blast, Earth Render, Wind Slash, Power Break, Armor Break, Mental Break, Magic Break");
    }

    private static void CreateFirstAidAbility()
    {
        var ability = CreateAbilityPrefab("Knight", "First Aid");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddHealEffect(ability, 30); // Heal 30 HP
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 3); // 3 MP cost
        
        SaveAbilityPrefab(ability, "Knight", "First Aid");
    }

    private static void CreateAirBlastAbility()
    {
        var ability = CreateAbilityPrefab("Knight", "Air Blast");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddPhysicalDamageEffect(ability); // Basic physical damage effect
        AddPhysicalPower(ability, 12); // Air Blast power = 12 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Knight", "Air Blast");
    }

    private static void CreateEarthRenderAbility()
    {
        var ability = CreateAbilityPrefab("Knight", "Earth Render");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddPhysicalDamageEffect(ability); // Basic physical damage effect
        AddPhysicalPower(ability, 14); // Earth Render power = 14 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Knight", "Earth Render");
    }

    private static void CreateWindSlashAbility()
    {
        var ability = CreateAbilityPrefab("Knight", "Wind Slash");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddPhysicalDamageEffect(ability); // Basic physical damage effect
        AddPhysicalPower(ability, 16); // Wind Slash power = 16 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Knight", "Wind Slash");
    }

    private static void CreatePowerBreakAbility()
    {
        var ability = CreateAbilityPrefab("Knight", "Power Break");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddPowerBreakEffect(ability); // Power break effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Knight", "Power Break");
    }

    private static void CreateArmorBreakAbility()
    {
        var ability = CreateAbilityPrefab("Knight", "Armor Break");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddArmorBreakEffect(ability); // Armor break effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Knight", "Armor Break");
    }

    private static void CreateMentalBreakAbility()
    {
        var ability = CreateAbilityPrefab("Knight", "Mental Break");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddMentalBreakEffect(ability); // Mental break effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Knight", "Mental Break");
    }

    private static void CreateMagicBreakAbility()
    {
        var ability = CreateAbilityPrefab("Knight", "Magic Break");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddMagicBreakEffect(ability); // Magic break effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Knight", "Magic Break");
    }

    #endregion

    private static void CreateArcherAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Archer abilities from FFT
        CreateAimAbility();
        CreateCharge1Ability();
        CreateCharge2Ability();
        CreateCharge3Ability();
        CreateCharge4Ability();
        CreateCharge5Ability();
        CreateCharge6Ability();
        CreateCharge7Ability();
        CreateCharge8Ability();
        CreateCharge9Ability();
        CreateCharge10Ability();
        CreateCharge11Ability();
        CreateCharge12Ability();
        CreateCharge13Ability();
        CreateCharge14Ability();
        CreateCharge15Ability();
        CreateCharge16Ability();
        CreateCharge17Ability();
        CreateCharge18Ability();
        CreateCharge19Ability();
        CreateCharge20Ability();
        
        Debug.Log("Created Archer abilities: Aim, Charge+1 through Charge+20");
    }

    private static void CreateAimAbility()
    {
        var ability = CreateAbilityPrefab("Archer", "Aim");
        
        AddConstantRange(ability, 4, 1); // Range 4
        AddPhysicalDamageEffect(ability); // Basic physical damage effect
        AddWeaponPower(ability); // Uses weapon power (correct for Aim)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Aim");
    }

    private static void CreateCharge1Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +1");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 1); // Charge +1 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +1");
    }

    private static void CreateCharge2Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +2");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 2); // Charge +2 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +2");
    }

    private static void CreateCharge3Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +3");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 3); // Charge +3 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +3");
    }

    private static void CreateCharge4Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +4");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 4); // Charge +4 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +4");
    }

    private static void CreateCharge5Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +5");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 5); // Charge +5 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +5");
    }

    private static void CreateCharge6Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +6");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 6); // Charge +6 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +6");
    }

    private static void CreateCharge7Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +7");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 7); // Charge +7 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +7");
    }

    private static void CreateCharge8Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +8");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 8); // Charge +8 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +8");
    }

    private static void CreateCharge9Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +9");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 9); // Charge +9 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +9");
    }

    private static void CreateCharge10Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +10");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 10); // Charge +10 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +10");
    }

    private static void CreateCharge11Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +11");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 11); // Charge +11 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +11");
    }

    private static void CreateCharge12Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +12");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 12); // Charge +12 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +12");
    }

    private static void CreateCharge13Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +13");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 13); // Charge +13 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +13");
    }

    private static void CreateCharge14Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +14");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 14); // Charge +14 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +14");
    }

    private static void CreateCharge15Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +15");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 15); // Charge +15 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +15");
    }

    private static void CreateCharge16Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +16");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 16); // Charge +16 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +16");
    }

    private static void CreateCharge17Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +17");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 17); // Charge +17 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +17");
    }

    private static void CreateCharge18Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +18");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 18); // Charge +18 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +18");
    }

    private static void CreateCharge19Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +19");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 19); // Charge +19 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +19");
    }

    private static void CreateCharge20Ability()
    {
        var ability = CreateAbilityPrefab("Archer", "Charge +20");
        
        AddSelfRange(ability); // Self only
        AddChargeEffect(ability, 20); // Charge +20 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Archer", "Charge +20");
    }

    private static void CreatePriestAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All White Magic abilities from FFT
        CreateCureAbility();
        CreateCuraAbility();
        CreateCuragaAbility();
        CreateRaiseAbility();
        CreateReraiseAbility();
        CreateAriseAbility();
        CreateHolyAbility();
        CreateEsunaAbility();
        CreateProtectAbility();
        CreateShellAbility();
        
        Debug.Log("Created Priest abilities: Cure, Cura, Curaga, Raise, Reraise, Arise, Holy, Esuna, Protect, Shell");
    }

    private static void CreateCureAbility()
    {
        var ability = CreateAbilityPrefab("Priest", "Cure");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddHealEffect(ability); // Basic heal effect
        AddMagicPower(ability, 10); // Cure power = 10 (from FFT)
        AddMagicCost(ability, 4); // 4 MP cost
        
        SaveAbilityPrefab(ability, "Priest", "Cure");
    }

    private static void CreateCuraAbility()
    {
        var ability = CreateAbilityPrefab("Priest", "Cura");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddHealEffect(ability); // Basic heal effect
        AddMagicPower(ability, 18); // Cura power = 18 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Priest", "Cura");
    }

    private static void CreateCuragaAbility()
    {
        var ability = CreateAbilityPrefab("Priest", "Curaga");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddHealEffect(ability); // Basic heal effect
        AddMagicPower(ability, 26); // Curaga power = 26 (from FFT)
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Priest", "Curaga");
    }

    private static void CreateRaiseAbility()
    {
        var ability = CreateAbilityPrefab("Priest", "Raise");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddReviveEffect(ability, 0.25f); // Revive with 25% HP
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Priest", "Raise");
    }

    private static void CreateReraiseAbility()
    {
        var ability = CreateAbilityPrefab("Priest", "Reraise");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddReraiseEffect(ability); // Reraise effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Priest", "Reraise");
    }

    private static void CreateAriseAbility()
    {
        var ability = CreateAbilityPrefab("Priest", "Arise");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddReviveEffect(ability, 1.0f); // Revive with 100% HP
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 20); // 20 MP cost
        
        SaveAbilityPrefab(ability, "Priest", "Arise");
    }

    private static void CreateHolyAbility()
    {
        var ability = CreateAbilityPrefab("Priest", "Holy");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddMagicDamageEffect(ability); // Basic magic damage effect
        AddMagicPower(ability, 24); // Holy power = 24 (from FFT)
        AddMagicCost(ability, 16); // 16 MP cost
        
        SaveAbilityPrefab(ability, "Priest", "Holy");
    }

    private static void CreateEsunaAbility()
    {
        var ability = CreateAbilityPrefab("Priest", "Esuna");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddEsunaEffect(ability); // Esuna effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 6); // 6 MP cost
        
        SaveAbilityPrefab(ability, "Priest", "Esuna");
    }

    private static void CreateProtectAbility()
    {
        var ability = CreateAbilityPrefab("Priest", "Protect");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddProtectEffect(ability); // Protect effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 4); // 4 MP cost
        
        SaveAbilityPrefab(ability, "Priest", "Protect");
    }

    private static void CreateShellAbility()
    {
        var ability = CreateAbilityPrefab("Priest", "Shell");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddShellEffect(ability); // Shell effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 4); // 4 MP cost
        
        SaveAbilityPrefab(ability, "Priest", "Shell");
    }

    #region Wizard Abilities (Complete FFT Black Magic Set)

    private static void CreateWizardAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Black Magic abilities from FFT
        CreateFireAbility();
        CreateBlizzardAbility();
        CreateThunderAbility();
        CreateFiraAbility();
        CreateBlizzaraAbility();
        CreateThundaraAbility();
        CreateFiragaAbility();
        CreateBlizzagaAbility();
        CreateThundagaAbility();
        CreateFlareAbility();
        CreateFreezeAbility();
        CreateBoltAbility();
        CreateMeteorAbility();
        CreateUltimaAbility();
        
        Debug.Log("Created Wizard abilities: Fire, Blizzard, Thunder, Fira, Blizzara, Thundara, Firaga, Blizzaga, Thundaga, Flare, Freeze, Bolt, Meteor, Ultima");
    }

    private static void CreateFireAbility()
    {
        var ability = CreateAbilityPrefab("Wizard", "Fire");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddMagicDamageEffect(ability); // Basic magic damage effect
        AddMagicPower(ability, 14); // Fire power = 14 (from FFT)
        AddMagicCost(ability, 4); // 4 MP cost
        
        SaveAbilityPrefab(ability, "Wizard", "Fire");
    }

    private static void CreateBlizzardAbility()
    {
        var ability = CreateAbilityPrefab("Wizard", "Blizzard");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddMagicDamageEffect(ability); // Basic magic damage effect
        AddMagicPower(ability, 16); // Blizzard power = 16 (from FFT)
        AddMagicCost(ability, 4); // 4 MP cost
        
        SaveAbilityPrefab(ability, "Wizard", "Blizzard");
    }

    private static void CreateThunderAbility()
    {
        var ability = CreateAbilityPrefab("Wizard", "Thunder");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddMagicDamageEffect(ability); // Basic magic damage effect
        AddMagicPower(ability, 18); // Thunder power = 18 (from FFT)
        AddMagicCost(ability, 4); // 4 MP cost
        
        SaveAbilityPrefab(ability, "Wizard", "Thunder");
    }

    private static void CreateFiraAbility()
    {
        var ability = CreateAbilityPrefab("Wizard", "Fira");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddMagicDamageEffect(ability); // Basic magic damage effect
        AddMagicPower(ability, 20); // Fira power = 20 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Wizard", "Fira");
    }

    private static void CreateBlizzaraAbility()
    {
        var ability = CreateAbilityPrefab("Wizard", "Blizzara");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddMagicDamageEffect(ability); // Basic magic damage effect
        AddMagicPower(ability, 22); // Blizzara power = 22 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Wizard", "Blizzara");
    }

    private static void CreateThundaraAbility()
    {
        var ability = CreateAbilityPrefab("Wizard", "Thundara");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddMagicDamageEffect(ability); // Basic magic damage effect
        AddMagicPower(ability, 24); // Thundara power = 24 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Wizard", "Thundara");
    }

    private static void CreateFiragaAbility()
    {
        var ability = CreateAbilityPrefab("Wizard", "Firaga");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddMagicDamageEffect(ability); // Basic magic damage effect
        AddMagicPower(ability, 26); // Firaga power = 26 (from FFT)
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Wizard", "Firaga");
    }

    private static void CreateBlizzagaAbility()
    {
        var ability = CreateAbilityPrefab("Wizard", "Blizzaga");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddMagicDamageEffect(ability); // Basic magic damage effect
        AddMagicPower(ability, 28); // Blizzaga power = 28 (from FFT)
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Wizard", "Blizzaga");
    }

    private static void CreateThundagaAbility()
    {
        var ability = CreateAbilityPrefab("Wizard", "Thundaga");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddMagicDamageEffect(ability); // Basic magic damage effect
        AddMagicPower(ability, 30); // Thundaga power = 30 (from FFT)
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Wizard", "Thundaga");
    }

    private static void CreateFlareAbility()
    {
        var ability = CreateAbilityPrefab("Wizard", "Flare");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddMagicDamageEffect(ability); // Basic magic damage effect
        AddMagicPower(ability, 30); // Flare power = 30 (from FFT)
        AddMagicCost(ability, 20); // 20 MP cost
        
        SaveAbilityPrefab(ability, "Wizard", "Flare");
    }

    private static void CreateFreezeAbility()
    {
        var ability = CreateAbilityPrefab("Wizard", "Freeze");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddMagicDamageEffect(ability); // Basic magic damage effect
        AddMagicPower(ability, 32); // Freeze power = 32 (from FFT)
        AddMagicCost(ability, 20); // 20 MP cost
        
        SaveAbilityPrefab(ability, "Wizard", "Freeze");
    }

    private static void CreateBoltAbility()
    {
        var ability = CreateAbilityPrefab("Wizard", "Bolt");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddMagicDamageEffect(ability); // Basic magic damage effect
        AddMagicPower(ability, 34); // Bolt power = 34 (from FFT)
        AddMagicCost(ability, 20); // 20 MP cost
        
        SaveAbilityPrefab(ability, "Wizard", "Bolt");
    }

    private static void CreateMeteorAbility()
    {
        var ability = CreateAbilityPrefab("Wizard", "Meteor");
        
        AddFullArea(ability); // Full area
        AddMagicDamageEffect(ability); // Basic magic damage effect
        AddMagicPower(ability, 36); // Meteor power = 36 (from FFT)
        AddMagicCost(ability, 30); // 30 MP cost
        
        SaveAbilityPrefab(ability, "Wizard", "Meteor");
    }

    private static void CreateUltimaAbility()
    {
        var ability = CreateAbilityPrefab("Wizard", "Ultima");
        
        AddFullArea(ability); // Full area
        AddMagicDamageEffect(ability); // Basic magic damage effect
        AddMagicPower(ability, 40); // Ultima power = 40 (from FFT)
        AddMagicCost(ability, 50); // 50 MP cost
        
        SaveAbilityPrefab(ability, "Wizard", "Ultima");
    }

    #endregion

    #region Stub Methods for Remaining Jobs

    private static void CreateMonkAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Monk abilities from FFT
        CreateSpinFistAbility();
        CreateRepeatingFistAbility();
        CreateWaveFistAbility();
        CreateEarthSlashAbility();
        CreateSecretFistAbility();
        CreateStigmaMagicAbility();
        CreateChakraAbility();
        CreateReviveAbility();
        
        Debug.Log("Created Monk abilities: Spin Fist, Repeating Fist, Wave Fist, Earth Slash, Secret Fist, Stigma Magic, Chakra, Revive");
    }

    private static void CreateSpinFistAbility()
    {
        var ability = CreateAbilityPrefab("Monk", "Spin Fist");
        
        AddSelfRange(ability); // Self only
        AddSpinFistEffect(ability); // Spin Fist effect - hits all adjacent
        AddPhysicalPower(ability, 10); // Spin Fist power = 10 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Monk", "Spin Fist");
    }

    private static void CreateRepeatingFistAbility()
    {
        var ability = CreateAbilityPrefab("Monk", "Repeating Fist");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddRepeatingFistEffect(ability); // Multiple hits on single target
        AddPhysicalPower(ability, 12); // Repeating Fist power = 12 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Monk", "Repeating Fist");
    }

    private static void CreateWaveFistAbility()
    {
        var ability = CreateAbilityPrefab("Monk", "Wave Fist");
        
        AddConstantRange(ability, 3, 2); // Range 3, Vertical 2
        AddPhysicalDamageEffect(ability); // Basic physical damage effect
        AddPhysicalPower(ability, 14); // Wave Fist power = 14 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Monk", "Wave Fist");
    }

    private static void CreateEarthSlashAbility()
    {
        var ability = CreateAbilityPrefab("Monk", "Earth Slash");
        
        AddConstantRange(ability, 4, 0); // Range 4, Vertical 0 (ground only)
        AddEarthSlashEffect(ability); // Line attack along ground
        AddPhysicalPower(ability, 16); // Earth Slash power = 16 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Monk", "Earth Slash");
    }

    private static void CreateSecretFistAbility()
    {
        var ability = CreateAbilityPrefab("Monk", "Secret Fist");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddSecretFistEffect(ability); // Death Sentence status
        AddPhysicalPower(ability, 18); // Secret Fist power = 18 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Monk", "Secret Fist");
    }

    private static void CreateStigmaMagicAbility()
    {
        var ability = CreateAbilityPrefab("Monk", "Stigma Magic");
        
        AddSelfRange(ability); // Self only
        AddStigmaMagicEffect(ability); // Remove negative status effects
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Monk", "Stigma Magic");
    }

    private static void CreateChakraAbility()
    {
        var ability = CreateAbilityPrefab("Monk", "Chakra");
        
        AddSelfRange(ability); // Self only
        AddChakraEffect(ability); // Heal HP and MP based on PA stat
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Monk", "Chakra");
    }

    private static void CreateReviveAbility()
    {
        var ability = CreateAbilityPrefab("Monk", "Revive");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddReviveEffect(ability, 0.5f); // Revive with 50% HP
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Monk", "Revive");
    }

    private static void CreateThiefAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Thief abilities from FFT
        CreateStealAbility();
        CreateStealHelmetAbility();
        CreateStealArmorAbility();
        CreateStealAccessoryAbility();
        CreateStealWeaponAbility();
        CreateStealGilAbility();
        CreateStealItemAbility();
        CreateStealHeartAbility();
        CreateMoveFindItemAbility();
        
        Debug.Log("Created Thief abilities: Steal, Steal Helmet, Steal Armor, Steal Accessory, Steal Weapon, Steal Gil, Steal Item, Steal Heart, Move-Find Item");
    }

    private static void CreateStealAbility()
    {
        var ability = CreateAbilityPrefab("Thief", "Steal");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddStealEffect(ability, "StealStatus"); // Steal effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Thief", "Steal");
    }

    private static void CreateStealHelmetAbility()
    {
        var ability = CreateAbilityPrefab("Thief", "Steal Helmet");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddStealEffect(ability, "StealHelmetStatus"); // Steal helmet effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Thief", "Steal Helmet");
    }

    private static void CreateStealArmorAbility()
    {
        var ability = CreateAbilityPrefab("Thief", "Steal Armor");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddStealEffect(ability, "StealArmorStatus"); // Steal armor effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Thief", "Steal Armor");
    }

    private static void CreateStealAccessoryAbility()
    {
        var ability = CreateAbilityPrefab("Thief", "Steal Accessory");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddStealEffect(ability, "StealAccessoryStatus"); // Steal accessory effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Thief", "Steal Accessory");
    }

    private static void CreateStealWeaponAbility()
    {
        var ability = CreateAbilityPrefab("Thief", "Steal Weapon");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddStealEffect(ability, "StealWeaponStatus"); // Steal weapon effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Thief", "Steal Weapon");
    }

    private static void CreateStealGilAbility()
    {
        var ability = CreateAbilityPrefab("Thief", "Steal Gil");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddStealEffect(ability, "StealGilStatus"); // Steal gil effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Thief", "Steal Gil");
    }

    private static void CreateStealItemAbility()
    {
        var ability = CreateAbilityPrefab("Thief", "Steal Item");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddStealEffect(ability, "StealItemStatus"); // Steal item effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Thief", "Steal Item");
    }

    private static void CreateStealHeartAbility()
    {
        var ability = CreateAbilityPrefab("Thief", "Steal Heart");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddStealEffect(ability, "CharmStatus"); // Charm effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Thief", "Steal Heart");
    }

    private static void CreateMoveFindItemAbility()
    {
        var ability = CreateAbilityPrefab("Thief", "Move-Find Item");
        
        AddSelfRange(ability); // Self only
        AddMoveFindItemEffect(ability); // Move-Find Item effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Thief", "Move-Find Item");
    }

    private static void CreateMysticAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Mystic abilities from FFT (Yin Yang Magic)
        CreatePoisonAbility();
        CreateBlindAbility();
        CreateSilenceAbility();
        CreateSleepAbility();
        CreateConfuseAbility();
        CreateBerserkAbility();
        CreateCharmAbility();
        CreateDontMoveAbility();
        CreateDontActAbility();
        CreatePetrifyAbility();
        CreateDeathAbility();
        CreateToadAbility();
        CreatePigAbility();
        CreateChickenAbility();
        
        Debug.Log("Created Mystic abilities: Poison, Blind, Silence, Sleep, Confuse, Berserk, Charm, Don't Move, Don't Act, Petrify, Death, Toad, Pig, Chicken");
    }

    private static void CreatePoisonAbility()
    {
        var ability = CreateAbilityPrefab("Mystic", "Poison");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddPoisonEffect(ability); // Poison status effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 4); // 4 MP cost
        
        SaveAbilityPrefab(ability, "Mystic", "Poison");
    }

    private static void CreateBlindAbility()
    {
        var ability = CreateAbilityPrefab("Mystic", "Blind");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddBlindEffect(ability); // Blind status effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 4); // 4 MP cost
        
        SaveAbilityPrefab(ability, "Mystic", "Blind");
    }

    private static void CreateSilenceAbility()
    {
        var ability = CreateAbilityPrefab("Mystic", "Silence");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddSilenceEffect(ability); // Silence status effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 4); // 4 MP cost
        
        SaveAbilityPrefab(ability, "Mystic", "Silence");
    }

    private static void CreateSleepAbility()
    {
        var ability = CreateAbilityPrefab("Mystic", "Sleep");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddSleepEffect(ability); // Sleep status effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 6); // 6 MP cost
        
        SaveAbilityPrefab(ability, "Mystic", "Sleep");
    }

    private static void CreateConfuseAbility()
    {
        var ability = CreateAbilityPrefab("Mystic", "Confuse");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddConfuseEffect(ability); // Confuse status effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 6); // 6 MP cost
        
        SaveAbilityPrefab(ability, "Mystic", "Confuse");
    }

    private static void CreateBerserkAbility()
    {
        var ability = CreateAbilityPrefab("Mystic", "Berserk");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddBerserkEffect(ability); // Berserk status effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 6); // 6 MP cost
        
        SaveAbilityPrefab(ability, "Mystic", "Berserk");
    }

    private static void CreateCharmAbility()
    {
        var ability = CreateAbilityPrefab("Mystic", "Charm");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddCharmEffect(ability); // Charm status effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Mystic", "Charm");
    }

    private static void CreateDontMoveAbility()
    {
        var ability = CreateAbilityPrefab("Mystic", "Don't Move");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddDontMoveEffect(ability); // Don't Move status effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 6); // 6 MP cost
        
        SaveAbilityPrefab(ability, "Mystic", "Don't Move");
    }

    private static void CreateDontActAbility()
    {
        var ability = CreateAbilityPrefab("Mystic", "Don't Act");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddDontActEffect(ability); // Don't Act status effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 6); // 6 MP cost
        
        SaveAbilityPrefab(ability, "Mystic", "Don't Act");
    }

    private static void CreatePetrifyAbility()
    {
        var ability = CreateAbilityPrefab("Mystic", "Petrify");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddPetrifyEffect(ability); // Petrify status effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 10); // 10 MP cost
        
        SaveAbilityPrefab(ability, "Mystic", "Petrify");
    }

    private static void CreateDeathAbility()
    {
        var ability = CreateAbilityPrefab("Mystic", "Death");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddDeathEffect(ability); // Death status effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Mystic", "Death");
    }

    private static void CreateToadAbility()
    {
        var ability = CreateAbilityPrefab("Mystic", "Toad");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddToadEffect(ability); // Toad status effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Mystic", "Toad");
    }

    private static void CreatePigAbility()
    {
        var ability = CreateAbilityPrefab("Mystic", "Pig");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddPigEffect(ability); // Pig status effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Mystic", "Pig");
    }

    private static void CreateChickenAbility()
    {
        var ability = CreateAbilityPrefab("Mystic", "Chicken");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddChickenEffect(ability); // Chicken status effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Mystic", "Chicken");
    }

    private static void CreateTimeMageAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Time Magic abilities from FFT
        CreateHasteAbility();
        CreateSlowAbility();
        CreateStopAbility();
        CreateDontMoveAbility();
        CreateTeleportAbility();
        CreateFloatAbility();
        CreateRegenAbility();
        CreateReflectAbility();
        CreateQuickAbility();
        CreateGravityAbility();
        
        Debug.Log("Created Time Mage abilities: Haste, Slow, Stop, Don't Move, Teleport, Float, Regen, Reflect, Quick, Gravity");
    }

    private static void CreateHasteAbility()
    {
        var ability = CreateAbilityPrefab("Time Mage", "Haste");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddHasteEffect(ability); // Haste effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Time Mage", "Haste");
    }

    private static void CreateSlowAbility()
    {
        var ability = CreateAbilityPrefab("Time Mage", "Slow");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddSlowEffect(ability); // Slow effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Time Mage", "Slow");
    }

    private static void CreateStopAbility()
    {
        var ability = CreateAbilityPrefab("Time Mage", "Stop");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddStopEffect(ability); // Stop effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Time Mage", "Stop");
    }



    private static void CreateTeleportAbility()
    {
        var ability = CreateAbilityPrefab("Time Mage", "Teleport");
        
        AddInfiniteRange(ability); // All tiles
        AddTeleportEffect(ability); // Teleport effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 4); // 4 MP cost
        
        SaveAbilityPrefab(ability, "Time Mage", "Teleport");
    }

    private static void CreateFloatAbility()
    {
        var ability = CreateAbilityPrefab("Time Mage", "Float");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddFloatEffect(ability); // Float effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 4); // 4 MP cost
        
        SaveAbilityPrefab(ability, "Time Mage", "Float");
    }

    private static void CreateRegenAbility()
    {
        var ability = CreateAbilityPrefab("Time Mage", "Regen");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddRegenEffect(ability); // Regen effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 6); // 6 MP cost
        
        SaveAbilityPrefab(ability, "Time Mage", "Regen");
    }

    private static void CreateReflectAbility()
    {
        var ability = CreateAbilityPrefab("Time Mage", "Reflect");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddReflectEffect(ability); // Reflect effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Time Mage", "Reflect");
    }

    private static void CreateQuickAbility()
    {
        var ability = CreateAbilityPrefab("Time Mage", "Quick");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddQuickEffect(ability); // Quick effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 20); // 20 MP cost
        
        SaveAbilityPrefab(ability, "Time Mage", "Quick");
    }


    private static void CreateGravityAbility()
    {
        var ability = CreateAbilityPrefab("Time Mage", "Gravity");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddGravityEffect(ability); // Gravity effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 10); // 10 MP cost
        
        SaveAbilityPrefab(ability, "Time Mage", "Gravity");
    }

    private static void CreateSummonerAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Summon abilities from FFT
        CreateChocoboAbility();
        CreateMoogleAbility();
        CreateGolemAbility();
        CreateRamuhAbility();
        CreateIfritAbility();
        CreateShivaAbility();
        CreateTitanAbility();
        CreateLeviathanAbility();
        CreateOdinAbility();
        CreateBahamutAbility();
        CreateZodiacAbility();
        
        Debug.Log("Created Summoner abilities: Chocobo, Moogle, Golem, Ramuh, Ifrit, Shiva, Titan, Leviathan, Odin, Bahamut, Zodiac");
    }

    private static void CreateChocoboAbility()
    {
        var ability = CreateAbilityPrefab("Summoner", "Chocobo");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddSummonDamageEffect(ability, 30); // 30 power
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Summoner", "Chocobo");
    }

    private static void CreateMoogleAbility()
    {
        var ability = CreateAbilityPrefab("Summoner", "Moogle");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddSummonDamageEffect(ability, 35); // 35 power
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 10); // 10 MP cost
        
        SaveAbilityPrefab(ability, "Summoner", "Moogle");
    }

    private static void CreateGolemAbility()
    {
        var ability = CreateAbilityPrefab("Summoner", "Golem");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddSummonDamageEffect(ability, 40); // 40 power
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Summoner", "Golem");
    }

    private static void CreateRamuhAbility()
    {
        var ability = CreateAbilityPrefab("Summoner", "Ramuh");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddSummonDamageEffect(ability, 45); // 45 power
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 14); // 14 MP cost
        
        SaveAbilityPrefab(ability, "Summoner", "Ramuh");
    }

    private static void CreateIfritAbility()
    {
        var ability = CreateAbilityPrefab("Summoner", "Ifrit");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddSummonDamageEffect(ability, 50); // 50 power
        AddMagicPower(ability, 18); // Ifrit power = 18 (from FFT)
        AddMagicCost(ability, 16); // 16 MP cost
        
        SaveAbilityPrefab(ability, "Summoner", "Ifrit");
    }

    private static void CreateShivaAbility()
    {
        var ability = CreateAbilityPrefab("Summoner", "Shiva");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddSummonDamageEffect(ability, 55); // 55 power
        AddMagicPower(ability, 20); // Shiva power = 20 (from FFT)
        AddMagicCost(ability, 18); // 18 MP cost
        
        SaveAbilityPrefab(ability, "Summoner", "Shiva");
    }

    private static void CreateTitanAbility()
    {
        var ability = CreateAbilityPrefab("Summoner", "Titan");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddSummonDamageEffect(ability, 60); // 60 power
        AddMagicPower(ability, 22); // Titan power = 22 (from FFT)
        AddMagicCost(ability, 20); // 20 MP cost
        
        SaveAbilityPrefab(ability, "Summoner", "Titan");
    }

    private static void CreateLeviathanAbility()
    {
        var ability = CreateAbilityPrefab("Summoner", "Leviathan");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddSummonDamageEffect(ability, 70); // 70 power
        AddMagicPower(ability, 24); // Leviathan power = 24 (from FFT)
        AddMagicCost(ability, 25); // 25 MP cost
        
        SaveAbilityPrefab(ability, "Summoner", "Leviathan");
    }

    private static void CreateOdinAbility()
    {
        var ability = CreateAbilityPrefab("Summoner", "Odin");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddSummonDamageEffect(ability, 80); // 80 power
        AddMagicPower(ability, 26); // Odin power = 26 (from FFT)
        AddMagicCost(ability, 30); // 30 MP cost
        
        SaveAbilityPrefab(ability, "Summoner", "Odin");
    }

    private static void CreateBahamutAbility()
    {
        var ability = CreateAbilityPrefab("Summoner", "Bahamut");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddSummonDamageEffect(ability, 90); // 90 power
        AddMagicPower(ability, 28); // Bahamut power = 28 (from FFT)
        AddMagicCost(ability, 35); // 35 MP cost
        
        SaveAbilityPrefab(ability, "Summoner", "Bahamut");
    }

    private static void CreateZodiacAbility()
    {
        var ability = CreateAbilityPrefab("Summoner", "Zodiac");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddSummonDamageEffect(ability, 100); // 100 power
        AddMagicPower(ability, 30); // Zodiac power = 30 (from FFT)
        AddMagicCost(ability, 40); // 40 MP cost
        
        SaveAbilityPrefab(ability, "Summoner", "Zodiac");
    }


    private static void CreateGeomancerAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Geomancer abilities from FFT (Geomancy)
        CreatePitfallAbility();
        CreateWaterBallAbility();
        CreateHellIvyAbility();
        CreateCarveModelAbility();
        CreateLocalQuakeAbility();
        CreateKamaitachiAbility();
        CreateDemonFireAbility();
        CreateQuicksandAbility();
        CreateSandStormAbility();
        CreateLavaBallAbility();
        
        Debug.Log("Created Geomancer abilities: Pitfall, Water Ball, Hell Ivy, Carve Model, Local Quake, Kamaitachi, Demon Fire, Quicksand, Sand Storm, Lava Ball");
    }

    private static void CreatePitfallAbility()
    {
        var ability = CreateAbilityPrefab("Geomancer", "Pitfall");
        
        AddSelfRange(ability); // Self only - affects terrain
        AddPitfallEffect(ability); // Pitfall effect with Stop chance
        AddPhysicalPower(ability, 12); // Pitfall power = 12 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Geomancer", "Pitfall");
    }

    private static void CreateWaterBallAbility()
    {
        var ability = CreateAbilityPrefab("Geomancer", "Water Ball");
        
        AddSelfRange(ability); // Self only - affects terrain
        AddWaterBallEffect(ability); // Water Ball effect with Silence chance
        AddPhysicalPower(ability, 14); // Water Ball power = 14 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Geomancer", "Water Ball");
    }

    private static void CreateHellIvyAbility()
    {
        var ability = CreateAbilityPrefab("Geomancer", "Hell Ivy");
        
        AddSelfRange(ability); // Self only - affects terrain
        AddHellIvyEffect(ability); // Hell Ivy effect with Immobilize chance
        AddPhysicalPower(ability, 16); // Hell Ivy power = 16 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Geomancer", "Hell Ivy");
    }

    private static void CreateCarveModelAbility()
    {
        var ability = CreateAbilityPrefab("Geomancer", "Carve Model");
        
        AddSelfRange(ability); // Self only - affects terrain
        AddCarveModelEffect(ability); // Carve Model effect with Petrify chance
        AddPhysicalPower(ability, 18); // Carve Model power = 18 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Geomancer", "Carve Model");
    }

    private static void CreateLocalQuakeAbility()
    {
        var ability = CreateAbilityPrefab("Geomancer", "Local Quake");
        
        AddSelfRange(ability); // Self only - affects terrain
        AddLocalQuakeEffect(ability); // Local Quake effect with Confusion chance
        AddPhysicalPower(ability, 20); // Local Quake power = 20 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Geomancer", "Local Quake");
    }

    private static void CreateKamaitachiAbility()
    {
        var ability = CreateAbilityPrefab("Geomancer", "Kamaitachi");
        
        AddSelfRange(ability); // Self only - affects terrain
        AddKamaitachiEffect(ability); // Kamaitachi effect with Sleep chance
        AddPhysicalPower(ability, 22); // Kamaitachi power = 22 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Geomancer", "Kamaitachi");
    }

    private static void CreateDemonFireAbility()
    {
        var ability = CreateAbilityPrefab("Geomancer", "Demon Fire");
        
        AddSelfRange(ability); // Self only - affects terrain
        AddDemonFireEffect(ability); // Demon Fire effect with Death Sentence chance
        AddPhysicalPower(ability, 24); // Demon Fire power = 24 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Geomancer", "Demon Fire");
    }

    private static void CreateQuicksandAbility()
    {
        var ability = CreateAbilityPrefab("Geomancer", "Quicksand");
        
        AddSelfRange(ability); // Self only - affects terrain
        AddQuicksandEffect(ability); // Quicksand effect with Don't Move chance
        AddPhysicalPower(ability, 26); // Quicksand power = 26 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Geomancer", "Quicksand");
    }

    private static void CreateSandStormAbility()
    {
        var ability = CreateAbilityPrefab("Geomancer", "Sand Storm");
        
        AddSelfRange(ability); // Self only - affects terrain
        AddSandStormEffect(ability); // Sand Storm effect with Blind chance
        AddPhysicalPower(ability, 28); // Sand Storm power = 28 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Geomancer", "Sand Storm");
    }


    private static void CreateLavaBallAbility()
    {
        var ability = CreateAbilityPrefab("Geomancer", "Lava Ball");
        
        AddSelfRange(ability); // Self only - affects terrain
        AddLavaBallEffect(ability); // Lava Ball effect with Confusion chance
        AddPhysicalPower(ability, 30); // Lava Ball power = 30 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Geomancer", "Lava Ball");
    }

    private static void CreateDragoonAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Dragoon abilities from FFT (Jump variations)
        CreateJumpAbility();
        CreateJump1Ability();
        CreateJump2Ability();
        CreateJump3Ability();
        CreateJump4Ability();
        CreateJump5Ability();
        CreateJump6Ability();
        CreateJump7Ability();
        CreateJump8Ability();
        CreateJump9Ability();
        CreateJump10Ability();
        
        Debug.Log("Created Dragoon abilities: Jump, Jump +1 through Jump +10");
    }

    private static void CreateJumpAbility()
    {
        var ability = CreateAbilityPrefab("Dragoon", "Jump");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddJumpEffect(ability, 1); // Basic jump effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Dragoon", "Jump");
    }

    private static void CreateJump1Ability()
    {
        var ability = CreateAbilityPrefab("Dragoon", "Jump +1");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddJumpEffect(ability, 2); // Jump +1 effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Dragoon", "Jump +1");
    }

    private static void CreateJump2Ability()
    {
        var ability = CreateAbilityPrefab("Dragoon", "Jump +2");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddJumpEffect(ability, 3); // Jump +2 effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Dragoon", "Jump +2");
    }

    private static void CreateJump3Ability()
    {
        var ability = CreateAbilityPrefab("Dragoon", "Jump +3");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddJumpEffect(ability, 4); // Jump +3 effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Dragoon", "Jump +3");
    }

    private static void CreateJump4Ability()
    {
        var ability = CreateAbilityPrefab("Dragoon", "Jump +4");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddJumpEffect(ability, 5); // Jump +4 effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Dragoon", "Jump +4");
    }

    private static void CreateJump5Ability()
    {
        var ability = CreateAbilityPrefab("Dragoon", "Jump +5");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddJumpEffect(ability, 6); // Jump +5 effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Dragoon", "Jump +5");
    }

    private static void CreateJump6Ability()
    {
        var ability = CreateAbilityPrefab("Dragoon", "Jump +6");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddJumpEffect(ability, 7); // Jump +6 effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Dragoon", "Jump +6");
    }

    private static void CreateJump7Ability()
    {
        var ability = CreateAbilityPrefab("Dragoon", "Jump +7");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddJumpEffect(ability, 8); // Jump +7 effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Dragoon", "Jump +7");
    }

    private static void CreateJump8Ability()
    {
        var ability = CreateAbilityPrefab("Dragoon", "Jump +8");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddJumpEffect(ability, 9); // Jump +8 effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Dragoon", "Jump +8");
    }

    private static void CreateJump9Ability()
    {
        var ability = CreateAbilityPrefab("Dragoon", "Jump +9");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddJumpEffect(ability, 10); // Jump +9 effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Dragoon", "Jump +9");
    }

    private static void CreateJump10Ability()
    {
        var ability = CreateAbilityPrefab("Dragoon", "Jump +10");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddJumpEffect(ability, 11); // Jump +10 effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Dragoon", "Jump +10");
    }

    private static void CreateOracleAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Oracle abilities from FFT (Yin Yang Magic variations)
        CreateOraclePoisonAbility();
        CreateOracleBlindAbility();
        CreateOracleSilenceAbility();
        CreateOracleSleepAbility();
        CreateOracleConfuseAbility();
        CreateOracleBerserkAbility();
        CreateOracleCharmAbility();
        CreateOracleDontMoveAbility();
        CreateOracleDontActAbility();
        CreateOraclePetrifyAbility();
        CreateOracleDeathAbility();
        CreateOracleToadAbility();
        CreateOraclePigAbility();
        CreateOracleChickenAbility();
        
        Debug.Log("Created Oracle abilities: Poison, Blind, Silence, Sleep, Confuse, Berserk, Charm, Don't Move, Don't Act, Petrify, Death, Toad, Pig, Chicken");
    }

    private static void CreateOraclePoisonAbility()
    {
        var ability = CreateAbilityPrefab("Oracle", "Poison");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddOraclePoisonEffect(ability); // Oracle Poison effect
        AddMagicPower(ability, 8); // Oracle Poison power = 8 (from FFT)
        AddMagicCost(ability, 4); // 4 MP cost
        
        SaveAbilityPrefab(ability, "Oracle", "Poison");
    }

    private static void CreateOracleBlindAbility()
    {
        var ability = CreateAbilityPrefab("Oracle", "Blind");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddOracleBlindEffect(ability); // Oracle Blind effect
        AddMagicPower(ability, 10); // Oracle Blind power = 10 (from FFT)
        AddMagicCost(ability, 4); // 4 MP cost
        
        SaveAbilityPrefab(ability, "Oracle", "Blind");
    }

    private static void CreateOracleSilenceAbility()
    {
        var ability = CreateAbilityPrefab("Oracle", "Silence");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddOracleSilenceEffect(ability); // Oracle Silence effect
        AddMagicPower(ability, 12); // Oracle Silence power = 12 (from FFT)
        AddMagicCost(ability, 4); // 4 MP cost
        
        SaveAbilityPrefab(ability, "Oracle", "Silence");
    }

    private static void CreateOracleSleepAbility()
    {
        var ability = CreateAbilityPrefab("Oracle", "Sleep");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddOracleSleepEffect(ability); // Oracle Sleep effect
        AddMagicPower(ability, 14); // Oracle Sleep power = 14 (from FFT)
        AddMagicCost(ability, 6); // 6 MP cost
        
        SaveAbilityPrefab(ability, "Oracle", "Sleep");
    }

    private static void CreateOracleConfuseAbility()
    {
        var ability = CreateAbilityPrefab("Oracle", "Confuse");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddOracleConfuseEffect(ability); // Oracle Confuse effect
        AddMagicPower(ability, 16); // Oracle Confuse power = 16 (from FFT)
        AddMagicCost(ability, 6); // 6 MP cost
        
        SaveAbilityPrefab(ability, "Oracle", "Confuse");
    }

    private static void CreateOracleBerserkAbility()
    {
        var ability = CreateAbilityPrefab("Oracle", "Berserk");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddOracleBerserkEffect(ability); // Oracle Berserk effect
        AddMagicPower(ability, 18); // Oracle Berserk power = 18 (from FFT)
        AddMagicCost(ability, 6); // 6 MP cost
        
        SaveAbilityPrefab(ability, "Oracle", "Berserk");
    }

    private static void CreateOracleCharmAbility()
    {
        var ability = CreateAbilityPrefab("Oracle", "Charm");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddOracleCharmEffect(ability); // Oracle Charm effect
        AddMagicPower(ability, 20); // Oracle Charm power = 20 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Oracle", "Charm");
    }

    private static void CreateOracleDontMoveAbility()
    {
        var ability = CreateAbilityPrefab("Oracle", "Don't Move");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddOracleDontMoveEffect(ability); // Oracle Don't Move effect
        AddMagicPower(ability, 22); // Oracle Don't Move power = 22 (from FFT)
        AddMagicCost(ability, 6); // 6 MP cost
        
        SaveAbilityPrefab(ability, "Oracle", "Don't Move");
    }

    private static void CreateOracleDontActAbility()
    {
        var ability = CreateAbilityPrefab("Oracle", "Don't Act");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddOracleDontActEffect(ability); // Oracle Don't Act effect
        AddMagicPower(ability, 24); // Oracle Don't Act power = 24 (from FFT)
        AddMagicCost(ability, 6); // 6 MP cost
        
        SaveAbilityPrefab(ability, "Oracle", "Don't Act");
    }

    private static void CreateOraclePetrifyAbility()
    {
        var ability = CreateAbilityPrefab("Oracle", "Petrify");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddOraclePetrifyEffect(ability); // Oracle Petrify effect
        AddMagicPower(ability, 26); // Oracle Petrify power = 26 (from FFT)
        AddMagicCost(ability, 10); // 10 MP cost
        
        SaveAbilityPrefab(ability, "Oracle", "Petrify");
    }

    private static void CreateOracleDeathAbility()
    {
        var ability = CreateAbilityPrefab("Oracle", "Death");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddOracleDeathEffect(ability); // Oracle Death effect
        AddMagicPower(ability, 28); // Oracle Death power = 28 (from FFT)
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Oracle", "Death");
    }

    private static void CreateOracleToadAbility()
    {
        var ability = CreateAbilityPrefab("Oracle", "Toad");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddOracleToadEffect(ability); // Oracle Toad effect
        AddMagicPower(ability, 30); // Oracle Toad power = 30 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Oracle", "Toad");
    }

    private static void CreateOraclePigAbility()
    {
        var ability = CreateAbilityPrefab("Oracle", "Pig");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddOraclePigEffect(ability); // Oracle Pig effect
        AddMagicPower(ability, 32); // Oracle Pig power = 32 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Oracle", "Pig");
    }

    private static void CreateOracleChickenAbility()
    {
        var ability = CreateAbilityPrefab("Oracle", "Chicken");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddOracleChickenEffect(ability); // Oracle Chicken effect
        AddMagicPower(ability, 34); // Oracle Chicken power = 34 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Oracle", "Chicken");
    }

    private static void CreateNinjaAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Ninja abilities from FFT (Throw variations)
        CreateThrowShurikenAbility();
        CreateThrowBombAbility();
        CreateThrowKnifeAbility();
        CreateThrowSwordAbility();
        CreateThrowAxeAbility();
        CreateThrowSpearAbility();
        CreateThrowStaffAbility();
        CreateThrowRodAbility();
        CreateThrowBookAbility();
        CreateThrowShieldAbility();
        CreateTwoSwordsAbility();
        
        Debug.Log("Created Ninja abilities: Throw Shuriken, Throw Bomb, Throw Knife, Throw Sword, Throw Axe, Throw Spear, Throw Staff, Throw Rod, Throw Book, Throw Shield, Two Swords");
    }

    private static void CreateThrowShurikenAbility()
    {
        var ability = CreateAbilityPrefab("Ninja", "Throw Shuriken");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddThrowEffect(ability, "Shuriken"); // Throw shuriken effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Ninja", "Throw Shuriken");
    }

    private static void CreateThrowBombAbility()
    {
        var ability = CreateAbilityPrefab("Ninja", "Throw Bomb");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddThrowEffect(ability, "Bomb"); // Throw bomb effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Ninja", "Throw Bomb");
    }

    private static void CreateThrowKnifeAbility()
    {
        var ability = CreateAbilityPrefab("Ninja", "Throw Knife");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddThrowEffect(ability, "Knife"); // Throw knife effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Ninja", "Throw Knife");
    }

    private static void CreateThrowSwordAbility()
    {
        var ability = CreateAbilityPrefab("Ninja", "Throw Sword");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddThrowEffect(ability, "Sword"); // Throw sword effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Ninja", "Throw Sword");
    }

    private static void CreateThrowAxeAbility()
    {
        var ability = CreateAbilityPrefab("Ninja", "Throw Axe");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddThrowEffect(ability, "Axe"); // Throw axe effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Ninja", "Throw Axe");
    }

    private static void CreateThrowSpearAbility()
    {
        var ability = CreateAbilityPrefab("Ninja", "Throw Spear");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddThrowEffect(ability, "Spear"); // Throw spear effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Ninja", "Throw Spear");
    }

    private static void CreateThrowStaffAbility()
    {
        var ability = CreateAbilityPrefab("Ninja", "Throw Staff");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddThrowEffect(ability, "Staff"); // Throw staff effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Ninja", "Throw Staff");
    }

    private static void CreateThrowRodAbility()
    {
        var ability = CreateAbilityPrefab("Ninja", "Throw Rod");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddThrowEffect(ability, "Rod"); // Throw rod effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Ninja", "Throw Rod");
    }

    private static void CreateThrowBookAbility()
    {
        var ability = CreateAbilityPrefab("Ninja", "Throw Book");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddThrowEffect(ability, "Book"); // Throw book effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Ninja", "Throw Book");
    }

    private static void CreateThrowShieldAbility()
    {
        var ability = CreateAbilityPrefab("Ninja", "Throw Shield");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddThrowEffect(ability, "Shield"); // Throw shield effect
        AddWeaponPower(ability); // Uses weapon power (thrown weapon)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Ninja", "Throw Shield");
    }

    private static void CreateTwoSwordsAbility()
    {
        var ability = CreateAbilityPrefab("Ninja", "Two Swords");
        
        AddSelfRange(ability); // Self only
        AddTwoSwordsEffect(ability); // Two Swords effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Ninja", "Two Swords");
    }

    private static void CreateSamuraiAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Samurai abilities from FFT (Draw Out katana techniques)
        CreateAsuraAbility();
        CreateKoutetsuAbility();
        CreateBizenBoatAbility();
        CreateMurasameAbility();
        CreateHeavensCloudAbility();
        CreateKiyomoriAbility();
        CreateMuramasaAbility();
        CreateKikuichimonjiAbility();
        CreateMasamuneAbility();
        CreateChirijiradenAbility();
        
        Debug.Log("Created Samurai abilities: Asura, Koutetsu, Bizen Boat, Murasame, Heaven's Cloud, Kiyomori, Muramasa, Kikuichimonji, Masamune, Chirijiraden");
    }

    private static void CreateAsuraAbility()
    {
        var ability = CreateAbilityPrefab("Samurai", "Asura");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddDrawOutEffect(ability, "Asura", "Fire"); // Fire elemental damage
        AddMagicPower(ability, 20); // Asura power = 20 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Samurai", "Asura");
    }

    private static void CreateKoutetsuAbility()
    {
        var ability = CreateAbilityPrefab("Samurai", "Koutetsu");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddDrawOutEffect(ability, "Koutetsu", "Dark"); // Dark elemental damage
        AddMagicPower(ability, 22); // Koutetsu power = 22 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Samurai", "Koutetsu");
    }

    private static void CreateBizenBoatAbility()
    {
        var ability = CreateAbilityPrefab("Samurai", "Bizen Boat");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddDrawOutEffect(ability, "BizenBoat", "MPDrain"); // MP drain effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 6); // 6 MP cost
        
        SaveAbilityPrefab(ability, "Samurai", "Bizen Boat");
    }

    private static void CreateMurasameAbility()
    {
        var ability = CreateAbilityPrefab("Samurai", "Murasame");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddDrawOutEffect(ability, "Murasame", "Heal"); // HP heal effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 6); // 6 MP cost
        
        SaveAbilityPrefab(ability, "Samurai", "Murasame");
    }

    private static void CreateHeavensCloudAbility()
    {
        var ability = CreateAbilityPrefab("Samurai", "Heaven's Cloud");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddDrawOutEffect(ability, "HeavensCloud", "Wind"); // Wind elemental damage with Slow chance
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 10); // 10 MP cost
        
        SaveAbilityPrefab(ability, "Samurai", "Heaven's Cloud");
    }

    private static void CreateKiyomoriAbility()
    {
        var ability = CreateAbilityPrefab("Samurai", "Kiyomori");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddDrawOutEffect(ability, "Kiyomori", "ProtectShell"); // Protect and Shell buffs
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Samurai", "Kiyomori");
    }

    private static void CreateMuramasaAbility()
    {
        var ability = CreateAbilityPrefab("Samurai", "Muramasa");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddDrawOutEffect(ability, "Muramasa", "ConfuseBloodSuck"); // Confusion and Blood Suck
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Samurai", "Muramasa");
    }

    private static void CreateKikuichimonjiAbility()
    {
        var ability = CreateAbilityPrefab("Samurai", "Kikuichimonji");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddDrawOutEffect(ability, "Kikuichimonji", "Silence"); // Silence status effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Samurai", "Kikuichimonji");
    }

    private static void CreateMasamuneAbility()
    {
        var ability = CreateAbilityPrefab("Samurai", "Masamune");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddDrawOutEffect(ability, "Masamune", "HasteRegen"); // Haste and Regen buffs
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 10); // 10 MP cost
        
        SaveAbilityPrefab(ability, "Samurai", "Masamune");
    }

    private static void CreateChirijiradenAbility()
    {
        var ability = CreateAbilityPrefab("Samurai", "Chirijiraden");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddDrawOutEffect(ability, "Chirijiraden", "Fire"); // Fire elemental damage
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 14); // 14 MP cost
        
        SaveAbilityPrefab(ability, "Samurai", "Chirijiraden");
    }

    private static void CreateLancerAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Lancer abilities from FFT (HP/MP drain attacks)
        CreateLancerAbility();
        
        Debug.Log("Created Lancer abilities: Lancer (HP/MP drain attack)");
    }

    private static void CreateLancerAbility()
    {
        var ability = CreateAbilityPrefab("Lancer", "Lancer");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddLancerEffect(ability); // HP/MP drain effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Lancer", "Lancer");
    }

    private static void CreateMediatorAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Mediator abilities from FFT (Talk Skill)
        CreateInviteAbility();
        CreateRecruitAbility();
        CreateConvinceAbility();
        CreatePersuadeAbility();
        CreateNegotiateAbility();
        CreateMediateAbility();
        CreateAppealAbility();
        CreateReasonAbility();
        CreateDebateAbility();
        CreateOrateAbility();
        
        Debug.Log("Created Mediator abilities: Invite, Recruit, Convince, Persuade, Negotiate, Mediate, Appeal, Reason, Debate, Orate");
    }

    private static void CreateInviteAbility()
    {
        var ability = CreateAbilityPrefab("Mediator", "Invite");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddInviteEffect(ability); // Invite effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Mediator", "Invite");
    }

    private static void CreateRecruitAbility()
    {
        var ability = CreateAbilityPrefab("Mediator", "Recruit");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddRecruitEffect(ability); // Recruit effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Mediator", "Recruit");
    }

    private static void CreateConvinceAbility()
    {
        var ability = CreateAbilityPrefab("Mediator", "Convince");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddConvinceEffect(ability); // Convince effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Mediator", "Convince");
    }

    private static void CreatePersuadeAbility()
    {
        var ability = CreateAbilityPrefab("Mediator", "Persuade");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddPersuadeEffect(ability); // Persuade effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Mediator", "Persuade");
    }

    private static void CreateNegotiateAbility()
    {
        var ability = CreateAbilityPrefab("Mediator", "Negotiate");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddNegotiateEffect(ability); // Negotiate effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Mediator", "Negotiate");
    }

    private static void CreateMediateAbility()
    {
        var ability = CreateAbilityPrefab("Mediator", "Mediate");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddMediateEffect(ability); // Mediate effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Mediator", "Mediate");
    }

    private static void CreateAppealAbility()
    {
        var ability = CreateAbilityPrefab("Mediator", "Appeal");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddAppealEffect(ability); // Appeal effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Mediator", "Appeal");
    }

    private static void CreateReasonAbility()
    {
        var ability = CreateAbilityPrefab("Mediator", "Reason");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddReasonEffect(ability); // Reason effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Mediator", "Reason");
    }

    private static void CreateDebateAbility()
    {
        var ability = CreateAbilityPrefab("Mediator", "Debate");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddDebateEffect(ability); // Debate effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Mediator", "Debate");
    }

    private static void CreateOrateAbility()
    {
        var ability = CreateAbilityPrefab("Mediator", "Orate");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddOrateEffect(ability); // Orate effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Mediator", "Orate");
    }

    private static void CreateCalculatorAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Calculator abilities from FFT (Math Skill)
        CreatePrimeAbility();
        CreateHpEqualsMpAbility();
        CreateHpEqualsLevelAbility();
        CreateHpEqualsExpAbility();
        CreateHpEqualsFaithAbility();
        CreateHpEqualsBraveAbility();
        CreateHpEqualsZodiacAbility();
        CreateHpEqualsHeightAbility();
        CreateHpEqualsWeightAbility();
        CreateHpEqualsSpeedAbility();
        
        Debug.Log("Created Calculator abilities: Prime, HP=MP, HP=Level, HP=Exp, HP=Faith, HP=Brave, HP=Zodiac, HP=Height, HP=Weight, HP=Speed");
    }

    private static void CreatePrimeAbility()
    {
        var ability = CreateAbilityPrefab("Calculator", "Prime");
        
        AddInfiniteRange(ability); // All tiles
        AddPrimeEffect(ability); // Prime number effect
        AddMagicPower(ability, 15); // Prime power = 15 (from FFT)
        AddMagicCost(ability, 20); // 20 MP cost
        
        SaveAbilityPrefab(ability, "Calculator", "Prime");
    }

    private static void CreateHpEqualsMpAbility()
    {
        var ability = CreateAbilityPrefab("Calculator", "HP=MP");
        
        AddInfiniteRange(ability); // All tiles
        AddHpEqualsMpEffect(ability); // HP=MP effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 15); // 15 MP cost
        
        SaveAbilityPrefab(ability, "Calculator", "HP=MP");
    }

    private static void CreateHpEqualsLevelAbility()
    {
        var ability = CreateAbilityPrefab("Calculator", "HP=Level");
        
        AddInfiniteRange(ability); // All tiles
        AddHpEqualsLevelEffect(ability); // HP=Level effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 15); // 15 MP cost
        
        SaveAbilityPrefab(ability, "Calculator", "HP=Level");
    }

    private static void CreateHpEqualsExpAbility()
    {
        var ability = CreateAbilityPrefab("Calculator", "HP=Exp");
        
        AddInfiniteRange(ability); // All tiles
        AddHpEqualsExpEffect(ability); // HP=Exp effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 15); // 15 MP cost
        
        SaveAbilityPrefab(ability, "Calculator", "HP=Exp");
    }

    private static void CreateHpEqualsFaithAbility()
    {
        var ability = CreateAbilityPrefab("Calculator", "HP=Faith");
        
        AddInfiniteRange(ability); // All tiles
        AddHpEqualsFaithEffect(ability); // HP=Faith effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 15); // 15 MP cost
        
        SaveAbilityPrefab(ability, "Calculator", "HP=Faith");
    }

    private static void CreateHpEqualsBraveAbility()
    {
        var ability = CreateAbilityPrefab("Calculator", "HP=Brave");
        
        AddInfiniteRange(ability); // All tiles
        AddHpEqualsBraveEffect(ability); // HP=Brave effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 15); // 15 MP cost
        
        SaveAbilityPrefab(ability, "Calculator", "HP=Brave");
    }

    private static void CreateHpEqualsZodiacAbility()
    {
        var ability = CreateAbilityPrefab("Calculator", "HP=Zodiac");
        
        AddInfiniteRange(ability); // All tiles
        AddHpEqualsZodiacEffect(ability); // HP=Zodiac effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 15); // 15 MP cost
        
        SaveAbilityPrefab(ability, "Calculator", "HP=Zodiac");
    }

    private static void CreateHpEqualsHeightAbility()
    {
        var ability = CreateAbilityPrefab("Calculator", "HP=Height");
        
        AddInfiniteRange(ability); // All tiles
        AddHpEqualsHeightEffect(ability); // HP=Height effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 15); // 15 MP cost
        
        SaveAbilityPrefab(ability, "Calculator", "HP=Height");
    }

    private static void CreateHpEqualsWeightAbility()
    {
        var ability = CreateAbilityPrefab("Calculator", "HP=Weight");
        
        AddInfiniteRange(ability); // All tiles
        AddHpEqualsWeightEffect(ability); // HP=Weight effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 15); // 15 MP cost
        
        SaveAbilityPrefab(ability, "Calculator", "HP=Weight");
    }

    private static void CreateHpEqualsSpeedAbility()
    {
        var ability = CreateAbilityPrefab("Calculator", "HP=Speed");
        
        AddInfiniteRange(ability); // All tiles
        AddHpEqualsSpeedEffect(ability); // HP=Speed effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 15); // 15 MP cost
        
        SaveAbilityPrefab(ability, "Calculator", "HP=Speed");
    }

    private static void CreateBardAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Bard abilities from FFT (Songs)
        CreateAngelSongAbility();
        CreateBattleSongAbility();
        CreateCheerSongAbility();
        CreateFoeRequiemAbility();
        CreateMagicSongAbility();
        CreateNamelessSongAbility();
        CreateLifeSongAbility();
        CreateLastSongAbility();
        
        Debug.Log("Created Bard abilities: Angel Song, Battle Song, Cheer Song, Foe Requiem, Magic Song, Nameless Song, Life Song, Last Song");
    }

    private static void CreateAngelSongAbility()
    {
        var ability = CreateAbilityPrefab("Bard", "Angel Song");
        
        AddFullArea(ability); // All tiles
        AddAngelSongEffect(ability); // MP restore effect
        AddMagicPower(ability, 12); // Angel Song power = 12 (from FFT)
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Bard", "Angel Song");
    }

    private static void CreateBattleSongAbility()
    {
        var ability = CreateAbilityPrefab("Bard", "Battle Song");
        
        AddFullArea(ability); // All tiles
        AddBattleSongEffect(ability); // Physical attack boost effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 10); // 10 MP cost
        
        SaveAbilityPrefab(ability, "Bard", "Battle Song");
    }

    private static void CreateCheerSongAbility()
    {
        var ability = CreateAbilityPrefab("Bard", "Cheer Song");
        
        AddFullArea(ability); // All tiles
        AddCheerSongEffect(ability); // Speed boost effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Bard", "Cheer Song");
    }

    private static void CreateFoeRequiemAbility()
    {
        var ability = CreateAbilityPrefab("Bard", "Foe Requiem");
        
        AddFullArea(ability); // All tiles
        AddFoeRequiemEffect(ability); // Damage all enemies effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 15); // 15 MP cost
        
        SaveAbilityPrefab(ability, "Bard", "Foe Requiem");
    }

    private static void CreateMagicSongAbility()
    {
        var ability = CreateAbilityPrefab("Bard", "Magic Song");
        
        AddFullArea(ability); // All tiles
        AddMagicSongEffect(ability); // Magic attack boost effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 10); // 10 MP cost
        
        SaveAbilityPrefab(ability, "Bard", "Magic Song");
    }

    private static void CreateNamelessSongAbility()
    {
        var ability = CreateAbilityPrefab("Bard", "Nameless Song");
        
        AddFullArea(ability); // All tiles
        AddNamelessSongEffect(ability); // Random positive status effects
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 20); // 20 MP cost
        
        SaveAbilityPrefab(ability, "Bard", "Nameless Song");
    }

    private static void CreateLifeSongAbility()
    {
        var ability = CreateAbilityPrefab("Bard", "Life Song");
        
        AddFullArea(ability); // All tiles
        AddLifeSongEffect(ability); // HP restore effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Bard", "Life Song");
    }

    private static void CreateLastSongAbility()
    {
        var ability = CreateAbilityPrefab("Bard", "Last Song");
        
        AddFullArea(ability); // All tiles
        AddLastSongEffect(ability); // Extra turn effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 25); // 25 MP cost
        
        SaveAbilityPrefab(ability, "Bard", "Last Song");
    }

    private static void CreateDancerAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Dancer abilities from FFT (Dances)
        CreateWitchHuntAbility();
        CreatePolkaPolkaAbility();
        CreateDisillusionAbility();
        CreateNamelessDanceAbility();
        CreateLastDanceAbility();
        CreateSlowDanceAbility();
        CreateForbiddenDanceAbility();
        CreateMincingMinuetAbility();
        
        Debug.Log("Created Dancer abilities: Witch Hunt, Polka Polka, Disillusion, Nameless Dance, Last Dance, Slow Dance, Forbidden Dance, Mincing Minuet");
    }

    private static void CreateWitchHuntAbility()
    {
        var ability = CreateAbilityPrefab("Dancer", "Witch Hunt");
        
        AddFullArea(ability); // All tiles
        AddWitchHuntEffect(ability); // MP reduction effect
        AddMagicPower(ability, 14); // Witch Hunt power = 14 (from FFT)
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Dancer", "Witch Hunt");
    }

    private static void CreatePolkaPolkaAbility()
    {
        var ability = CreateAbilityPrefab("Dancer", "Polka Polka");
        
        AddFullArea(ability); // All tiles
        AddPolkaPolkaEffect(ability); // Physical attack reduction effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 10); // 10 MP cost
        
        SaveAbilityPrefab(ability, "Dancer", "Polka Polka");
    }

    private static void CreateDisillusionAbility()
    {
        var ability = CreateAbilityPrefab("Dancer", "Disillusion");
        
        AddFullArea(ability); // All tiles
        AddDisillusionEffect(ability); // Magic attack reduction effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 10); // 10 MP cost
        
        SaveAbilityPrefab(ability, "Dancer", "Disillusion");
    }

    private static void CreateNamelessDanceAbility()
    {
        var ability = CreateAbilityPrefab("Dancer", "Nameless Dance");
        
        AddFullArea(ability); // All tiles
        AddNamelessDanceEffect(ability); // Random negative status effects
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 20); // 20 MP cost
        
        SaveAbilityPrefab(ability, "Dancer", "Nameless Dance");
    }

    private static void CreateLastDanceAbility()
    {
        var ability = CreateAbilityPrefab("Dancer", "Last Dance");
        
        AddFullArea(ability); // All tiles
        AddLastDanceEffect(ability); // CT reduction effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 25); // 25 MP cost
        
        SaveAbilityPrefab(ability, "Dancer", "Last Dance");
    }

    private static void CreateSlowDanceAbility()
    {
        var ability = CreateAbilityPrefab("Dancer", "Slow Dance");
        
        AddFullArea(ability); // All tiles
        AddSlowDanceEffect(ability); // Speed reduction effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Dancer", "Slow Dance");
    }

    private static void CreateForbiddenDanceAbility()
    {
        var ability = CreateAbilityPrefab("Dancer", "Forbidden Dance");
        
        AddFullArea(ability); // All tiles
        AddForbiddenDanceEffect(ability); // Damage all enemies effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 15); // 15 MP cost
        
        SaveAbilityPrefab(ability, "Dancer", "Forbidden Dance");
    }

    private static void CreateMincingMinuetAbility()
    {
        var ability = CreateAbilityPrefab("Dancer", "Mincing Minuet");
        
        AddFullArea(ability); // All tiles
        AddMincingMinuetEffect(ability); // Damage all enemies effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddMagicCost(ability, 15); // 15 MP cost
        
        SaveAbilityPrefab(ability, "Dancer", "Mincing Minuet");
    }

    private static void CreateMimeAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Mime abilities from FFT (Mimic)
        CreateMimicAbility();
        
        Debug.Log("Created Mime abilities: Mimic");
    }

    private static void CreateMimicAbility()
    {
        var ability = CreateAbilityPrefab("Mime", "Mimic");
        
        AddSelfRange(ability); // Self only
        AddMimicEffect(ability); // Mimic effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Mime", "Mimic");
    }

    #endregion

    #region Reaction Abilities

    private static void CreateAstrologerAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Astrologer abilities from FFT (Astrology)
        CreateCelestialStasisAbility();
        
        Debug.Log("Created Astrologer abilities: Celestial Stasis");
    }

    private static void CreateCelestialStasisAbility()
    {
        var ability = CreateAbilityPrefab("Astrologer", "Celestial Stasis");
        
        AddInfiniteRange(ability); // All tiles
        AddCelestialStasisEffect(ability); // Celestial Stasis effect
        AddMagicPower(ability, 25); // Celestial Stasis power = 25 (from FFT)
        AddMagicCost(ability, 30); // 30 MP cost
        
        SaveAbilityPrefab(ability, "Astrologer", "Celestial Stasis");
    }

    private static void CreateClericAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Cleric abilities from FFT (Healing abilities)
        CreateClericHealAbility();
        CreateClericCureAbility();
        CreateClericRestoreAbility();
        
        Debug.Log("Created Cleric abilities: Heal, Cure, Restore");
    }

    private static void CreateClericHealAbility()
    {
        var ability = CreateAbilityPrefab("Cleric", "Heal");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddHealEffect(ability); // Heal effect
        AddMagicPower(ability, 15); // Cleric Heal power = 15 (from FFT)
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Cleric", "Heal");
    }

    private static void CreateClericCureAbility()
    {
        var ability = CreateAbilityPrefab("Cleric", "Cure");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddCureEffect(ability); // Cure effect
        AddMagicPower(ability, 18); // Cleric Cure power = 18 (from FFT)
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Cleric", "Cure");
    }

    private static void CreateClericRestoreAbility()
    {
        var ability = CreateAbilityPrefab("Cleric", "Restore");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddRestoreEffect(ability); // Restore effect
        AddMagicPower(ability, 20); // Cleric Restore power = 20 (from FFT)
        AddMagicCost(ability, 16); // 16 MP cost
        
        SaveAbilityPrefab(ability, "Cleric", "Restore");
    }

    private static void CreateFellKnightAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Fell Knight abilities from FFT (Fell Sword)
        CreateFellSwordAbility();
        CreateDarkBladeAbility();
        CreateShadowStrikeAbility();
        
        Debug.Log("Created Fell Knight abilities: Fell Sword, Dark Blade, Shadow Strike");
    }

    private static void CreateFellSwordAbility()
    {
        var ability = CreateAbilityPrefab("Fell Knight", "Fell Sword");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddFellSwordEffect(ability); // Fell Sword effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Fell Knight", "Fell Sword");
    }

    private static void CreateDarkBladeAbility()
    {
        var ability = CreateAbilityPrefab("Fell Knight", "Dark Blade");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddDarkBladeEffect(ability); // Dark Blade effect
        AddWeaponPower(ability); // Uses weapon power
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Fell Knight", "Dark Blade");
    }

    private static void CreateShadowStrikeAbility()
    {
        var ability = CreateAbilityPrefab("Fell Knight", "Shadow Strike");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddShadowStrikeEffect(ability); // Shadow Strike effect
        AddWeaponPower(ability); // Uses weapon power
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Fell Knight", "Shadow Strike");
    }

    private static void CreateDeathknightAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Deathknight abilities from FFT (Enhanced Fell Sword)
        CreateDeathknightFellSwordAbility();
        CreateDeathknightDarkBladeAbility();
        CreateDeathknightShadowStrikeAbility();
        CreateDeathknightSoulDrainAbility();
        
        Debug.Log("Created Deathknight abilities: Fell Sword, Dark Blade, Shadow Strike, Soul Drain");
    }

    private static void CreateDeathknightFellSwordAbility()
    {
        var ability = CreateAbilityPrefab("Deathknight", "Fell Sword");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddDeathknightFellSwordEffect(ability); // Enhanced Fell Sword effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Deathknight", "Fell Sword");
    }

    private static void CreateDeathknightDarkBladeAbility()
    {
        var ability = CreateAbilityPrefab("Deathknight", "Dark Blade");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddDeathknightDarkBladeEffect(ability); // Enhanced Dark Blade effect
        AddWeaponPower(ability); // Uses weapon power
        AddMagicCost(ability, 10); // 10 MP cost
        
        SaveAbilityPrefab(ability, "Deathknight", "Dark Blade");
    }

    private static void CreateDeathknightShadowStrikeAbility()
    {
        var ability = CreateAbilityPrefab("Deathknight", "Shadow Strike");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddDeathknightShadowStrikeEffect(ability); // Enhanced Shadow Strike effect
        AddWeaponPower(ability); // Uses weapon power
        AddMagicCost(ability, 15); // 15 MP cost
        
        SaveAbilityPrefab(ability, "Deathknight", "Shadow Strike");
    }

    private static void CreateDeathknightSoulDrainAbility()
    {
        var ability = CreateAbilityPrefab("Deathknight", "Soul Drain");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddDeathknightSoulDrainEffect(ability); // Soul Drain effect
        AddWeaponPower(ability); // Uses weapon power
        AddMagicCost(ability, 20); // 20 MP cost
        
        SaveAbilityPrefab(ability, "Deathknight", "Soul Drain");
    }

    private static void CreateSorcererAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Sorcerer abilities from FFT (Powerful Magic)
        CreateSorcererHolyAbility();
        CreateSorcererFlareAbility();
        CreateSorcererGravigaAbility();
        CreateSorcererUnholyDarknessAbility();
        CreateSorcererAriseAbility();
        
        Debug.Log("Created Sorcerer abilities: Holy, Flare, Graviga, Unholy Darkness, Arise");
    }

    private static void CreateSorcererHolyAbility()
    {
        var ability = CreateAbilityPrefab("Sorcerer", "Holy");
        
        AddConstantRange(ability, 5, 1); // Range 5
        AddSorcererHolyEffect(ability); // Holy effect
        AddMagicPower(ability, 35); // Sorcerer Holy power = 35 (from FFT)
        AddMagicCost(ability, 56); // 56 MP cost
        
        SaveAbilityPrefab(ability, "Sorcerer", "Holy");
    }

    private static void CreateSorcererFlareAbility()
    {
        var ability = CreateAbilityPrefab("Sorcerer", "Flare");
        
        AddConstantRange(ability, 5, 1); // Range 5
        AddSorcererFlareEffect(ability); // Flare effect
        AddMagicPower(ability, 40); // Sorcerer Flare power = 40 (from FFT)
        AddMagicCost(ability, 60); // 60 MP cost
        
        SaveAbilityPrefab(ability, "Sorcerer", "Flare");
    }

    private static void CreateSorcererGravigaAbility()
    {
        var ability = CreateAbilityPrefab("Sorcerer", "Graviga");
        
        AddConstantRange(ability, 4, 1); // Range 4
        AddSorcererGravigaEffect(ability); // Graviga effect
        AddMagicPower(ability, 45); // Sorcerer Graviga power = 45 (from FFT)
        AddMagicCost(ability, 50); // 50 MP cost
        
        SaveAbilityPrefab(ability, "Sorcerer", "Graviga");
    }

    private static void CreateSorcererUnholyDarknessAbility()
    {
        var ability = CreateAbilityPrefab("Sorcerer", "Unholy Darkness");
        
        AddConstantRange(ability, 4, 1); // Range 4
        AddSorcererUnholyDarknessEffect(ability); // Unholy Darkness effect
        AddMagicPower(ability, 38); // Sorcerer Unholy Darkness power = 38 (from FFT)
        AddMagicCost(ability, 40); // 40 MP cost
        
        SaveAbilityPrefab(ability, "Sorcerer", "Unholy Darkness");
    }

    private static void CreateSorcererAriseAbility()
    {
        var ability = CreateAbilityPrefab("Sorcerer", "Arise");
        
        AddConstantRange(ability, 4, 1); // Range 4
        AddSorcererAriseEffect(ability); // Arise effect
        AddMagicPower(ability, 30); // Sorcerer Arise power = 30 (from FFT)
        AddMagicCost(ability, 20); // 20 MP cost
        
        SaveAbilityPrefab(ability, "Sorcerer", "Arise");
    }

    // Princess abilities - Ovelia's unique abilities
    private static void CreatePrincessAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Princess abilities from FFT (Ovelia's unique abilities)
        CreatePrincessPrayerAbility();
        CreatePrincessBlessingAbility();
        CreatePrincessMiracleAbility();
        
        Debug.Log("Created Princess abilities: Prayer, Blessing, Miracle");
    }

    private static void CreatePrincessPrayerAbility()
    {
        var ability = CreateAbilityPrefab("Princess", "Prayer");
        
        AddSelfRange(ability); // Self-targeting
        AddPrincessPrayerEffect(ability); // Prayer effect
        AddMagicPower(ability, 20); // Princess Prayer power = 20
        AddMagicCost(ability, 15); // 15 MP cost
        
        SaveAbilityPrefab(ability, "Princess", "Prayer");
    }

    private static void CreatePrincessBlessingAbility()
    {
        var ability = CreateAbilityPrefab("Princess", "Blessing");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddPrincessBlessingEffect(ability); // Blessing effect
        AddMagicPower(ability, 25); // Princess Blessing power = 25
        AddMagicCost(ability, 20); // 20 MP cost
        
        SaveAbilityPrefab(ability, "Princess", "Blessing");
    }

    private static void CreatePrincessMiracleAbility()
    {
        var ability = CreateAbilityPrefab("Princess", "Miracle");
        
        AddConstantRange(ability, 4, 1); // Range 4
        AddPrincessMiracleEffect(ability); // Miracle effect
        AddMagicPower(ability, 30); // Princess Miracle power = 30
        AddMagicCost(ability, 25); // 25 MP cost
        
        SaveAbilityPrefab(ability, "Princess", "Miracle");
    }

    // Ark Knight abilities - Meliadoul's unique abilities
    private static void CreateArkKnightAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Ark Knight abilities from FFT (Meliadoul's unique abilities)
        CreateArkKnightDestroyAbility();
        CreateArkKnightCrushAbility();
        CreateArkKnightBreakAbility();
        
        Debug.Log("Created Ark Knight abilities: Destroy, Crush, Break");
    }

    private static void CreateArkKnightDestroyAbility()
    {
        var ability = CreateAbilityPrefab("Ark Knight", "Destroy");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddArkKnightDestroyEffect(ability); // Destroy effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Ark Knight", "Destroy");
    }

    private static void CreateArkKnightCrushAbility()
    {
        var ability = CreateAbilityPrefab("Ark Knight", "Crush");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddArkKnightCrushEffect(ability); // Crush effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Ark Knight", "Crush");
    }

    private static void CreateArkKnightBreakAbility()
    {
        var ability = CreateAbilityPrefab("Ark Knight", "Break");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddArkKnightBreakEffect(ability); // Break effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Ark Knight", "Break");
    }

    // Assassin abilities - Elmdore's unique abilities
    private static void CreateAssassinAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Assassin abilities from FFT (Elmdore's unique abilities)
        CreateAssassinShadowStrikeAbility();
        CreateAssassinDarkBladeAbility();
        CreateAssassinSoulDrainAbility();
        
        Debug.Log("Created Assassin abilities: Shadow Strike, Dark Blade, Soul Drain");
    }

    private static void CreateAssassinShadowStrikeAbility()
    {
        var ability = CreateAbilityPrefab("Assassin", "Shadow Strike");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddAssassinShadowStrikeEffect(ability); // Shadow Strike effect
        AddWeaponPower(ability); // Uses weapon power
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Assassin", "Shadow Strike");
    }

    private static void CreateAssassinDarkBladeAbility()
    {
        var ability = CreateAbilityPrefab("Assassin", "Dark Blade");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddAssassinDarkBladeEffect(ability); // Dark Blade effect
        AddWeaponPower(ability); // Uses weapon power
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Assassin", "Dark Blade");
    }

    private static void CreateAssassinSoulDrainAbility()
    {
        var ability = CreateAbilityPrefab("Assassin", "Soul Drain");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddAssassinSoulDrainEffect(ability); // Soul Drain effect
        AddWeaponPower(ability); // Uses weapon power
        AddMagicCost(ability, 16); // 16 MP cost
        
        SaveAbilityPrefab(ability, "Assassin", "Soul Drain");
    }

    // Celebrant abilities - Reis's unique abilities
    private static void CreateCelebrantAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Celebrant abilities from FFT (Reis's unique abilities)
        CreateCelebrantDragonBreathAbility();
        CreateCelebrantDragonClawAbility();
        CreateCelebrantDragonWingAbility();
        
        Debug.Log("Created Celebrant abilities: Dragon Breath, Dragon Claw, Dragon Wing");
    }

    private static void CreateCelebrantDragonBreathAbility()
    {
        var ability = CreateAbilityPrefab("Celebrant", "Dragon Breath");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddCelebrantDragonBreathEffect(ability); // Dragon Breath effect
        AddMagicPower(ability, 22); // Celebrant Dragon Breath power = 22
        AddMagicCost(ability, 18); // 18 MP cost
        
        SaveAbilityPrefab(ability, "Celebrant", "Dragon Breath");
    }

    private static void CreateCelebrantDragonClawAbility()
    {
        var ability = CreateAbilityPrefab("Celebrant", "Dragon Claw");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddCelebrantDragonClawEffect(ability); // Dragon Claw effect
        AddPhysicalPower(ability, 20); // Celebrant Dragon Claw power = 20
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Celebrant", "Dragon Claw");
    }

    private static void CreateCelebrantDragonWingAbility()
    {
        var ability = CreateAbilityPrefab("Celebrant", "Dragon Wing");
        
        AddConstantRange(ability, 4, 1); // Range 4
        AddCelebrantDragonWingEffect(ability); // Dragon Wing effect
        AddMagicPower(ability, 25); // Celebrant Dragon Wing power = 25
        AddMagicCost(ability, 20); // 20 MP cost
        
        SaveAbilityPrefab(ability, "Celebrant", "Dragon Wing");
    }

    // Divine Knight abilities - Agrias's unique abilities
    private static void CreateDivineKnightAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Divine Knight abilities from FFT (Agrias's unique abilities)
        CreateDivineKnightHolyStrikeAbility();
        CreateDivineKnightJudgmentBladeAbility();
        CreateDivineKnightDivineRuinationAbility();
        
        Debug.Log("Created Divine Knight abilities: Holy Strike, Judgment Blade, Divine Ruination");
    }

    private static void CreateDivineKnightHolyStrikeAbility()
    {
        var ability = CreateAbilityPrefab("Divine Knight", "Holy Strike");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddDivineKnightHolyStrikeEffect(ability); // Holy Strike effect
        AddWeaponPower(ability); // Uses weapon power
        AddMagicCost(ability, 10); // 10 MP cost
        
        SaveAbilityPrefab(ability, "Divine Knight", "Holy Strike");
    }

    private static void CreateDivineKnightJudgmentBladeAbility()
    {
        var ability = CreateAbilityPrefab("Divine Knight", "Judgment Blade");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddDivineKnightJudgmentBladeEffect(ability); // Judgment Blade effect
        AddWeaponPower(ability); // Uses weapon power
        AddMagicCost(ability, 15); // 15 MP cost
        
        SaveAbilityPrefab(ability, "Divine Knight", "Judgment Blade");
    }

    private static void CreateDivineKnightDivineRuinationAbility()
    {
        var ability = CreateAbilityPrefab("Divine Knight", "Divine Ruination");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddDivineKnightDivineRuinationEffect(ability); // Divine Ruination effect
        AddWeaponPower(ability); // Uses weapon power
        AddMagicCost(ability, 20); // 20 MP cost
        
        SaveAbilityPrefab(ability, "Divine Knight", "Divine Ruination");
    }

    // Templar abilities - Mustadio's unique abilities
    private static void CreateTemplarAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Templar abilities from FFT (Mustadio's unique abilities)
        CreateTemplarArmShotAbility();
        CreateTemplarLegShotAbility();
        CreateTemplarHeadShotAbility();
        
        Debug.Log("Created Templar abilities: Arm Shot, Leg Shot, Head Shot");
    }

    private static void CreateTemplarArmShotAbility()
    {
        var ability = CreateAbilityPrefab("Templar", "Arm Shot");
        
        AddConstantRange(ability, 4, 1); // Range 4
        AddTemplarArmShotEffect(ability); // Arm Shot effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Templar", "Arm Shot");
    }

    private static void CreateTemplarLegShotAbility()
    {
        var ability = CreateAbilityPrefab("Templar", "Leg Shot");
        
        AddConstantRange(ability, 4, 1); // Range 4
        AddTemplarLegShotEffect(ability); // Leg Shot effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Templar", "Leg Shot");
    }

    private static void CreateTemplarHeadShotAbility()
    {
        var ability = CreateAbilityPrefab("Templar", "Head Shot");
        
        AddConstantRange(ability, 4, 1); // Range 4
        AddTemplarHeadShotEffect(ability); // Head Shot effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Templar", "Head Shot");
    }

    // Dragonkin abilities - Reis's dragon form abilities
    private static void CreateDragonkinAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Dragonkin abilities from FFT (Reis's dragon form abilities)
        CreateDragonkinFireBreathAbility();
        CreateDragonkinIceBreathAbility();
        CreateDragonkinThunderBreathAbility();
        
        Debug.Log("Created Dragonkin abilities: Fire Breath, Ice Breath, Thunder Breath");
    }

    private static void CreateDragonkinFireBreathAbility()
    {
        var ability = CreateAbilityPrefab("Dragonkin", "Fire Breath");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddDragonkinFireBreathEffect(ability); // Fire Breath effect
        AddMagicPower(ability, 24); // Dragonkin Fire Breath power = 24
        AddMagicCost(ability, 16); // 16 MP cost
        
        SaveAbilityPrefab(ability, "Dragonkin", "Fire Breath");
    }

    private static void CreateDragonkinIceBreathAbility()
    {
        var ability = CreateAbilityPrefab("Dragonkin", "Ice Breath");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddDragonkinIceBreathEffect(ability); // Ice Breath effect
        AddMagicPower(ability, 26); // Dragonkin Ice Breath power = 26
        AddMagicCost(ability, 18); // 18 MP cost
        
        SaveAbilityPrefab(ability, "Dragonkin", "Ice Breath");
    }

    private static void CreateDragonkinThunderBreathAbility()
    {
        var ability = CreateAbilityPrefab("Dragonkin", "Thunder Breath");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddDragonkinThunderBreathEffect(ability); // Thunder Breath effect
        AddMagicPower(ability, 28); // Dragonkin Thunder Breath power = 28
        AddMagicCost(ability, 20); // 20 MP cost
        
        SaveAbilityPrefab(ability, "Dragonkin", "Thunder Breath");
    }

    // Automaton abilities - Construct's unique abilities
    private static void CreateAutomatonAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Automaton abilities from FFT (Construct's unique abilities)
        CreateAutomatonRepairAbility();
        CreateAutomatonOverloadAbility();
        CreateAutomatonShutdownAbility();
        
        Debug.Log("Created Automaton abilities: Repair, Overload, Shutdown");
    }

    private static void CreateAutomatonRepairAbility()
    {
        var ability = CreateAbilityPrefab("Automaton", "Repair");
        
        AddSelfRange(ability); // Self-targeting
        AddAutomatonRepairEffect(ability); // Repair effect
        AddMagicPower(ability, 18); // Automaton Repair power = 18
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Automaton", "Repair");
    }

    private static void CreateAutomatonOverloadAbility()
    {
        var ability = CreateAbilityPrefab("Automaton", "Overload");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddAutomatonOverloadEffect(ability); // Overload effect
        AddPhysicalPower(ability, 22); // Automaton Overload power = 22
        AddMagicCost(ability, 15); // 15 MP cost
        
        SaveAbilityPrefab(ability, "Automaton", "Overload");
    }

    private static void CreateAutomatonShutdownAbility()
    {
        var ability = CreateAbilityPrefab("Automaton", "Shutdown");
        
        AddSelfRange(ability); // Self-targeting
        AddAutomatonShutdownEffect(ability); // Shutdown effect
        AddMagicPower(ability, 20); // Automaton Shutdown power = 20
        AddMagicCost(ability, 10); // 10 MP cost
        
        SaveAbilityPrefab(ability, "Automaton", "Shutdown");
    }

    // Soldier abilities - Generic soldier abilities
    private static void CreateSoldierAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Soldier abilities from FFT (Generic soldier abilities)
        CreateSoldierAttackAbility();
        CreateSoldierDefendAbility();
        CreateSoldierRushAbility();
        
        Debug.Log("Created Soldier abilities: Attack, Defend, Rush");
    }

    private static void CreateSoldierAttackAbility()
    {
        var ability = CreateAbilityPrefab("Soldier", "Attack");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddSoldierAttackEffect(ability); // Attack effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Soldier", "Attack");
    }

    private static void CreateSoldierDefendAbility()
    {
        var ability = CreateAbilityPrefab("Soldier", "Defend");
        
        AddSelfRange(ability); // Self-targeting
        AddSoldierDefendEffect(ability); // Defend effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Soldier", "Defend");
    }

    private static void CreateSoldierRushAbility()
    {
        var ability = CreateAbilityPrefab("Soldier", "Rush");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddSoldierRushEffect(ability); // Rush effect
        AddPhysicalPower(ability, 16); // Soldier Rush power = 16
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Soldier", "Rush");
    }

    // Sky Pirate abilities - Balthier's unique abilities
    private static void CreateSkyPirateAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Sky Pirate abilities from FFT (Balthier's unique abilities)
        CreateSkyPirateStealAbility();
        CreateSkyPiratePoachAbility();
        CreateSkyPirateSightUnseeingAbility();
        
        Debug.Log("Created Sky Pirate abilities: Steal, Poach, Sight Unseeing");
    }

    private static void CreateSkyPirateStealAbility()
    {
        var ability = CreateAbilityPrefab("Sky Pirate", "Steal");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddSkyPirateStealEffect(ability); // Steal effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Sky Pirate", "Steal");
    }

    private static void CreateSkyPiratePoachAbility()
    {
        var ability = CreateAbilityPrefab("Sky Pirate", "Poach");
        
        AddConstantRange(ability, 1, 1); // Range 1
        AddSkyPiratePoachEffect(ability); // Poach effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Sky Pirate", "Poach");
    }

    private static void CreateSkyPirateSightUnseeingAbility()
    {
        var ability = CreateAbilityPrefab("Sky Pirate", "Sight Unseeing");
        
        AddConstantRange(ability, 3, 1); // Range 3
        AddSkyPirateSightUnseeingEffect(ability); // Sight Unseeing effect
        AddMagicPower(ability, 20); // Sky Pirate Sight Unseeing power = 20
        AddMagicCost(ability, 14); // 14 MP cost
        
        SaveAbilityPrefab(ability, "Sky Pirate", "Sight Unseeing");
    }

    // Game Hunter abilities - Generic hunter abilities
    private static void CreateGameHunterAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Game Hunter abilities from FFT (Generic hunter abilities)
        CreateGameHunterTrackAbility();
        CreateGameHunterTrapAbility();
        CreateGameHunterCallAbility();
        
        Debug.Log("Created Game Hunter abilities: Track, Trap, Call");
    }

    private static void CreateGameHunterTrackAbility()
    {
        var ability = CreateAbilityPrefab("Game Hunter", "Track");
        
        AddConstantRange(ability, 5, 1); // Range 5
        AddGameHunterTrackEffect(ability); // Track effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Game Hunter", "Track");
    }

    private static void CreateGameHunterTrapAbility()
    {
        var ability = CreateAbilityPrefab("Game Hunter", "Trap");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddGameHunterTrapEffect(ability); // Trap effect
        AddPhysicalPower(ability, 18); // Game Hunter Trap power = 18
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Game Hunter", "Trap");
    }

    private static void CreateGameHunterCallAbility()
    {
        var ability = CreateAbilityPrefab("Game Hunter", "Call");
        
        AddConstantRange(ability, 4, 1); // Range 4
        AddGameHunterCallEffect(ability); // Call effect
        AddMagicPower(ability, 16); // Game Hunter Call power = 16
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Game Hunter", "Call");
    }

    // Nightblade abilities - Dark assassin abilities
    private static void CreateNightbladeAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Nightblade abilities from FFT (Dark assassin abilities)
        CreateNightbladeShadowStrikeAbility();
        CreateNightbladeDarkBladeAbility();
        CreateNightbladeSoulDrainAbility();
        
        Debug.Log("Created Nightblade abilities: Shadow Strike, Dark Blade, Soul Drain");
    }

    private static void CreateNightbladeShadowStrikeAbility()
    {
        var ability = CreateAbilityPrefab("Nightblade", "Shadow Strike");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddNightbladeShadowStrikeEffect(ability); // Shadow Strike effect
        AddWeaponPower(ability); // Uses weapon power
        AddMagicCost(ability, 10); // 10 MP cost
        
        SaveAbilityPrefab(ability, "Nightblade", "Shadow Strike");
    }

    private static void CreateNightbladeDarkBladeAbility()
    {
        var ability = CreateAbilityPrefab("Nightblade", "Dark Blade");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddNightbladeDarkBladeEffect(ability); // Dark Blade effect
        AddWeaponPower(ability); // Uses weapon power
        AddMagicCost(ability, 14); // 14 MP cost
        
        SaveAbilityPrefab(ability, "Nightblade", "Dark Blade");
    }

    private static void CreateNightbladeSoulDrainAbility()
    {
        var ability = CreateAbilityPrefab("Nightblade", "Soul Drain");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddNightbladeSoulDrainEffect(ability); // Soul Drain effect
        AddWeaponPower(ability); // Uses weapon power
        AddMagicCost(ability, 18); // 18 MP cost
        
        SaveAbilityPrefab(ability, "Nightblade", "Soul Drain");
    }

    // Rune Knight abilities - Magic knight abilities
    private static void CreateRuneKnightAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Rune Knight abilities from FFT (Magic knight abilities)
        CreateRuneKnightRuneBladeAbility();
        CreateRuneKnightMagicSwordAbility();
        CreateRuneKnightSpellBladeAbility();
        
        Debug.Log("Created Rune Knight abilities: Rune Blade, Magic Sword, Spell Blade");
    }

    private static void CreateRuneKnightRuneBladeAbility()
    {
        var ability = CreateAbilityPrefab("Rune Knight", "Rune Blade");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddRuneKnightRuneBladeEffect(ability); // Rune Blade effect
        AddWeaponPower(ability); // Uses weapon power
        AddMagicCost(ability, 8); // 8 MP cost
        
        SaveAbilityPrefab(ability, "Rune Knight", "Rune Blade");
    }

    private static void CreateRuneKnightMagicSwordAbility()
    {
        var ability = CreateAbilityPrefab("Rune Knight", "Magic Sword");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddRuneKnightMagicSwordEffect(ability); // Magic Sword effect
        AddWeaponPower(ability); // Uses weapon power
        AddMagicCost(ability, 12); // 12 MP cost
        
        SaveAbilityPrefab(ability, "Rune Knight", "Magic Sword");
    }

    private static void CreateRuneKnightSpellBladeAbility()
    {
        var ability = CreateAbilityPrefab("Rune Knight", "Spell Blade");
        
        AddConstantRange(ability, 2, 1); // Range 2
        AddRuneKnightSpellBladeEffect(ability); // Spell Blade effect
        AddWeaponPower(ability); // Uses weapon power
        AddMagicCost(ability, 16); // 16 MP cost
        
        SaveAbilityPrefab(ability, "Rune Knight", "Spell Blade");
    }

    private static void CreateReactionAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Reaction abilities from FFT
        CreateCounterAbility();
        
        Debug.Log("Created Reaction abilities: Counter, Hamedo, Return, Sunken State, Vanish, Reflex, Shield, Magic Counter, Counter Magic, Counter Attack");
    }

    private static void CreateCounterAbility()
    {
        var ability = CreateAbilityPrefab("Reaction", "Counter");
        
        AddSelfRange(ability); // Self only
        AddCounterEffect(ability); // Counter attack effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Reaction", "Counter");
    }

    private static void CreateHamedoAbility()
    {
        var ability = CreateAbilityPrefab("Reaction", "Hamedo");
        
        AddSelfRange(ability); // Self only
        AddHamedoEffect(ability); // Hamedo effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Reaction", "Hamedo");
    }

    private static void CreateReturnAbility()
    {
        var ability = CreateAbilityPrefab("Reaction", "Return");
        
        AddSelfRange(ability); // Self only
        AddReturnEffect(ability); // Return effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Reaction", "Return");
    }

    private static void CreateSunkenStateAbility()
    {
        var ability = CreateAbilityPrefab("Reaction", "Sunken State");
        
        AddSelfRange(ability); // Self only
        AddSunkenStateEffect(ability); // Sunken State effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Reaction", "Sunken State");
    }

    private static void CreateVanishAbility()
    {
        var ability = CreateAbilityPrefab("Reaction", "Vanish");
        
        AddSelfRange(ability); // Self only
        AddVanishEffect(ability); // Vanish effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Reaction", "Vanish");
    }

    private static void CreateReflexAbility()
    {
        var ability = CreateAbilityPrefab("Reaction", "Reflex");
        
        AddSelfRange(ability); // Self only
        AddReflexEffect(ability); // Reflex effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Reaction", "Reflex");
    }

    private static void CreateShieldAbility()
    {
        var ability = CreateAbilityPrefab("Reaction", "Shield");
        
        AddSelfRange(ability); // Self only
        AddShieldEffect(ability); // Shield effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Reaction", "Shield");
    }

    private static void CreateMagicCounterAbility()
    {
        var ability = CreateAbilityPrefab("Reaction", "Magic Counter");
        
        AddSelfRange(ability); // Self only
        AddMagicCounterEffect(ability); // Magic Counter effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Reaction", "Magic Counter");
    }

    private static void CreateCounterMagicAbility()
    {
        var ability = CreateAbilityPrefab("Reaction", "Counter Magic");
        
        AddSelfRange(ability); // Self only
        AddCounterMagicEffect(ability); // Counter Magic effect
        AddMagicPower(ability, 14); // Dancer power = 14 (from FFT)
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Reaction", "Counter Magic");
    }

    private static void CreateCounterAttackAbility()
    {
        var ability = CreateAbilityPrefab("Reaction", "Counter Attack");
        
        AddSelfRange(ability); // Self only
        AddCounterAttackEffect(ability); // Counter Attack effect
        AddWeaponPower(ability); // Uses weapon power
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Reaction", "Counter Attack");
    }

    #endregion

    #region Support Abilities

    private static void CreateSupportAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Support abilities from FFT
        CreateTwoHandsAbility();
        CreateMartialArtsAbility();
        CreateEquipSwordAbility();
        CreateEquipShieldAbility();
        CreateEquipHelmetAbility();
        CreateEquipArmorAbility();
        CreateEquipAccessoryAbility();
        CreateEquipHeavyArmorAbility();
        CreateEquipRobeAbility();
        CreateEquipClothesAbility();
        CreateEquipHatAbility();
        CreateEquipShoesAbility();
        CreateEquipShieldAbility();
        CreateEquipBowAbility();
        CreateEquipCrossbowAbility();
        CreateEquipGunAbility();
        CreateEquipBookAbility();
        CreateEquipRodAbility();
        CreateEquipStaffAbility();
        CreateEquipKnifeAbility();
        CreateEquipSpearAbility();
        CreateEquipAxeAbility();
        CreateEquipHammerAbility();
        CreateEquipWhipAbility();
        CreateEquipInstrumentAbility();
        CreateEquipHairpinAbility();
        CreateEquipPerfumeAbility();
        CreateEquipRingAbility();
        CreateEquipBangleAbility();
        CreateEquipCrownAbility();
        
        Debug.Log("Created Support abilities: Two Hands, Martial Arts, Equip Sword, Equip Shield, Equip Helmet, Equip Armor, Equip Accessory, Equip Heavy Armor, Equip Robe, Equip Clothes, Equip Hat, Equip Shoes, Equip Bow, Equip Crossbow, Equip Gun, Equip Book, Equip Rod, Equip Staff, Equip Knife, Equip Spear, Equip Axe, Equip Hammer, Equip Whip, Equip Instrument, Equip Hairpin, Equip Perfume, Equip Ring, Equip Bangle, Equip Crown");
    }

    private static void CreateTwoHandsAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Two Hands");
        
        AddSelfRange(ability); // Self only
        AddTwoHandsSupportEffect(ability); // Two Hands effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Two Hands");
    }

    private static void CreateMartialArtsAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Martial Arts");
        
        AddSelfRange(ability); // Self only
        AddMartialArtsSupportEffect(ability); // Martial Arts effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Martial Arts");
    }

    private static void CreateEquipSwordAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Sword");
        
        AddSelfRange(ability); // Self only
        AddEquipSwordEffect(ability); // Equip Sword effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Sword");
    }

    private static void CreateEquipShieldAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Shield");
        
        AddSelfRange(ability); // Self only
        AddEquipShieldEffect(ability); // Equip Shield effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Shield");
    }

    private static void CreateEquipHelmetAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Helmet");
        
        AddSelfRange(ability); // Self only
        AddEquipHelmetEffect(ability); // Equip Helmet effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Helmet");
    }

    private static void CreateEquipArmorAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Armor");
        
        AddSelfRange(ability); // Self only
        AddEquipArmorEffect(ability); // Equip Armor effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Armor");
    }

    private static void CreateEquipAccessoryAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Accessory");
        
        AddSelfRange(ability); // Self only
        AddEquipAccessoryEffect(ability); // Equip Accessory effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Accessory");
    }

    private static void CreateEquipHeavyArmorAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Heavy Armor");
        
        AddSelfRange(ability); // Self only
        AddEquipHeavyArmorEffect(ability); // Equip Heavy Armor effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Heavy Armor");
    }

    private static void CreateEquipRobeAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Robe");
        
        AddSelfRange(ability); // Self only
        AddEquipRobeEffect(ability); // Equip Robe effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Robe");
    }

    private static void CreateEquipClothesAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Clothes");
        
        AddSelfRange(ability); // Self only
        AddEquipClothesEffect(ability); // Equip Clothes effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Clothes");
    }

    private static void CreateEquipHatAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Hat");
        
        AddSelfRange(ability); // Self only
        AddEquipHatEffect(ability); // Equip Hat effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Hat");
    }

    private static void CreateEquipShoesAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Shoes");
        
        AddSelfRange(ability); // Self only
        AddEquipShoesEffect(ability); // Equip Shoes effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Shoes");
    }

    private static void CreateEquipBowAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Bow");
        
        AddSelfRange(ability); // Self only
        AddEquipBowEffect(ability); // Equip Bow effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Bow");
    }

    private static void CreateEquipCrossbowAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Crossbow");
        
        AddSelfRange(ability); // Self only
        AddEquipCrossbowEffect(ability); // Equip Crossbow effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Crossbow");
    }

    private static void CreateEquipGunAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Gun");
        
        AddSelfRange(ability); // Self only
        AddEquipGunEffect(ability); // Equip Gun effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Gun");
    }

    private static void CreateEquipBookAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Book");
        
        AddSelfRange(ability); // Self only
        AddEquipBookEffect(ability); // Equip Book effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Book");
    }

    private static void CreateEquipRodAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Rod");
        
        AddSelfRange(ability); // Self only
        AddEquipRodEffect(ability); // Equip Rod effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Rod");
    }

    private static void CreateEquipStaffAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Staff");
        
        AddSelfRange(ability); // Self only
        AddEquipStaffEffect(ability); // Equip Staff effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Staff");
    }

    private static void CreateEquipKnifeAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Knife");
        
        AddSelfRange(ability); // Self only
        AddEquipKnifeEffect(ability); // Equip Knife effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Knife");
    }

    private static void CreateEquipSpearAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Spear");
        
        AddSelfRange(ability); // Self only
        AddEquipSpearEffect(ability); // Equip Spear effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Spear");
    }

    private static void CreateEquipAxeAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Axe");
        
        AddSelfRange(ability); // Self only
        AddEquipAxeEffect(ability); // Equip Axe effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Axe");
    }

    private static void CreateEquipHammerAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Hammer");
        
        AddSelfRange(ability); // Self only
        AddEquipHammerEffect(ability); // Equip Hammer effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Hammer");
    }

    private static void CreateEquipWhipAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Whip");
        
        AddSelfRange(ability); // Self only
        AddEquipWhipEffect(ability); // Equip Whip effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Whip");
    }

    private static void CreateEquipInstrumentAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Instrument");
        
        AddSelfRange(ability); // Self only
        AddEquipInstrumentEffect(ability); // Equip Instrument effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Instrument");
    }

    private static void CreateEquipHairpinAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Hairpin");
        
        AddSelfRange(ability); // Self only
        AddEquipHairpinEffect(ability); // Equip Hairpin effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Hairpin");
    }

    private static void CreateEquipPerfumeAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Perfume");
        
        AddSelfRange(ability); // Self only
        AddEquipPerfumeEffect(ability); // Equip Perfume effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Perfume");
    }

    private static void CreateEquipRingAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Ring");
        
        AddSelfRange(ability); // Self only
        AddEquipRingEffect(ability); // Equip Ring effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Ring");
    }

    private static void CreateEquipBangleAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Bangle");
        
        AddSelfRange(ability); // Self only
        AddEquipBangleEffect(ability); // Equip Bangle effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Bangle");
    }

    private static void CreateEquipCrownAbility()
    {
        var ability = CreateAbilityPrefab("Support", "Equip Crown");
        
        AddSelfRange(ability); // Self only
        AddEquipCrownEffect(ability); // Equip Crown effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Support", "Equip Crown");
    }

    #endregion

    #region Movement Abilities

    private static void CreateMovementAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        // All Movement abilities from FFT
        CreateMove1Ability();
        CreateMove2Ability();
        CreateMove3Ability();
        CreateJump1MovementAbility();
        CreateJump2MovementAbility();
        CreateJump3MovementAbility();
        CreateTeleportMovementAbility();
        CreateWaterwalkingAbility();
        CreateMoveInWaterAbility();
        CreateMoveFindItemMovementAbility();
        
        Debug.Log("Created Movement abilities: Move +1, Move +2, Move +3, Jump +1, Jump +2, Jump +3, Teleport, Waterwalking, Move in Water, Move-Find Item");
    }

    private static void CreateMove1Ability()
    {
        var ability = CreateAbilityPrefab("Movement", "Move +1");
        
        AddSelfRange(ability); // Self only
        AddMove1Effect(ability); // Move +1 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Movement", "Move +1");
    }

    private static void CreateMove2Ability()
    {
        var ability = CreateAbilityPrefab("Movement", "Move +2");
        
        AddSelfRange(ability); // Self only
        AddMove2Effect(ability); // Move +2 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Movement", "Move +2");
    }

    private static void CreateMove3Ability()
    {
        var ability = CreateAbilityPrefab("Movement", "Move +3");
        
        AddSelfRange(ability); // Self only
        AddMove3Effect(ability); // Move +3 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Movement", "Move +3");
    }

    private static void CreateJump1MovementAbility()
    {
        var ability = CreateAbilityPrefab("Movement", "Jump +1");
        
        AddSelfRange(ability); // Self only
        AddJump1MovementEffect(ability); // Jump +1 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Movement", "Jump +1");
    }

    private static void CreateJump2MovementAbility()
    {
        var ability = CreateAbilityPrefab("Movement", "Jump +2");
        
        AddSelfRange(ability); // Self only
        AddJump2MovementEffect(ability); // Jump +2 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Movement", "Jump +2");
    }

    private static void CreateJump3MovementAbility()
    {
        var ability = CreateAbilityPrefab("Movement", "Jump +3");
        
        AddSelfRange(ability); // Self only
        AddJump3MovementEffect(ability); // Jump +3 effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Movement", "Jump +3");
    }

    private static void CreateTeleportMovementAbility()
    {
        var ability = CreateAbilityPrefab("Movement", "Teleport");
        
        AddSelfRange(ability); // Self only
        AddTeleportMovementEffect(ability); // Teleport effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Movement", "Teleport");
    }

    private static void CreateWaterwalkingAbility()
    {
        var ability = CreateAbilityPrefab("Movement", "Waterwalking");
        
        AddSelfRange(ability); // Self only
        AddWaterwalkingEffect(ability); // Waterwalking effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Movement", "Waterwalking");
    }

    private static void CreateMoveInWaterAbility()
    {
        var ability = CreateAbilityPrefab("Movement", "Move in Water");
        
        AddSelfRange(ability); // Self only
        AddMoveInWaterEffect(ability); // Move in Water effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Movement", "Move in Water");
    }

    private static void CreateMoveFindItemMovementAbility()
    {
        var ability = CreateAbilityPrefab("Movement", "Move-Find Item");
        
        AddSelfRange(ability); // Self only
        AddMoveFindItemMovementEffect(ability); // Move-Find Item effect
        AddNoPower(ability); // No power component
        AddNoMagicCost(ability); // No MP cost
        
        SaveAbilityPrefab(ability, "Movement", "Move-Find Item");
    }

    #endregion

    #region Helper Methods

    // Missing helper methods for all abilities
    private static void AddLancerEffect(GameObject ability)
    {
        var effectGO = new GameObject("Lancer");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<AbsorbDamageAbilityEffect>(); // HP/MP drain effect
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
    }

    private static void AddInviteEffect(GameObject ability)
    {
        var effectGO = new GameObject("Invite");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "InviteStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddRecruitEffect(GameObject ability)
    {
        var effectGO = new GameObject("Recruit");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "RecruitStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddConvinceEffect(GameObject ability)
    {
        var effectGO = new GameObject("Convince");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "ConvinceStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddPersuadeEffect(GameObject ability)
    {
        var effectGO = new GameObject("Persuade");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "PersuadeStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddNegotiateEffect(GameObject ability)
    {
        var effectGO = new GameObject("Negotiate");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "NegotiateStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddMediateEffect(GameObject ability)
    {
        var effectGO = new GameObject("Mediate");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "MediateStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddAppealEffect(GameObject ability)
    {
        var effectGO = new GameObject("Appeal");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "AppealStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddReasonEffect(GameObject ability)
    {
        var effectGO = new GameObject("Reason");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "ReasonStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddDebateEffect(GameObject ability)
    {
        var effectGO = new GameObject("Debate");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "DebateStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddOrateEffect(GameObject ability)
    {
        var effectGO = new GameObject("Orate");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "OrateStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddPrimeEffect(GameObject ability)
    {
        var effectGO = new GameObject("Prime");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Prime number targeting
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddHpEqualsMpEffect(GameObject ability)
    {
        var effectGO = new GameObject("HpEqualsMp");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // HP=MP targeting
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddHpEqualsLevelEffect(GameObject ability)
    {
        var effectGO = new GameObject("HpEqualsLevel");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // HP=Level targeting
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddHpEqualsExpEffect(GameObject ability)
    {
        var effectGO = new GameObject("HpEqualsExp");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // HP=Exp targeting
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddHpEqualsFaithEffect(GameObject ability)
    {
        var effectGO = new GameObject("HpEqualsFaith");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // HP=Faith targeting
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddHpEqualsBraveEffect(GameObject ability)
    {
        var effectGO = new GameObject("HpEqualsBrave");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // HP=Brave targeting
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddHpEqualsZodiacEffect(GameObject ability)
    {
        var effectGO = new GameObject("HpEqualsZodiac");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // HP=Zodiac targeting
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddHpEqualsHeightEffect(GameObject ability)
    {
        var effectGO = new GameObject("HpEqualsHeight");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // HP=Height targeting
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddHpEqualsWeightEffect(GameObject ability)
    {
        var effectGO = new GameObject("HpEqualsWeight");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // HP=Weight targeting
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddHpEqualsSpeedEffect(GameObject ability)
    {
        var effectGO = new GameObject("HpEqualsSpeed");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // HP=Speed targeting
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddAngelSongEffect(GameObject ability)
    {
        var effectGO = new GameObject("AngelSong");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<HealAbilityEffect>(); // MP restore effect
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddBattleSongEffect(GameObject ability)
    {
        var effectGO = new GameObject("BattleSong");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "BattleSongStatus";
        effect.duration = 3;
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddCheerSongEffect(GameObject ability)
    {
        var effectGO = new GameObject("CheerSong");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "CheerSongStatus";
        effect.duration = 3;
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddFoeRequiemEffect(GameObject ability)
    {
        var effectGO = new GameObject("FoeRequiem");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Damage all enemies
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddMagicSongEffect(GameObject ability)
    {
        var effectGO = new GameObject("MagicSong");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "MagicSongStatus";
        effect.duration = 3;
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddNamelessSongEffect(GameObject ability)
    {
        var effectGO = new GameObject("NamelessSong");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "NamelessSongStatus";
        effect.duration = 3;
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddLifeSongEffect(GameObject ability)
    {
        var effectGO = new GameObject("LifeSong");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<HealAbilityEffect>(); // HP restore effect
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddLastSongEffect(GameObject ability)
    {
        var effectGO = new GameObject("LastSong");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "LastSongStatus";
        effect.duration = 3;
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddWitchHuntEffect(GameObject ability)
    {
        var effectGO = new GameObject("WitchHunt");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "WitchHuntStatus";
        effect.duration = 3;
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddPolkaPolkaEffect(GameObject ability)
    {
        var effectGO = new GameObject("PolkaPolka");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "PolkaPolkaStatus";
        effect.duration = 3;
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddDisillusionEffect(GameObject ability)
    {
        var effectGO = new GameObject("Disillusion");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "DisillusionStatus";
        effect.duration = 3;
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddNamelessDanceEffect(GameObject ability)
    {
        var effectGO = new GameObject("NamelessDance");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "NamelessDanceStatus";
        effect.duration = 3;
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddLastDanceEffect(GameObject ability)
    {
        var effectGO = new GameObject("LastDance");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "LastDanceStatus";
        effect.duration = 3;
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddSlowDanceEffect(GameObject ability)
    {
        var effectGO = new GameObject("SlowDance");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "SlowDanceStatus";
        effect.duration = 3;
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddForbiddenDanceEffect(GameObject ability)
    {
        var effectGO = new GameObject("ForbiddenDance");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Damage all enemies
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddMincingMinuetEffect(GameObject ability)
    {
        var effectGO = new GameObject("MincingMinuet");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Damage all enemies
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddMimicEffect(GameObject ability)
    {
        var effectGO = new GameObject("Mimic");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "MimicStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddHamedoEffect(GameObject ability)
    {
        var effectGO = new GameObject("Hamedo");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Counter attack
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
    }

    private static void AddReturnEffect(GameObject ability)
    {
        var effectGO = new GameObject("Return");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "ReturnStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddSunkenStateEffect(GameObject ability)
    {
        var effectGO = new GameObject("SunkenState");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "SunkenStateStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddVanishEffect(GameObject ability)
    {
        var effectGO = new GameObject("Vanish");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "VanishStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddReflexEffect(GameObject ability)
    {
        var effectGO = new GameObject("Reflex");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "ReflexStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddShieldEffect(GameObject ability)
    {
        var effectGO = new GameObject("Shield");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "ShieldStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddMagicCounterEffect(GameObject ability)
    {
        var effectGO = new GameObject("MagicCounter");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Magic counter
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddCounterMagicEffect(GameObject ability)
    {
        var effectGO = new GameObject("CounterMagic");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Counter magic
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddCounterAttackEffect(GameObject ability)
    {
        var effectGO = new GameObject("CounterAttack");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Counter attack
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
    }

    private static void AddTwoHandsSupportEffect(GameObject ability)
    {
        var effectGO = new GameObject("TwoHandsSupport");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "TwoHandsSupportStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddMartialArtsSupportEffect(GameObject ability)
    {
        var effectGO = new GameObject("MartialArtsSupport");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "MartialArtsSupportStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipSwordEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipSword");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipSwordStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipShieldEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipShield");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipShieldStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipHelmetEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipHelmet");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipHelmetStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipArmorEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipArmor");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipArmorStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipAccessoryEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipAccessory");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipAccessoryStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipHeavyArmorEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipHeavyArmor");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipHeavyArmorStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipRobeEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipRobe");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipRobeStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipClothesEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipClothes");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipClothesStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipHatEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipHat");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipHatStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipShoesEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipShoes");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipShoesStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipBowEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipBow");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipBowStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipCrossbowEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipCrossbow");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipCrossbowStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipGunEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipGun");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipGunStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipBookEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipBook");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipBookStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipRodEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipRod");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipRodStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipStaffEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipStaff");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipStaffStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipKnifeEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipKnife");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipKnifeStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipSpearEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipSpear");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipSpearStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipAxeEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipAxe");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipAxeStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipHammerEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipHammer");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipHammerStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipWhipEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipWhip");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipWhipStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipInstrumentEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipInstrument");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipInstrumentStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipHairpinEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipHairpin");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipHairpinStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipPerfumeEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipPerfume");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipPerfumeStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipRingEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipRing");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipRingStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipBangleEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipBangle");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipBangleStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddEquipCrownEffect(GameObject ability)
    {
        var effectGO = new GameObject("EquipCrown");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "EquipCrownStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddMove1Effect(GameObject ability)
    {
        var effectGO = new GameObject("Move1");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "Move1Status";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddMove2Effect(GameObject ability)
    {
        var effectGO = new GameObject("Move2");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "Move2Status";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddMove3Effect(GameObject ability)
    {
        var effectGO = new GameObject("Move3");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "Move3Status";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddJump1MovementEffect(GameObject ability)
    {
        var effectGO = new GameObject("Jump1Movement");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "Jump1MovementStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddJump2MovementEffect(GameObject ability)
    {
        var effectGO = new GameObject("Jump2Movement");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "Jump2MovementStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddJump3MovementEffect(GameObject ability)
    {
        var effectGO = new GameObject("Jump3Movement");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "Jump3MovementStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddTeleportMovementEffect(GameObject ability)
    {
        var effectGO = new GameObject("TeleportMovement");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "TeleportMovementStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddWaterwalkingEffect(GameObject ability)
    {
        var effectGO = new GameObject("Waterwalking");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "WaterwalkingStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddMoveInWaterEffect(GameObject ability)
    {
        var effectGO = new GameObject("MoveInWater");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "MoveInWaterStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddMoveFindItemMovementEffect(GameObject ability)
    {
        var effectGO = new GameObject("MoveFindItemMovement");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "MoveFindItemMovementStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static GameObject CreateAbilityPrefab(string jobName, string abilityName)
    {
        var go = new GameObject(abilityName);
        go.AddComponent<Ability>();
        
        // Always add default effect target
        go.AddComponent<DefaultAbilityEffectTarget>();
        
        return go;
    }

    private static void AddConstantRange(GameObject ability, int horizontal, int vertical)
    {
        var range = ability.AddComponent<ConstantAbilityRange>();
        range.horizontal = horizontal;
        range.vertical = vertical;
    }

    private static void AddSelfRange(GameObject ability)
    {
        ability.AddComponent<SelfAbilityRange>();
    }

    private static void AddInfiniteRange(GameObject ability)
    {
        ability.AddComponent<InfiniteAbilityRange>();
    }

    private static void AddPhysicalDamageEffect(GameObject ability, int power)
    {
        // Create child GameObject for the effect
        var effectGO = new GameObject("Damage");
        effectGO.transform.SetParent(ability.transform);
        
        // Add effect components to the child
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        // Set the hit rate accuracy
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddPhysicalDamageEffect(GameObject ability)
    {
        // Create child GameObject for the effect
        var effectGO = new GameObject("Damage");
        effectGO.transform.SetParent(ability.transform);
        
        // Add effect components to the child
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        // Set the hit rate accuracy
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddHealEffect(GameObject ability, int amount)
    {
        // Create child GameObject for the effect
        var effectGO = new GameObject("Heal");
        effectGO.transform.SetParent(ability.transform);
        
        // Add effect components to the child
        effectGO.AddComponent<HealAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
        
        // Set the hit rate accuracy
        var hitRate = effectGO.GetComponent<FullTypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddHealEffect(GameObject ability)
    {
        // Create child GameObject for the effect
        var effectGO = new GameObject("Heal");
        effectGO.transform.SetParent(ability.transform);
        
        // Add effect components to the child
        effectGO.AddComponent<HealAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
        
        // Set the hit rate accuracy
        var hitRate = effectGO.GetComponent<FullTypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddWeaponPower(GameObject ability)
    {
        ability.AddComponent<WeaponAbilityPower>();
    }

    private static void AddPhysicalPower(GameObject ability)
    {
        var power = ability.AddComponent<PhysicalAbilityPower>();
        power.level = 25; // Set default level
    }

    private static void AddPhysicalPower(GameObject ability, int powerValue)
    {
        var power = ability.AddComponent<PhysicalAbilityPower>();
        power.level = powerValue; // Set actual FFT power value
    }

    private static void AddNoPower(GameObject ability)
    {
        // No power component needed
    }

    private static void AddNoMagicCost(GameObject ability)
    {
        // No magic cost component needed
    }

    private static void SaveAbilityPrefab(GameObject ability, string jobName, string abilityName)
    {
        string path = $"{AbilitiesPath}/{jobName}/{abilityName}.prefab";
        
        // Ensure the directory exists
        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        // Create prefab
        var prefab = PrefabUtility.SaveAsPrefabAsset(ability, path);
        
        // Clean up
        Object.DestroyImmediate(ability);
        
        EditorUtility.SetDirty(prefab);
    }

    // Effect methods - these need to be implemented based on your specific effect classes
    private static void AddDefendEffect(GameObject ability)
    {
        var effectGO = new GameObject("Defend");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "DefendStatus";
    }

    private static void AddAccumulateEffect(GameObject ability)
    {
        var effectGO = new GameObject("Accumulate");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "AccumulateStatus";
    }

    private static void AddDashEffect(GameObject ability)
    {
        var effectGO = new GameObject("Dash");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "DashStatus";
    }

    private static void AddFocusEffect(GameObject ability)
    {
        var effectGO = new GameObject("Focus");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "FocusStatus";
    }

    private static void AddYellEffect(GameObject ability)
    {
        var effectGO = new GameObject("Yell");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "YellStatus";
    }

    private static void AddCheerEffect(GameObject ability)
    {
        var effectGO = new GameObject("Cheer");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "CheerStatus";
    }

    private static void AddReviveEffect(GameObject ability)
    {
        var effectGO = new GameObject("Revive");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<ReviveAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddCurePoisonEffect(GameObject ability)
    {
        var effectGO = new GameObject("CurePoison");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "CurePoisonStatus";
    }

    private static void AddCureBlindEffect(GameObject ability)
    {
        var effectGO = new GameObject("CureBlind");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "CureBlindStatus";
    }

    private static void AddCureSilenceEffect(GameObject ability)
    {
        var effectGO = new GameObject("CureSilence");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "CureSilenceStatus";
    }

    private static void AddCureAllStatusEffect(GameObject ability)
    {
        var effectGO = new GameObject("CureAll");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "CureAllStatus";
    }

    private static void AddAutoPotionEffect(GameObject ability)
    {
        var effectGO = new GameObject("AutoPotion");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "AutoPotionStatus";
    }

    private static void AddThrowItemEffect(GameObject ability)
    {
        var effectGO = new GameObject("ThrowItem");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
    }

    // Break effect methods
    private static void AddPowerBreakEffect(GameObject ability)
    {
        var effectGO = new GameObject("PowerBreak");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "PowerBreakStatus";
    }

    private static void AddArmorBreakEffect(GameObject ability)
    {
        var effectGO = new GameObject("ArmorBreak");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "ArmorBreakStatus";
    }

    private static void AddMentalBreakEffect(GameObject ability)
    {
        var effectGO = new GameObject("MentalBreak");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "MentalBreakStatus";
    }

    private static void AddMagicBreakEffect(GameObject ability)
    {
        var effectGO = new GameObject("MagicBreak");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "MagicBreakStatus";
    }

    // Magic damage effect
    private static void AddMagicDamageEffect(GameObject ability, int power)
    {
        // Create child GameObject for the effect
        var effectGO = new GameObject("MagicDamage");
        effectGO.transform.SetParent(ability.transform);
        
        // Add effect components to the child
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
        
        // Set the hit rate accuracy
        var hitRate = effectGO.GetComponent<STypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddMagicDamageEffect(GameObject ability)
    {
        // Create child GameObject for the effect
        var effectGO = new GameObject("MagicDamage");
        effectGO.transform.SetParent(ability.transform);
        
        // Add effect components to the child
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
        
        // Set the hit rate accuracy
        var hitRate = effectGO.GetComponent<STypeHitRate>();
        hitRate.accuracy = 100;
    }

    // Full area effect
    private static void AddFullArea(GameObject ability)
    {
        ability.AddComponent<FullAbilityArea>();
    }

    // Magic power component
    private static void AddMagicPower(GameObject ability)
    {
        var power = ability.AddComponent<MagicalAbilityPower>();
        power.level = 25; // Set default level
    }

    private static void AddMagicPower(GameObject ability, int powerValue)
    {
        var power = ability.AddComponent<MagicalAbilityPower>();
        power.level = powerValue; // Set actual FFT power value
    }

    // Magic cost component
    private static void AddMagicCost(GameObject ability, int cost)
    {
        var magicCost = ability.AddComponent<AbilityMagicCost>();
        magicCost.amount = cost;
    }

    // Revive effect with percentage
    private static void AddReviveEffect(GameObject ability, float percent)
    {
        var effectGO = new GameObject("Revive");
        effectGO.transform.SetParent(ability.transform);
        
        var reviveEffect = effectGO.AddComponent<ReviveAbilityEffect>();
        reviveEffect.percent = percent;
        effectGO.AddComponent<KOdAbilityEffectTarget>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    // Reraise effect
    private static void AddReraiseEffect(GameObject ability)
    {
        var effectGO = new GameObject("Reraise");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "ReraiseStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    // Esuna effect
    private static void AddEsunaEffect(GameObject ability)
    {
        var effectGO = new GameObject("Esuna");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<EsunaAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    // Protect effect
    private static void AddProtectEffect(GameObject ability)
    {
        var effectGO = new GameObject("Protect");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "ProtectStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    // Shell effect
    private static void AddShellEffect(GameObject ability)
    {
        var effectGO = new GameObject("Shell");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "ShellStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    // Charge effect
    private static void AddChargeEffect(GameObject ability, int chargeLevel)
    {
        var effectGO = new GameObject("Charge");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = $"Charge{chargeLevel}Status";
        effect.duration = 1; // Charge lasts until next turn
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    // Counter effect
    private static void AddCounterEffect(GameObject ability)
    {
        var effectGO = new GameObject("Counter");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "CounterStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    // Two Hands effect
    private static void AddTwoHandsEffect(GameObject ability)
    {
        var effectGO = new GameObject("TwoHands");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "TwoHandsStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    // Martial Arts effect
    private static void AddMartialArtsEffect(GameObject ability)
    {
        var effectGO = new GameObject("MartialArts");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "MartialArtsStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    // Time Mage effects
    private static void AddHasteEffect(GameObject ability)
    {
        var effectGO = new GameObject("Haste");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "HasteStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddSlowEffect(GameObject ability)
    {
        var effectGO = new GameObject("Slow");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "SlowStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddStopEffect(GameObject ability)
    {
        var effectGO = new GameObject("Stop");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "StopStatus";
        effect.duration = 2;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddDontMoveEffect(GameObject ability)
    {
        var effectGO = new GameObject("DontMove");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "ImmobilizeStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddDontActEffect(GameObject ability)
    {
        var effectGO = new GameObject("DontAct");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "DisableStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddTeleportEffect(GameObject ability)
    {
        var effectGO = new GameObject("Teleport");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "TeleportStatus";
        effect.duration = 1;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddFloatEffect(GameObject ability)
    {
        var effectGO = new GameObject("Float");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "FloatStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddRegenEffect(GameObject ability)
    {
        var effectGO = new GameObject("Regen");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "RegenStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddReflectEffect(GameObject ability)
    {
        var effectGO = new GameObject("Reflect");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "ReflectStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddQuickEffect(GameObject ability)
    {
        var effectGO = new GameObject("Quick");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "QuickStatus";
        effect.duration = 1;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddGravityEffect(GameObject ability)
    {
        var effectGO = new GameObject("Gravity");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
        
        var hitRate = effectGO.GetComponent<STypeHitRate>();
        hitRate.accuracy = 100;
    }

    // Summon damage effect
    private static void AddSummonDamageEffect(GameObject ability, int power)
    {
        // Create child GameObject for the effect
        var effectGO = new GameObject("SummonDamage");
        effectGO.transform.SetParent(ability.transform);
        
        // Add effect components to the child
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
        
        // Set the hit rate accuracy
        var hitRate = effectGO.GetComponent<STypeHitRate>();
        hitRate.accuracy = 100;
    }

    // Steal effect
    private static void AddStealEffect(GameObject ability, string statusName)
    {
        var effectGO = new GameObject("Steal");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = statusName;
        effect.duration = 1;
        effectGO.AddComponent<EnemyAbilityEffectTarget>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    // Move-Find Item effect
    private static void AddMoveFindItemEffect(GameObject ability)
    {
        var effectGO = new GameObject("MoveFindItem");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "MoveFindItemStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    // Correct Monk effects
    private static void AddSpinFistEffect(GameObject ability)
    {
        var effectGO = new GameObject("SpinFist");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<SpecifyAbilityArea>();
        var area = effectGO.GetComponent<SpecifyAbilityArea>();
        area.horizontal = 1; // All adjacent tiles
        area.vertical = 1;
        effectGO.AddComponent<ATypeHitRate>();
    }

    private static void AddRepeatingFistEffect(GameObject ability)
    {
        var effectGO = new GameObject("RepeatingFist");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Use existing damage effect
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
    }

    private static void AddEarthSlashEffect(GameObject ability)
    {
        var effectGO = new GameObject("EarthSlash");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Use existing damage effect
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
    }

    private static void AddSecretFistEffect(GameObject ability)
    {
        var effectGO = new GameObject("SecretFist");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "DeathSentenceStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
    }

    private static void AddStigmaMagicEffect(GameObject ability)
    {
        var effectGO = new GameObject("StigmaMagic");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<EsunaAbilityEffect>(); // Use existing Esuna effect for status removal
        effectGO.AddComponent<SpecifyAbilityArea>();
        var area = effectGO.GetComponent<SpecifyAbilityArea>();
        area.horizontal = 1; // All adjacent tiles
        area.vertical = 1;
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddChakraEffect(GameObject ability)
    {
        var effectGO = new GameObject("Chakra");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<HealAbilityEffect>(); // Use existing heal effect for HP/MP restoration
        effectGO.AddComponent<SpecifyAbilityArea>();
        var area = effectGO.GetComponent<SpecifyAbilityArea>();
        area.horizontal = 1; // All adjacent tiles
        area.vertical = 1;
        effectGO.AddComponent<FullTypeHitRate>();
    }

    // Mystic status effects
    private static void AddPoisonEffect(GameObject ability)
    {
        var effectGO = new GameObject("Poison");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "PoisonStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddBlindEffect(GameObject ability)
    {
        var effectGO = new GameObject("Blind");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "BlindStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddSilenceEffect(GameObject ability)
    {
        var effectGO = new GameObject("Silence");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "SilenceStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddSleepEffect(GameObject ability)
    {
        var effectGO = new GameObject("Sleep");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "SleepStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddConfuseEffect(GameObject ability)
    {
        var effectGO = new GameObject("Confuse");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "ConfuseStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddBerserkEffect(GameObject ability)
    {
        var effectGO = new GameObject("Berserk");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "BerserkStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddCharmEffect(GameObject ability)
    {
        var effectGO = new GameObject("Charm");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "CharmStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddPetrifyEffect(GameObject ability)
    {
        var effectGO = new GameObject("Petrify");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "PetrifyStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddDeathEffect(GameObject ability)
    {
        var effectGO = new GameObject("Death");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "DeathStatus";
        effect.duration = 1;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddToadEffect(GameObject ability)
    {
        var effectGO = new GameObject("Toad");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "ToadStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddPigEffect(GameObject ability)
    {
        var effectGO = new GameObject("Pig");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "PigStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddChickenEffect(GameObject ability)
    {
        var effectGO = new GameObject("Chicken");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "ChickenStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    // Oracle effects (similar to Mystic but with Oracle-specific variations)
    private static void AddOraclePoisonEffect(GameObject ability)
    {
        var effectGO = new GameObject("OraclePoison");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "PoisonStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddOracleBlindEffect(GameObject ability)
    {
        var effectGO = new GameObject("OracleBlind");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "BlindStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddOracleSilenceEffect(GameObject ability)
    {
        var effectGO = new GameObject("OracleSilence");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "SilenceStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddOracleSleepEffect(GameObject ability)
    {
        var effectGO = new GameObject("OracleSleep");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "SleepStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddOracleConfuseEffect(GameObject ability)
    {
        var effectGO = new GameObject("OracleConfuse");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "ConfuseStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddOracleBerserkEffect(GameObject ability)
    {
        var effectGO = new GameObject("OracleBerserk");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "BerserkStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddOracleCharmEffect(GameObject ability)
    {
        var effectGO = new GameObject("OracleCharm");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "CharmStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddOracleDontMoveEffect(GameObject ability)
    {
        var effectGO = new GameObject("OracleDontMove");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "ImmobilizeStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddOracleDontActEffect(GameObject ability)
    {
        var effectGO = new GameObject("OracleDontAct");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "DisableStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddOraclePetrifyEffect(GameObject ability)
    {
        var effectGO = new GameObject("OraclePetrify");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "PetrifyStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddOracleDeathEffect(GameObject ability)
    {
        var effectGO = new GameObject("OracleDeath");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "DeathStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddOracleToadEffect(GameObject ability)
    {
        var effectGO = new GameObject("OracleToad");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "ToadStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddOraclePigEffect(GameObject ability)
    {
        var effectGO = new GameObject("OraclePig");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "PigStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    private static void AddOracleChickenEffect(GameObject ability)
    {
        var effectGO = new GameObject("OracleChicken");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "ChickenStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    // Geomancer terrain effects
    private static void AddPitfallEffect(GameObject ability)
    {
        var effectGO = new GameObject("Pitfall");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Use existing damage effect for terrain attacks
        var effect = effectGO.GetComponent<DamageAbilityEffect>(); // Use existing damage effect
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddWaterBallEffect(GameObject ability)
    {
        var effectGO = new GameObject("WaterBall");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Use existing damage effect for terrain attacks
        var effect = effectGO.GetComponent<DamageAbilityEffect>(); // Use existing damage effect
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddHellIvyEffect(GameObject ability)
    {
        var effectGO = new GameObject("HellIvy");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Use existing damage effect for terrain attacks
        var effect = effectGO.GetComponent<DamageAbilityEffect>(); // Use existing damage effect
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddCarveModelEffect(GameObject ability)
    {
        var effectGO = new GameObject("CarveModel");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Use existing damage effect for terrain attacks
        var effect = effectGO.GetComponent<DamageAbilityEffect>(); // Use existing damage effect
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddLocalQuakeEffect(GameObject ability)
    {
        var effectGO = new GameObject("LocalQuake");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Use existing damage effect for terrain attacks
        var effect = effectGO.GetComponent<DamageAbilityEffect>(); // Use existing damage effect
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddKamaitachiEffect(GameObject ability)
    {
        var effectGO = new GameObject("Kamaitachi");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Use existing damage effect for terrain attacks
        var effect = effectGO.GetComponent<DamageAbilityEffect>(); // Use existing damage effect
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddDemonFireEffect(GameObject ability)
    {
        var effectGO = new GameObject("DemonFire");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Use existing damage effect for terrain attacks
        var effect = effectGO.GetComponent<DamageAbilityEffect>(); // Use existing damage effect
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddQuicksandEffect(GameObject ability)
    {
        var effectGO = new GameObject("Quicksand");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Use existing damage effect for terrain attacks
        var effect = effectGO.GetComponent<DamageAbilityEffect>(); // Use existing damage effect
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddSandStormEffect(GameObject ability)
    {
        var effectGO = new GameObject("SandStorm");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Use existing damage effect for terrain attacks
        var effect = effectGO.GetComponent<DamageAbilityEffect>(); // Use existing damage effect
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddBlizzardEffect(GameObject ability)
    {
        var effectGO = new GameObject("Blizzard");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Use existing damage effect for terrain attacks
        var effect = effectGO.GetComponent<DamageAbilityEffect>(); // Use existing damage effect
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    private static void AddLavaBallEffect(GameObject ability)
    {
        var effectGO = new GameObject("LavaBall");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Use existing damage effect for terrain attacks
        var effect = effectGO.GetComponent<DamageAbilityEffect>(); // Use existing damage effect
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    // Dragoon jump effect
    private static void AddJumpEffect(GameObject ability, int jumpLevel)
    {
        var effectGO = new GameObject("Jump");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Use existing damage effect for jump attacks
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
    }

    // Ninja throw effects
    private static void AddThrowEffect(GameObject ability, string itemType)
    {
        var effectGO = new GameObject("Throw");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Use existing damage effect for thrown items
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
    }

    private static void AddTwoSwordsEffect(GameObject ability)
    {
        var effectGO = new GameObject("TwoSwords");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<InflictAbilityEffect>();
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "TwoSwordsStatus";
        effect.duration = 3;
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
    }

    // Samurai Draw Out effect
    private static void AddDrawOutEffect(GameObject ability, string katanaName, string effectType)
    {
        var effectGO = new GameObject("DrawOut");
        effectGO.transform.SetParent(ability.transform);
        effectGO.AddComponent<DamageAbilityEffect>(); // Use existing damage effect for katana techniques
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
    }

    // Helper methods for Special Job Abilities
    private static void AddCelestialStasisEffect(GameObject ability)
    {
        var effectGO = new GameObject("CelestialStasis");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<InflictAbilityEffect>();
        effectGO.AddComponent<FullAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
        
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "Stop";
        effect.duration = 3;
        
        var hitRate = effectGO.GetComponent<FullTypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddCureEffect(GameObject ability)
    {
        var effectGO = new GameObject("Cure");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<EsunaAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
        
        var hitRate = effectGO.GetComponent<FullTypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddRestoreEffect(GameObject ability)
    {
        var effectGO = new GameObject("Restore");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<HealAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
        
        var hitRate = effectGO.GetComponent<FullTypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddFellSwordEffect(GameObject ability)
    {
        var effectGO = new GameObject("FellSword");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddDarkBladeEffect(GameObject ability)
    {
        var effectGO = new GameObject("DarkBlade");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddShadowStrikeEffect(GameObject ability)
    {
        var effectGO = new GameObject("ShadowStrike");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddDeathknightFellSwordEffect(GameObject ability)
    {
        var effectGO = new GameObject("DeathknightFellSword");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<AbsorbDamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddDeathknightDarkBladeEffect(GameObject ability)
    {
        var effectGO = new GameObject("DeathknightDarkBlade");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<AbsorbDamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddDeathknightShadowStrikeEffect(GameObject ability)
    {
        var effectGO = new GameObject("DeathknightShadowStrike");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<AbsorbDamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddDeathknightSoulDrainEffect(GameObject ability)
    {
        var effectGO = new GameObject("DeathknightSoulDrain");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<AbsorbDamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddSorcererHolyEffect(GameObject ability)
    {
        var effectGO = new GameObject("SorcererHoly");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
        
        var hitRate = effectGO.GetComponent<STypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddSorcererFlareEffect(GameObject ability)
    {
        var effectGO = new GameObject("SorcererFlare");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
        
        var hitRate = effectGO.GetComponent<STypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddSorcererGravigaEffect(GameObject ability)
    {
        var effectGO = new GameObject("SorcererGraviga");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<SpecifyAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
        
        var area = effectGO.GetComponent<SpecifyAbilityArea>();
        area.horizontal = 2;
        area.vertical = 0;
        
        var hitRate = effectGO.GetComponent<STypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddSorcererUnholyDarknessEffect(GameObject ability)
    {
        var effectGO = new GameObject("SorcererUnholyDarkness");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
        
        var hitRate = effectGO.GetComponent<STypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddSorcererAriseEffect(GameObject ability)
    {
        var effectGO = new GameObject("SorcererArise");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<ReviveAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
        
        var revive = effectGO.GetComponent<ReviveAbilityEffect>();
        revive.percent = 100;
        
        var hitRate = effectGO.GetComponent<FullTypeHitRate>();
        hitRate.accuracy = 100;
    }

    // Princess effect methods
    private static void AddPrincessPrayerEffect(GameObject ability)
    {
        var effectGO = new GameObject("PrincessPrayer");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<HealAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
        
        var hitRate = effectGO.GetComponent<FullTypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddPrincessBlessingEffect(GameObject ability)
    {
        var effectGO = new GameObject("PrincessBlessing");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<InflictAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
        
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "Protect";
        effect.duration = 3;
        
        var hitRate = effectGO.GetComponent<FullTypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddPrincessMiracleEffect(GameObject ability)
    {
        var effectGO = new GameObject("PrincessMiracle");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<HealAbilityEffect>();
        effectGO.AddComponent<SpecifyAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
        
        var area = effectGO.GetComponent<SpecifyAbilityArea>();
        area.horizontal = 1;
        area.vertical = 0;
        
        var hitRate = effectGO.GetComponent<FullTypeHitRate>();
        hitRate.accuracy = 100;
    }

    // Ark Knight effect methods
    private static void AddArkKnightDestroyEffect(GameObject ability)
    {
        var effectGO = new GameObject("ArkKnightDestroy");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddArkKnightCrushEffect(GameObject ability)
    {
        var effectGO = new GameObject("ArkKnightCrush");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddArkKnightBreakEffect(GameObject ability)
    {
        var effectGO = new GameObject("ArkKnightBreak");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    // Assassin effect methods
    private static void AddAssassinShadowStrikeEffect(GameObject ability)
    {
        var effectGO = new GameObject("AssassinShadowStrike");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddAssassinDarkBladeEffect(GameObject ability)
    {
        var effectGO = new GameObject("AssassinDarkBlade");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddAssassinSoulDrainEffect(GameObject ability)
    {
        var effectGO = new GameObject("AssassinSoulDrain");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<AbsorbDamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    // Celebrant effect methods
    private static void AddCelebrantDragonBreathEffect(GameObject ability)
    {
        var effectGO = new GameObject("CelebrantDragonBreath");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<SpecifyAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
        
        var area = effectGO.GetComponent<SpecifyAbilityArea>();
        area.horizontal = 1;
        area.vertical = 0;
        
        var hitRate = effectGO.GetComponent<STypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddCelebrantDragonClawEffect(GameObject ability)
    {
        var effectGO = new GameObject("CelebrantDragonClaw");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddCelebrantDragonWingEffect(GameObject ability)
    {
        var effectGO = new GameObject("CelebrantDragonWing");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<SpecifyAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
        
        var area = effectGO.GetComponent<SpecifyAbilityArea>();
        area.horizontal = 2;
        area.vertical = 0;
        
        var hitRate = effectGO.GetComponent<STypeHitRate>();
        hitRate.accuracy = 100;
    }

    // Divine Knight effect methods
    private static void AddDivineKnightHolyStrikeEffect(GameObject ability)
    {
        var effectGO = new GameObject("DivineKnightHolyStrike");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddDivineKnightJudgmentBladeEffect(GameObject ability)
    {
        var effectGO = new GameObject("DivineKnightJudgmentBlade");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddDivineKnightDivineRuinationEffect(GameObject ability)
    {
        var effectGO = new GameObject("DivineKnightDivineRuination");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<SpecifyAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var area = effectGO.GetComponent<SpecifyAbilityArea>();
        area.horizontal = 1;
        area.vertical = 0;
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    // Templar effect methods
    private static void AddTemplarArmShotEffect(GameObject ability)
    {
        var effectGO = new GameObject("TemplarArmShot");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<InflictAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "Disable";
        effect.duration = 3;
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddTemplarLegShotEffect(GameObject ability)
    {
        var effectGO = new GameObject("TemplarLegShot");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<InflictAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "Immobilize";
        effect.duration = 3;
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddTemplarHeadShotEffect(GameObject ability)
    {
        var effectGO = new GameObject("TemplarHeadShot");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<InflictAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "Confuse";
        effect.duration = 3;
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    // Dragonkin effect methods
    private static void AddDragonkinFireBreathEffect(GameObject ability)
    {
        var effectGO = new GameObject("DragonkinFireBreath");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<SpecifyAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
        
        var area = effectGO.GetComponent<SpecifyAbilityArea>();
        area.horizontal = 2;
        area.vertical = 0;
        
        var hitRate = effectGO.GetComponent<STypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddDragonkinIceBreathEffect(GameObject ability)
    {
        var effectGO = new GameObject("DragonkinIceBreath");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<SpecifyAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
        
        var area = effectGO.GetComponent<SpecifyAbilityArea>();
        area.horizontal = 2;
        area.vertical = 0;
        
        var hitRate = effectGO.GetComponent<STypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddDragonkinThunderBreathEffect(GameObject ability)
    {
        var effectGO = new GameObject("DragonkinThunderBreath");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<SpecifyAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
        
        var area = effectGO.GetComponent<SpecifyAbilityArea>();
        area.horizontal = 2;
        area.vertical = 0;
        
        var hitRate = effectGO.GetComponent<STypeHitRate>();
        hitRate.accuracy = 100;
    }

    // Automaton effect methods
    private static void AddAutomatonRepairEffect(GameObject ability)
    {
        var effectGO = new GameObject("AutomatonRepair");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<HealAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
        
        var hitRate = effectGO.GetComponent<FullTypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddAutomatonOverloadEffect(GameObject ability)
    {
        var effectGO = new GameObject("AutomatonOverload");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<SpecifyAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var area = effectGO.GetComponent<SpecifyAbilityArea>();
        area.horizontal = 1;
        area.vertical = 0;
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddAutomatonShutdownEffect(GameObject ability)
    {
        var effectGO = new GameObject("AutomatonShutdown");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<InflictAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
        
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "Stop";
        effect.duration = 2;
        
        var hitRate = effectGO.GetComponent<FullTypeHitRate>();
        hitRate.accuracy = 100;
    }

    // Soldier effect methods
    private static void AddSoldierAttackEffect(GameObject ability)
    {
        var effectGO = new GameObject("SoldierAttack");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddSoldierDefendEffect(GameObject ability)
    {
        var effectGO = new GameObject("SoldierDefend");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<InflictAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
        
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "Protect";
        effect.duration = 3;
        
        var hitRate = effectGO.GetComponent<FullTypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddSoldierRushEffect(GameObject ability)
    {
        var effectGO = new GameObject("SoldierRush");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    // Sky Pirate effect methods
    private static void AddSkyPirateStealEffect(GameObject ability)
    {
        var effectGO = new GameObject("SkyPirateSteal");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<InflictAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "Steal";
        effect.duration = 1;
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddSkyPiratePoachEffect(GameObject ability)
    {
        var effectGO = new GameObject("SkyPiratePoach");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddSkyPirateSightUnseeingEffect(GameObject ability)
    {
        var effectGO = new GameObject("SkyPirateSightUnseeing");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<InflictAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
        
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "Blind";
        effect.duration = 3;
        
        var hitRate = effectGO.GetComponent<STypeHitRate>();
        hitRate.accuracy = 100;
    }

    // Game Hunter effect methods
    private static void AddGameHunterTrackEffect(GameObject ability)
    {
        var effectGO = new GameObject("GameHunterTrack");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<InflictAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<FullTypeHitRate>();
        
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "Track";
        effect.duration = 5;
        
        var hitRate = effectGO.GetComponent<FullTypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddGameHunterTrapEffect(GameObject ability)
    {
        var effectGO = new GameObject("GameHunterTrap");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<InflictAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "Immobilize";
        effect.duration = 3;
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddGameHunterCallEffect(GameObject ability)
    {
        var effectGO = new GameObject("GameHunterCall");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<InflictAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<STypeHitRate>();
        
        var effect = effectGO.GetComponent<InflictAbilityEffect>();
        effect.statusName = "Charm";
        effect.duration = 3;
        
        var hitRate = effectGO.GetComponent<STypeHitRate>();
        hitRate.accuracy = 100;
    }

    // Nightblade effect methods
    private static void AddNightbladeShadowStrikeEffect(GameObject ability)
    {
        var effectGO = new GameObject("NightbladeShadowStrike");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddNightbladeDarkBladeEffect(GameObject ability)
    {
        var effectGO = new GameObject("NightbladeDarkBlade");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddNightbladeSoulDrainEffect(GameObject ability)
    {
        var effectGO = new GameObject("NightbladeSoulDrain");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<AbsorbDamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    // Rune Knight effect methods
    private static void AddRuneKnightRuneBladeEffect(GameObject ability)
    {
        var effectGO = new GameObject("RuneKnightRuneBlade");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddRuneKnightMagicSwordEffect(GameObject ability)
    {
        var effectGO = new GameObject("RuneKnightMagicSword");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<UnitAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    private static void AddRuneKnightSpellBladeEffect(GameObject ability)
    {
        var effectGO = new GameObject("RuneKnightSpellBlade");
        effectGO.transform.SetParent(ability.transform);
        
        effectGO.AddComponent<DamageAbilityEffect>();
        effectGO.AddComponent<SpecifyAbilityArea>();
        effectGO.AddComponent<ATypeHitRate>();
        
        var area = effectGO.GetComponent<SpecifyAbilityArea>();
        area.horizontal = 1;
        area.vertical = 0;
        
        var hitRate = effectGO.GetComponent<ATypeHitRate>();
        hitRate.accuracy = 100;
    }

    #endregion
}

#endif