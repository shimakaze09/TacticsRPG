using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestItems : MonoBehaviour
{
    #region Fields

    private readonly List<GameObject> inventory = new();
    private readonly List<GameObject> combatants = new();

    #endregion

    #region MonoBehaviour

    private void Start()
    {
        CreateItems();
        CreateCombatants();
        StartCoroutine(SimulateBattle());
    }

    private void OnEnable()
    {
        this.Subscribe<ItemEquippedEvent>(OnEquippedItem);
        this.Subscribe<ItemUnequippedEvent>(OnUnEquippedItem);
    }

    private void OnDisable()
    {
        this.Unsubscribe<ItemEquippedEvent>(OnEquippedItem);
        this.Unsubscribe<ItemUnequippedEvent>(OnUnEquippedItem);
    }

    #endregion

    #region Notification Handlers

    private void OnEquippedItem(ItemEquippedEvent e)
    {
        var item = e.Item.GetComponent<Equippable>();
        inventory.Remove(e.Item);
        var message = $"Equipped {item.name}";
        Debug.Log(message);
    }

    private void OnUnEquippedItem(ItemUnequippedEvent e)
    {
        var item = e.Item.GetComponent<Equippable>();
        inventory.Add(e.Item);
        var message = $"Un-equipped {item.name}";
        Debug.Log(message);
    }

    #endregion

    #region Factory

    private GameObject CreateItem(string title, StatTypes type, int amount)
    {
        var item = new GameObject(title);
        var smf = item.AddComponent<StatModifierFeature>();
        smf.type = type;
        smf.amount = amount;
        return item;
    }

    private GameObject CreateConumableItem(string title, StatTypes type, int amount)
    {
        var item = CreateItem(title, type, amount);
        item.AddComponent<Consumable>();
        return item;
    }

    private GameObject CreateEquippableItem(string title, StatTypes type, int amount, EquipSlots slot)
    {
        var item = CreateItem(title, type, amount);
        var equip = item.AddComponent<Equippable>();
        equip.defaultSlots = slot;
        return item;
    }

    private GameObject CreateHero()
    {
        var actor = CreateActor("Hero");
        actor.AddComponent<Equipment>();
        return actor;
    }

    private GameObject CreateActor(string title)
    {
        var actor = new GameObject(title);
        var s = actor.AddComponent<Stats>();
        s[StatTypes.HP] = s[StatTypes.MHP] = Random.Range(500, 1000);
        s[StatTypes.ATK] = Random.Range(30, 50);
        s[StatTypes.DEF] = Random.Range(30, 50);
        return actor;
    }

    #endregion

    #region Private

    private void CreateItems()
    {
        inventory.Add(CreateConumableItem("Health Potion", StatTypes.HP, 300));
        inventory.Add(CreateConumableItem("Bomb", StatTypes.HP, -150));
        inventory.Add(CreateEquippableItem("Sword", StatTypes.ATK, 10, EquipSlots.Primary));
        inventory.Add(CreateEquippableItem("Broad Sword", StatTypes.ATK, 15,
            EquipSlots.Primary | EquipSlots.Secondary));
        inventory.Add(CreateEquippableItem("Shield", StatTypes.DEF, 10, EquipSlots.Secondary));
    }

    private void CreateCombatants()
    {
        combatants.Add(CreateHero());
        combatants.Add(CreateActor("Monster"));
    }

    private IEnumerator SimulateBattle()
    {
        while (VictoryCheck() == false)
        {
            LogCombatants();
            HeroTurn();
            EnemyTurn();
            yield return new WaitForSeconds(1);
        }

        LogCombatants();
        Debug.Log("Battle Completed");
    }

    private void HeroTurn()
    {
        var rnd = Random.Range(0, 2);
        switch (rnd)
        {
            case 0:
                Attack(combatants[0], combatants[1]);
                break;
            default:
                UseInventory();
                break;
        }
    }

    private void EnemyTurn()
    {
        Attack(combatants[1], combatants[0]);
    }

    private void Attack(GameObject attacker, GameObject defender)
    {
        var s1 = attacker.GetComponent<Stats>();
        var s2 = defender.GetComponent<Stats>();
        var damage = Mathf.FloorToInt((s1[StatTypes.ATK] * 4 - s2[StatTypes.DEF] * 2) * Random.Range(0.9f, 1.1f));
        s2[StatTypes.HP] -= damage;
        var message = $"{attacker.name} hits {defender.name} for {damage} damage!";
        Debug.Log(message);
    }

    private void UseInventory()
    {
        var rnd = Random.Range(0, inventory.Count);
        var item = inventory[rnd];
        if (item.GetComponent<Consumable>() != null)
            ConsumeItem(item);
        else
            EquipItem(item);
    }

    private void ConsumeItem(GameObject item)
    {
        inventory.Remove(item);
        // This is dummy code - a user would know how to use an item and who to target with it
        var smf = item.GetComponent<StatModifierFeature>();
        if (smf.amount > 0)
        {
            item.GetComponent<Consumable>().Consume(combatants[0]);
            Debug.Log("Ah... a potion!");
        }
        else
        {
            item.GetComponent<Consumable>().Consume(combatants[1]);
            Debug.Log("Take this you stupid monster!");
        }
    }

    private void EquipItem(GameObject item)
    {
        Debug.Log("Perhaps this will help...");
        var toEquip = item.GetComponent<Equippable>();
        var equipment = combatants[0].GetComponent<Equipment>();
        equipment.Equip(toEquip, toEquip.defaultSlots);
    }

    private bool VictoryCheck()
    {
        for (var i = 0; i < 2; i++)
        {
            var s = combatants[i].GetComponent<Stats>();
            if (s[StatTypes.HP] <= 0)
                return true;
        }

        return false;
    }

    private void LogCombatants()
    {
        Debug.Log("============");
        for (var i = 0; i < 2; i++)
            LogToConsole(combatants[i]);
        Debug.Log("============");
    }

    private void LogToConsole(GameObject actor)
    {
        var s = actor.GetComponent<Stats>();
        var message =
            $"Name:{actor.name} HP:{s[StatTypes.HP]}/{s[StatTypes.MHP]} ATK:{s[StatTypes.ATK]} DEF:{s[StatTypes.DEF]}";
        Debug.Log(message);
    }

    #endregion
}