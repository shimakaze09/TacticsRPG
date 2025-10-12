using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatPanel : MonoBehaviour
{
    public Sprite allyBackground;
    public Image avatar;
    public Image background;
    public Sprite enemyBackground;
    public TextMeshProUGUI hpLabel;
    public TextMeshProUGUI lvLabel;
    public TextMeshProUGUI mpLabel;
    public TextMeshProUGUI nameLabel;
    public Panel panel;

    public void Display(GameObject obj)
    {
        var alliance = obj.GetComponent<Alliance>();
        background.sprite = alliance.type == Alliances.Enemy ? enemyBackground : allyBackground;
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