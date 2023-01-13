﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Equippable : MonoBehaviour
{
    #region Fields

    /// <summary>
    /// The EquipSlots flag which is the default
    /// equip location(s) for this item.
    /// For example, a normal weapon would only specify 
    /// primary, but a two-handed weapon would specify
    /// both primary and secondary.
    /// </summary>
    public EquipSlots defaultSlots;

    /// <summary>
    /// Some equipment may be allowed to be equipped in
    /// more than one slot location, such as when 
    /// dual-wielding swords.
    /// </summary>
    public EquipSlots secondarySlots;

    /// <summary>
    /// The slot(s) where an item is currently equipped
    /// </summary>
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