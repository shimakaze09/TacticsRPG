using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class AbilityMenuPanelController : MonoBehaviour
{
    #region Constants

    private const string ShowKey = "Show";
    private const string HideKey = "Hide";
    private const string EntryPoolKey = "AbilityMenuPanel.Entry";
    private const int MenuCount = 4;

    #endregion

    #region Fields / Properties

    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private TextMeshProUGUI titleLabel;
    [SerializeField] private Panel panel;
    [SerializeField] private GameObject canvas;
    private List<AbilityMenuEntry> menuEntries = new(MenuCount);
    public int selection { get; private set; }

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        GameObjectPoolController.AddEntry(EntryPoolKey, entryPrefab, MenuCount, int.MaxValue);
    }

    private void Start()
    {
        panel.SetPosition(HideKey, false);
        canvas.SetActive(false);
    }

    #endregion

    #region Public

    public void Show(string title, List<string> options)
    {
        canvas.SetActive(true);
        Clear();
        titleLabel.text = title;
        foreach (var option in options)
        {
            var entry = Dequeue();
            entry.Title = option;
            menuEntries.Add(entry);
        }

        SetSelection(0);
        TogglePos(ShowKey);
    }

    public void Hide()
    {
        var t = TogglePos(HideKey);
        t.completedEvent += delegate
        {
            if (panel.CurrentPosition == panel[HideKey])
            {
                Clear();
                canvas.SetActive(false);
            }
        };
    }

    public void SetLocked(int index, bool value)
    {
        if (index < 0 || index >= menuEntries.Count)
            return;

        menuEntries[index].IsLocked = value;
        if (value && selection == index)
            Next();
    }

    public void Next()
    {
        for (var i = selection + 1; i < selection + menuEntries.Count; i++)
        {
            var index = i % menuEntries.Count;
            if (SetSelection(index))
                break;
        }
    }

    public void Previous()
    {
        for (var i = selection - 1 + menuEntries.Count; i > selection; i--)
        {
            var index = i % menuEntries.Count;
            if (SetSelection(index))
                break;
        }
    }

    #endregion

    #region Private

    private AbilityMenuEntry Dequeue()
    {
        var p = GameObjectPoolController.Dequeue(EntryPoolKey);
        var entry = p.GetComponent<AbilityMenuEntry>();
        entry.transform.SetParent(panel.transform, false);
        entry.transform.localScale = Vector3.one;
        entry.gameObject.SetActive(true);
        entry.Reset();
        return entry;
    }

    private void Enqueue(AbilityMenuEntry entry)
    {
        var p = entry.GetComponent<Poolable>();
        GameObjectPoolController.Enqueue(p);
    }

    private void Clear()
    {
        foreach (var entry in menuEntries)
            Enqueue(entry);

        menuEntries.Clear();
    }

    private bool SetSelection(int value)
    {
        if (menuEntries[value].IsLocked)
            return false;

        // Deselect the previously selected entry
        if (selection >= 0 && selection < menuEntries.Count)
            menuEntries[selection].IsSelected = false;

        selection = value;

        // Select the new entry
        if (selection >= 0 && selection < menuEntries.Count)
            menuEntries[selection].IsSelected = true;

        return true;
    }

    private Tweener TogglePos(string pos)
    {
        var t = panel.SetPosition(pos, true);
        t.duration = 0.5f;
        t.equation = EasingEquations.EaseOutQuad;
        return t;
    }

    #endregion
}