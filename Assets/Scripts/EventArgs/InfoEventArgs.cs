using System;

/// <summary>
/// Generic single-value EventArgs wrapper used by the static input events.
/// </summary>
public class InfoEventArgs<T> : EventArgs
{
    public T info;

    public InfoEventArgs()
    {
        info = default;
    }

    public InfoEventArgs(T info)
    {
        this.info = info;
    }
}