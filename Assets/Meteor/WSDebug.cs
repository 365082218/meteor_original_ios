using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
//using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Reflection;
using Idevgame.Debug;
using UnityEngine.UI;
using System;

public interface DebugInstance
{
    string Name { get; }
}
//负责一些辅助功能，加载原流星的gmc转换为level和解析原流星的引用图片等等
public class WSDebug : MonoBehaviour {
    //public int MoveSpeed;
    //public float GameSpeed = 1.0f;
    //public int level;
    [SerializeField] private GameObject tabRoot;
    [SerializeField] private GameObject contentRoot;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject panel;
    private object SelectedObject;
    public Dictionary<object, List<MethodInfo>> Buttons = new Dictionary<object, List<MethodInfo>>();
    List<GameObject> tabButton = new List<GameObject>();
    List<GameObject> contentButton = new List<GameObject>();
    private List<object> GlobalDebuggableObjects = new List<object>();
    public static WSDebug Ins = null;
    bool Opened;
    void Awake() {
        Ins = this;
        Opened = false;
        HiDebug.SetFontSize(35);
    }
    void OnDestroy() {
        Ins = null;
    }

    bool logOpend = false;
    HiDebugView logView = null;
    public void OpenLogView()
    {
        logOpend = !logOpend;
        HiDebug.EnableOnText(logOpend);
        HiDebug.EnableOnScreen(logOpend);
        HiDebug.EnableDebuger(logOpend);
        if (logView == null)
            logView = FindObjectOfType<HiDebugView>();
        if (logView != null)
            logView.enabled = logOpend;
    }

    public void CloseLogView()
    {
        logOpend = false;
        HiDebug.EnableOnText(logOpend);
        HiDebug.EnableOnScreen(logOpend);
        HiDebug.EnableDebuger(logOpend);
        if (logView == null)
            logView = FindObjectOfType<HiDebugView>();
        if (logView != null)
            logView.enabled = logOpend;
    }

    public void OpenGUIDebug()
    {
        gameObject.SetActive(true);
        if (GlobalDebuggableObjects.Count == 0)
        {
#if !STRIP_LOGS
            WSLog.Print("no debug data");
#endif
            return;
        }
        ClearAllButtons(true, true);
        for (int i = 0; i < GlobalDebuggableObjects.Count; i++)
        {
            string buttonName = GetDebugClassName(GlobalDebuggableObjects[i]);
            AddButton(OpenSubGUIDebug, i, buttonName, true);
        }
        AddButton(CloseGUIDebug, "Close", true);
        OpenSubGUIDebug(0);
        Opened = true;
        panel.SetActive(Opened);
    }

    private string GetDebugClassName(object obj)
    {
#if NETFX_CORE
        DebugClass[] attrs = (DebugClass[]) obj.GetType().GetTypeInfo().GetCustomAttributes(typeof(DebugClass), false);
#else
        DebugClass[] attrs = (DebugClass[])obj.GetType().GetCustomAttributes(typeof(DebugClass), false);
#endif
        string className = obj.GetType().Name;

        if (attrs.Length > 0)
            className = attrs[0].TabName;

        DebugInstance ins = obj as DebugInstance;
        if (ins != null)
            className = ins.Name;
        return className;
    }

    public void CloseGUIDebug()
    {
        ClearAllButtons(true, true);
        Opened = false;
        panel.SetActive(Opened);
    }

    private void OpenSubGUIDebug(int id)
    {
        SelectedObject = GlobalDebuggableObjects[id];

        ClearAllButtons(false, true);
        // Draw buttons
        if (Buttons.ContainsKey(SelectedObject))
        {
            for (int i = 0; i < Buttons[SelectedObject].Count; i++)
            {
                string buttonName = Buttons[SelectedObject][i].Name;
                DebugMethod[] attrs = (DebugMethod[])Buttons[SelectedObject][i].GetCustomAttributes(typeof(DebugMethod), false);
                if (attrs.Length > 0 && !string.IsNullOrEmpty(attrs[0].ButtonName))
                {
                    buttonName = attrs[0].ButtonName;
                }
                AddButton(CallDebugMethod, i, buttonName, false);
            }
        }

        // Draw checkbox
        //if (CheckBoxes.ContainsKey(SelectedObject))
        //{
        //    for (int i = 0; i < CheckBoxes[SelectedObject].Count; i++)
        //    {
        //        bool value = (bool)CheckBoxes[SelectedObject][i].GetValue(SelectedObject);
        //        string buttonName = CheckBoxes[SelectedObject][i].Name;
        //        DebugField[] attrs = (DebugField[])CheckBoxes[SelectedObject][i].GetCustomAttributes(typeof(DebugField), false);
        //        if (attrs.Length > 0 && !string.IsNullOrEmpty(attrs[0].FieldName))
        //        {
        //            buttonName = attrs[0].FieldName;
        //        }
        //        AddToggleButton(CallToggleMethod, i, buttonName, value);
        //    }
        //}

        // Draw sliders
        //if (Sliders.ContainsKey(SelectedObject))
        //{
        //    for (int i = 0; i < Sliders[SelectedObject].Count; i++)
        //    {
        //        DebugField[] fields = (DebugField[])Sliders[SelectedObject][i].GetCustomAttributes(typeof(DebugField), false);
        //        DebugField debugField = fields[0];
        //        FieldInfo fieldInfo = Sliders[SelectedObject][i];
        //        string sliderName;
        //        if (string.IsNullOrEmpty(debugField.FieldName))
        //        {
        //            sliderName = Capitals(Sliders[SelectedObject][i].Name);
        //        }
        //        else
        //        {
        //            sliderName = debugField.FieldName;
        //        }
        //        bool isInt;
        //        float value = UtilsDebug.GetFloat(fieldInfo, SelectedObject, out isInt);
        //        AddSlider(CallSliderMethod, i, sliderName, debugField.StartF, debugField.EndF, value, isInt);
        //    }
        //}

        // Draw labels
        //if (Labels.ContainsKey(SelectedObject))
        //{
        //    for (int i = 0; i < Labels[SelectedObject].Count; i++)
        //    {
        //        object value = Labels[SelectedObject][i].GetValue(SelectedObject);
        //        string valueName = value == null ? "null" : value.ToString();
        //        AddLabel(valueName, Labels[SelectedObject][i]);
        //    }
        //}
    }

    private void ClearAllButtons(bool clearTabs, bool clearContent)
    {
        if (clearTabs)
        {
            for (int i = 0; i < tabButton.Count; i++)
            {
                tabButton[i].SetActive(false);
            }
        }
        if (clearContent)
        {
            for (int i = 0; i < contentButton.Count; i++)
            {
                contentButton[i].SetActive(false);
            }
            //for (int i = 0; i < ToggleButtons.Count; i++)
            //{
            //    ToggleButtons[i].SetActive(false);
            //}
            //for (int i = 0; i < ContentSliders.Count; i++)
            //{
            //    ContentSliders[i].SetActive(false);
            //}
            //for (int i = 0; i < LabelButtons.Count; i++)
            //{
            //    LabelButtons[i].SetActive(false);
            //}
        }
    }

    private void CallDebugMethod(int id)
    {
        var button = contentButton[id];
        var buttonText = button.GetComponentInChildren<UnityEngine.UI.Text>().text;
        var i = Buttons[SelectedObject][id];
        i.Invoke(SelectedObject, null);
    }
    //public void Open
    protected void AddButton(UnityAction<int> action, int id, string text, bool isTabButton)
    {
        AddButton(action, null, text, isTabButton, id);
    }

    protected void AddButton(UnityAction action, string text, bool isTabButton)
    {
        AddButton(null, action, text, isTabButton, -1);
    }

    protected void AddButton(UnityAction<int>tabAction, UnityAction action, string text, bool isTabButton, int id)
    {
        GameObject go = isTabButton ? GetNextUnusedButton(tabButton) : GetNextUnusedButton(contentButton);
        Text txt = null;
        Button btn = null;
        if (go == null)
        {
            go = GameObject.Instantiate(buttonPrefab);
            go.name = "btn_" + name;
            go.transform.SetParent(isTabButton ? tabRoot.transform : contentRoot.transform, false);
            if (isTabButton)
            {
                tabButton.Add(go);
            }
            else
            {
                contentButton.Add(go);
            }
        }
        go.SetActive(true);
        btn = go.GetComponent<Button>();
        txt = go.GetComponentInChildren<Text>();
        if (tabAction != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(
                () => 
                {
                    tabAction(id);
                });
        }
        else if (action != null)
        {
            btn.onClick.AddListener(action);
        }
        txt.text = text;
    }

    protected GameObject GetNextUnusedButton(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].activeSelf)
            {
                return list[i];
            }
        }
        return null;
    }

#if STRIP_DBG_SETTINGS
    [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
    public void AddDebuggableObject(object obj)
    {
        InsertDebuggableObject(GlobalDebuggableObjects.Count, obj);
    }

    public void InsertDebuggableObject(int i, object obj)
    {
        if (obj == null)
            return;
        if (!GlobalDebuggableObjects.Contains(obj))
            GlobalDebuggableObjects.Insert(i, obj);
        Repopulate(GlobalDebuggableObjects);
    }

#if STRIP_DBG_SETTINGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
    public void RemoveDebuggableObject(object obj)
    {
        if (SelectedObject == obj)
            SelectedObject = null;
        CloseGUIDebug();
        if (GlobalDebuggableObjects.Contains(obj))
            GlobalDebuggableObjects.Remove(obj);
    }

    public void Clear()
    {
        CloseGUIDebug();
        GlobalDebuggableObjects.Clear();
    }

    public void Repopulate(List<object> debuggableObjects)
    {
        if (debuggableObjects != null)
            GlobalDebuggableObjects = debuggableObjects;

        for (int i = 0; i < GlobalDebuggableObjects.Count; i++)
        {
            object obj = GlobalDebuggableObjects[i];
            Type type = obj.GetType();

#if NETFX_CORE
            var methods = type.GetTypeInfo().DeclaredMethods;
#else
            var methods = type.GetMethods();
#endif
            foreach (MethodInfo method in methods)
            {
                if (method.GetParameters().Length == 0)
                {
                    DebugMethod[] attributes = method.GetCustomAttributes(typeof(DebugMethod), true) as DebugMethod[];
                    if (attributes.Length > 0)
                    {
                        if (!Buttons.ContainsKey(obj))
                        {
                            Buttons[obj] = new List<MethodInfo>();
                        }
                        if (!Buttons[obj].Contains(method))
                        {
                            Buttons[obj].Add(method);
                        }
                    }
                }
            }

#if NETFX_CORE
            //var fieldInfos = type.GetTypeInfo().DeclaredFields;
#else
            //FieldInfo[] fieldInfos = type.GetFields();
#endif
            //foreach (FieldInfo fieldInfo in fieldInfos)
            //{
                //DebugField[] attributes = fieldInfo.GetCustomAttributes(typeof(DebugField), true) as DebugField[];
                //if (attributes.Length > 0)
                //{

                //    if (fieldInfo.FieldType == typeof(bool))
                //    {

                //        if (!CheckBoxes.ContainsKey(obj))
                //        {
                //            CheckBoxes[obj] = new List<FieldInfo>();
                //        }

                //        if (!CheckBoxes[obj].Contains(fieldInfo))
                //        {
                //            CheckBoxes[obj].Add(fieldInfo);
                //        }
                //    }
                //    else if (fieldInfo.FieldType == typeof(int))
                //    {

                //        if (!Sliders.ContainsKey(obj))
                //        {
                //            Sliders[obj] = new List<FieldInfo>();
                //        }

                //        if (!Sliders[obj].Contains(fieldInfo))
                //        {
                //            Sliders[obj].Add(fieldInfo);
                //        }

                //    }
                //    else if (fieldInfo.FieldType == typeof(float))
                //    {

                //        if (!Sliders.ContainsKey(obj))
                //        {
                //            Sliders[obj] = new List<FieldInfo>();
                //        }

                //        if (!Sliders[obj].Contains(fieldInfo))
                //        {
                //            Sliders[obj].Add(fieldInfo);
                //        }
                //    }
                //}

                //DebugLabel[] debugLabels = fieldInfo.GetCustomAttributes(typeof(DebugLabel), true) as DebugLabel[];
                //if (debugLabels.Length > 0)
                //{
                //    if (!Labels.ContainsKey(obj))
                //    {
                //        Labels[obj] = new List<FieldInfo>();
                //    }
                //    if (!Labels[obj].Contains(fieldInfo))
                //    {
                //        Labels[obj].Add(fieldInfo);
                //    }
                //}

            //}
        }
    }
    //public void SyncAttr()
    //{
    //    if (MeteorManager.Instance.LocalPlayer != null && MeteorManager.Instance.LocalPlayer.Attr != null)
    //        MoveSpeed = MeteorManager.Instance.LocalPlayer.Attr.Speed;
    //}

    //public void Apply()
    //{
    //    if (MeteorManager.Instance.LocalPlayer != null && MeteorManager.Instance.LocalPlayer.Attr != null)
    //        MeteorManager.Instance.LocalPlayer.Attr.Speed = MoveSpeed;
    //    GameSpeed %= 4.0f;
    //    Time.timeScale = GameSpeed;
    //}
    //public void CalcMeshCount()
    //{
    //    int vertexCnt = 0;
    //    int triangleCnt = 0;
    //    int submeshCnt = 0;
    //    MeshFilter[] f = FindObjectsOfType<MeshFilter>();

    //    for (int i = 0; i < f.Length; i++)
    //    {
    //        bool repeat = false;
    //        for (int j = 0; j < i; j++)
    //        {
    //            if (f[i] == f[j])
    //            {
    //                repeat = true;
    //                break;
    //            }
    //            if (f[i].sharedMesh == f[j].sharedMesh)
    //            {
    //                repeat = true;
    //                break;
    //            }
    //        }
    //        if (repeat)
    //            continue;
    //        vertexCnt += f[i].sharedMesh.vertexCount;
    //        triangleCnt += f[i].sharedMesh.triangles.Length / 3;
    //        submeshCnt += f[i].sharedMesh.subMeshCount;
    //    }

    //    Debug.Log("顶点数量:" + vertexCnt + " 面数:" + triangleCnt + " 子网格数:" + submeshCnt);
    //}

    //int missCnt = 0;
    //public void ProcessNextLevel()
    //{
    //    System.Collections.Generic.List<string> ls = new System.Collections.Generic.List<string>();
    //    Level lev = LevelMng.Instance.GetItem(level);
    //    if (lev == null || ls.Contains(lev.Scene))
    //    {
    //        missCnt++;
    //        if (missCnt >= 5)
    //            return;
    //        level++;
    //        ProcessNextLevel();
    //        return;
    //    }
    //    else
    //    {
    //        missCnt = 0;
    //        Scene sc = SceneManager.GetSceneByName(lev.Scene);
    //        if (sc == null)
    //        {
    //            level++;
    //            ProcessNextLevel();
    //            return;
    //        }
    //        SceneManager.LoadScene(lev.Scene, LoadSceneMode.Single);
    //        ls.Add(lev.Scene);


    //        //ProcessTexture();
    //        return;
    //        level++;
    //        ProcessNextLevel();
    //    }
    //}

    //public System.Collections.Generic.List<string> lsTexture = new System.Collections.Generic.List<string>();
    //public void ProcessTexture()
    //{
    //Level lev = LevelMng.Instance.GetItem(level);
    //if (lev == null)
    //    return;
    //if (!System.IO.Directory.Exists("Assets/Meteor/" + lev.goodList  + "/resources/"))
    //{
    //    System.IO.Directory.CreateDirectory("Assets/Meteor/" + lev.goodList + "/resources/");
    //    //AssetDatabase.Refresh();
    //    //generateFile = true;
    //}

    //MeshRenderer[] mr = FindObjectsOfType<MeshRenderer>();
    //for (int i = 0; i < mr.Length; i++)
    //{
    //    for (int j = 0; j < mr[i].materials.Length; j++)
    //    {
    //        if (mr[i].materials[j] != null)
    //        {
    //            if (mr[i].materials[j].shader.name == "TextureMask")
    //            {
    //                //2张图
    //                Texture tex = mr[i].materials[j].mainTexture;
    //                Texture texMask = mr[i].materials[j].GetTexture("_MaskTex");
    //                if (tex != null)
    //                {
    //                    string path = AssetDatabase.GetAssetPath(tex);
    //                    string[] filenames = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
    //                    string filename = filenames[filenames.Length - 1];
    //                    string error = AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/" + lev.goodList + "/resources/" + filename);
    //                    if ("" == AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/" + lev.goodList + "/resources/" + filename))
    //                    {
    //                        AssetDatabase.MoveAsset(path, "Assets/Meteor/" + lev.goodList + "/resources/" + filename);
    //                        AssetDatabase.Refresh();
    //                    }
    //                    else
    //                        Debug.LogError(error);
    //                }
    //                if (texMask != null)
    //                {
    //                    string path = AssetDatabase.GetAssetPath(texMask);
    //                    string[] filenames = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
    //                    string filename = filenames[filenames.Length - 1];
    //                    string error = AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/" + lev.goodList + "/resources/" + filename);
    //                    if ("" == AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/" + lev.goodList + "/resources/" + filename))
    //                    {
    //                        AssetDatabase.MoveAsset(path, "Assets/Meteor/" + lev.goodList + "/resources/" + filename);
    //                        AssetDatabase.Refresh();
    //                    }
    //                    else
    //                        Debug.LogError(error);
    //                }
    //            }
    //            else
    //            {
    //                if (mr[i].materials[j].mainTexture != null)
    //                {
    //                    string path = AssetDatabase.GetAssetPath(mr[i].materials[j].mainTexture);
    //                    string[] filenames = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
    //                    string filename = filenames[filenames.Length - 1];
    //                    string error = AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/" + lev.goodList + "/resources/" + filename);
    //                    if ("" == AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/" + lev.goodList + "/resources/" + filename))
    //                    {
    //                        AssetDatabase.MoveAsset(path, "Assets/Meteor/" + lev.goodList + "/resources/" + filename);
    //                        AssetDatabase.Refresh();
    //                    }
    //                    else
    //                        Debug.LogError(error);
    //                }
    //            }
    //        }
    //    }
    //}

    //IFLLoader[] lo = FindObjectsOfType<IFLLoader>();
    //if (lo.Length != 0)
    //{
    //    for (int i = 0; i < lo.Length; i++)
    //    {
    //        if (lo[i].tex != null && lo[i].tex.Length != 0)
    //        {
    //            for (int j = 0; j < lo[i].tex.Length; j++)
    //            {
    //                string path = AssetDatabase.GetAssetPath(lo[i].tex[j]);
    //                string[] filenames = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
    //                string filename = filenames[filenames.Length - 1];
    //                string error = AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/ifl"  + "/resources/" + filename);
    //                if ("" == AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/ifl" + "/resources/" + filename))
    //                {
    //                    AssetDatabase.MoveAsset(path, "Assets/Meteor/ifl"  + "/resources/" + filename);
    //                    AssetDatabase.Refresh();
    //                }
    //                else
    //                    Debug.LogError(error);
    //            }
    //        }
    //    }
    //}
    //Debug.LogError("ProcessTexture done");
    //level++;
    //AssetDatabase.MoveAsset();
    //}

    //有此脚本，即可在任何关卡开启游戏
    public void Start()
    {

        for (int i = 0; i < 20; i++)
            AmbLoader.Ins.LoadCharacterAmb(i);
        AmbLoader.Ins.LoadCharacterAmb();
        ActionInterrupt.Instance.Init();
        MenuResLoader.Instance.Init();
            //level = 1;
        DontDestroyOnLoad(gameObject);
            //DontDestroyOnLoad(canvas);
            //Dictionary<string, List<string>> kv = new Dictionary<string, List<string>>();
            //for (int i = 0; i < 45; i++)
            //{
            //    if (i == 41)
            //        continue;
            //    Level lev = LevelMng.Instance.GetItem(i);
            //    if (lev == null)
            //        continue;
                //DesFile des = DesLoader.Instance.Load(lev.goodList);
                //GMBFile gmb = GMBLoader.Instance.Load(lev.goodList);
                //if (gmb == null)
                //    continue;
                ////for (int j = 0; j < gmb.TexturesNames.Count; j++)
                ////{
                //if (!kv.ContainsKey(lev.goodList))
                //    kv.Add(lev.goodList, gmb.TexturesNames);
                //}
            //}

            //System.IO.StreamWriter wr = System.IO.File.CreateText("Assets/debug.txt");
            //foreach (var each in kv)
            //{
            //    //wr.WriteLine(each.Key);
            //    for (int i = 0; i < each.Value.Count; i++)
            //    {
            //        string[] file = each.Value[i].Split(new char[] { '.' }, System.StringSplitOptions.RemoveEmptyEntries);
            //        Texture tex = Resources.Load<Texture>(file[0]);
            //        if (tex != null)
            //        {
            //            string path = AssetDatabase.GetAssetPath(tex);
            //            tex = null;
            //            string[] filenames = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
            //            string filename = filenames[filenames.Length - 1];
            //            string error = AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/" + each.Key + "/resources/" + filename);
            //            if ("" == AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/" + each.Key + "/resources/" + filename))
            //            {
            //                AssetDatabase.MoveAsset(path, "Assets/Meteor/" + each.Key + "/resources/" + filename);
            //                AssetDatabase.Refresh();
            //            }
            //            else
            //                Debug.LogError(error);
            //        }
            //    }
            //    //wr.WriteLine(each.Value[i]);
            //}
            //wr.Flush();
            //wr.Close();
            //ProcessItemTexture();

    }

    public System.Collections.Generic.List<string> ltemTexture = new System.Collections.Generic.List<string>();
    //public void ProcessItemTexture()
    //{
    //    Level lev = LevelMng.Instance.GetItem(level);
    //    if (lev == null)
    //        return;
    //    if (!System.IO.Directory.Exists("Assets/Meteor/Item" + "/resources/"))
    //    {
    //        System.IO.Directory.CreateDirectory("Assets/Meteor/Item" + "/resources/");
    //        AssetDatabase.Refresh();
    //        //generateFile = true;
    //    }

    //    MeshRenderer[] mr = FindObjectsOfType<MeshRenderer>();
    //    for (int i = 0; i < mr.Length; i++)
    //    {
    //        for (int j = 0; j < mr[i].materials.Length; j++)
    //        {
    //            if (mr[i].materials[j] != null)
    //            {

    //                if (mr[i].materials[j].shader.name == "TextureMask")
    //                {
    //                    //2张图
    //                    Texture tex = mr[i].materials[j].mainTexture;
    //                    Texture texMask = mr[i].materials[j].GetTexture("_MaskTex");
    //                    if (tex != null)
    //                    {
    //                        string path = AssetDatabase.GetAssetPath(tex);
    //                        string[] filenames = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
    //                        string filename = filenames[filenames.Length - 1];
    //                        string error = AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/Item" + "/resources/" + filename);
    //                        if ("" == AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/Item" + "/resources/" + filename))
    //                        {
    //                            AssetDatabase.MoveAsset(path, "Assets/Meteor/Item" + "/resources/" + filename);
    //                            AssetDatabase.Refresh();
    //                        }
    //                        else
    //                            Debug.LogError(error);
    //                    }
    //                    if (texMask != null)
    //                    {
    //                        string path = AssetDatabase.GetAssetPath(texMask);
    //                        string[] filenames = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
    //                        string filename = filenames[filenames.Length - 1];
    //                        string error = AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/Item" + "/resources/" + filename);
    //                        if ("" == AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/Item" + "/resources/" + filename))
    //                        {
    //                            AssetDatabase.MoveAsset(path, "Assets/Meteor/Item" + "/resources/" + filename);
    //                            AssetDatabase.Refresh();
    //                        }
    //                        else
    //                            Debug.LogError(error);
    //                    }
    //                }
    //                else
    //                {
    //                    if (mr[i].materials[j].mainTexture != null)
    //                    {
    //                        string path = AssetDatabase.GetAssetPath(mr[i].materials[j].mainTexture);
    //                        string[] filenames = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
    //                        string filename = filenames[filenames.Length - 1];
    //                        string error = AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/Item" + "/resources/" + filename);
    //                        if ("" == AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/Item" + "/resources/" + filename))
    //                        {
    //                            AssetDatabase.MoveAsset(path, "Assets/Meteor/Item" + "/resources/" + filename);
    //                            AssetDatabase.Refresh();
    //                        }
    //                        else
    //                            Debug.LogError(error);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    Debug.LogError("ProcessItemTexture done");
    //    level++;
    //    AssetDatabase.MoveAsset();
    //}

    //public void GenerateAll()
    //{
    //    Level[] all = LevelMng.Instance.GetAllItem();
    //    for (int i = 0; i < all.Length; i++)
    //    {
    //        GameObject obj = new GameObject(all[i].goodList);
    //        obj.transform.position = Vector3.zero;
    //        obj.transform.localScale = Vector3.one;
    //        obj.transform.rotation = Quaternion.identity;
    //        obj.layer = LayerMask.NameToLayer("Scene");
    //        obj.transform.SetParent(null);
    //        MapLoader load = obj.AddComponent<MapLoader>();
    //        load.levelId = all[i].ID;
    //        load.LoadMap(all[i].ID);
    //    }
    //}
}
