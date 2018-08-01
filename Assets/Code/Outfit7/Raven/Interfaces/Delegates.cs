namespace Starlite.Raven {

    public delegate void RavenPropertySetter<T>(UnityEngine.Object component, T value);

    public delegate T RavenPropertyGetter<T>(UnityEngine.Object component);

    public delegate void RavenFunctionCall(UnityEngine.Object component);
}