/// <summary>
/// Base for modifiers that transform the pending value of a
/// ValueChangeException.
/// </summary>
public abstract class ValueModifier : Modifier
{
    public ValueModifier(int sortOrder) : base(sortOrder)
    {
    }

    public abstract float Modify(float fromValue, float toValue);
}