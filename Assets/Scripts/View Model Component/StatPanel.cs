using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatPanel : MonoBehaviour
{
    public Panel panel;
    public Sprite allyBackground;
    public Sprite enemyBackground;
    public Image background;
    public Image avatar;
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI hpLabel;
    public TextMeshProUGUI mpLabel;
    public TextMeshProUGUI lvLabel;

    public void Display(GameObject obj)
    {
        // Temp until I add a component to determine unit alliances
        background.sprite = Random.value > 0.5f ? enemyBackground : allyBackground;
        // avatar.sprite = null; Need a component which provides this data
        nameLabel.text = obj.name;
        var stats = obj.GetComponent<Stats>();
        if (stats)
        {
            hpLabel.text = $"HP {stats[StatTypes.HP]} / {stats[StatTypes.MHP]}";
            mpLabel.text = $"MP {stats[StatTypes.MP]} / {stats[StatTypes.MMP]}";
            lvLabel.text = $"LV. {stats[StatTypes.LVL]}";
        }
    }
}