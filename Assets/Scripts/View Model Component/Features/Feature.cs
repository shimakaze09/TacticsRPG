using UnityEngine;

/// <summary>
/// Base for activatable perks carried by items/abilities: Activate applies,
/// Deactivate reverts.
/// </summary>
public abstract class Feature : MonoBehaviour
{
    #region Fields / Properties

    protected GameObject _target { get; private set; }

    #endregion

    #region Public

    public void Activate(GameObject target)
    {
        if (_target == null)
        {
            _target = target;
            OnApply();
        }
    }

    public void Deactivate()
    {
        if (_target != null)
        {
            OnRemove();
            _target = null;
        }
    }

    public void Apply(GameObject target)
    {
        _target = target;
        OnApply();
        _target = null;
    }

    #endregion

    #region Private

    protected abstract void OnApply();

    protected virtual void OnRemove()
    {
    }

    #endregion
}