using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCell : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI atkLabel;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI lvlLabel;
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI priceLabel;

    public Item item { get; private set; }

    public void Load(Item item)
    {
        this.item = item;
        icon.sprite = item.sprite;
        nameLabel.text = item.name;
        atkLabel.text = $"ATK:{item.attack}";
        lvlLabel.text = $"LVL:{item.level}";
        priceLabel.text = item.price.ToString();
    }

    public void OnBuyButton()
    {
        this.Publish(new ItemPurchasedEvent(item));
    }
}