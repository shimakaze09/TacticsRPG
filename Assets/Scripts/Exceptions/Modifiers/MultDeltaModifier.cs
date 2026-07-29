/// <summary>
/// Multiplies only the delta (change amount) rather than the whole value.
/// </summary>
public class MultDeltaModifier : ValueModifier
{
    public readonly float toMultiply;

    public MultDeltaModifier(int sortOrder, float toMultiply) : base(sortOrder)
    {
        this.toMultiply = toMultiply;
    }

    public override float Modify(float fromValue, float toValue)
    {
        var delta = toValue - fromValue;
        return fromValue + delta * toMultiply;
    }
}