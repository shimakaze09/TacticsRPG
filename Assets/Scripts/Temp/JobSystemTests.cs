using UnityEngine;

/// <summary>
/// Test harness for the FFT-style Job System
/// Run these tests to validate job switching, stat calculations, and ability management
/// </summary>
public class JobSystemTests : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runTestsOnStart = false;
    [SerializeField] private JobDefinition testJobSquire;
    [SerializeField] private JobDefinition testJobKnight;
    [SerializeField] private JobDefinition testJobWizard;

    private void Start()
    {
        if (runTestsOnStart)
        {
            RunAllTests();
        }
    }

    [ContextMenu("Run All Job System Tests")]
    public void RunAllTests()
    {
        Debug.Log("========== JOB SYSTEM TESTS ==========");
        
        TestJobProgressData();
        TestAbilityMemory();
        TestJobPrerequisites();
        TestStatCalculation();
        TestJobSwitching();
        TestJPGains();
        
        Debug.Log("========== TESTS COMPLETE ==========");
    }

    [ContextMenu("Test: Job Progress Data")]
    private void TestJobProgressData()
    {
        Debug.Log("\n--- Testing JobProgressData ---");

        var progressData = new JobProgressData();
        
        if (testJobSquire == null)
        {
            Debug.LogWarning("Test job (Squire) not assigned. Skipping test.");
            return;
        }

        // Test initialization
        progressData.InitializeWithBasicJobs(testJobSquire);
        Assert(progressData.currentJob == testJobSquire, "Current job should be Squire");
        Assert(progressData.IsJobUnlocked(testJobSquire), "Squire should be unlocked");
        Assert(progressData.GetJobLevel(testJobSquire) == 1, "Squire should start at level 1");

        // Test JP addition
        bool leveledUp = progressData.AddJobPoints(testJobSquire, 100);
        Assert(leveledUp, "Should level up after gaining 100 JP");
        Assert(progressData.GetJobLevel(testJobSquire) == 2, "Squire should be level 2");

        // Test multiple level ups
        progressData.AddJobPoints(testJobSquire, 500);
        int level = progressData.GetJobLevel(testJobSquire);
        Assert(level > 2, $"Squire should be higher than level 2, got {level}");

        Debug.Log("✓ JobProgressData tests passed");
    }

    [ContextMenu("Test: Ability Memory")]
    private void TestAbilityMemory()
    {
        Debug.Log("\n--- Testing AbilityMemory ---");

        var abilityMemory = new AbilityMemory();

        // Test learning abilities
        bool learned = abilityMemory.LearnAbility("Fire");
        Assert(learned, "Should learn new ability");
        Assert(abilityMemory.HasLearnedAbility("Fire"), "Fire should be learned");

        // Test duplicate learning
        learned = abilityMemory.LearnAbility("Fire");
        Assert(!learned, "Should not learn already-learned ability");

        // Test equipping abilities
        bool equipped = abilityMemory.EquipSupportAbility("Fire");
        Assert(equipped, "Should equip learned ability");
        Assert(abilityMemory.IsAbilityEquipped("Fire"), "Fire should be equipped");

        // Test slot limits
        abilityMemory.LearnAbility("Blizzard");
        abilityMemory.LearnAbility("Thunder");
        abilityMemory.LearnAbility("Cure");
        abilityMemory.LearnAbility("Protect");
        abilityMemory.LearnAbility("Shell");

        abilityMemory.EquipSupportAbility("Blizzard");
        abilityMemory.EquipSupportAbility("Thunder");
        abilityMemory.EquipSupportAbility("Cure");
        abilityMemory.EquipSupportAbility("Protect");

        bool shouldFail = abilityMemory.EquipSupportAbility("Shell");
        Assert(!shouldFail, "Should not equip beyond slot limit (5)");

        Assert(abilityMemory.GetAvailableSupportSlots() == 0, "Should have 0 available slots");

        Debug.Log("✓ AbilityMemory tests passed");
    }

    [ContextMenu("Test: Job Prerequisites")]
    private void TestJobPrerequisites()
    {
        Debug.Log("\n--- Testing Job Prerequisites ---");

        if (testJobSquire == null || testJobKnight == null)
        {
            Debug.LogWarning("Test jobs not assigned. Skipping test.");
            return;
        }

        var progressData = new JobProgressData();
        progressData.InitializeWithBasicJobs(testJobSquire);

        // Knight should require Squire Lv2
        bool canUnlock = testJobKnight.CanUnlock(progressData, 1, "TestCharacter");
        Assert(!canUnlock, "Knight should not be unlockable at Squire Lv1");

        // Level up Squire
        progressData.SetJobLevel(testJobSquire, 2);
        canUnlock = testJobKnight.CanUnlock(progressData, 1, "TestCharacter");
        Assert(canUnlock, "Knight should be unlockable at Squire Lv2");

        Debug.Log("✓ Job Prerequisites tests passed");
    }

    [ContextMenu("Test: Stat Calculation")]
    private void TestStatCalculation()
    {
        Debug.Log("\n--- Testing Stat Calculation ---");

        if (testJobSquire == null)
        {
            Debug.LogWarning("Test job not assigned. Skipping test.");
            return;
        }

        int[] stats = new int[7];
        
        // Calculate stats for 5 levels in Squire
        testJobSquire.CalculateStatContribution(5, ref stats);
        
        Assert(stats[0] > 0, "HP should be calculated");
        Assert(stats[2] > 0, "ATK should be calculated");

        Debug.Log($"Stats after 5 levels in Squire: HP={stats[0]}, MP={stats[1]}, ATK={stats[2]}, DEF={stats[3]}, MAT={stats[4]}, MDF={stats[5]}, SPD={stats[6]}");

        // Test multi-job stat calculation
        if (testJobKnight != null)
        {
            int[] multiJobStats = new int[7];
            testJobSquire.CalculateStatContribution(3, ref multiJobStats);
            testJobKnight.CalculateStatContribution(2, ref multiJobStats);

            Debug.Log($"Stats after 3 Squire + 2 Knight: HP={multiJobStats[0]}, ATK={multiJobStats[2]}");
            Assert(multiJobStats[0] > stats[0], "Multi-job HP should be higher");
        }

        Debug.Log("✓ Stat Calculation tests passed");
    }

    [ContextMenu("Test: Job Switching")]
    private void TestJobSwitching()
    {
        Debug.Log("\n--- Testing Job Switching ---");

        if (testJobSquire == null || testJobKnight == null)
        {
            Debug.LogWarning("Test jobs not assigned. Skipping test.");
            return;
        }

        var progressData = new JobProgressData();
        progressData.InitializeWithBasicJobs(testJobSquire);
        progressData.UnlockJob(testJobKnight);

        bool switched = progressData.SwitchJob(testJobKnight);
        Assert(switched, "Should switch to unlocked job");
        Assert(progressData.currentJob == testJobKnight, "Current job should be Knight");

        // Try switching to locked job
        if (testJobWizard != null && !progressData.IsJobUnlocked(testJobWizard))
        {
            switched = progressData.SwitchJob(testJobWizard);
            Assert(!switched, "Should not switch to locked job");
        }

        Debug.Log("✓ Job Switching tests passed");
    }

    [ContextMenu("Test: JP Gains and Levels")]
    private void TestJPGains()
    {
        Debug.Log("\n--- Testing JP Gains ---");

        if (testJobSquire == null)
        {
            Debug.LogWarning("Test job not assigned. Skipping test.");
            return;
        }

        var progressData = new JobProgressData();
        progressData.InitializeWithBasicJobs(testJobSquire);

        // Test JP to level conversion
        int level1 = testJobSquire.GetJobLevelForJP(0);
        int level2 = testJobSquire.GetJobLevelForJP(100);
        int level3 = testJobSquire.GetJobLevelForJP(250);

        Assert(level1 == 1, "0 JP should be level 1");
        Assert(level2 == 2, "100 JP should be level 2");
        Assert(level3 == 3, "250 JP should be level 3");

        // Test JP requirements
        int jpForNext = testJobSquire.GetJPForNextLevel(0);
        Assert(jpForNext == testJobSquire.jpRequirements[0], "Should return correct JP for next level");

        Debug.Log("✓ JP Gains tests passed");
    }

    private void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError($"ASSERTION FAILED: {message}");
        }
        else
        {
            Debug.Log($"✓ {message}");
        }
    }
}
