using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Post-battle results screen: shows EXP/Cert/scrip earned and waits for the
/// player to continue. Shown/hidden by UIManager via MenuType.BattleResults;
/// populated by PostBattleController.
/// </summary>
public class BattleResultsPanelController : MonoBehaviour
{
    [Header("Labels")]
    [Tooltip("Panel title")]
    public TMP_Text titleLabel;

    [Tooltip("EXP earned line")]
    public TMP_Text expLabel;

    [Tooltip("Cert (job points) earned line")]
    public TMP_Text certLabel;

    [Tooltip("Scrip (currency) earned line")]
    public TMP_Text scripLabel;

    [Header("Buttons")]
    [Tooltip("Closes the results screen and continues the post-battle flow")]
    public Button continueButton;

    private PostBattleController postBattleController;

    private void Awake()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);
    }

    /// <summary>
    /// Fill the panel from battle results. Called by PostBattleController
    /// right after UIManager shows this menu.
    /// </summary>
    public void Display(BattleResultsData results)
    {
        if (results == null)
            return;

        if (titleLabel != null)
            titleLabel.text = results.victory ? "Contract Fulfilled" : "Contract Failed";

        if (expLabel != null)
            expLabel.text = $"EXP earned: {results.expGained}";

        if (certLabel != null)
            certLabel.text = $"Cert earned: {results.jpGained}";

        if (scripLabel != null)
            scripLabel.text = $"Scrip earned: {results.goldGained}";
    }

    private void OnContinueClicked()
    {
        if (postBattleController == null)
            postBattleController = FindAnyObjectByType<PostBattleController>();

        if (postBattleController != null)
            postBattleController.OnResultsClosed();
        else if (UIManager.Instance != null)
            UIManager.Instance.HideMenu();
    }
}
