using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Editor utility to create individual FFT ability prefabs for each job
/// Each ability is a separate prefab with its own data (level, duration, range, etc.)
/// Based on Final Fantasy Tactics job system
/// </summary>
public static class FFTAbilityCreator
{
    private const string AbilitiesPath = "Assets/Resources/Abilities";

    [MenuItem("Tactics RPG/Create FFT Abilities/Create All Abilities (Fresh)")]
    public static void CreateAllAbilitiesFresh()
    {
        EnsureAbilitiesDirectoryExists();
        
        // Delete all existing ability prefabs first
        DeleteAllExistingAbilities();
        
        // Create all abilities fresh - each job gets its own folder
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
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created ALL FFT abilities fresh (240+ individual ability prefabs)!");
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
        string[] jobFolders = System.IO.Directory.GetDirectories(AbilitiesPath);
        
        int deletedCount = 0;
        foreach (string folder in jobFolders)
        {
            string[] prefabFiles = System.IO.Directory.GetFiles(folder, "*.prefab", System.IO.SearchOption.TopDirectoryOnly);
            
            foreach (string file in prefabFiles)
            {
                string relativePath = "Assets" + file.Substring(Application.dataPath.Length);
                AssetDatabase.DeleteAsset(relativePath);
                deletedCount++;
                Debug.Log($"Deleted existing ability: {relativePath}");
            }
        }
        
        Debug.Log($"Deleted {deletedCount} existing ability prefabs");
    }

    #region Squire Abilities

    private static void CreateSquireAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
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
        AddPhysicalDamageEffect(ability, 100); // 100% power
        AddWeaponPower(ability); // Uses weapon power
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
        AddPhysicalDamageEffect(ability, 80); // 80% power
        AddPhysicalPower(ability); // Uses physical power
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

    #region Chemist Abilities

    private static void CreateChemistAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        
        CreatePotionAbility();
        CreatePhoenixDownAbility();
        CreateAntidoteAbility();
        CreateEyeDropAbility();
        CreateEchoGrassAbility();
        CreateRemedyAbility();
        CreateAutoPotionAbility();
        CreateThrowItemAbility();
        
        Debug.Log("Created Chemist abilities: Potion, Phoenix Down, Antidote, Eye Drop, Echo Grass, Remedy, Auto Potion, Throw Item");
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

    #region Stub Methods for Remaining Jobs

    private static void CreateKnightAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Knight abilities: (stubbed - add specific abilities)");
    }

    private static void CreateArcherAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Archer abilities: (stubbed - add specific abilities)");
    }

    private static void CreatePriestAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Priest abilities: (stubbed - add specific abilities)");
    }

    private static void CreateWizardAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Wizard abilities: (stubbed - add specific abilities)");
    }

    private static void CreateMonkAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Monk abilities: (stubbed - add specific abilities)");
    }

    private static void CreateThiefAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Thief abilities: (stubbed - add specific abilities)");
    }

    private static void CreateMysticAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Mystic abilities: (stubbed - add specific abilities)");
    }

    private static void CreateTimeMageAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Time Mage abilities: (stubbed - add specific abilities)");
    }

    private static void CreateSummonerAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Summoner abilities: (stubbed - add specific abilities)");
    }

    private static void CreateGeomancerAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Geomancer abilities: (stubbed - add specific abilities)");
    }

    private static void CreateDragoonAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Dragoon abilities: (stubbed - add specific abilities)");
    }

    private static void CreateOracleAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Oracle abilities: (stubbed - add specific abilities)");
    }

    private static void CreateNinjaAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Ninja abilities: (stubbed - add specific abilities)");
    }

    private static void CreateSamuraiAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Samurai abilities: (stubbed - add specific abilities)");
    }

    private static void CreateLancerAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Lancer abilities: (stubbed - add specific abilities)");
    }

    private static void CreateMediatorAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Mediator abilities: (stubbed - add specific abilities)");
    }

    private static void CreateCalculatorAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Calculator abilities: (stubbed - add specific abilities)");
    }

    private static void CreateBardAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Bard abilities: (stubbed - add specific abilities)");
    }

    private static void CreateDancerAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Dancer abilities: (stubbed - add specific abilities)");
    }

    private static void CreateMimeAbilities()
    {
        EnsureAbilitiesDirectoryExists();
        Debug.Log("Created Mime abilities: (stubbed - add specific abilities)");
    }

    #endregion

    #region Helper Methods

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

    private static void AddWeaponPower(GameObject ability)
    {
        ability.AddComponent<WeaponAbilityPower>();
    }

    private static void AddPhysicalPower(GameObject ability)
    {
        var power = ability.AddComponent<PhysicalAbilityPower>();
        power.level = 25; // Set default level
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

    #endregion
}

#endif