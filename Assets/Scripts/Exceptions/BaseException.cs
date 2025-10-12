public class BaseException
{
    public readonly bool defaultToggle;

    public BaseException(bool defaultToggle)
    {
        this.defaultToggle = defaultToggle;
        toggle = defaultToggle;
    }

    public bool toggle { get; private set; }

    public void FlipToggle()
    {
        toggle = !defaultToggle;
    }
}