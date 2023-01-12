using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class HitSuccessIndicator : MonoBehaviour
{
    private const string ShowKey = "Show";
    private const string HideKey = "Hide";

    [SerializeField] private Canvas canvas;
    [SerializeField] private Panel panel;
    [SerializeField] private Image arrow;
    [SerializeField] private TextMeshProUGUI label;
    private Tweener transition;

    private void Start()
    {
        panel.SetPosition(HideKey, false);
        canvas.gameObject.SetActive(false);
    }

    public void SetStats(int chance, int amount)
    {
        arrow.fillAmount = chance / 100f;
        label.text = $"{chance}% {Mathf.Abs(amount)}pt(s)";
        label.color = amount > 0 ? Color.green : Color.red;
    }

    public void Show()
    {
        canvas.gameObject.SetActive(true);
        SetPanelPos(ShowKey);
    }

    public void Hide()
    {
        SetPanelPos(HideKey);
        transition.completedEvent += delegate(object sender, System.EventArgs e)
        {
            canvas.gameObject.SetActive(false);
        };
    }

    private void SetPanelPos(string pos)
    {
        if (transition != null && transition.IsPlaying)
            transition.Stop();

        transition = panel.SetPosition(pos, true);
        transition.duration = 0.5f;
        transition.equation = EasingEquations.EaseInOutQuad;
    }
}