using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Entry for displaying a single job in the job selection menu
/// Similar to AbilityMenuEntry but for jobs
/// </summary>
public class JobMenuEntry : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI jobNameLabel;
    [SerializeField] private TextMeshProUGUI jobLevelLabel;
    [SerializeField] private TextMeshProUGUI jpLabel;
    [SerializeField] private Image jobIcon;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image selectionHighlight;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private Image progressBar;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color lockedColor = Color.gray;
    [SerializeField] private Color currentJobColor = Color.green;

    private JobDefinition job;
    private bool isSelected;
    private bool isLocked;
    private bool isCurrentJob;

    #region Properties

    public JobDefinition Job
    {
        get => job;
        set
        {
            job = value;
            UpdateDisplay();
        }
    }

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            UpdateSelection();
        }
    }

    public bool IsLocked
    {
        get => isLocked;
        set
        {
            isLocked = value;
            UpdateLockState();
        }
    }

    public bool IsCurrentJob
    {
        get => isCurrentJob;
        set
        {
            isCurrentJob = value;
            UpdateDisplay();
        }
    }

    #endregion

    #region Public Methods

    public void SetJobData(JobDefinition jobDef, int jobLevel, int currentJP, bool current, bool locked)
    {
        job = jobDef;
        isCurrentJob = current;
        isLocked = locked;

        if (jobNameLabel != null && jobDef != null)
            jobNameLabel.text = jobDef.jobName;

        if (jobLevelLabel != null)
            jobLevelLabel.text = JobSystemHelper.FormatJobLevel(jobLevel);

        if (jpLabel != null)
            jpLabel.text = JobSystemHelper.FormatJP(currentJP);

        if (jobIcon != null && jobDef != null && jobDef.jobIcon != null)
        {
            jobIcon.sprite = jobDef.jobIcon;
            jobIcon.enabled = true;
        }
        else if (jobIcon != null)
        {
            jobIcon.enabled = false;
        }

        // Update progress bar
        if (progressBar != null && jobDef != null)
        {
            int jpForNext = jobDef.GetJPForNextLevel(currentJP);
            int jpForCurrent = jobLevel > 1 ? jobDef.jpRequirements[jobLevel - 2] : 0;
            float progress = JobSystemHelper.GetProgressFill(currentJP - jpForCurrent, jpForNext - jpForCurrent);
            progressBar.fillAmount = progress;
        }

        UpdateLockState();
        UpdateDisplay();
    }

    public void Reset()
    {
        job = null;
        isSelected = false;
        isLocked = false;
        isCurrentJob = false;

        if (jobNameLabel != null)
            jobNameLabel.text = "";

        if (jobLevelLabel != null)
            jobLevelLabel.text = "";

        if (jpLabel != null)
            jpLabel.text = "";

        if (jobIcon != null)
            jobIcon.enabled = false;

        if (selectionHighlight != null)
            selectionHighlight.enabled = false;

        if (lockedOverlay != null)
            lockedOverlay.SetActive(false);

        if (progressBar != null)
            progressBar.fillAmount = 0f;
    }

    #endregion

    #region Private Methods

    private void UpdateDisplay()
    {
        if (job == null)
            return;

        Color textColor = normalColor;

        if (isCurrentJob)
        {
            textColor = currentJobColor;
        }
        else if (isLocked)
        {
            textColor = lockedColor;
        }

        if (jobNameLabel != null)
            jobNameLabel.color = textColor;

        if (jobLevelLabel != null)
            jobLevelLabel.color = textColor;

        if (jpLabel != null)
            jpLabel.color = textColor;

        // Set background color based on job category
        if (backgroundImage != null && job != null)
        {
            backgroundImage.color = JobSystemHelper.GetCategoryColor(job.category);
        }
    }

    private void UpdateSelection()
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.enabled = isSelected;
            if (isSelected)
                selectionHighlight.color = selectedColor;
        }
    }

    private void UpdateLockState()
    {
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(isLocked);
        }
    }

    #endregion
}
