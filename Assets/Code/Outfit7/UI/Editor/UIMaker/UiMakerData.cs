using UnityEngine;
using UnityEditor;

namespace Outfit7.UI {
    public abstract class UiMakerData : ScriptableObject {

        protected abstract string TypeName { get; }

        public string DataAssetPath(string projectPath) {
            return projectPath + "/" + TypeName + "UiMaker.asset";
        }

        public abstract void OnInit();
        public abstract void OnGui();
        public abstract void OnApplyChangedData();
        public abstract void OnRevertChangedData();
        public abstract bool ChangesMade { get; }

        protected bool FoldOut = false;

        protected void OnBottomButtonGui() {
            GUI.enabled = ChangesMade;
            if (GUILayout.Button("Apply")) {
                OnApplyChangedData();
            }
            if (GUILayout.Button("Revert")) {
                if (EditorUtility.DisplayDialog("Data has been changed", 
                    "Are you sure you want to revert the changes made?", 
                    "Yes", 
                    "No")) {
                    OnRevertChangedData();
                }
            }
            GUI.enabled = true;
        }
    }
}
