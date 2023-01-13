using UnityEngine;
using System;
using System.Collections;

public class ConversationController : MonoBehaviour
{
    #region Events

    public static event EventHandler completeEvent;

    #endregion

    #region Const

    private const string ShowTop = "Show Top";
    private const string ShowBottom = "Show Bottom";
    private const string HideTop = "Hide Top";
    private const string HideBottom = "Hide Bottom";

    #endregion

    #region Fields

    [SerializeField] private ConversationPanel leftPanel;
    [SerializeField] private ConversationPanel rightPanel;

    private Canvas canvas;
    private IEnumerator conversation;
    private Tweener transition;

    #endregion

    #region MonoBehaviour

    private void Start()
    {
        canvas = GetComponentInChildren<Canvas>();
        if (leftPanel.panel.CurrentPosition == null)
            leftPanel.panel.SetPosition(HideBottom, false);
        if (rightPanel.panel.CurrentPosition == null)
            rightPanel.panel.SetPosition(HideBottom, false);
        canvas.gameObject.SetActive(false);
    }

    #endregion

    #region Public

    public void Show(ConversationData data)
    {
        canvas.gameObject.SetActive(true);
        conversation = Sequence(data);
        conversation.MoveNext();
    }

    public void Next()
    {
        if (conversation == null || transition != null)
            return;

        conversation.MoveNext();
    }

    #endregion

    #region Private

    private IEnumerator Sequence(ConversationData data)
    {
        foreach (var sd in data.list)
        {
            var currentPanel =
                sd.anchor is TextAnchor.UpperLeft or TextAnchor.MiddleLeft or TextAnchor.LowerLeft
                    ? leftPanel
                    : rightPanel;
            var presenter = currentPanel.Display(sd);
            presenter.MoveNext();

            string show, hide;
            if (sd.anchor is TextAnchor.UpperLeft or TextAnchor.UpperCenter or TextAnchor.UpperRight)
            {
                show = ShowTop;
                hide = HideTop;
            }
            else
            {
                show = ShowBottom;
                hide = HideBottom;
            }

            currentPanel.panel.SetPosition(hide, false);
            MovePanel(currentPanel, show);

            yield return null;
            while (presenter.MoveNext())
                yield return null;

            MovePanel(currentPanel, hide);
            transition.completedEvent += delegate(object sender, EventArgs e) { conversation.MoveNext(); };

            yield return null;
        }

        canvas.gameObject.SetActive(false);
        completeEvent?.Invoke(this, EventArgs.Empty);
    }

    private void MovePanel(ConversationPanel obj, string pos)
    {
        transition = obj.panel.SetPosition(pos, true);
        transition.duration = 0.5f;
        transition.equation = EasingEquations.EaseOutQuad;
    }

    #endregion
}