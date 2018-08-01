using System.Collections.Generic;
using System.Text;

namespace Starlite.Raven {

    public static class RavenQuickSearchRemap {

        private static Dictionary<string, string> s_RemapTable = new Dictionary<string, string>() {
            { "UnityEngine.Transform.localPosition", "Local Position" },
            { "UnityEngine.Transform.localEulerAngles", "Local Rotation" },
            { "UnityEngine.Transform.position", "World Position" },
            { "UnityEngine.Transform.eulerAngles", "World Rotation" },
            { "UnityEngine.RectTransform.localPosition", "Local Position" },
            { "UnityEngine.RectTransform.localEulerAngles", "Local Rotation" },
            { "UnityEngine.RectTransform.position", "World Position" },
            { "UnityEngine.RectTransform.eulerAngles", "World Rotation" }
        };

        private static readonly StringBuilder m_StringBuilder = new StringBuilder(128);

        public static RavenQuickSearchWrapper DoRemap(RavenPropertyTrackView.PropertyEventMenuData menuData) {
            var remap = string.Empty;

            if (!RavenPreferences.QuickSearchShowAllProperties || RavenPreferences.QuickSearchRemapAllProperties) {
                if (RavenEditorCallbacks.e_RemapCallback != null) {
                    remap = RavenEditorCallbacks.e_RemapCallback(menuData);
                }

                if (string.IsNullOrEmpty(remap)) {
                    m_StringBuilder.Length = 0;
                    m_StringBuilder.AppendFormat("{0}.{1}", menuData.m_ComponentType, menuData.m_MemberName);
                    s_RemapTable.TryGetValue(m_StringBuilder.ToString(), out remap);
                }
            }

            if (RavenPreferences.QuickSearchShowAllProperties && string.IsNullOrEmpty(remap)) {
                m_StringBuilder.Length = 0;
                m_StringBuilder.AppendFormat("{0}.{1}", RavenUtility.GetTypeWithoutNamespace(menuData.m_ComponentType), menuData.m_MemberName);
                remap = m_StringBuilder.ToString();
            }

            if (!string.IsNullOrEmpty(remap)) {
                return new RavenQuickSearchWrapper() {
                    m_Data = menuData,
                    m_PrettyName = remap,
                };
            }

            return null;
        }
    }

    public class RavenQuickSearchWrapper {
        public RavenPropertyTrackView.PropertyEventMenuData m_Data;
        public string m_PrettyName;
    }
}