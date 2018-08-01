using UnityEditor;
using UnityEditorInternal;

namespace Starlite.Raven {

    [CustomEditor(typeof(RavenContinuousEvent), true)]
    public class RavenContinuousEventEditor : RavenEventEditor {

        protected override void DrawInspector() {
            var evnt = target as RavenContinuousEvent;

            // draw duration
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("Duration", evnt.Duration.ToString("F6"));
            EditorGUI.EndDisabledGroup();

            base.DrawInspector();
        }
    }
}
