using UnityEngine;

public abstract class BaseAbilityPicker : MonoBehaviour
{
    #region MonoBehaviour

    private void Start()
    {
        owner = GetComponentInParent<Unit>();
        ac = owner.GetComponentInChildren<AbilityCatalog>();
    }

    #endregion

    #region Public

    public abstract void Pick(PlanOfAttack plan);

    #endregion

    #region Fields

    protected Unit owner;
    protected AbilityCatalog ac;

    #endregion

    #region Protected

    protected Ability Find(string abilityName)
    {
        for (var i = 0; i < ac.transform.childCount; i++)
        {
            var category = ac.transform.GetChild(i);
            var child = category.Find(abilityName);
            if (child != null)
                return child.GetComponent<Ability>();
        }

        return null;
    }

    protected Ability Default()
    {
        return owner.GetComponentInChildren<Ability>();
    }

    #endregion
}