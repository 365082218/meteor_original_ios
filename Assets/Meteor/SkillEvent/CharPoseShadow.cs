//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class CharPoseShadow : MonoBehaviour {

//    float freq = 0.2f;//生成残影间隔设定
//    float genfreq = 0.0f;//产生残影的间隔计数
//    float shadowLast = 0.8f;//阴影持续时间
//    float Reductionfreq = 0.02f;//修改透明度的间隔
//    float distanceLimit = 5.0f;//超越8米距离产生阴影
//    float alphaInitialize = 0.6f;
//    Color colorMul = new Color(0, 0/ 255.0f, 0/255.0f);//颜色叠加
//    //specialItem层
//    Vector3 pos;
//    Quaternion quat;
//    SortedDictionary<GameObject, ShadowModel> copyedModel = new SortedDictionary<GameObject, ShadowModel>();
//    public class ShadowModel
//    {
//        public float alpha;//只控制透明度
//        public float shadowLast;//每个阴影倒计时
//    }
//	// Use this for initialization
//	void Start () {
//        StartCoroutine(Reduction());
//        pos = transform.position;
//        quat = transform.rotation;
//    }

//    bool stop = false;
//	// Update is called once per frame
//	void Update () {

//        if (!stop)
//        {
//            genfreq -= FrameReplay.deltaTime;
//            if (genfreq <= 0.0f || distanceLimit < Vector3.Distance(pos, transform.position))
//            {
//                CopyModel();
//                pos = transform.position;
//                quat = transform.rotation;
//                genfreq = freq;
//            }
//        }

        
//    }

//    List<GameObject> keys = new List<GameObject>();
//    IEnumerator Reduction()
//    {
//        while (true)
//        {
//            keys.Clear();
//            foreach (var each in copyedModel)
//            {
//                each.Value.shadowLast -= FrameReplay.deltaTime;
//                each.Value.alpha = Mathf.Lerp(0.0f, alphaInitialize, each.Value.shadowLast / shadowLast);
//                if (each.Value.shadowLast <= 0.0f)
//                    keys.Add(each.Key);
//            }

//            for (int i = 0; i < keys.Count; i++)
//            {
//                copyedModel.Remove(keys[i]);
//                GameObject.DestroyImmediate(keys[i]);
//            }

//            foreach (var each in copyedModel)
//            {
//                MeshRenderer[] mr = each.Key.GetComponentsInChildren<MeshRenderer>();
//                for (int j = 0; j < mr.Length; j++)
//                {
//                    for (int i = 0; i < mr[j].materials.Length; i++)
//                    {
//                        mr[j].materials[i].SetFloat("_Alpha", each.Value.alpha);
//                        mr[j].materials[i].SetColor("_TintColor", colorMul);
//                    }
//                }
//            }

//            if (copyedModel.Count == 0 && stop)
//                DestroyImmediate(this);
//            yield return new WaitForSeconds(Reductionfreq);
//        }
//    }

//    public class ShadowInfo
//    {
//        public Material[] mat;
//        public Transform attach;
//        public bool normalMesh;//坐标直接设置世界坐标，动画的直接给0
//    }
//    void CopyModel()
//    {
//        GameObject objShadow = new GameObject();
//        objShadow.transform.position = pos;
//        objShadow.transform.rotation = quat;
//        objShadow.transform.SetParent(null);
//        ShadowModel shadowModel = new ShadowModel();
//        shadowModel.alpha = 1;
//        SkinnedMeshRenderer[] msrChild = GetComponentsInChildren<SkinnedMeshRenderer>();
//        MeshRenderer[] mrChild = GetComponentsInChildren<MeshRenderer>();
//        SortedDictionary<Mesh, ShadowInfo> ShadowMesh = new SortedDictionary<Mesh, ShadowInfo>();//静态的mesh 武器挂载点要设置坐标
//        //objShadow.AddComponent<MeshRenderer>();
//        for (int i = 0; i < msrChild.Length; i++)
//        {
//            if (!msrChild[i].enabled)
//                continue;
//            Mesh ms = new Mesh();
//            msrChild[i].BakeMesh(ms);
//            ShadowInfo info = new ShadowInfo();
//            List<Material> mat = new List<Material>();
//            for (int j = 0; j < msrChild[i].materials.Length; j++)
//            {
//                mat.Add(Instantiate(msrChild[i].materials[j]));
//            }
//            info.mat = mat.ToArray();
//            info.attach = msrChild[i].transform;
//            info.normalMesh = false;
//            ShadowMesh.Add(ms, info);
//        }

//        for (int i = 0; i < mrChild.Length; i++)
//        {
//            if (!mrChild[i].enabled)
//                continue;
//            Mesh ms = mrChild[i].GetComponent<MeshFilter>().mesh;
//            ShadowInfo info = new ShadowInfo();
//            List<Material> mat = new List<Material>();
//            for (int j = 0; j < mrChild[i].materials.Length; j++)
//            {
//                mat.Add(Instantiate(mrChild[i].materials[j]));
//            }
//            info.mat = mat.ToArray();
//            info.attach = mrChild[i].transform;
//            info.normalMesh = true;
//            ShadowMesh.Add(ms, info);
//        }

        
        
//        foreach (var each in ShadowMesh)
//        {
//            GameObject subMesh = new GameObject();
//            subMesh.name = each.Value.attach.name;
//            MeshFilter mf = subMesh.AddComponent<MeshFilter>();
//            mf.mesh = each.Key;
//            MeshRenderer mr = subMesh.AddComponent<MeshRenderer>();
//            mr.materials = each.Value.mat;
//            subMesh.transform.SetParent(objShadow.transform);

//            if (each.Value.normalMesh)
//            {
//                subMesh.transform.rotation = each.Value.attach.transform.rotation;
//                subMesh.transform.position = each.Value.attach.transform.position;
//            }
//            else
//            {
//                subMesh.transform.localPosition = Vector3.zero;
//                subMesh.transform.localRotation = Quaternion.identity;
//            }

//            for (int i = 0; i < mr.materials.Length; i++)
//            {
//                mr.materials[i].SetFloat("_Alpha", alphaInitialize);
//                mr.materials[i].SetColor("_TintColor", colorMul);
//            }
//            //mr.material.SetFloat("_Alpha", shadowModel.alpha);
//        }
//        shadowModel.shadowLast = shadowLast;
//        copyedModel.Add(objShadow, shadowModel);
//    }

//    public void StopAndAutoDelete()
//    {
//        stop = true;
//    }

//}
