using System;
using System.Collections.Generic;
using System.Reflection;
using Outfit7.Common;
using Outfit7.Devel.O7Debug.UI;
using Outfit7.Devel.O7Debug.UI.Exceptions;
using Outfit7.Util;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Outfit7.Devel.O7Debug {

    public class DebugSettings : MonoBehaviour {

        private const string Tag = "Debug Settings";

        [SerializeField] private GridLayoutGroup TabLayoutGroup = null;
        [SerializeField] private VerticalLayoutGroup ContentLayoutGroup = null;
        [SerializeField] private GridLayoutGroup ButtonsLayoutGroup = null;
        [SerializeField] private Font DebugFont = null;

        [SerializeField] private Sprite BgSprite = null;
        [SerializeField] private Sprite ButtonSprite = null;
        [SerializeField] private Sprite ButtonHighlightSprite = null;
        [SerializeField] private Sprite ButtonPressedSprite = null;
        [SerializeField] private Sprite LabelSprite = null;

        [SerializeField] private Material DefaultMaterial = null;
        [SerializeField] private Material FontMaterial = null;

        [SerializeField] private GameObject SliderPrefab = null;
        [SerializeField] private GameObject TogglePrefab = null;

        [SerializeField] private Color LabelTextColor = Color.black;

        private const string FormatI = "0";
        private const string FormatF = "0.00";

        public Dictionary<object, List<MethodInfo>> Buttons = new Dictionary<object, List<MethodInfo>>();
        public Dictionary<object, List<FieldInfo>> CheckBoxes = new Dictionary<object, List<FieldInfo>>();
        public Dictionary<object, List<FieldInfo>> Sliders = new Dictionary<object, List<FieldInfo>>();
        public Dictionary<object, List<FieldInfo>> Labels = new Dictionary<object, List<FieldInfo>>();
        public Dictionary<object, List<MethodInfo>> InitMethods = new Dictionary<object, List<MethodInfo>>();

        private List<GameObject> TabButtons = new List<GameObject>();
        private List<GameObject> ContentButtons = new List<GameObject>();
        private List<GameObject> ToggleButtons = new List<GameObject>();
        private List<GameObject> ContentSliders = new List<GameObject>();
        private List<GameObject> LabelButtons = new List<GameObject>();

        private List<object> DebuggableObjects;
        private object SelectedObject;
        private bool Opened;
        private Action<DebugSettings> OnUiDestroyed;

        protected virtual void Awake() {
            CreateDebugMenu();
        }

        public void Repopulate(List<object> debuggableObjects, Action<DebugSettings> OnUiDestroyed) {
            this.OnUiDestroyed = OnUiDestroyed;

            if (debuggableObjects != null) {
                DebuggableObjects = debuggableObjects;
            }

            for (int i = 0; i < DebuggableObjects.Count; i++) {
                object obj = DebuggableObjects[i];
                Type type = obj.GetType();

#if NETFX_CORE
                var methods = type.GetTypeInfo().DeclaredMethods;

#else
                var methods = type.GetMethods();
#endif
                foreach (MethodInfo method in methods) {
                    if (method.GetParameters().Length == 0) {
                        DebugMethod[] attributes = method.GetCustomAttributes(typeof(DebugMethod), true) as DebugMethod[];
                        if (attributes.Length > 0) {
                            if (!Buttons.ContainsKey(obj)) {
                                Buttons[obj] = new List<MethodInfo>();
                            }
                            if (!Buttons[obj].Contains(method)) {
                                Buttons[obj].Add(method);
                            }
                        }

                        DebugInit[] debuginit = method.GetCustomAttributes(typeof(DebugInit), true) as DebugInit[];
                        if (debuginit.Length > 0) {
                            var parameters = method.GetParameters();
                            if (parameters == null || parameters.Length == 0) {
                                if (!InitMethods.ContainsKey(obj)) {
                                    InitMethods[obj] = new List<MethodInfo>();
                                }
                                if (!InitMethods[obj].Contains(method)) {
                                    InitMethods[obj].Add(method);
                                }
                            }
                        }

                    }
                }

#if NETFX_CORE
                var fieldInfos = type.GetTypeInfo().DeclaredFields;
#else
                FieldInfo[] fieldInfos = type.GetFields();
#endif
                foreach (FieldInfo fieldInfo in fieldInfos) {
                    DebugField[] attributes = fieldInfo.GetCustomAttributes(typeof(DebugField), true) as DebugField[];
                    if (attributes.Length > 0) {

                        if (fieldInfo.FieldType == typeof(bool)) {

                            if (!CheckBoxes.ContainsKey(obj)) {
                                CheckBoxes[obj] = new List<FieldInfo>();
                            }

                            if (!CheckBoxes[obj].Contains(fieldInfo)) {
                                CheckBoxes[obj].Add(fieldInfo);
                            }
                        } else if (fieldInfo.FieldType == typeof(int)) {

                            if (!Sliders.ContainsKey(obj)) {
                                Sliders[obj] = new List<FieldInfo>();
                            }

                            if (!Sliders[obj].Contains(fieldInfo)) {
                                Sliders[obj].Add(fieldInfo);
                            }

                        } else if (fieldInfo.FieldType == typeof(float)) {

                            if (!Sliders.ContainsKey(obj)) {
                                Sliders[obj] = new List<FieldInfo>();
                            }

                            if (!Sliders[obj].Contains(fieldInfo)) {
                                Sliders[obj].Add(fieldInfo);
                            }
                        }
                    }

                    DebugLabel[] debugLabels = fieldInfo.GetCustomAttributes(typeof(DebugLabel), true) as DebugLabel[];
                    if (debugLabels.Length > 0) {
                        if (!Labels.ContainsKey(obj)) {
                            Labels[obj] = new List<FieldInfo>();
                        }
                        if (!Labels[obj].Contains(fieldInfo)) {
                            Labels[obj].Add(fieldInfo);
                        }
                    }

                }
            }
        }

        private void CreateDebugMenu() {
            CreateBasicDBGButtons();
            SetBackground(TabLayoutGroup.gameObject, true);
            Opened = false;
        }

        protected virtual void OpenGUIDebug() {
            Opened = true;
            ClearAllButtons(true, true);

            AddButton(AppPlugin.ShowNativeSettings, "Native Settings", true);

            for (int i = 0; i < DebuggableObjects.Count; i++) {
                string buttonName = GetDebugClassName(DebuggableObjects[i]);
                AddButton(OpenSubGUIDebug, i, buttonName, true);
            }

            AddButton(CloseGUIDebug, "Close", true);

            //default open first tab
            OpenSubGUIDebug(0);
        }

        private string GetDebugClassName(object obj) {
#if NETFX_CORE
            DebugClass[] attrs = (DebugClass[]) obj.GetType().GetTypeInfo().GetCustomAttributes(typeof(DebugClass), false);
#else
            DebugClass[] attrs = (DebugClass[]) obj.GetType().GetCustomAttributes(typeof(DebugClass), false);
#endif
            string className = obj.GetType().Name;
            if (attrs.Length > 0) {
                className = attrs[0].TabName;
            }
            return className;
        }

        private void OpenSubGUIDebug(int id) {
            SelectedObject = DebuggableObjects[id];

            ClearAllButtons(false, true);
            SetBackground(ContentLayoutGroup.gameObject, true);

            // Draw buttons
            if (Buttons.ContainsKey(SelectedObject)) {
                for (int i = 0; i < Buttons[SelectedObject].Count; i++) {
                    string buttonName = Buttons[SelectedObject][i].Name;
                    DebugMethod[] attrs = (DebugMethod[]) Buttons[SelectedObject][i].GetCustomAttributes(typeof(DebugMethod), false);
                    if (attrs.Length > 0 && !string.IsNullOrEmpty(attrs[0].ButtonName)) {
                        buttonName = attrs[0].ButtonName;
                    }
                    AddButton(CallDebugMethod, i, buttonName, false);
                }
            }

            // Draw checkbox
            if (CheckBoxes.ContainsKey(SelectedObject)) {
                for (int i = 0; i < CheckBoxes[SelectedObject].Count; i++) {
                    bool value = (bool) CheckBoxes[SelectedObject][i].GetValue(SelectedObject);
                    string buttonName = CheckBoxes[SelectedObject][i].Name;
                    DebugField[] attrs = (DebugField[]) CheckBoxes[SelectedObject][i].GetCustomAttributes(typeof(DebugField), false);
                    if (attrs.Length > 0 && !string.IsNullOrEmpty(attrs[0].FieldName)) {
                        buttonName = attrs[0].FieldName;
                    }
                    AddToggleButton(CallToggleMethod, i, buttonName, value);
                }
            }

            // Draw sliders
            if (Sliders.ContainsKey(SelectedObject)) {
                for (int i = 0; i < Sliders[SelectedObject].Count; i++) {
                    DebugField[] fields = (DebugField[]) Sliders[SelectedObject][i].GetCustomAttributes(typeof(DebugField), false);
                    DebugField debugField = fields[0];
                    FieldInfo fieldInfo = Sliders[SelectedObject][i];
                    string sliderName;
                    if (string.IsNullOrEmpty(debugField.FieldName)) {
                        sliderName = Capitals(Sliders[SelectedObject][i].Name);
                    } else {
                        sliderName = debugField.FieldName;
                    }
                    bool isInt;
                    float value = UtilsDebug.GetFloat(fieldInfo, SelectedObject, out isInt);
                    AddSlider(CallSliderMethod, i, sliderName, debugField.StartF, debugField.EndF, value, isInt);
                }
            }

            // Draw labels
            if (Labels.ContainsKey(SelectedObject)) {
                for (int i = 0; i < Labels[SelectedObject].Count; i++) {
                    object value = Labels[SelectedObject][i].GetValue(SelectedObject);
                    string valueName = value == null ? "null" : value.ToString();
                    AddLabel(valueName, Labels[SelectedObject][i]);
                }
            }

            if (InitMethods.ContainsKey(SelectedObject)) {
                for (int i = 0; i < InitMethods[SelectedObject].Count; i++) {
                    InitMethods[SelectedObject][i].Invoke(SelectedObject, null);
                }
            }
        }

        private void CallDebugMethod(int id) {
            var button = ContentButtons[id];
            var buttonText = button.GetComponentInChildren<UnityEngine.UI.Text>().text;
            O7Log.DebugT(Tag, buttonText + " button pressed");

            var i = Buttons[SelectedObject][id];
            i.Invoke(SelectedObject, null);
        }

        private void CallSliderMethod(int id, float value) {
            var slider = ContentSliders[id];
            var sliderText = slider.GetComponentInChildren<UnityEngine.UI.Text>().text;
            O7Log.DebugT(Tag, sliderText + " slider change");

            FieldInfo fieldInfo = Sliders[SelectedObject][id];
            bool isInt;
            UtilsDebug.SetFloat(fieldInfo, SelectedObject, value, out isInt);
            UpdateSliderText(id, value, isInt);
        }

        private void CallToggleMethod(int id, bool value) {
            var toggle = ToggleButtons[id];
            var toggleText = toggle.GetComponentInChildren<UnityEngine.UI.Text>().text;
            O7Log.DebugT(Tag, toggleText + " toggle pressed");

            CheckBoxes[SelectedObject][id].SetValue(SelectedObject, value);
        }

        public virtual void CloseGUIDebug() {
            CreateBasicDBGButtons();
            SetBackground(ContentLayoutGroup.gameObject, false);
        }

        protected virtual void CreateBasicDBGButtons() {
            ClearAllButtons(true, true);
            AddButton(OpenGUIDebug, "GUI", true);
        }

        private void ClearAllButtons(bool clearTabs, bool clearContent) {
            if (clearTabs) {
                for (int i = 0; i < TabButtons.Count; i++) {
                    TabButtons[i].SetActive(false);
                }
            }
            if (clearContent) {
                for (int i = 0; i < ContentButtons.Count; i++) {
                    ContentButtons[i].SetActive(false);
                }
                for (int i = 0; i < ToggleButtons.Count; i++) {
                    ToggleButtons[i].SetActive(false);
                }
                for (int i = 0; i < ContentSliders.Count; i++) {
                    ContentSliders[i].SetActive(false);
                }
                for (int i = 0; i < LabelButtons.Count; i++) {
                    LabelButtons[i].SetActive(false);
                }
            }
        }

        private string Capitals(string str) {
            string upper = "";
            for (int i = 0; i < str.Length; i++) {
                if (char.IsUpper(str[i])) {
                    upper += str[i];
                }
            }
            return upper;
        }

        protected void AddButton(UnityAction<int> action, int id, string text, bool isTabButton) {
            AddButton(action, null, text, isTabButton, id);
        }

        protected void AddButton(UnityAction action, string text, bool isTabButton) {
            AddButton(null, action, text, isTabButton, -1);
        }

        protected void AddButton(UnityAction<int> tabAction, UnityAction action, string text, bool isTabButton, int id) {
            GameObject go = isTabButton ? GetNextUnusedButton(TabButtons) : GetNextUnusedButton(ContentButtons);
            GameObject txtGo;
            DebugButton btn;
            UnityEngine.UI.Text txt;

            if (go == null) {
                go = new GameObject();
                go.name = "btn_" + name;
                go.transform.SetParent(isTabButton ? TabLayoutGroup.transform : ButtonsLayoutGroup.transform, false);
                btn = SetButton(go);

                txtGo = new GameObject();
                txtGo.transform.SetParent(go.transform, false);
                txtGo.name = "txt_" + name;

                var txtRT = txtGo.AddComponent<RectTransform>();
                var txtUi = txtGo.AddComponent<UnityEngine.UI.Text>();
                txt = SetText(txtRT, txtUi);

                if (isTabButton) {
                    TabButtons.Add(go);
                    LayoutElement le = go.AddComponent<LayoutElement>();
                    le.minHeight = 100;
                } else {
                    ContentButtons.Add(go);
                }
            } else {
                go.SetActive(true);
                btn = go.GetComponent<DebugButton>();
                txt = go.GetComponentInChildren<UnityEngine.UI.Text>();
                btn.Clear();
            }

            if (tabAction != null) {
                btn.AddAction(tabAction, id);
            } else if (action != null) {
                btn.onClick.AddListener(action);
            }
            txt.text = text;
        }

        private void AddLabel(string text, FieldInfo fieldInfo) {
            GameObject go = GetNextUnusedButton(LabelButtons);
            DebugUILabel label;

            if (go == null) {
                go = new GameObject();
                go.name = "lbl_" + fieldInfo.Name;
                go.transform.SetParent(ButtonsLayoutGroup.transform, false);
                label = SetLabel(go);
                LabelButtons.Add(go);
            } else {
                go.SetActive(true);
                label = go.GetComponentInChildren<DebugUILabel>();
            }
            label.FieldInfo = fieldInfo;

            label.text = text;
        }

        protected GameObject GetNextUnusedButton(List<GameObject> list) {
            for (int i = 0; i < list.Count; i++) {
                if (!list[i].activeSelf) {
                    return list[i];
                }
            }
            return null;
        }

        protected UnityEngine.UI.Text SetText(RectTransform txtRT, UnityEngine.UI.Text txt) {
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = Vector2.zero;
            txtRT.offsetMax = Vector2.zero;

            txt.color = Color.white;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = 20;
            txt.resizeTextMaxSize = 50;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = DebugFont;
            txt.material = FontMaterial;
            return txt;
        }

        protected DebugButton SetButton(GameObject go) {
            Image img = go.AddComponent<Image>();
            img.sprite = ButtonSprite;
            img.material = DefaultMaterial;
            DebugButton btn = go.AddComponent<DebugButton>();
            btn.transition = Selectable.Transition.SpriteSwap;
            SpriteState st = new SpriteState();
            st.pressedSprite = ButtonPressedSprite;
            st.highlightedSprite = ButtonHighlightSprite;
            btn.spriteState = st;

            return btn;
        }

        protected DebugUILabel SetLabel(GameObject go) {
            GameObject child = new GameObject("txt");
            child.transform.SetParent(go.transform, false);

            go.AddComponent<RectTransform>();
            Image img = go.AddComponent<Image>();
            img.sprite = LabelSprite;
            img.material = DefaultMaterial;

            var txtRT = child.AddComponent<RectTransform>();
            var txt = child.AddComponent<DebugUILabel>();
            SetText(txtRT, txt);
            txt.color = LabelTextColor;

            return txt;
        }

        protected void SetBackground(GameObject go, bool active) {
            Image img = go.GetComponent<Image>();
            if (img == null) {
                if (!active) {
                    return;
                }
                img = go.AddComponent<Image>();
                img.sprite = BgSprite;
                img.material = DefaultMaterial;
            }
            img.enabled = active;
        }

        protected void AddToggleButton(UnityAction<int, bool> action, int id, string toggleName, bool value) {
            GameObject go = GetNextUnusedButton(ToggleButtons);
            if (go == null) {
                go = Instantiate(TogglePrefab);
                go.transform.SetParent(ButtonsLayoutGroup.transform, false);
                ToggleButtons.Add(go);
            } else {
                go.SetActive(true);
            }

            DebugToggle toggle = go.GetComponent<DebugToggle>();
            toggle.Clear();
            toggle.isOn = value;
            toggle.AddAction(action, id);

            UnityEngine.UI.Text txt = go.GetComponentInChildren<UnityEngine.UI.Text>();
            txt.text = toggleName;
        }

        protected void AddSlider(UnityAction<int, float> action, int id, string sliderName, float minValue, float maxValue, float currentValue, bool isInt) {
            GameObject go = GetNextUnusedButton(ContentSliders);
            if (go == null) {
                go = Instantiate(SliderPrefab);
                go.transform.SetParent(ContentLayoutGroup.transform, false);
                ContentSliders.Add(go);
            } else {
                go.SetActive(true);
            }

            UnityEngine.UI.Text txt = go.GetComponentInChildren<UnityEngine.UI.Text>();
            txt.text = sliderName + ": " + currentValue.ToString(isInt ? FormatI : FormatF);

            DebugSlider slider = go.GetComponentInChildren<DebugSlider>();
            slider.Clear();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = currentValue;
            slider.wholeNumbers = isInt;
            slider.onValueChanged.RemoveAllListeners();
            slider.AddAction(action, id, sliderName);
        }

        protected void UpdateSliderText(int id, float value, bool isInt) {
            GameObject go = ContentSliders[id];
            go.GetComponentInChildren<UnityEngine.UI.Text>().text = go.GetComponentInChildren<DebugSlider>().SliderInfo + ": " + value.ToString(isInt ? FormatI : FormatF);
        }

        public void Update() {
            if (!Opened)
                return;

            //We dynamically update all labels, so if the value changes, we see it immediatelly, without having to redraw the whole menu
            UpdateLabels();
        }

        private void UpdateLabels() {
            if (SelectedObject != null) {//Update every frame if there is a label present!
                for (int i = 0; i < LabelButtons.Count; i++) {
                    if (LabelButtons[i].activeSelf) {
                        var uiLabel = LabelButtons[i].GetComponentInChildren<DebugUILabel>();
                        object value = uiLabel.FieldInfo.GetValue(SelectedObject);
                        string valueName = value == null ? "null" : value.ToString();
                        uiLabel.text = valueName;
                    }
                }
            }
        }

        protected virtual void OnDestroy() {
            if (OnUiDestroyed != null) {
                OnUiDestroyed(this);
            }
        }

    }
}
