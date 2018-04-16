using System;

public class Singleton<T>
{
    protected static readonly T ms_instance = Activator.CreateInstance<T>();
    public static T Instance { get { return ms_instance; } }

    protected Singleton() { }
}
