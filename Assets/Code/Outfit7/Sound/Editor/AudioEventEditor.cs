using UnityEditor;
using UnityEngine;

namespace Outfit7.Audio {
    [CustomEditor(typeof(AudioEvent))]
    public class AudioEventEditor : UnityEditor.Editor {

        public static AudioEventEditor Instance = null;

        public override void OnInspectorGUI() {
            AudioEvent myTarget = (AudioEvent) target;
            myTarget.EditorGUIRender();
            //
            Instance = this;

            if (myTarget.IsPlaying && GUILayout.Button("Stop")) {
                myTarget.Stop(true);
            }
        }
    }
}