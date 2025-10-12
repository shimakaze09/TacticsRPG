using System;
using TMPro;
using UnityEngine;

public class DialogController : MonoBehaviour
{
    [SerializeField] private GameObject blocker;
    [SerializeField] private Transform content;
    [SerializeField] private TextMeshProUGUI messageLabel;
    private Action onCancel;
    private Action onConfirm;
    [SerializeField] private TextMeshProUGUI titleLabel;
    private Tweener tweener;

    private void Start()
    {
        blocker.SetActive(false);
        content.localScale = Vector3.zero;
    }

    public void Show(string title, string message, Action confirm, Action cancel)
    {
        titleLabel.text = title;
        messageLabel.text = message;
        onConfirm = confirm;
        onCancel = cancel;
        blocker.SetActive(true);
        StopAnimation();
        tweener = content.ScaleTo(Vector3.one, 0.5f, EasingEquations.EaseOutBack);
    }

    public void Hide()
    {
        blocker.SetActive(false);
        onConfirm = onCancel = null;
        StopAnimation();
        tweener = content.ScaleTo(Vector3.zero, 0.5f, EasingEquations.EaseInBack);
    }

    private void StopAnimation()
    {
        if (tweener != null && tweener.IsPlaying)
            tweener.Stop();
    }

    public void OnConfirmButton()
    {
        onConfirm?.Invoke();
        Hide();
    }

    public void OnCancelButton()
    {
        onCancel?.Invoke();
        Hide();
    }
}