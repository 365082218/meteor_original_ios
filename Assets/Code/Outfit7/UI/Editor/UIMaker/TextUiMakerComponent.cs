using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Outfit7.Text.Localization;

namespace Outfit7.UI {
    public class TextUiMakerComponent : UiMakerComponent {

        // TODO: Make a custom text component that supports arabic

        private enum MaterialType {
            Default,
            // add more here
            Custom,
        }

        private enum LocalizationType {
            None,
            StaticLocalized,
            DynamicLocalized,
        }

        public override string TypeName { get { return "Text"; } }

        public override bool IsSimpleType { get { return true; } }

        public override string NamePrefix { get { return LocalizationTypeValue == LocalizationType.None ? "txt" : "loc"; } }

        private TextUiMakerData Data = null;

        private MaterialType MaterialTypeValue;

        private Font Font;

        private int ColorIdx;
        private Color Color = new Color32(35, 35, 35, 255);

        private int SizeIdx;
        private int Size = 50;
        private int MinSize = 10;

        private bool IsRichTextComponent;
        private bool IsRichText;
        private bool IsBestFit;

        private Material Material = null;
        private UnityEditor.Editor MaterialPreviewEditor;

        private LocalizationType LocalizationTypeValue = LocalizationType.StaticLocalized;

        private List<string> AllLocalizationKeys = new List<string>();
        private Dictionary<string, LocalizationAsset> LocalizationAssets = new Dictionary<string, LocalizationAsset>();
        private string LocalizationKey = string.Empty;
        private Vector2 ScrollPosition;
        private bool IsAsianBold;
        private bool IsUpperCase;

        private readonly List<string> MaterialTypes = new List<string>((int) MaterialType.Custom);

        public void SetUnlocalizedText(bool richText) {
            IsRichTextComponent = richText;
            LocalizationTypeValue = LocalizationType.None;
        }

        public void SetStaticLocalizedText(bool richText) {
            IsRichTextComponent = richText;
            LocalizationTypeValue = LocalizationType.StaticLocalized;
        }

        public void SetDynamicLocalizedText(bool richText) {
            IsRichTextComponent = richText;
            LocalizationTypeValue = LocalizationType.DynamicLocalized;
        }

        public override void Init() {
            Data = ProjectUiMakerComponent.GetData<TextUiMakerData>();

            RefreshLocalizationFile();

            MaterialTypes.Clear();
            for (int i = 0; i <= (int) MaterialType.Custom; i++) {
                MaterialTypes.Add(((MaterialType) i).ToString());
            }
        }

        private void RefreshLocalizationFile() {
            //            AssetDatabase.ImportAsset(); // for a single specific file
            AssetDatabase.Refresh();

            LocalizerEditor.LoadLocalizationAssets();
        }

        public override void OnGui() {
            base.OnGui();

            EditorGUILayout.Separator();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Font", EditorStyles.label, GUILayout.Width(30f));
            if (Font == null) {
                Font[] fonts = GetAssetsWithFilter<Font>("t:font");
                if (fonts.Length > 0) {
                    Font = fonts[0];
                }
            }
            Font = EditorGUILayout.ObjectField(Font, typeof(Font), false) as Font;
            GUILayout.EndHorizontal();

            GUILayout.Label("Color", EditorStyles.boldLabel);
            if (Data != null) {
                List<string> colors = new List<string>(Data.Colors.Length);
                for (int i = 0; i < Data.Colors.Length; i++) {
                    colors.Add(Data.Colors[i].Name);
                }
                colors.Add("Custom");
                EditorGUI.BeginChangeCheck();
                ColorIdx = GUILayout.SelectionGrid(ColorIdx, colors.ToArray(), 1, EditorStyles.miniButton, GUILayout.MinHeight(colors.Count * 30f));
                if (EditorGUI.EndChangeCheck() && ColorIdx == Data.Colors.Length) {
                    Color = Data.DefaultColor;
                }
            }
            if (Data == null || Data.Colors == null || Data.Colors.Length == 0 || ColorIdx == Data.Colors.Length) {
                Color = EditorGUILayout.ColorField(Color);
            } else {
                Color = Data.Colors[ColorIdx].Color;
                GUI.enabled = false;
                EditorGUILayout.ColorField(Color);
                GUI.enabled = true;
            }

            IsRichTextComponent = EditorGUILayout.Toggle("Rich text Component", IsRichTextComponent);
            if (IsRichTextComponent) {
                IsRichText = true;
            } else {
                IsRichText = EditorGUILayout.Toggle("Support Rich text", IsRichText);
            }
            IsBestFit = EditorGUILayout.Toggle("Best fit", IsBestFit);

            if (IsBestFit) {
                EditorGUILayout.LabelField("Min size", EditorStyles.boldLabel);
                MinSize = EditorGUILayout.IntField(MinSize);
            }
            GUILayout.Label("Size", EditorStyles.boldLabel);
            if (Data != null) {
                List<string> sizes = new List<string>(Data.Sizes.Length);
                for (int i = 0; i < Data.Sizes.Length; i++) {
                    sizes.Add(Data.Sizes[i].Name);
                }
                sizes.Add("Custom");
                EditorGUI.BeginChangeCheck();
                SizeIdx = GUILayout.SelectionGrid(SizeIdx, sizes.ToArray(), 1, EditorStyles.miniButton, GUILayout.MinHeight(sizes.Count * 30f));
                if (EditorGUI.EndChangeCheck() && SizeIdx == Data.Sizes.Length) {
                    Size = Data.DefaultSize;
                }
            }
            if (Data == null || Data.Sizes == null || Data.Sizes.Length == 0 || SizeIdx == Data.Sizes.Length) {
                Size = EditorGUILayout.IntField(Size);
            } else {
                Size = Data.Sizes[SizeIdx].Size;
                GUI.enabled = false;
                EditorGUILayout.IntField(Size);
                GUI.enabled = true;
            }

            GUILayout.Label("Localization", EditorStyles.boldLabel);
            List<string> localizationTypes = new List<string>((int) LocalizationType.DynamicLocalized);
            for (int i = 0; i <= (int) LocalizationType.DynamicLocalized; i++) {
                localizationTypes.Add(((LocalizationType) i).ToString());
            }

            LocalizationTypeValue = (LocalizationType) GUILayout.SelectionGrid((int) LocalizationTypeValue, localizationTypes.ToArray(), 1, EditorStyles.miniButton, GUILayout.MinHeight(100f));

            EditorGUILayout.Separator();
            if (LocalizationAssets == null) {
                EditorGUILayout.HelpBox("The localization file doesn't exist", MessageType.Error);    
                EditorGUILayout.Separator();    
            }

            if (LocalizationAssets != null && LocalizationTypeValue != LocalizationType.None) {
                if (GUILayout.Button("Refresh localization", GUILayout.Height(33f))) {
                    RefreshLocalizationFile();
                }
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Key", EditorStyles.label, GUILayout.Width(30f));
                LocalizationKey = EditorGUILayout.TextField(LocalizationKey);
                Dictionary<string, string> localization = LocalizerEditor.GetLocalizations(LocalizationKey);
                GUI.color = localization != null ? Color.green : Color.red;
                GUILayout.Label(localization != null ? "\u2714" : "\u2718", "TL SelectionButtonNew", GUILayout.Height(20f), GUILayout.Width(30f));
                EditorGUILayout.EndHorizontal();
                GUI.color = Color.white;

                EditorGUILayout.Separator();
                GUILayout.BeginHorizontal();
                GUILayout.Space(35f);
                GUILayout.BeginVertical();
                ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);
                GUI.backgroundColor = new Color(1f, 1f, 1f, 0.35f);

                for (int i = 0; i < AllLocalizationKeys.Count; ++i) {
                    if (AllLocalizationKeys[i].StartsWith(LocalizationKey, System.StringComparison.OrdinalIgnoreCase) || AllLocalizationKeys[i].Contains(LocalizationKey)) {
                        if (GUILayout.Button(AllLocalizationKeys[i] + " \u25B2", "CN CountBadge")) {
                            LocalizationKey = AllLocalizationKeys[i];
                            GUIUtility.hotControl = 0;
                            GUIUtility.keyboardControl = 0;
                        }
                    }
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndScrollView();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUI.color = Color.white;

                EditorGUILayout.Separator();
                IsUpperCase = EditorGUILayout.Toggle("Upper case", IsUpperCase);
                IsAsianBold = EditorGUILayout.Toggle("Asian bold", IsAsianBold);
            }
            EditorGUILayout.Separator();

            GUILayout.Label("Material", EditorStyles.boldLabel);

            MaterialTypeValue = (MaterialType) GUILayout.SelectionGrid((int) MaterialTypeValue, MaterialTypes.ToArray(), 1, EditorStyles.miniButton, GUILayout.MinHeight(66f));
            if (MaterialTypeValue == MaterialType.Custom) {

                Material = EditorGUILayout.ObjectField(Material, typeof(Material), false) as Material;
            }

            EditorGUILayout.Separator();
            SetCommonCreatePanel();
        }

        protected override RectTransform OnCreateExecute() {
            if (Font == null) {
                throw new UnityException("Font is not set!");
            }

            Material mat;
            if (MaterialTypeValue == MaterialType.Custom) {
                mat = Material;
            } else {
                mat = GetFontMaterial();
            }

            if (mat == null) {
                throw new UnityException("Material was null");
            }

            RectTransform rectTransform = base.OnCreateExecute();

            UnityEngine.UI.Text text;

            if (IsRichTextComponent) {
                RichText richText = rectTransform.gameObject.AddComponent<RichText>();
                // TODO: Rich text specific
                text = richText;
                text.alignByGeometry = true;
            } else {
                text = rectTransform.gameObject.AddComponent<UnityEngine.UI.Text>();
            } 
            text.font = Font;

            // TODO: anchors, text alignment

            text.alignment = TextAnchor.MiddleCenter;

            text.material = mat;
            text.color = Color;
            text.resizeTextForBestFit = IsBestFit;
            text.resizeTextMinSize = MinSize;
            text.resizeTextMaxSize = Size;
            text.fontSize = Size;
            text.supportRichText = IsRichText;

            if (LocalizationTypeValue != LocalizationType.None) {
                Localizer localizer = rectTransform.gameObject.AddComponent<Localizer>();
                localizer.Dynamic = LocalizationTypeValue == LocalizationType.DynamicLocalized;
                localizer.SetKeyEditor(LocalizationKey);
                if (LocalizationTypeValue == LocalizationType.StaticLocalized) {
                    Dictionary<string, string> localizations = LocalizerEditor.GetLocalizations(LocalizationKey);
                    if (localizations == null || localizations.Count == 0) {
                        localizer.LocalizeEditor("MISSING_LOCA_KEY");
                    } else {
                        localizer.LocalizeStatic();
                    }
                }

                localizer.AllCaps = IsUpperCase;
                localizer.AsianBold = IsAsianBold;
            }

            return rectTransform;
        }

        private Material GetFontMaterial() {
            string materialFolderPath = string.Format("Assets/{0}", Data.DefaultFontMaterialFolderPath);
            string materialFilePath = string.Format("{0}/{1}.mat", materialFolderPath, Data.DefaultFontMaterialName);

            if (!System.IO.File.Exists(materialFilePath)) {
                if (!System.IO.Directory.Exists(materialFolderPath)) {
                    if (EditorUtility.DisplayDialog("Are you sure?", 
                            "Directory " + materialFolderPath + " doesn't exist. Do you want to create it?", 
                            "Yes", 
                            "No")) {
                        System.IO.Directory.CreateDirectory(materialFolderPath);
                    } else {
                        return null;
                    }
                }
                Material material = new Material(Shader.Find("Outfit7/UI/Default Font"));
                AssetDatabase.CreateAsset(material, materialFilePath);
                return material;
            } else {
                return AssetDatabase.LoadAssetAtPath<Material>(materialFilePath);
            }
        }
    }
}
