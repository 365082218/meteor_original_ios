using System;

public class Singleton<T>
{
    protected static readonly T _Ins = Activator.CreateInstance<T>();
    public static T Ins { get { return _Ins; } }

    protected Singleton() { }
}
