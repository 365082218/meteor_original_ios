using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(CobLoader))]
public class CobInspector:Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Apply"))
        {
            var obj = (CobLoader)target;
            obj.Load();
        }
    }
}
[CustomEditor(typeof(ChangeShader))]
public class ChangeShaderInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Change"))
        {
            var obj = (ChangeShader)target;
            obj.Change();
        }
    }
}

[CustomEditor(typeof(PMDSave))]
public class PMDSaveInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Save"))
        {
            var obj = (PMDSave)target;
            string file = EditorUtility.SaveFilePanel("save pmd file", "", "", "pmd");
            if (!string.IsNullOrEmpty(file))
                obj.Save(file);
        }

        if (GUILayout.Button("SaveMap"))
        {
            var obj = (PMDSave)target;
            string file = EditorUtility.SaveFilePanel("save pmd file", "", "", "pmd");
            if (!string.IsNullOrEmpty(file))
                obj.SaveMap(file);
        }
    }
}

[CustomEditor(typeof(MeshInfo))]
public class MeshInfoInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MeshInfo myTarget = (MeshInfo)target;
        if (GUILayout.Button("LoadMesh"))
        {
            myTarget.Init();
        }
    }
}

[CustomEditor(typeof(WeaponLoaderEx))]
public class WeaponLoaderExInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        WeaponLoaderEx myTarget = (WeaponLoaderEx)target;
        if (GUILayout.Button("LoadWeapon"))
        {
            myTarget.EquipWeapon();
        }
    }
}
[CustomEditor(typeof(Loader))]
public class LoadInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Loader myTarget = (Loader)target;
        //if (GUILayout.Button("AddSceneObj"))
        //{
        //    myTarget.LoadSceneObjs(myTarget.LevelId);
        //}
        if (GUILayout.Button("AddSceneObjsFromDes"))
        {
            myTarget.LoadSceneObjsFromDes(myTarget.defFile);
        }
        //if (GUILayout.Button("SaveFixedSceneObjs"))
        //{
            //myTarget.SaveFixedSceneObjs();
        //}
    }
}

[CustomEditor(typeof(FMCPlayer))]
public class FixedTriggerInspector: Editor
{
    string pos = "Pos Id";
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        FMCPlayer myTarget = (FMCPlayer)target;
        pos = GUILayout.TextField(pos);
        if (GUILayout.Button("ChangePose"))
        {
            myTarget.ChangePose(int.Parse(pos), 0);
        }
    }
}
[CustomEditor(typeof(IFLLoader))]
public class IFLInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        IFLLoader myTarget = (IFLLoader)target;
        if (GUILayout.Button("LoadIFL"))
        {
            myTarget.LoadIFL();
        }
    }
}

[CustomEditor(typeof(WSDebug))]
public class DebugInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        WSDebug myTarget = (WSDebug)target;
        if (GUILayout.Button("Apply"))
        {
            myTarget.Apply();
        }
        if (GUILayout.Button("LoadAmbTest"))
        {
            AmbLoader.Ins.LoadAmb("characteramb");
        }
    }
}

[CustomEditor(typeof(UIMoveControl))]
public class UIMoveInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        UIMoveControl myTarget = (UIMoveControl)target;
        if (GUILayout.Button("ShowAction"))
        {
            myTarget.ShowAction();
        }
        if (GUILayout.Button("HideAction"))
        {
            myTarget.HideAction();
        }
    }

    
}
//[CustomEditor(typeof(PoseStatus))]
//public class PoseStatusInspector : Editor
//{
//    string pos = "Pos Id";
//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();
//        PoseStatus myTarget = (PoseStatus)target;
//        pos = GUILayout.TextField(pos);
//        if (GUILayout.Button("PlayPos"))
//        {
//            if (!string.IsNullOrEmpty(pos))
//                myTarget.ChangeAction(int.Parse(pos));
//        }
//    }
//}
//public class StatusMachineEditor
//{
//    [MenuItem("Assets/Create/Meteor/PassCondition")]
//    public static void CreateCondition()
//    {
//        string assetPath = EditorUtility.SaveFilePanelInProject("New PassCondition", "New PassCondition", "asset", "Create a new PassCondition.");
//        if (string.IsNullOrEmpty(assetPath))
//            return;
//        PassCondition asset = ScriptableObject.CreateInstance<PassCondition>();  //scriptable object
//        AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(assetPath));
//        AssetDatabase.Refresh();
//    }

//    [MenuItem("Assets/Create/Meteor/StateMachine")]
//    public static void CreateDatabase()
//    {
//        string assetPath = EditorUtility.SaveFilePanelInProject("New StateMachine", "New StateMachine", "asset", "Create a new StateMachin.");
//        if (string.IsNullOrEmpty(assetPath))
//            return;
//        StatusMachine asset = ScriptableObject.CreateInstance<StatusMachine>();  //scriptable object
//        AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(assetPath));
//        AssetDatabase.Refresh();
//    }
//}

[CustomEditor(typeof(CharacterLoader))]
public class CharacterLoaderInspector : Editor
{
    string source = "source idx";
    string frame = "frame idx";
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CharacterLoader myTarget = (CharacterLoader)target;
        source = GUILayout.TextField(source);
        frame = GUILayout.TextField(frame);
        if (GUILayout.Button("PlayFrame"))
        {
            if (!string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(frame))
                myTarget.ChangeFrame(int.Parse(source), int.Parse(frame));
        }
    }
}

[CustomEditor(typeof(PoseStatusDebug))]
public class PoseStatusDebugInspector : Editor
{
    string pos = "Pos Id";
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PoseStatusDebug myTarget = (PoseStatusDebug)target;
        pos = GUILayout.TextField(pos);
        if (GUILayout.Button("PlayPos"))
        {
            if (!string.IsNullOrEmpty(pos))
                myTarget.ChangeAction(int.Parse(pos));
        }
    }
}

[CustomEditor(typeof(MeteorUnit))]
public class MeteorUnitInspector : Editor
{
    string pos = "Weapon Id";
    string ModelId = "Model Id";
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MeteorUnit myTarget = (MeteorUnit)target;
        pos = GUILayout.TextField(pos);
        if (GUILayout.Button("EquipWeapon"))
        {
            //if (!string.IsNullOrEmpty(pos))
            //    myTarget.EquipWeapon(null);
        }
        ModelId = GUILayout.TextField(ModelId);
        if (GUILayout.Button("Init"))
        {
            if (!string.IsNullOrEmpty(ModelId))
                myTarget.Init(int.Parse(ModelId));
        }
    }
}

[CustomEditor(typeof(CameraSnapShot))]
public class CameraSnapShotInspector : Editor
{
    string pos = "Weapon Id";
    string ModelId = "Model Id";
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CameraSnapShot myTarget = (CameraSnapShot)target;
        if (GUILayout.Button("SnapShot"))
        {
            myTarget.OnSaveMapFogTexture();
        }
    }
}

[CustomEditor(typeof(MapLoader))]
public class MapLoaderInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MapLoader myTarget = (MapLoader)target;
        if (GUILayout.Button("LoadFromXls"))
        {
            myTarget.LoadMap(myTarget.levelId);
        }
        if (GUILayout.Button("LoadFromDes"))
        {
            myTarget.LoadDesMap(myTarget.desFile);
        }
    }
}

[CustomEditor(typeof(CalcDistance))]
public class CalcDistanceInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CalcDistance myTarget = (CalcDistance)target;
        if (GUILayout.Button("Calc"))
        {
            myTarget.Calc();
        }
    }
}