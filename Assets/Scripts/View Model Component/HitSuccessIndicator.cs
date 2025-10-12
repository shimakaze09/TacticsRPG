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
        TrySetPanelPos(ShowKey);
    }

    public void Hide()
    {
        if (!TrySetPanelPos(HideKey))
        {
            canvas.gameObject.SetActive(false);
            return;
        }

        transition.completedEvent += delegate (object sender, System.EventArgs e)
        {
            canvas.gameObject.SetActive(false);
        };
    }

    private bool TrySetPanelPos(string pos)
    {
        if (transition != null && transition.IsPlaying)
        {
            transition.Stop();
            transition = null;
        }

        transition = panel.SetPosition(pos, true);
        if (transition == null)
            return false;

        transition.duration = 0.5f;
        transition.equation = EasingEquations.EaseInOutQuad;
        return true;
    }
}