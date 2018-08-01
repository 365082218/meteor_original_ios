using UnityEditor;
using UnityEngine;

namespace Outfit7.UI {
    public class TextUiMakerData : UiMakerData {

        [System.Serializable]
        public class ColorWithName {
            public string Name = "General buttons";
            public Color Color = Color.black;

            public ColorWithName() {}

            public ColorWithName(ColorWithName copy) {
                Name = copy.Name;
                Color = copy.Color;
            }
        }

        [System.Serializable]
        public class SizeWithName {
            public string Name = "General buttons";
            public int Size = 50;

            public SizeWithName() {}

            public SizeWithName(SizeWithName copy) {
                Name = copy.Name;
                Size = copy.Size;
            }
        }

        protected override string TypeName { get { return "Text"; } }

        public string DefaultFontMaterialFolderPath = "EditorResources/Materials";
        public string DefaultFontMaterialName = "DefaultFont_MAT";
        public ColorWithName[] Colors = {};
        public SizeWithName[] Sizes = {};
        public Color DefaultColor = new Color32(35, 35, 35, 255);
        public int DefaultSize = 50;

        private string CurrentDefaultFontMaterialFolderPath = string.Empty;
        private string CurrentDefaultFontMaterialName = string.Empty;
        private ColorWithName[] CurrentColors = {};
        private SizeWithName[] CurrentSizes = {};
        private Color CurrentDefaultColor = new Color32(35, 35, 35, 255);
        private int CurrentDefaultSize = 50;

        private bool ColorFoldOut = true;
        private bool SizeFoldOut = true;

        public override bool ChangesMade { get { 
                return 
                    DefaultFontMaterialFolderPath != CurrentDefaultFontMaterialFolderPath ||
                    DefaultFontMaterialName != CurrentDefaultFontMaterialName ||
                    ColorsChanged ||
                    SizesChanged ||
                    DefaultColor != CurrentDefaultColor ||
                    DefaultSize != CurrentDefaultSize;
            }
        }

        private bool ColorsChanged {
            get {
                if (Colors.Length != CurrentColors.Length) {
                    return true;
                }

                for (int i = 0; i < Colors.Length; i++) {
                    if (!Colors[i].Name.Equals(CurrentColors[i].Name)) {
                        return true;
                    }

                    if (!Colors[i].Color.Equals(CurrentColors[i].Color)) {
                        return true;
                    }
                }

                return false;
            }
        }

        private bool SizesChanged {
            get {
                if (Sizes.Length != CurrentSizes.Length) {
                    return true;
                }

                for (int i = 0; i < Sizes.Length; i++) {
                    if (!Sizes[i].Name.Equals(CurrentSizes[i].Name)) {
                        return true;
                    }

                    if (!Sizes[i].Size.Equals(CurrentSizes[i].Size)) {
                        return true;
                    }
                }

                return false;
            }
        }

        public override void OnInit() {
            OnRevertChangedData();
        }

        public override void OnGui() {
            FoldOut = EditorGUILayout.Foldout(FoldOut, TypeName);
            if (FoldOut) {
                EditorGUI.indentLevel++;
                CurrentDefaultFontMaterialFolderPath = EditorGUILayout.TextField("Default font material path", CurrentDefaultFontMaterialFolderPath);
                CurrentDefaultFontMaterialName = EditorGUILayout.TextField("Default font material name", CurrentDefaultFontMaterialName);

                CurrentDefaultColor = EditorGUILayout.ColorField("Default color", CurrentDefaultColor);
                CurrentDefaultSize = EditorGUILayout.IntField("Default size", CurrentDefaultSize);

                ColorFoldOut = EditorGUILayout.Foldout(ColorFoldOut, string.Format("Colors ({0})", CurrentColors.Length));
                if (ColorFoldOut) {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < CurrentColors.Length; i++) {
                        EditorGUILayout.BeginHorizontal();
                        CurrentColors[i].Name = EditorGUILayout.TextField(CurrentColors[i].Name);
                        CurrentColors[i].Color = EditorGUILayout.ColorField(CurrentColors[i].Color);
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(18f);
                    if (GUILayout.Button("+", GUILayout.Height(33f))) {
                        ArrayUtility.Add<ColorWithName>(ref CurrentColors, new ColorWithName());
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(18f);
                    if (CurrentColors.Length > 0 && GUILayout.Button("-", GUILayout.Height(33f))) {
                        ArrayUtility.RemoveAt<ColorWithName>(ref CurrentColors, CurrentColors.Length - 1);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                }

                SizeFoldOut = EditorGUILayout.Foldout(SizeFoldOut, string.Format("Sizes ({0})", CurrentSizes.Length));
                if (SizeFoldOut) {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < CurrentSizes.Length; i++) {
                        EditorGUILayout.BeginHorizontal();
                        CurrentSizes[i].Name = EditorGUILayout.TextField(CurrentSizes[i].Name);
                        CurrentSizes[i].Size = EditorGUILayout.IntField(CurrentSizes[i].Size);
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(18f);
                    if (GUILayout.Button("+", GUILayout.Height(33f))) {
                        ArrayUtility.Add<SizeWithName>(ref CurrentSizes, new SizeWithName());
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(18f);
                    if (CurrentSizes.Length > 0 && GUILayout.Button("-", GUILayout.Height(33f))) {
                        ArrayUtility.RemoveAt<SizeWithName>(ref CurrentSizes, CurrentSizes.Length - 1);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                }

                OnBottomButtonGui();
                EditorGUI.indentLevel--;
            }
        }

        public override void OnApplyChangedData() {
            DefaultFontMaterialFolderPath = CurrentDefaultFontMaterialFolderPath;
            DefaultFontMaterialName = CurrentDefaultFontMaterialName;
            ArrayUtility.Clear<ColorWithName>(ref Colors);
            for (int i = 0; i < CurrentColors.Length; i++) {
                ArrayUtility.Add<ColorWithName>(ref Colors, new ColorWithName(CurrentColors[i]));
            }
            ArrayUtility.Clear<SizeWithName>(ref Sizes);
            for (int i = 0; i < CurrentSizes.Length; i++) {
                ArrayUtility.Add<SizeWithName>(ref Sizes, new SizeWithName(CurrentSizes[i]));
            }
            DefaultColor = CurrentDefaultColor;
            DefaultSize = CurrentDefaultSize;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public override void OnRevertChangedData() {
            CurrentDefaultFontMaterialFolderPath = DefaultFontMaterialFolderPath;
            CurrentDefaultFontMaterialName = DefaultFontMaterialName;
            ArrayUtility.Clear<ColorWithName>(ref CurrentColors);
            for (int i = 0; i < Colors.Length; i++) {
                ArrayUtility.Add<ColorWithName>(ref CurrentColors, new ColorWithName(Colors[i]));
            }
            ArrayUtility.Clear<SizeWithName>(ref CurrentSizes);
            for (int i = 0; i < Sizes.Length; i++) {
                ArrayUtility.Add<SizeWithName>(ref CurrentSizes, new SizeWithName(Sizes[i]));
            }
            CurrentDefaultColor = DefaultColor;
            CurrentDefaultSize = DefaultSize;
            AssetDatabase.SaveAssets();
        }
    }
}
