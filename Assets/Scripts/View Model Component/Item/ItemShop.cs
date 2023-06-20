using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemShop : MonoBehaviour
{
    #region Consts

    public const string BuyNotification = "ItemShop.BuyNotification";
    private const string CellKey = "ItemShop.cellPrefab";

    #endregion

    #region Fields

    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform content;
    private List<Item> items;
    private List<Poolable> cells;

    #endregion

    #region MonoBehaviour

    private void OnEnable()
    {
        this.AddObserver(OnBuyItemNotification, ItemCell.BuyNotification);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnBuyItemNotification, ItemCell.BuyNotification);
    }

    #endregion

    #region Public

    public void Load(List<Item> items)
    {
        if (cells == null)
        {
            GameObjectPoolController.AddEntry(CellKey, cellPrefab, items.Count, int.MaxValue);
            cells = new List<Poolable>(items.Count);
        }

        this.items = items;
        Reload();
    }

    public void Reload()
    {
        DequeueCells(items);
    }

    #endregion

    #region Event Handlers

    private void OnBuyItemNotification(object sender, object args)
    {
        var cell = sender as ItemCell;
        if (Bank.Instance.gold >= cell.item.price)
            Purchase(cell.item);
        else
            GetComponent<DialogController>().Show("Need Gold!",
                "You don't have enough gold to complete this purchase.  Would you like to buy more?", FakeBuyGold,
                null);
    }

    public void OnSortByName()
    {
        items.Sort((x, y) => x.name.CompareTo(y.name));
        Reload();
    }

    public void OnSortByPrice()
    {
        items.Sort((x, y) => x.price.CompareTo(y.price));
        Reload();
    }

    public void OnSortByAttack()
    {
        items.Sort((x, y) => x.attack.CompareTo(y.attack));
        Reload();
    }

    public void OnSortByLevel()
    {
        items.Sort((x, y) => x.level.CompareTo(y.level));
        Reload();
    }

    #endregion

    #region Private

    private void FakeBuyGold()
    {
        Bank.Instance.gold += 5000;
    }

    private void Purchase(Item item)
    {
        Bank.Instance.gold -= item.price;
        this.PostNotification(BuyNotification, item);
    }

    private void EnqueueCells()
    {
        foreach (var cell in cells)
            GameObjectPoolController.Enqueue(cell);

        cells.Clear();
    }

    private void DequeueCells(List<Item> items)
    {
        EnqueueCells();
        if (items == null)
            return;

        foreach (var item in items)
        {
            var obj = GameObjectPoolController.Dequeue(CellKey);
            obj.GetComponent<ItemCell>().Load(item);
            obj.transform.SetParent(content);
            obj.gameObject.SetActive(true);
            cells.Add(obj);
        }
    }

    #endregion
}