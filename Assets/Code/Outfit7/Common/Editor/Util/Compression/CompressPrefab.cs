using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Outfit7.Util.Compression {

    public class CompressPrefab {

        private const string TAG = "CompressPrefab";
        private const string ImportTag = "IMPORT";
        private const string ImportUsedTag = "IMPORTUSED";

        [System.Flags]
        public enum CompressFlags {
            None,
            CompressMesh = 1,
            OverrideMaterial = 2,
        }

        private struct CompressQueueElement {
            public string SourcePath;
            public string DestinationPath;
            public CompressFlags CompressFlags;

            public CompressQueueElement(string sourcePath, string destinationPath, CompressFlags compressFlags) {
                SourcePath = sourcePath;
                DestinationPath = destinationPath;
                CompressFlags = compressFlags;
            }
        }

        private struct RendererInfo {
            public SkinnedMeshRenderer SourceRenderer;
            public SkinnedMeshRenderer TargetRenderer;
        }

        private static Queue<CompressQueueElement> CompressQueue;

        private static void OnUpdate() {
            while (CompressQueue.Count > 0) {
                CompressQueueElement element = CompressQueue.Dequeue();
                CompressInternal(AssetDatabase.LoadAssetAtPath<GameObject>(element.SourcePath), element.SourcePath, element.DestinationPath, element.CompressFlags);
            }
        }

        private static void FillTargetHierarchy(Transform t, Dictionary<string,Transform> transforms) {
            if (transforms.ContainsKey(t.name)) {
                O7Log.WarnT(TAG, "Theres more than one Transform {0} in the hierarchy. Merging will most probably fail.", t.name);
            } else {
                transforms.Add(t.name, t);
            }
            for (int i = 0; i < t.childCount; i++) {
                FillTargetHierarchy(t.GetChild(i), transforms);
            }
        }

        private static void CleanUpTargetHierarchy(Transform t) {
            // If the transform tag is still import it means it wasnt imported so it needs to be deleted
            if (t.tag == ImportTag) {
                for (int i = 0; i < t.childCount; i++) {
                    Transform child = t.GetChild(i);
                    // If this transform is not part of the import re-parent
                    if (child.tag != ImportTag && child.tag != ImportUsedTag) {
                        child.parent = t.parent;
                        i--;
                    }
                }
                O7Log.WarnT(TAG, "Deleting unused tranform {0}!", t.name);
                GameObject.DestroyImmediate(t.gameObject);
                return;
            } else if (t.tag == ImportUsedTag) {
                t.tag = ImportTag;
            }
            for (int i = 0; i < t.childCount; i++) {
                CleanUpTargetHierarchy(t.GetChild(i));
            }
        }

        private static void CopyTransform(Transform source, Transform target) {
            target.localPosition = source.localPosition;
            target.localRotation = source.localRotation;
            target.localScale = source.localScale;
            EditorUtility.SetDirty(target);
        }

        private static void CreateTargetHierarchy(GameObject sourceAsset, string sourcePath, GameObject targetAsset, Transform from, Transform to, CompressFlags compressFlags, Dictionary<string,Transform> transforms, List<RendererInfo> rendererInfos) {
            to.tag = ImportUsedTag;
            // Traverse children
            for (int i = 0; i < from.transform.childCount; i++) {
                Transform fromChild = from.transform.GetChild(i);
                Transform toChild = to.FindChild(fromChild.name);
                // Create hierarchy
                if (toChild == null) {
                    transforms.TryGetValue(fromChild.name, out toChild);
                    if (toChild == null) {
                        GameObject toChildGo = new GameObject(fromChild.name);
                        toChild = toChildGo.transform;
                        transforms.Add(toChild.name, toChild);
                        O7Log.WarnT(TAG, "Creating new child {0} on {1}!", toChild.name, to.name);
                    } else {
                        O7Log.WarnT(TAG, "Re-parenting child {0} on {1}!", toChild.name, to.name);
                    }
                    toChild.parent = to;
                }
                CopyTransform(fromChild, toChild);
                CreateTargetHierarchy(sourceAsset, sourcePath, targetAsset, fromChild, toChild, compressFlags, transforms, rendererInfos);
            }
            // Create renderer
            if (from.GetComponent<Renderer>() != null) {
                CreateRenderer(sourceAsset, sourcePath, targetAsset, from.gameObject, to.gameObject, compressFlags, transforms, rendererInfos);
            } else if (from.GetComponent<Renderer>() == null && to.GetComponent<Renderer>() != null) {
                O7Log.WarnT(TAG, "Deleting renderer on transform {0}!", to.name);
                if (to.GetComponent<Renderer>() != null) {
                    GameObject.DestroyImmediate(to.GetComponent<Renderer>());
                }
                MeshFilter meshFilter = to.GetComponent<MeshFilter>();
                if (meshFilter != null) {
                    GameObject.DestroyImmediate(meshFilter);
                }
            }
        }

        private class GeneralizedComparer<T> : IComparer<T> where T: System.IComparable {
            int IComparer<T>.Compare(T x, T y) {
                int comp = x.CompareTo(y);
                if (comp != 0)
                    return comp;
                else
                    return 1;
            }
        }

        private static bool CreateRenderer(GameObject sourceAsset, string sourcePath, GameObject targetAsset, GameObject gameObjectSource, GameObject gameObjectTarget, CompressFlags compressFlags, Dictionary<string,Transform> transforms, List<RendererInfo> rendererInfos) {
            Renderer renderer = gameObjectSource.GetComponent<Renderer>();
            // Check is it skinned or not
            SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            Mesh mesh = meshFilter != null ? meshFilter.sharedMesh : skinnedMeshRenderer.sharedMesh;
            Mesh asset = null;
            // Get path
            if ((compressFlags & CompressFlags.CompressMesh) > 0) {
                string path = sourcePath;
                string meshName = mesh.name.Replace(":", "");
                path = path.Replace(Path.GetFileName(sourcePath), "");
                path = path.Replace("/Editor/", "/");
                string assetPathAndName = string.Format("{0}{1}_{2}.mesh.asset", path, sourceAsset.name, meshName);
                asset = CopyAndCompressMesh(assetPathAndName, mesh, true);
            } else {
                asset = mesh;
            }
            // Create or update mesh renderer and filter
            if (skinnedMeshRenderer != null) {
                SkinnedMeshRenderer targetSkinnedMeshRenderer = gameObjectTarget.GetComponent<SkinnedMeshRenderer>();
                if (targetSkinnedMeshRenderer == null) {
                    targetSkinnedMeshRenderer = gameObjectTarget.AddComponent<SkinnedMeshRenderer>();
                    targetSkinnedMeshRenderer.sharedMaterial = renderer.sharedMaterial;
                }
                targetSkinnedMeshRenderer.sharedMesh = asset;
                if ((compressFlags & CompressFlags.OverrideMaterial) > 0) {
                    targetSkinnedMeshRenderer.sharedMaterial = renderer.sharedMaterial;
                }
                RendererInfo rendererInfo = new RendererInfo();
                rendererInfo.SourceRenderer = skinnedMeshRenderer;
                rendererInfo.TargetRenderer = targetSkinnedMeshRenderer;
                rendererInfos.Add(rendererInfo);
                return true;
            } else {
                MeshRenderer targetMeshRenderer = gameObjectTarget.GetComponent<MeshRenderer>();
                if (targetMeshRenderer == null) {
                    targetMeshRenderer = gameObjectTarget.AddComponent<MeshRenderer>();
                    targetMeshRenderer.sharedMaterial = renderer.sharedMaterial;
                }
                if ((compressFlags & CompressFlags.OverrideMaterial) > 0) {
                    targetMeshRenderer.sharedMaterial = renderer.sharedMaterial;
                }
                MeshFilter targetMeshFilter = gameObjectTarget.GetComponent<MeshFilter>();
                if (targetMeshFilter == null) {
                    targetMeshFilter = gameObjectTarget.AddComponent<MeshFilter>();
                }
                targetMeshFilter.sharedMesh = asset;
                return false;
            }
        }

        private static void InitializeSkinnedMeshRenderer(RendererInfo rendererInfo, Dictionary<string,Transform> transforms) {
            Transform root;
            if (rendererInfo.SourceRenderer.rootBone != null) {
                transforms.TryGetValue(rendererInfo.SourceRenderer.rootBone.name, out root);
                rendererInfo.TargetRenderer.rootBone = root;
            }
            Transform[] bones = new Transform[rendererInfo.SourceRenderer.bones.Length];
            for (int i = 0; i < rendererInfo.SourceRenderer.bones.Length; i++) {
                transforms.TryGetValue(rendererInfo.SourceRenderer.bones[i].name, out bones[i]);
            }
            rendererInfo.TargetRenderer.bones = bones;
        }

        public static string GetTargetPath(string sourcePath) {            
            string targetPath = Path.ChangeExtension(sourcePath, "prefab");
            targetPath = targetPath.Replace("_MESH", "_CMESH");
            targetPath = targetPath.Replace("_RIG", "_CRIG");
            return targetPath;
        }

        [MenuItem("Assets/Outfit7/Compress Prefab")]
        private static void Compress() {
            Object[] objects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets | SelectionMode.Assets);
            for (int i = 0; i < objects.Length; i++) {
                GameObject sourceAsset = objects[i] as GameObject;
                if (sourceAsset == null) {
                    continue;
                }
                string sourcePath = AssetDatabase.GetAssetPath(sourceAsset);
                Compress(sourceAsset, sourcePath, GetTargetPath(sourcePath), CompressFlags.None);
            }
        }

        [MenuItem("Assets/Outfit7/Refresh Compressed Prefab")]
        private static void Refresh() {
            Object[] objects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets | SelectionMode.Assets);
            for (int i = 0; i < objects.Length; i++) {
                GameObject targetAssset = objects[i] as GameObject;
                if (targetAssset == null) {
                    continue;
                }
                CompressedPrefab compressedPrefab = targetAssset.GetComponent<CompressedPrefab>();
                if (compressedPrefab == null) {
                    O7Log.Warn(TAG, "Compressed prefab not found on object {0}!", targetAssset.name);
                    continue;
                }
                string sourcePath = AssetDatabase.GUIDToAssetPath(compressedPrefab.PrefabReferenceGUID);
                string targetPath = AssetDatabase.GetAssetPath(targetAssset);
#if UNITY_5
                GameObject sourceAsset = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
#else
                GameObject sourceAsset = Resources.LoadAssetAtPath<GameObject>(sourcePath);
#endif
                if (sourceAsset == null) {
                    O7Log.Warn(TAG, "Compressed prefab {0} source asset {1} not found!", targetPath, sourcePath);
                    continue;
                }
                O7Log.Debug("Refreshing compressed prefab {0} from {1}...", targetPath, sourcePath);
                Compress(sourceAsset, sourcePath, targetPath, CompressFlags.None);
            }
        }

        [MenuItem("Assets/Outfit7/Show mesh info")]
        private static void ShowMeshInfo() {
            if (Selection.activeObject == null || (Selection.activeObject as Mesh) == null) {
                O7Log.Warn("Selected object not a Mesh!");
                return;
            }
            Mesh mesh = Selection.activeObject as Mesh;
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++) {
                O7Log.Debug("V#{0} X:{1}, Y:{2}, Z:{3}", i, vertices[i].x, vertices[i].y, vertices[i].z);
            }
            int[] triangles = mesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3) {
                O7Log.Debug("T#{0} {1}, {2}, {3}", i, triangles[i + 0], triangles[i + 1], triangles[i + 2]);
            }
        }

        private static void UpdateCompressedPrefab(GameObject gameObjectTarget, string sourcePath) {
            // Add CompressedPrefab component
            CompressedPrefab compressedPrefab = gameObjectTarget.GetComponent<CompressedPrefab>();
            if (compressedPrefab == null) {
                compressedPrefab = gameObjectTarget.AddComponent<CompressedPrefab>();
            }
            compressedPrefab.PrefabReferenceGUID = AssetDatabase.AssetPathToGUID(sourcePath);
        }

        private static float CompressFloat(float f) {
            return Util.FloatingPointUtils.HalfToFloat(Util.FloatingPointUtils.FloatToHalf(f));
        }

        public static void CopyAndCompressMesh(Mesh sourceMesh, Mesh targetMesh) {
            Vector3[] vertices = sourceMesh.vertices;
            for (int i = 0; i < vertices.Length; i++) {
                vertices[i].x = CompressFloat(vertices[i].x);
                vertices[i].y = CompressFloat(vertices[i].y);
                vertices[i].z = CompressFloat(vertices[i].z);
            }
            targetMesh.vertices = vertices;
            Vector3[] normals = sourceMesh.normals;
            for (int i = 0; i < normals.Length; i++) {
                normals[i].x = CompressFloat(normals[i].x);
                normals[i].y = CompressFloat(normals[i].y);
                normals[i].z = CompressFloat(normals[i].z);
            }
            targetMesh.normals = normals;
            Vector2[] uvs = sourceMesh.uv;
            for (int i = 0; i < uvs.Length; i++) {
                uvs[i].x = CompressFloat(uvs[i].x);
                uvs[i].y = CompressFloat(uvs[i].y);
            }
            targetMesh.uv = uvs;
#if UNITY_5
            Vector2[] uvs1 = sourceMesh.uv2;
#else
            Vector2[] uvs1 = sourceMesh.uv1;
#endif
            for (int i = 0; i < uvs1.Length; i++) {
                uvs1[i].x = CompressFloat(uvs1[i].x);
                uvs1[i].y = CompressFloat(uvs1[i].y);
            }
#if UNITY_5
            targetMesh.uv2 = uvs1;
#else
            targetMesh.uv1 = uvs1;
#endif

            Vector2[] uvs2 = sourceMesh.uv2;
            for (int i = 0; i < uvs2.Length; i++) {
                uvs2[i].x = CompressFloat(uvs2[i].x);
                uvs2[i].y = CompressFloat(uvs2[i].y);
            }
            targetMesh.uv2 = uvs2;
            targetMesh.colors = sourceMesh.colors;
            BoneWeight[] boneWeights = sourceMesh.boneWeights;
            /*
            for (int i=0; i<boneWeights.Length; i++) {
                boneWeights[i].weight0 = CompressFloat(boneWeights[i].weight0);
                boneWeights[i].weight1 = CompressFloat(boneWeights[i].weight1);
                boneWeights[i].weight2 = CompressFloat(boneWeights[i].weight2);
                boneWeights[i].weight3 = CompressFloat(boneWeights[i].weight3);
            }*/
            targetMesh.boneWeights = boneWeights;
            Matrix4x4[] bindPoses = sourceMesh.bindposes;
            for (int i = 0; i < bindPoses.Length; i++) {
                for (int j = 0; j < 16; j++) {
                    bindPoses[i][j] = CompressFloat(bindPoses[i][j]);
                }
            }
            targetMesh.bindposes = bindPoses;
            targetMesh.triangles = sourceMesh.triangles;
        }

        public static Mesh CopyAndCompressMesh(string path, Mesh sourceMesh, bool reupdate) {
            // Load or create asset
            Mesh asset = Object.Instantiate(sourceMesh) as Mesh;
            asset.name = sourceMesh.name;
            // Compress mesh
            CopyAndCompressMesh(sourceMesh, asset);
            AssetDatabase.CreateAsset(asset, path);
            SaveAssets();
            return asset;
        }

        public static void Compress(GameObject sourceAsset, string sourcePath, string targetPath, CompressFlags compressFlags) {
            if (CompressQueue == null) {
                CompressQueue = new Queue<CompressQueueElement>();
                EditorApplication.update -= OnUpdate;
                EditorApplication.update += OnUpdate;
            }
            CompressQueue.Enqueue(new CompressQueueElement(sourcePath, targetPath, compressFlags));
        }

        private static void CompressInternal(GameObject sourceAsset, string sourcePath, string targetPath, CompressFlags compressFlags) {
            if (sourceAsset == null) {
                return;
            }
            O7Log.Debug("Compressing prefab {0} from {1}...", targetPath, sourcePath);
#if UNITY_5
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(targetPath);
#else
            GameObject prefab = Resources.LoadAssetAtPath<GameObject>(targetPath);
#endif
            GameObject gameObjectSource = Object.Instantiate(sourceAsset) as GameObject;
            GameObject gameObjectTarget = null;
            if (prefab == null) {
                gameObjectTarget = new GameObject(sourceAsset.name);
            } else {
                gameObjectTarget = Object.Instantiate(prefab) as GameObject;
                gameObjectTarget.name = sourceAsset.name;
            }
            // Fill search hierarchy
            Dictionary<string,Transform> transforms = new Dictionary<string, Transform>();
            for (int i = 0; i < gameObjectTarget.transform.childCount; i++) {
                FillTargetHierarchy(gameObjectTarget.transform.GetChild(i), transforms);
            }
            CopyTransform(gameObjectSource.transform, gameObjectTarget.transform);
            List<RendererInfo> renderInfos = new List<RendererInfo>();
            CreateTargetHierarchy(sourceAsset, sourcePath, gameObjectTarget, gameObjectSource.transform, gameObjectTarget.transform, compressFlags, transforms, renderInfos);
            // Cleanup hierarchy
            for (int i = 0; i < gameObjectTarget.transform.childCount; i++) {
                CleanUpTargetHierarchy(gameObjectTarget.transform.GetChild(i));
            }
            // Initialize renderers
            for (int i = 0; i < renderInfos.Count; i++) {
                InitializeSkinnedMeshRenderer(renderInfos[i], transforms);
            }
            GameObject.DestroyImmediate(gameObjectSource);
            // Add or remove animation component
            UnityEngine.Animation animation = gameObjectTarget.GetComponent<UnityEngine.Animation>();
            if (renderInfos.Count > 0) {
                // Create animation component
                if (animation == null) {
                    gameObjectTarget.AddComponent<UnityEngine.Animation>();
                    O7Log.WarnT(TAG, "Creating animation component on {0}", gameObjectTarget.name);
                }
            } else if (animation != null) {
                GameObject.DestroyImmediate(animation);
            }
            // Save the prefab
            if (prefab == null) {
                prefab = PrefabUtility.CreatePrefab(targetPath, gameObjectTarget);
                UpdateCompressedPrefab(prefab, sourcePath);
                EditorUtility.SetDirty(prefab);
            } else {
                UpdateCompressedPrefab(gameObjectTarget, sourcePath);
                PrefabUtility.ReplacePrefab(gameObjectTarget, prefab, ReplacePrefabOptions.Default);
            }
            SaveAssets();
            GameObject.DestroyImmediate(gameObjectTarget);
        }

        private static void SaveAssets() {
            if (!EditorApplication.isUpdating) {
                O7Log.Warn("Saving assets!");
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                AssetDatabase.SaveAssets();
            } else {
                O7Log.Warn("Updating - Saving assets skipped!");
            }
        }

    }
}
