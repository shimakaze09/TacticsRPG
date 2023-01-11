using UnityEngine;

public class Demo : MonoBehaviour
{
    private Unit cursedUnit;
    private Equippable cursedItem;
    private int step;

    private void OnEnable()
    {
        this.AddObserver(OnTurnCheck, TurnOrderController.TurnCheckNotification);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnTurnCheck, TurnOrderController.TurnCheckNotification);
    }

    private void OnTurnCheck(object sender, object args)
    {
        var exc = args as BaseException;
        if (exc.toggle == false)
            return;
        var target = sender as Unit;
        switch (step)
        {
            case 0:
                EquipCursedItem(target);
                break;
            case 1:
                Add<SlowStatusEffect>(target, 15);
                break;
            case 2:
                Add<StopStatusEffect>(target, 15);
                break;
            case 3:
                Add<HasteStatusEffect>(target, 15);
                break;
            default:
                UnEquipCursedItem(target);
                break;
        }

        step++;
    }

    private void Add<T>(Unit target, int duration) where T : StatusEffect
    {
        var condition = target.GetComponent<Status>().Add<T, DurationStatusCondition>();
        condition.duration = duration;
    }

    private void EquipCursedItem(Unit target)
    {
        cursedUnit = target;
        var obj = new GameObject("Cursed Sword");
        obj.AddComponent<AddPoisonStatusFeature>();
        cursedItem = obj.AddComponent<Equippable>();
        cursedItem.defaultSlots = EquipSlots.Primary;
        var equipment = target.GetComponent<Equipment>();
        equipment.Equip(cursedItem, EquipSlots.Primary);
    }

    private void UnEquipCursedItem(Unit target)
    {
        if (target != cursedUnit || step < 10)
            return;
        var equipment = target.GetComponent<Equipment>();
        equipment.UnEquip(cursedItem);
        Destroy(cursedItem.gameObject);
        Destroy(this);
    }
}