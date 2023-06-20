using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bank
{
    #region Consts

    public const string GoldChanged = "Bank.GoldChanged";
    private const string GoldKey = "Bank.GoldKey";

    #endregion

    #region Fields

    public int gold
    {
        get => _gold;
        set
        {
            if (_gold == value)
                return;
            _gold = value;
            Save();
            this.PostNotification(GoldChanged);
        }
    }

    private int _gold;

    #endregion

    #region Singleton

    public static readonly Bank Instance = new();

    private Bank()
    {
        Load();
    }

    #endregion

    #region Private

    private void Load()
    {
        _gold = PlayerPrefs.GetInt(GoldKey, 5000);
    }

    private void Save()
    {
        PlayerPrefs.SetInt(GoldKey, _gold);
    }

    #endregion
}