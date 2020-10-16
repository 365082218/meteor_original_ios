using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Net;

//[CustomEditor(typeof(SFXUnit))]
//public class SFXUnitInspector : Editor {
//    public override void OnInspectorGUI() {
//        base.OnInspectorGUI();
//        if (GUILayout.Button("DumpParticle")) {
//            var obj = (SFXUnit)target;
//            string file = EditorUtility.SaveFilePanel("DumpParticle", "", "Particle", "bytes");
//            if (!string.IsNullOrEmpty(file))
//                obj.SaveParticle(file);
//        }
//    }
//}

[CustomEditor(typeof(AudioButton))]
public class AudioButtonInspector : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
    }
}

[CustomEditor(typeof(GameBattleEx))]
public class GameBattleExInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var obj = (GameBattleEx)target;
        if (GUILayout.Button("ShowWayPoint")) {
            obj.ShowWayPoint(true);
        }
        if (GUILayout.Button("HideWayPoint")) {
            obj.ShowWayPoint(false);
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

[CustomEditor(typeof(Loader))]
public class LoadInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Loader myTarget = (Loader)target;
        if (GUILayout.Button("CreateSpwanPoint")) {
            myTarget.CreateSpawnPoint();
        }
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

        if (GUILayout.Button("LoadFile"))
        {
            myTarget.Init(myTarget.fmcFile);
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
[CustomEditor(typeof(CalcDistance))]
public class CalcDistanceInspector : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        CalcDistance myTarget = (CalcDistance)target;
        if (GUILayout.Button("CalcDis")) {
            myTarget.CalcDis();
        }
        if (GUILayout.Button("CalcAngle")) {
            myTarget.CalcAngle();
        }
        if (GUILayout.Button("LookAt")) {
            myTarget.LookAt();
        }
    }
}

[CustomEditor(typeof(CharacterLoader))]
public class CharacterLoaderInspector : Editor
{
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
       
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
        if (GUILayout.Button("SaveMaterial"))
        {
            myTarget.SaveMaterial();
        }
        if (GUILayout.Button("SaveMesh"))
        {
            myTarget.SaveMesh();
        }
        if (GUILayout.Button("LoadMesh"))
        {
            myTarget.LoadMeshFromPath();
        }
        if (GUILayout.Button("LoadWayPoint"))
        {
            myTarget.LoadWayPoint();
        }
        if (GUILayout.Button("CheckName")) {
            myTarget.CheckName();
        }
    }
}
[CustomEditor(typeof(UGUIJoystick))]
public class UGUIJoystickInspector : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
    }
}

[CustomEditor(typeof(MeteorUnit))]
public class MeteorUnitInspector : Editor
{
    public int wayIndex = -1;
    public string way = "";
    public GameObject vecTarget;
    string action = "action";
    //float step = 0.05f;
    //float FadeIn = 0.1f;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MeteorUnit myTarget = (MeteorUnit)target;
        way = GUILayout.TextField(way);
        action = GUILayout.TextField(action);
        if (GUILayout.Button("GetWayIndex"))
        {
            wayIndex = PathMng.Ins.GetWayIndex(myTarget.mSkeletonPivot);
            way = wayIndex.ToString();
        }

        if (GUILayout.Button("GetWayIndex2"))
        {
            wayIndex = PathMng.Ins.GetWayIndex(myTarget.mSkeletonPivot);
            way = wayIndex.ToString();
        }

        if (GUILayout.Button("FaceToTarget"))
        {
            if (vecTarget == null)
                vecTarget = GameObject.Find("facetotarget");
            if (vecTarget == null)
                return;
            myTarget.StateMachine.ChangeState(myTarget.StateMachine.FaceToState, vecTarget.transform.position);
        }

        if (GUILayout.Button("ChangeAction")) {
            int act = int.Parse(action);
            myTarget.ActionMgr.ChangeAction(act, 0.1f);
        }

        //if (GUILayout.Button("BakeAnimation")) {
        //    int act = int.Parse(action);
        //    myTarget.ActionMgr.BakePose(act);
        //}

        if (GUILayout.Button("StopAllAction")) {
            
        }

        if (GUILayout.Button("SampleFrame")) {
            //PoseState FadeOutState = myTarget.ActionMgr.AddState(0);//从原先的动作
            //FadeInState.SetEnabled(true);
            //FadeInState.SetNormalizedTime(FadeIn);
            //FadeInState.SetWeight(1);
            //FadeOutState.SetEnabled(true);
            //FadeOutState.SetNormalizedTime(1 - FadeIn);
            //FadeOutState.SetWeight(1);
            //FrameReplay.deltaTime = Time.deltaTime;
            //FrameReplay.Instance.time += Time.deltaTime;
            
        }
    }
}

[CustomEditor(typeof(FrameReplay))]
public class FrameReplayInspector : Editor {
    string trackerid = "trackid";
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        FrameReplay frameReplay = (FrameReplay)target;
        trackerid = GUILayout.TextField(trackerid);
        if (GUILayout.Button("SetTrackerId")) {
            int track = 0;
            if (int.TryParse(trackerid, out track))
                FrameSnapShot.Ins.NewTracker(track);
        }
    }
}