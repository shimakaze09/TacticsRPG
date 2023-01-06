using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equippable : MonoBehaviour
{
    #region Fields

    public EquipSlots defaultSlots;
    public EquipSlots secondarySlots;
    public EquipSlots slots;
    private bool _isEquipped;

    #endregion

    #region Public

    public void OnEquip()
    {
        if (_isEquipped)
            return;

        _isEquipped = true;

        var features = GetComponentsInChildren<Feature>();
        foreach (var feature in features)
            feature.Activate(gameObject);
    }

    public void OnUnEquip()
    {
        if (!_isEquipped)
            return;

        _isEquipped = false;

        var features = GetComponentsInChildren<Feature>();
        foreach (var feature in features)
            feature.Deactivate();
    }

    #endregion
}