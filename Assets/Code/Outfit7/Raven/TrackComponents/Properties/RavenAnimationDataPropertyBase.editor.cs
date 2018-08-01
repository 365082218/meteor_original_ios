using System.Reflection;

#if UNITY_EDITOR
#endif

namespace Starlite.Raven {

    public abstract partial class RavenAnimationDataPropertyBase<T> {
#if UNITY_EDITOR

        public sealed override object GetValueEditor(RavenSequence sequence) {
            // don't get default value for object references
            if (typeof(T).IsSubclassOf(typeof(UnityEngine.Object))) {
                return null;
            }

            var mi = m_TargetComponent.GetType().GetMember(MemberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            var fieldInfo = mi[0] as FieldInfo;
            if (fieldInfo != null) {
                return fieldInfo.GetValue(m_TargetComponent);
            } else {
                return (mi[0] as PropertyInfo).GetValue(m_TargetComponent, null);
            }
        }

#endif
    }
}