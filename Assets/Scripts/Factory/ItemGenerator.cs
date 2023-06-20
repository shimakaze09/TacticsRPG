using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemGenerator : MonoBehaviour
{
    [SerializeField] private TextAsset weaponNames;
    [SerializeField] private Sprite[] icons;
    private string[] lines;

    private void Start()
    {
        lines = weaponNames.text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
        var items = new List<Item>(icons.Length);
        items.AddRange(icons.Select(Create));

        GetComponent<ItemShop>().Load(items);
    }

    private Item Create(Sprite icon)
    {
        var retValue = new Item();
        retValue.sprite = icon;
        retValue.name = RandomName();
        retValue.attack = Random.Range(0, 100);
        retValue.level = retValue.attack / 10;
        retValue.price = 50 * (retValue.level + Random.Range(0, 5)) + 100;
        return retValue;
    }

    private string RandomName()
    {
        var s1 = lines[Random.Range(0, lines.Length)];
        var s2 = lines[Random.Range(0, lines.Length)];
        return Random.Range(0, 2) == 0 ? $"{s1} of {s2}" : $"{s1} {s2}";
    }
}