using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// One-shot content generation (issue #7): validates the content JSON, then
/// runs the three generators in the required order — Abilities → Catalogs →
/// Jobs — so a fresh checkout needs a single command instead of three manual
/// menu steps. The headless entry exits non-zero when validation or any
/// generator reports an error, which is what CI gates on.
/// </summary>
public static class ContentGenerationMenu
{
    /// <summary>Interactive form of the same ordered, validated run.</summary>
    [MenuItem("Tactics RPG/Generate Content/All (Validated)")]
    public static void GenerateAll()
    {
        Run();
    }

    /// <summary>
    /// Batch entry point (CI / headless):
    /// Unity -batchmode -nographics -projectPath . -executeMethod ContentGenerationMenu.RunHeadless
    /// — no -quit; this method exits with 0 only when validation passes and
    /// every generator finishes without logging an error.
    /// </summary>
    public static void RunHeadless()
    {
        EditorApplication.Exit(Run() ? 0 : 1);
    }

    /// <summary>
    /// Validates first (nothing is written when the data is broken), then
    /// generates while counting every logged error so silent generator
    /// failures still fail the run. Returns true when everything succeeded —
    /// CIBuild gates the player build on it.
    /// </summary>
    public static bool Run()
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        ContentValidator.Validate(errors, warnings);

        foreach (var warning in warnings)
            Debug.LogWarning("[Content] " + warning);

        if (errors.Count > 0)
        {
            foreach (var error in errors)
                Debug.LogError("[Content] " + error);
            Debug.LogError($"[Content] Validation FAILED with {errors.Count} error(s) — nothing was generated.");
            return false;
        }

        var loggedErrors = 0;

        void CountErrors(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                loggedErrors++;
        }

        Application.logMessageReceived += CountErrors;
        try
        {
            AbilityAssetGenerator.GenerateAbilitiesFromJSON();
            CatalogAssetGenerator.GenerateCatalogsFromJSON();
            JobAssetGenerator.GenerateJobsFromJSON();
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Content] Generation threw: " + e);
        }
        finally
        {
            Application.logMessageReceived -= CountErrors;
        }

        if (loggedErrors > 0)
        {
            Debug.LogError($"[Content] Generation FAILED — {loggedErrors} error(s) logged.");
            return false;
        }

        Debug.Log("[Content] Validation and generation complete — Abilities, Catalogs, Jobs.");
        return true;
    }
}
#endif
