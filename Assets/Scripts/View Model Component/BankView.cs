using System;
using TMPro;
using UnityEngine;

public class BankView : MonoBehaviour
{
    #region Fields

    [SerializeField] private TextMeshProUGUI label;
    private EasingControl ec;
    private int startGold;
    private int endGold;
    private int currentGold;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        ec = gameObject.AddComponent<EasingControl>();
        ec.equation = EasingEquations.EaseOutQuad;
        ec.duration = 0.5f;
        startGold = currentGold = endGold = Bank.Instance.gold;
        label.text = Bank.Instance.gold.ToString();
    }

    private void OnEnable()
    {
        this.Subscribe<GoldChangedEvent>(OnGoldChanged);
        ec.updateEvent += OnEasingUpdate;
    }

    private void OnDisable()
    {
        this.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
        ec.updateEvent -= OnEasingUpdate;
    }

    #endregion

    #region Event Handlers

    private void OnGoldChanged(GoldChangedEvent e)
    {
        if (ec.IsPlaying)
            ec.Stop();
        startGold = currentGold;
        endGold = e.NewAmount;
        ec.SeekToBeginning();
        ec.Play();
    }

    private void OnEasingUpdate(object sender, EventArgs e)
    {
        if (ec.IsPlaying)
        {
            currentGold = Mathf.RoundToInt((endGold - startGold) * ec.currentValue) + startGold;
            label.text = currentGold.ToString();
        }
    }

    #endregion
}