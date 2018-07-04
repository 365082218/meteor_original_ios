using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

[CustomEditor(typeof(Lightbeam))]
public class LightbeamEditor : Editor
{
    [MenuItem("GameObject/Create Other/Light Beam")]
    static void NewLightBeam()
    {
        GameObject gameObject = new GameObject("lightbeam");
        Lightbeam lightBeam = gameObject.AddComponent(typeof(Lightbeam)) as Lightbeam;
        Selection.activeGameObject = gameObject;

        SaveNewAsset(lightBeam, false);
        ModifyMesh(lightBeam);

        if (SceneView.currentDrawingSceneView != null)
        {
            SceneView.currentDrawingSceneView.MoveToView(gameObject.transform);
            gameObject.transform.position += new Vector3(0, lightBeam.Length / 2, 0);
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeInspector();
        Lightbeam lightBeam = (target as Lightbeam);

        if (lightBeam.IsModifyingMesh == true && IsLightBeamMesh() == false)
        {
            Debug.LogWarning("Escaped lightbeam modification mode. No valid lightbeam mesh was found.");
            lightBeam.IsModifyingMesh = false;
        }

        if (lightBeam.IsModifyingMesh == false)
        {
            EditorGUILayout.BeginHorizontal();
            if (IsLightBeamMesh())
            {
                if (GUILayout.Button(new GUIContent("Modify", "Modify the lightbeam mesh.")))
                {
                    Undo.RegisterUndo(lightBeam, "Modify light beam");
                    ModifyMesh(lightBeam);
                    EditorUtility.SetDirty(lightBeam);
                }
            }
            else
            {
                GUI.enabled = false;
                if (GUILayout.Button(new GUIContent("Modify", "Missing a valid lightbeam mesh.")))
                {
                }
                GUI.enabled = true;
            }

            if (GUILayout.Button(new GUIContent("New Lightbeam Mesh", "Create a new lightbeam mesh for this object.")))
            {
                int result = EditorUtility.DisplayDialogComplex("Duplicate material", "Do you want to duplicate the material?\nIf you don't duplicate the same material will be used.", "Yes", "No", "Cancel");

                if (result == 0) // ok
                {
                    Undo.RegisterSceneUndo("New Lightbeam Mesh");
                    SaveNewAsset(lightBeam, true);
                    ModifyMesh(lightBeam);
                    EditorUtility.SetDirty(lightBeam);
                }
                if (result == 1) // no
                {
                    Undo.RegisterSceneUndo("New Lightbeam Mesh");
                    SaveNewAsset(lightBeam, false);
                    ModifyMesh(lightBeam);
                    EditorUtility.SetDirty(lightBeam);
                }

            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.BeginVertical();

            string RadiusTopTooltip = "The top radius of the lightbeam.";
            string RadiusBottomTooltip = "The bottom radius of the lightbeam.";
            string LengthTooltip = "Length of the lightbeam.";
            string SubdivisionsTooltip = "Horizontal Subdivisions\n\nIncreasing this will smoothen the lightbeam by adding vertices.";
            string SubdivisionsHeightTooltip = "Vertical Subdivisions\n\nIncreasing this will smoothen the lightbeam by adding vertices.";

            lightBeam.RadiusTop = EditorGUILayout.FloatField(new GUIContent("Radius Top", RadiusTopTooltip), lightBeam.RadiusTop);
            lightBeam.RadiusBottom = EditorGUILayout.FloatField(new GUIContent("Radius Bottom", RadiusBottomTooltip), lightBeam.RadiusBottom);
            lightBeam.Length = EditorGUILayout.FloatField(new GUIContent("Length", LengthTooltip), lightBeam.Length);
            lightBeam.Subdivisions = EditorGUILayout.IntSlider(new GUIContent("Subdivisions", SubdivisionsTooltip), lightBeam.Subdivisions, 3, 50);
            lightBeam.SubdivisionsHeight = EditorGUILayout.IntSlider(new GUIContent("Subdivisions Height", SubdivisionsHeightTooltip), lightBeam.SubdivisionsHeight, 1, 10);

            if (lightBeam.RadiusTop <= 0.01f)
                lightBeam.RadiusTop = 0.01f;
            if (lightBeam.RadiusBottom <= 0.01f)
                lightBeam.RadiusBottom = 0.01f;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Done", "Finish modifying the lightbeam mesh.")))
            {
                Undo.RegisterUndo(lightBeam, "Finished Modifying Lightbeam Mesh");
                lightBeam.IsModifyingMesh = false;
                EditorUtility.SetDirty(lightBeam);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        if (GUI.changed)
        {
            Undo.SetSnapshotTarget(lightBeam.Settings, "Modify Lightbeam Mesh");
            Undo.CreateSnapshot();
            Undo.RegisterSnapshot();
            lightBeam.GenerateBeam();
            EditorUtility.SetDirty(lightBeam);
        }
    }

    public void OnSceneGUI()
    {
        Lightbeam lightBeam = (target as Lightbeam);

        if (lightBeam.IsModifyingMesh == true && IsLightBeamMesh() == false)
        {
            Debug.LogWarning("Exited lightbeam modification mode. No valid lightbeam mesh was found.");
            lightBeam.IsModifyingMesh = false;
        }

        if (Event.current.type == EventType.ValidateCommand)
        {
            if (Event.current.commandName == "UndoRedoPerformed")
            {
                if (IsLightBeamMesh())
                    lightBeam.GenerateBeam();
            }
        }

        if (lightBeam.IsModifyingMesh)
        {
            // Register the undos when we press the Mouse button.
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Undo.SetSnapshotTarget(lightBeam.Settings, "Modify Lightbeam Mesh");
                Undo.CreateSnapshot();
                Undo.RegisterSnapshot();
                Undo.ClearSnapshotTarget();
            }

            // red radius circles
            Handles.matrix = lightBeam.transform.localToWorldMatrix;
            Handles.color = Color.black;
            Handles.DrawWireDisc(new Vector3(0, -lightBeam.Length, 0), Vector3.up, lightBeam.RadiusBottom);
            Handles.DrawWireDisc(Vector3.zero, Vector3.up, lightBeam.RadiusTop);

            // handle positions and sizes
            Vector3 rTopPos = new Vector3(-lightBeam.RadiusTop, 0, 0);
            Vector3 rBottomPos = new Vector3(-lightBeam.RadiusBottom, -lightBeam.Length, 0);
            Vector3 lengthPos = new Vector3(0, -lightBeam.Length, 0);
            float rTopSize = HandleUtility.GetHandleSize(lightBeam.transform.TransformDirection(rTopPos));
            float rBottomSize = HandleUtility.GetHandleSize(lightBeam.transform.TransformDirection(rBottomPos));
            float lengthSize = HandleUtility.GetHandleSize(lightBeam.transform.TransformDirection(lengthPos));

            // yellow radius handles
            Handles.color = Color.yellow;
            lightBeam.RadiusTop = Handles.ScaleValueHandle(lightBeam.RadiusTop, rTopPos, Quaternion.identity, rTopSize, Handles.CylinderCap, 2);
            lightBeam.RadiusBottom = Handles.ScaleValueHandle(lightBeam.RadiusBottom, rBottomPos, Quaternion.identity, rBottomSize, Handles.CylinderCap, 2);
            lightBeam.Length = Handles.ScaleValueHandle(lightBeam.Length, lengthPos, Quaternion.Euler(Vector3.up), lengthSize, Handles.CubeCap, 0);

            if (lightBeam.RadiusTop <= 0.01f)
                lightBeam.RadiusTop = 0.01f;
            if (lightBeam.RadiusBottom <= 0.01f)
                lightBeam.RadiusBottom = 0.01f;

            if (GUI.changed)
            {
                lightBeam.GenerateBeam();
                EditorUtility.SetDirty(lightBeam);
            }
        }
    }

    private bool IsLightBeamMesh()
    {
        Lightbeam lightBeam = (target as Lightbeam);
        LightbeamSettings settings = GetLigthbeamSettings(lightBeam);
        if (settings != null)
            return true;
        else
            return false;
    }

    static void ModifyMesh(Lightbeam lightBeam)
    {
        lightBeam.Settings = GetLigthbeamSettings(lightBeam);
        lightBeam.IsModifyingMesh = true;
        lightBeam.GenerateBeam();
    }

    private static LightbeamSettings GetLigthbeamSettings(Lightbeam lightBeam)
    {
        LightbeamSettings settings = null;
        MeshFilter meshFilter = lightBeam.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(meshFilter.sharedMesh)))
            {
                if (asset is LightbeamSettings)
                {
                    settings = asset as LightbeamSettings;
                }
            }
        }
        return settings;
    }

    private static void SaveNewAsset(Lightbeam lightBeam, bool duplicateMaterial)
    {
        MeshRenderer meshRenderer = (MeshRenderer)lightBeam.GetComponent(typeof(MeshRenderer));
        if (meshRenderer == null)
            meshRenderer = lightBeam.gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

        MeshFilter meshFilter = (MeshFilter)lightBeam.GetComponent(typeof(MeshFilter));
        if (meshFilter == null)
            meshFilter = lightBeam.gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;

        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == null)
            mesh = new Mesh();

        LightbeamSettings settings = GetLigthbeamSettings(lightBeam);
        if (settings != null)
            lightBeam.Settings = ScriptableObject.Instantiate(settings) as LightbeamSettings;
        else
            lightBeam.Settings = ScriptableObject.CreateInstance<LightbeamSettings>();

        lightBeam.Settings.hideFlags = HideFlags.HideInHierarchy;

        Material material = null;
        Material sourceMaterial = null;
        if (meshRenderer.sharedMaterial != null)
        {
            if (duplicateMaterial)
                material = new Material(meshRenderer.sharedMaterial);
            else
                material = meshRenderer.sharedMaterial;
            sourceMaterial = meshRenderer.sharedMaterial;
        }

        // The default material can be set on the script file from the inspector, this is the preferred way of doing it since you can choose what material to use as default.
        if (material == null && lightBeam.DefaultMaterial != null)
            material = new Material(lightBeam.DefaultMaterial);

        // If the default material wasn't set, try to find the material from the script path.
        if (material == null)
        {
            // Get the light beam folder by getting the path to the script and using that path to find the material.
            MonoScript script = MonoScript.FromMonoBehaviour(lightBeam);
            string scriptPath = AssetDatabase.GetAssetPath(script);

            scriptPath = Path.GetDirectoryName(scriptPath);
            string[] directories = scriptPath.Split('/');
            scriptPath = string.Join("/", directories, 0, directories.Length - 1);

            string materialPath = scriptPath + "/Source/Lightbeam.mat";
            Material mat = AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;
            if (mat != null)
                material = new Material(mat);
        }

        // If the material still is null we'll create a new material and assign the shader to it.
        if (material == null)
            material = new Material(Shader.Find("Lightbeam/Lightbeam"));

        meshRenderer.sharedMaterial = material;

        // Copy mesh
        Mesh newMesh = new Mesh();
        newMesh.Clear();
        newMesh.vertices = mesh.vertices;
        newMesh.triangles = mesh.triangles;
        newMesh.normals = mesh.vertices;
        newMesh.uv = mesh.uv;
        newMesh.tangents = mesh.tangents;
        newMesh.colors = mesh.colors;

        // Save the mesh asset
        // Find a free name to use
        string savePath = "";
        string assetPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
        if (assetPath == "")
            assetPath = "Assets/lightbeam";
        else
            assetPath = Path.GetDirectoryName(assetPath) + "/" + Path.GetFileNameWithoutExtension(assetPath);

        int i = 0;
        while (true)
        {
            savePath = assetPath + "_" + i + ".asset";
            if (File.Exists(savePath) == false)
                break;
            i++;
        }
        AssetDatabase.CreateAsset(newMesh, savePath);
        AssetDatabase.AddObjectToAsset(lightBeam.Settings, newMesh);
        AssetDatabase.ImportAsset(savePath);

        // Save the material
        string materialAssetPath = AssetDatabase.GetAssetPath(sourceMaterial);
        if (materialAssetPath == "" || duplicateMaterial)
        {
            if (materialAssetPath == "")
                materialAssetPath = "Assets/lightbeam";
            else
                materialAssetPath = Path.GetDirectoryName(materialAssetPath) + "/" + Path.GetFileNameWithoutExtension(materialAssetPath);

            savePath = "";
            i = 0;
            while (true)
            {
                savePath = materialAssetPath + "_" + i + ".mat";
                if (File.Exists(savePath) == false)
                    break;
                i++;
            }
            AssetDatabase.CreateAsset(material, savePath);
        }

        meshFilter.sharedMesh = newMesh;
        meshRenderer.sharedMaterial = material;
    }
}
