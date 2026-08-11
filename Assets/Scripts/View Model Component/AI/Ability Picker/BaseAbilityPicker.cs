using UnityEngine;

/// <summary>
/// Base for the Easy AI's ability choosers: locates an ability on the acting
/// unit and writes it into the plan. Owner and catalog resolve lazily so a
/// picker works the same frame its unit spawns (ARCHITECTURE.md lifecycle
/// rules — parent-chain lookups must not depend on Start having run).
/// </summary>
public abstract class BaseAbilityPicker : MonoBehaviour
{
    #region Public

    /// <summary>Writes this picker's chosen ability (and target kind) into the plan.</summary>
    public abstract void Pick(PlanOfAttack plan);

    #endregion

    #region Fields

    private Unit cachedOwner;
    private AbilityCatalog cachedCatalog;

    /// <summary>The unit this picker chooses for, resolved at first use.</summary>
    protected Unit owner
    {
        get
        {
            if (cachedOwner == null)
                cachedOwner = GetComponentInParent<Unit>();
            return cachedOwner;
        }
    }

    /// <summary>The owner's ability catalog, resolved at first use.</summary>
    protected AbilityCatalog ac
    {
        get
        {
            if (cachedCatalog == null && owner != null)
                cachedCatalog = owner.GetComponentInChildren<AbilityCatalog>();
            return cachedCatalog;
        }
    }

    #endregion

    #region Protected

    /// <summary>The named ability from any category of the owner's catalog, or null.</summary>
    protected Ability Find(string abilityName)
    {
        if (ac == null)
            return null;

        for (var i = 0; i < ac.transform.childCount; i++)
        {
            var category = ac.transform.GetChild(i);
            var child = category.Find(abilityName);
            if (child != null)
                return child.GetComponent<Ability>();
        }

        return null;
    }

    /// <summary>The owner's basic attack — the picker fallback when a named ability is unusable.</summary>
    protected Ability Default()
    {
        return BasicAttackResolver.Resolve(owner);
    }

    #endregion
}
