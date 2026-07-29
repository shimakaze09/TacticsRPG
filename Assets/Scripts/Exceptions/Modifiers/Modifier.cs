/// <summary>
/// Base for anything appended to an exception; sortOrder controls application
/// order.
/// </summary>
public abstract class Modifier
{
    public readonly int sortOrder;

    public Modifier(int sortOrder)
    {
        this.sortOrder = sortOrder;
    }
}