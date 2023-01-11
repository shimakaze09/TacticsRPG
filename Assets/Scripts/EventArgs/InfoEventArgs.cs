using System;

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