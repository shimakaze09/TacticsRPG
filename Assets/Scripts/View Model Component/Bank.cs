using UnityEngine;

/// <summary>
/// Holds the player's money (scrip) and publishes changes; currently backed by
/// PlayerPrefs.
/// </summary>
public class Bank
{
    #region Consts

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
            int oldGold = _gold;
            _gold = value;
            Save();
            this.Publish(new GoldChangedEvent(oldGold, value));
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