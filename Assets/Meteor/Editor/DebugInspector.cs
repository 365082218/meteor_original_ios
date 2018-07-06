using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(WSDebug))]
public class WSDebugInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //WSDebug myTarget = (WSDebug)target;
        //if (GUILayout.Button("Apply"))
        //{
        //    myTarget.Apply();
        //}
        //if (GUILayout.Button("Sync"))
        //{
        //    myTarget.SyncAttr();
        //}
        //if (GUILayout.Button("CalcMeshCount"))
        //{
        //    myTarget.CalcMeshCount();
        //}
        //if (GUILayout.Button("GenerateAllMaterial"))
        //{
        //    myTarget.GenerateAll();
        //}
        //if (GUILayout.Button("Apply"))
        //{
        //    myTarget.Apply();
        //}
        //if (GUILayout.Button("LoadAmbTest"))
        //{
        //    AmbLoader.Ins.LoadAmb("characteramb");
        //}
    }
}
