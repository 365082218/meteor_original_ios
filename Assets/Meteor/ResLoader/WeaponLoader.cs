using UnityEngine;
using System.Collections;

using System.Linq;
using System.Collections.Generic;
using Excel2Json;
//using UnityEditor;
//武器单双手-各自内部还有子骨骼，要把左右手东西都加载对就OK
public class WeaponLoader : MonoBehaviour {
    List<WeaponTrail> trail = new List<WeaponTrail>();
    InventoryItem Weapon;
    public InventoryItem GetCurrentWeapon() { return Weapon; }
    MeteorUnit owner;
    Transform LP;//武器父-除非角色整个换了，否则不会删除。武器挂载点
    Transform RP;//武器父
    Transform L;//武器
    Transform R;//武器

    GameObject d_wpnRS;
    public GameObject GetGunTrans()
    {
        if (d_wpnRS != null)
            return d_wpnRS;
        d_wpnRS = NodeHelper.Find("d_wpnRS", R.gameObject);
        if (d_wpnRS != null)
            return d_wpnRS;
        d_wpnRS = NodeHelper.Find("D_wpnRS", R.gameObject);
        return d_wpnRS;
    }

    public string StrWeaponR;
    public string StrWeaponL;

    //主要是用来隐藏飞轮
    public void HideWeapon()
    {
        if (RP != null)
            RP.gameObject.SetActive(false);
    }

    public void ShowWeapon()
    {
        if (RP != null)
            RP.gameObject.SetActive(true);
    }

    public void RemoveTrail()
    {
        for (int i = 0; i < trail.Count; i++)
            trail[i].enabled = false;
        trail.Clear();
    }

    public List<BoxCollider> weaponDamage = new List<BoxCollider>();
    void InitHitBox() {
        for (int i = 0; i < weaponDamage.Count; i++) {
            FightBox fb = weaponDamage[i].gameObject.AddComponent<FightBox>();
            fb.Init(owner, null);
            //角色有刚体，可以由角色驱动.
        }
    }
    public int WeaponType()
    {
        return Weapon == null ? -1 : Weapon.Info().SubType;
    }
    //乾坤刀状态,0默认，1POSA 2POSB
    int PoseType = 0;
    public int WeaponSubType()
    {
        return PoseType; 
    }

    //无设置父对象时，使用其上设置的2个骨骼挂点作为武器挂载点.
    [SerializeField] private Transform LParent = null;
    [SerializeField] private Transform RParent = null;
    public void Init()
    {
        LP = new GameObject().transform;
        LP.localPosition = Vector3.zero;
        LP.localScale = Vector3.one;
        LP.rotation = Quaternion.identity;
        LP.SetParent(LParent);
        LP.localPosition = Vector3.zero;
        LP.localScale = Vector3.one;
        LP.name = "WeaponL_TPos";
        RP = new GameObject().transform;
        RP.localPosition = Vector3.zero;
        RP.localScale = Vector3.one;
        RP.rotation = Quaternion.identity;
        RP.SetParent(RParent);
        RP.localPosition = Vector3.zero;
        RP.localScale = Vector3.one;
        RP.name = "WeaponR_TPos";
        LP.gameObject.layer = LParent.gameObject.layer;
        RP.gameObject.layer = RParent.gameObject.layer;

        transform.localPosition = new Vector3(0, 0, 110);
        transform.localEulerAngles = new Vector3(90, 0, 0);
        transform.localScale = new Vector3(10, 10, 10);
        GetComponent<RectTransform>().sizeDelta = Vector3.zero;
        //LParent.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 20);
        //RParent.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 20);
    }

    public void Init(MeteorUnit unit)
    {
        owner = unit;
        //在TPOS放一个物体到武器点，以后所有武器点都放此点下

        LP = new GameObject().transform;
        LP.localPosition = Vector3.zero;
        LP.localScale = Vector3.one;
        LP.rotation = Quaternion.identity;
        LP.SetParent(owner.WeaponL);
        LP.localPosition = Vector3.zero;
        LP.localScale = Vector3.one;
        LP.name = "WeaponL_TPos";
        RP = new GameObject().transform;
        RP.localPosition = Vector3.zero;
        RP.localScale = Vector3.one;
        RP.rotation = Quaternion.identity;
        RP.SetParent(owner.WeaponR);
        RP.localPosition = Vector3.zero;
        RP.localScale = Vector3.one;
        RP.name = "WeaponR_TPos";
        LP.gameObject.layer = unit.gameObject.layer;
        RP.gameObject.layer = unit.gameObject.layer;
    }


    public void UnEquipWeapon()
    {
        if (Weapon != null)
        {
            if (L != null)
            {
                Destroy(L.gameObject);
                L = null;
            }
            if (R != null)
            {
                Destroy(R.gameObject);
                R = null;
            }
            Weapon = null;
            RemoveTrail();
            weaponDamage.Clear();
        }
    }

    //后者决定是否真正切换到这个武器对应的POSE里，
    //乾坤刀部分姿态可切换，在切换未完成时
    public void ChangeWeaponPos(WeaponPos pose)
    {
        InventoryItem curWeapon = Weapon;
        if (Weapon != null)
        {
            curWeapon = Weapon;
            Weapon.WeaponPos = (int)pose;
            UnEquipWeapon();
            EquipWeapon(curWeapon);
        }

    }

    //要判断是双手还是单手的
    InventoryItem weaponModel;
    public void EquipWeapon(int unitid, bool ui = true)
    {
        InventoryItem i = ui ? weaponModel : Weapon;
        if (i == null)
        {
            i = ui ? weaponModel = GameStateMgr.Ins.MakeEquip(unitid):GameStateMgr.Ins.MakeEquip(unitid);
            //WeaponBase weaponProperty = U3D.GetWeaponProperty(i.Info().UnitId);
            EquipWeapon(i, true);
        }
        else
        {
            UnEquipWeapon();
            i = ui ? weaponModel = GameStateMgr.Ins.MakeEquip(unitid):GameStateMgr.Ins.MakeEquip(unitid);
            //WeaponBase weaponProperty = U3D.GetWeaponProperty(i.Info().UnitId);
            EquipWeapon(i, true);
        }

        if (ui)
        {
            if (i.Info().SubType == (int)(EquipWeaponType.Lance))
                transform.localPosition = new Vector3(0, 9, 130);
            else
                transform.localPosition = new Vector3(0, 9, 110);
        }
        if (RParent != null)
            Utility.SetObjectLayer(RParent.gameObject, LayerManager.RenderModel);
        if (LParent != null)
            Utility.SetObjectLayer(LParent.gameObject, LayerManager.RenderModel);
    }

    //把背包里的物品，装备到身上.
    public void EquipWeapon(InventoryItem item, bool ModelLayer = false)
    {
        if (Weapon == null && item != null)
        {
            if (item.Info().MainType == (int)UnitType.Weapon)
            {
                float scale = 1.0f;
                WeaponData weaponProperty = U3D.GetWeaponProperty(item.Info().UnitId);
                

                string weaponL = "";
                string weaponR = "";
                PoseType = item.WeaponPos;
                if (item.WeaponPos == 0)
                {
                    weaponL = weaponProperty.WeaponL;
                    weaponR = weaponProperty.WeaponR;
                }
                else if (item.WeaponPos == 1)
                {
                    weaponL = weaponProperty.PosAWL;
                    weaponR = weaponProperty.PosAWR;
                }
                else if (item.WeaponPos == 2)
                {
                    weaponL = weaponProperty.PosBWL;
                    weaponR = weaponProperty.PosBWR;
                }

                //Debug.Log("加载武器:" + " 左手:" + weaponL + "右手:" + weaponR);
                if (!string.IsNullOrEmpty(weaponL))
                {
                    GameObject weaponPrefab = Resources.Load<GameObject>(weaponL);
                    if (weaponPrefab == null)
                    {
                        GMCFile fGmcL = GMCLoader.Ins.Load(weaponL);
                        DesFile fDesL = DesLoader.Ins.Load(weaponL);

                        if (fGmcL != null && fDesL != null)
                            GenerateWeaponModel(weaponL, fGmcL, fDesL, false, scale, weaponProperty.TextureL, ModelLayer);
                        else if (fGmcL == null && fDesL != null)
                        {
                            GMBFile fGmbL = GMBLoader.Ins.Load(weaponL);
                            GenerateWeaponModel(weaponL, fGmbL, fDesL, false, scale, weaponProperty.TextureL, ModelLayer);
                        }
                    }
                    else
                    {
                        if (L != null)
                            DestroyImmediate(L);
                        GameObject objWeapon = GameObject.Instantiate(weaponPrefab);
                        objWeapon.layer = ModelLayer ? LayerManager.RenderModel : LayerManager.Weapon ;
                        L = objWeapon.transform;
                        //L = new GameObject().transform;
                        L.SetParent(LP);
                        L.localPosition = Vector3.zero;
                        //这种导入来的模型，需要Y轴旋转180，与原系统的物件坐标系存在一些问题
                        L.localRotation = new Quaternion(0, 1, 0, 0);
                        L.name = weaponL;
                        L.localScale = Vector3.one;
                        //WE = L;
                        //武器挂点必须在缩放正确后才能到指定的位置
                        WeaponTrail wt = L.gameObject.AddComponent<WeaponTrail>();
                        //右手
                        //GameObject ctrlRs = Global.ldaControlX("d_wpnRS", L.gameObject);
                        //if (ctrlRs != null)
                        //    wt.AddTransform(ctrlRs.transform);
                        //GameObject ctrlRe = Global.ldaControlX("d_wpnRE", L.gameObject);
                        //if (ctrlRe != null)
                        //    wt.AddTransform(ctrlRe.transform);
                        //左手
                        GameObject ctrlLs = NodeHelper.Find("d_wpnLS", L.gameObject);
                        if (ctrlLs != null)
                            wt.AddTransform(ctrlLs.transform);
                        GameObject ctrlLe = NodeHelper.Find("d_wpnLE", L.gameObject);
                        if (ctrlLe != null)
                            wt.AddTransform(ctrlLe.transform);

                        //每个武器只能有一个碰撞盒
                        BoxCollider box = L.GetComponentInChildren<BoxCollider>();
                        if (box != null)
                        {
                            box.enabled = false;
                            box.isTrigger = true;
                            box.gameObject.layer = ModelLayer ? LayerManager.RenderModel : LayerManager.Weapon;
                            weaponDamage.Add(box);
                        }
                        else
                        {
                            Debug.LogError("新增武器上找不到碰撞盒[除了暗器火枪飞轮外都应该有碰撞盒]");
                        }
                        if (owner != null)
                        {
                            wt.Init(owner);
                            trail.Add(wt);

                        }
                        else
                        {
                            GameObject.Destroy(wt);
                            wt = null;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(weaponR))
                {
                    GameObject weaponPrefab = Resources.Load<GameObject>(weaponR);
                    if (weaponPrefab == null)
                    {
                        GMCFile fGmcR = GMCLoader.Ins.Load(weaponR);
                        DesFile fDesR = DesLoader.Ins.Load(weaponR);
                        if (fGmcR != null && fDesR != null)
                            GenerateWeaponModel(weaponR, fGmcR, fDesR, true, scale, weaponProperty.TextureR, ModelLayer);
                        else if (fGmcR == null && fDesR != null)
                        {
                            GMBFile fGmbR = GMBLoader.Ins.Load(weaponR);
                            GenerateWeaponModel(weaponR, fGmbR, fDesR, true, scale, weaponProperty.TextureR, ModelLayer);
                        }
                    }
                    else
                    {
                        
                        if (R != null)
                            DestroyImmediate(R);
                        GameObject objWeapon = GameObject.Instantiate(weaponPrefab);
                        objWeapon.layer = ModelLayer ? LayerManager.RenderModel : LayerManager.Weapon;
                        R = objWeapon.transform;
                        R.SetParent(RP);
                        R.localPosition = Vector3.zero;
                        R.localRotation = new Quaternion(0, 1, 0, 0);
                        R.name = weaponR;
                        R.localScale = Vector3.one;

                        //武器挂点必须在缩放正确后才能到指定的位置
                        WeaponTrail wt = R.gameObject.AddComponent<WeaponTrail>();
                        //右手
                        GameObject ctrlRs = NodeHelper.Find("d_wpnRS", R.gameObject);
                        if (ctrlRs != null)
                            wt.AddTransform(ctrlRs.transform);
                        GameObject ctrlRe = NodeHelper.Find("d_wpnRE", R.gameObject);
                        if (ctrlRe != null)
                            wt.AddTransform(ctrlRe.transform);
                        BoxCollider box = R.GetComponentInChildren<BoxCollider>();
                        if (box != null)
                        {
                            box.enabled = false;
                            box.isTrigger = true;
                            box.gameObject.layer = ModelLayer ? LayerManager.RenderModel : LayerManager.Weapon;
                            weaponDamage.Add(box);
                        }
                        else
                        {
                            Debug.LogError("新增武器上找不到碰撞盒[除了暗器火枪飞轮外都应该有碰撞盒]");
                        }

                        if (owner != null)
                        {
                            wt.Init(owner);
                            trail.Add(wt);
                        }
                        else
                        {
                            GameObject.Destroy(wt);
                        }
                    }
                }


            }
            Weapon = item;
        }
        else
        {
            Debug.LogError("if you want equip you must call unequip first if the weapon exist");
        }

        InitHitBox();
        if (owner != null)
        {
            MeshRenderer[] mrl = LP.gameObject.GetComponentsInChildren<MeshRenderer>();
            MeshRenderer[] mrr = RP.gameObject.GetComponentsInChildren<MeshRenderer>();
            //if (owner.Attr.IsPlayer) {
            //    for (int i = 0; i < mrl.Length; i++) {
            //        for (int j = 0; j < mrl[i].materials.Length; j++) {
            //            mrl[i].materials[j].shader = Shader.Find("MainPlayer");
            //        }
            //    }
            //    for (int i = 0; i < mrr.Length; i++) {
            //        for (int j = 0; j < mrr[i].materials.Length; j++) {
            //            mrr[i].materials[j].shader = Shader.Find("MainPlayer");
            //        }
            //    }
            //}
            
            for (int i = 0; i < mrl.Length; i++)
            {
                if (!mrl[i].enabled)
                    continue;
                for (int j = 0; j < mrl[i].materials.Length; j++)
                    mrl[i].materials[j].SetFloat("_Alpha", owner.Stealth ? 0.2f : 1.0f);
            }

            
            for (int i = 0; i < mrr.Length; i++)
            {
                if (!mrr[i].enabled)
                    continue;
                for (int j = 0; j < mrr[i].materials.Length; j++)
                    mrr[i].materials[j].SetFloat("_Alpha", owner.Stealth ? 0.2f : 1.0f);
            }
        }
    }

    //通过描述文件生成实际文件.描述文件类似使用一系列模板一样，指定
    //装备要发武器姿态来。否则不知道预先加载哪个材质，有些装备需要先生成材质后调整的
    public void GenerateWeaponModel(string DesFile, GMCFile fModel, DesFile fIns, bool rightHand = true, float scale = 1.0f, string textureOverrite = "", bool ModelLayer = false)
    {
        if (string.IsNullOrEmpty(DesFile) || fModel == null || fIns == null)
            return;
        string matIden = DesFile;
        Transform WR = null;
        if (rightHand)
        {
            if (R != null)
                DestroyImmediate(R);
            R = new GameObject().transform;
            R.gameObject.layer = ModelLayer ? LayerManager.RenderModel : LayerManager.Weapon;
            R.SetParent(RP);
            R.localRotation = Quaternion.identity;
            R.localPosition = Vector3.zero;
            R.localScale = Vector3.one;
            R.name = matIden;
            WR = R;
        }
        else
        {
            if (L != null)
                DestroyImmediate(L);
            L = new GameObject().transform;
            L.SetParent(LP);
            L.gameObject.layer = ModelLayer ? LayerManager.RenderModel : LayerManager.Weapon;
            L.localRotation = Quaternion.identity;
            L.localPosition = Vector3.zero;
            L.name = matIden;
            L.localScale = Vector3.one;
            WR = L;
        }
        //武器挂点必须在缩放正确后才能到指定的位置
        WR.localScale = Vector3.one;
        Material[] mat = new Material[fModel.TexturesCount];
        SortedDictionary<int, string> iflMaterials = new SortedDictionary<int, string>();
        for (int x = 0; x < fModel.TexturesCount; x++)
        {
            mat[x] = null;
            string tex = "";
            if (!string.IsNullOrEmpty(textureOverrite))
                tex = textureOverrite;
            else
                tex = fModel.TexturesNames[x];
            //若不是以ifl结尾的材质，那么要分割文件后缀，若是ifl类型的，则直接是xx.ifl.bytes类型的数据，是OK的
            bool iflMaterial = true;
            if (!tex.ToLower().EndsWith(".ifl"))
            {
                iflMaterial = false;
                int del = tex.LastIndexOf('.');
                if (del != -1)
                    tex = tex.Substring(0, del);
            }

            if (iflMaterial)
            {
                if (!iflMaterials.ContainsKey(x))
                    iflMaterials.Add(x, tex);//记录x号材质是ifl类型的材质,以后会用到，这个序号的材质并且动态更换这个材质的贴图的
                
            }
            else
            {

                string weaponIden = string.Format("{0}_{1:D2}{2}", matIden, x, textureOverrite);
                mat[x] = Resources.Load<Material>(weaponIden);
                if (mat[x] == null)
                {
                    Texture texture = Resources.Load<Texture>(tex);
                    if (texture == null)
                        Debug.LogError("texture miss on load gmb:" + tex + " texture name:" + tex);
                    mat[x] = new Material(ShaderMng.Find("AlphaTexture"));
                    mat[x].SetTexture("_MainTex", texture);
                    mat[x].name = weaponIden;
                    //if (!System.IO.Directory.Exists("Assets/Materials/" + "Weapons" + "/resources/"))
                    //    System.IO.Directory.CreateDirectory("Assets/Materials/" + "Weapons" + "/resources/");
                    //AssetDatabase.CreateAsset(mat[x], "Assets/Materials/" + "Weapons" + "/resources/" + mat[x].name + ".mat");
                    //AssetDatabase.Refresh();
                }
            }
        }
        for (int i = 0; i < fIns.SceneItems.Count; i++)
        {
            GameObject objMesh = new GameObject();
            bool addIflComponent = false;
            int iflParam = -1;
            int iflMatIndex = 0;
            objMesh.name = fIns.SceneItems[i].name;
            objMesh.transform.localRotation = Quaternion.identity;
            objMesh.transform.localPosition = Vector3.zero;
            objMesh.transform.localScale = Vector3.one;
            bool realObject = false;//是不是正常物体，虚拟体无需设置材质球之类
            if (i < fModel.SceneObjectsCount)
            {
            //for (int j = 0; j < fModel.SceneObjectsCount; j++)
            //{
                //if (fModel.mesh[j].name == objMesh.name)
                {
                    realObject = true;
                    Mesh w = new Mesh();
                    //前者子网格编号，后者 索引缓冲区
                    SortedDictionary<int, List<int>> tr = new SortedDictionary<int, List<int>>();
                    List<Vector3> ve = new List<Vector3>();
                    List<Vector2> uv = new List<Vector2>();
                    List<Vector3> nor = new List<Vector3>();
                    List<Color> col = new List<Color>();
                    for (int k = 0; k < fModel.mesh[i].faces.Count; k++)
                    {
                        int key = fModel.mesh[i].faces[k].material;
                        if (tr.ContainsKey(key))
                        {
                            tr[key].Add(fModel.mesh[i].faces[k].triangle[0]);
                            tr[key].Add(fModel.mesh[i].faces[k].triangle[1]);
                            tr[key].Add(fModel.mesh[i].faces[k].triangle[2]);
                        }
                        else
                        {
                            tr.Add(key, new List<int>());
                            tr[key].Add(fModel.mesh[i].faces[k].triangle[0]);
                            tr[key].Add(fModel.mesh[i].faces[k].triangle[1]);
                            tr[key].Add(fModel.mesh[i].faces[k].triangle[2]);
                        }

                    }
                    for (int k = 0; k < fModel.mesh[i].vertices.Count; k++)
                    {
                        ve.Add(fModel.mesh[i].vertices[k].pos);
                        uv.Add(fModel.mesh[i].vertices[k].uv);
                        col.Add(fModel.mesh[i].vertices[k].color);
                        nor.Add(fModel.mesh[i].vertices[k].normal);
                    }
                    w.SetVertices(ve);
                    w.uv = uv.ToArray();
                    w.subMeshCount = tr.Count;
                    int ss = 0;
                    List<Material> targetMat = new List<Material>();
                    foreach (var each in tr)
                    {
                        w.SetIndices(each.Value.ToArray(), MeshTopology.Triangles, ss++);
                        if (each.Key >= 0 && each.Key < fModel.shader.Length)
                        {
                            int materialIndex = fModel.shader[each.Key].TextureArg0;
                            if (materialIndex >= 0 && materialIndex < mat.Length)
                            {
                                if (mat[materialIndex] == null)
                                {
                                    targetMat.Add(new Material(Shader.Find("Unlit/Transparent")));
                                    addIflComponent = true;
                                    iflParam = materialIndex;
                                    iflMatIndex = targetMat.Count - 1;
                                }
                                else
                                    targetMat.Add(mat[materialIndex]);
                            }
                            else
                            {
                                //占位材质，代表原文件里用到了序号<0的材质(即空材质)，这里使用默认材质代替,一般武器加载不会触发这里，但是有极个别情况
                                Material defaults = new Material(Shader.Find("Unlit/Texture"));
                                defaults.name = string.Format("{0}_{1:D2}", objMesh.name, materialIndex);
                                targetMat.Add(defaults);
                            }
                        }
                    }
                    MeshRenderer mr = objMesh.AddComponent<MeshRenderer>();
                    MeshFilter mf = objMesh.AddComponent<MeshFilter>();
                    mf.mesh = w;
                    mf.mesh.colors = col.ToArray() ;
                    mf.mesh.normals = nor.ToArray();
                    mf.mesh.RecalculateBounds();
                    mf.mesh.RecalculateNormals();

                    mr.materials = targetMat.ToArray();
                    string vis = "";
                    if (fIns.SceneItems[i].ContainsKey("visible", out vis))
                    {
                        if (vis == "0")
                        {
                            mr.enabled = false;
                            BoxCollider box = mr.gameObject.AddComponent<BoxCollider>();
                            objMesh.tag = "weapon";
                            box.enabled = false;
                            box.isTrigger = true;
                            weaponDamage.Add(box);
                        }
                    }
                    else
                    if (fIns.SceneItems[i].name.EndsWith("Box"))
                    {
                        BoxCollider box = mr.gameObject.AddComponent<BoxCollider>();
                        box.enabled = false;
                        box.isTrigger = true;
                        weaponDamage.Add(box);
                    }
                }
            }
            
            objMesh.transform.SetParent(WR);
            objMesh.layer = WR.gameObject.layer;
            if (addIflComponent && iflMaterials.ContainsKey(iflParam))
            {
                IFLLoader iflL = objMesh.AddComponent<IFLLoader>();
                iflL.fileNameReadOnly = iflMaterials[iflParam];
                iflL.IFLFile = Resources.Load<TextAsset>(iflMaterials[iflParam]);
                iflL.matIndex = iflMatIndex;
                iflL.LoadIFL();
            }
            //读一个gmb或者gmc内部的模型实体都不需要位置和旋转，因为都是在自身坐标系设置的坐标
            //只有虚拟体，可能在gmb或者gmc内没有这个东西，这个时候才需要设置位置和旋转，地图加载那块也是
            if (!realObject)
            {
                objMesh.transform.localRotation = fIns.SceneItems[i].quat;
                objMesh.transform.localPosition = fIns.SceneItems[i].pos;
            }
            else
            {
                objMesh.transform.localRotation = Quaternion.identity;
                objMesh.transform.localScale = Vector3.one;
                objMesh.transform.localPosition = Vector3.zero;
            }
        }


        WR.localScale = Vector3.one * scale;
        //WR.localRotation = new Quaternion(0.7071f, 0, 0, -0.7071f);//挂右手试试不旋转会怎样
        WeaponTrail wt = WR.gameObject.AddComponent<WeaponTrail>();
        //右手
        if (rightHand)
        {
            GameObject ctrlRs = NodeHelper.Find("d_wpnRS", WR.gameObject);
            if (ctrlRs != null)
                wt.AddTransform(ctrlRs.transform);
            GameObject ctrlRe = NodeHelper.Find("d_wpnRE", WR.gameObject);
            if (ctrlRe != null)
                wt.AddTransform(ctrlRe.transform);
        }
        else
        {
            //左手
            GameObject ctrlLs = NodeHelper.Find("d_wpnLS", WR.gameObject);
            if (ctrlLs != null)
                wt.AddTransform(ctrlLs.transform);
            GameObject ctrlLe = NodeHelper.Find("d_wpnLE", WR.gameObject);
            if (ctrlLe != null)
                wt.AddTransform(ctrlLe.transform);
        }
        if (owner != null)
        {
            wt.Init(owner);
            trail.Add(wt);
        }
        else
        {
            GameObject.Destroy(wt);
        }
    }

    //GMB文件合并了网格，应该是不用设置位置和旋转了
    public void GenerateWeaponModel(string DesFile, GMBFile fModel, DesFile fIns, bool rightHand = true, float scale = 1.0f, string textureOverrite = "", bool ModelLayer = false)
    {
        if (string.IsNullOrEmpty(DesFile) || fModel == null || fIns == null)
            return;
        string matIden = DesFile;
        Transform WR = null;
        if (rightHand)
        {
            if (R != null)
                DestroyImmediate(R);
            R = new GameObject().transform;
            R.gameObject.layer = ModelLayer ? LayerManager.RenderModel : LayerManager.Weapon;
            R.SetParent(RP);
            R.localRotation = Quaternion.identity;
            R.localPosition = Vector3.zero;
            R.localScale = Vector3.one;
            R.name = matIden;
            WR = R;
        }
        else
        {
            if (L != null)
                DestroyImmediate(L);
            L = new GameObject().transform;
            L.gameObject.layer = ModelLayer ? LayerMask.NameToLayer("RenderModel") : LayerMask.NameToLayer("Weapon");
            L.SetParent(LP);
            L.localRotation = Quaternion.identity;
            L.localPosition = Vector3.zero;
            L.name = matIden;
            L.localScale = Vector3.one;
            WR = L;
        }
        //武器挂点必须在缩放正确后才能到指定的位置
        WR.localScale = Vector3.one;
        Material[] mat = new Material[fModel.TexturesCount];
        SortedDictionary<int, string> iflMaterials = new SortedDictionary<int, string>();
        for (int x = 0; x < fModel.TexturesCount; x++)
        {
            mat[x] = null;
            string tex = "";
            if (!string.IsNullOrEmpty(textureOverrite))
                tex = textureOverrite;
            else
                tex = fModel.TexturesNames[x];
            //若不是以ifl结尾的材质，那么要分割文件后缀，若是ifl类型的，则直接是xx.ifl.bytes类型的数据，是OK的
            bool iflMaterial = true;
            if (!tex.ToLower().EndsWith(".ifl"))
            {
                iflMaterial = false;
                int del = tex.LastIndexOf('.');
                if (del != -1)
                    tex = tex.Substring(0, del);
            }

            if (iflMaterial)
            {
                if (!iflMaterials.ContainsKey(x))
                    iflMaterials.Add(x, tex);//记录x号材质是ifl类型的材质,以后会用到，这个序号的材质并且动态更换这个材质的贴图的

            }
            else
            {
                string weaponIden = string.Format("{0}_{1:D2}{2}", matIden, x, textureOverrite);//还要考虑重载贴图也要生成对应的新材质
                mat[x] = Resources.Load<Material>(weaponIden);
                if (mat[x] == null)
                {
                    Texture texture = Resources.Load<Texture>(tex);
                    if (texture == null)
                        Debug.LogError("texture miss on load gmb:" + tex + " texture name:" + tex);
                    mat[x] = new Material(ShaderMng.Find("AlphaTexture"));
                    mat[x].SetTexture("_MainTex", texture);
                    mat[x].name = weaponIden;
                    //if (!System.IO.Directory.Exists("Assets/Materials/" + "Weapons" + "/resources/"))
                    //    System.IO.Directory.CreateDirectory("Assets/Materials/" + "Weapons" + "/resources/");
                    //AssetDatabase.CreateAsset(mat[x], "Assets/Materials/" + "Weapons" + "/resources/" + mat[x].name + ".mat");
                    //AssetDatabase.Refresh();
                }
            }
        }

        for (int i = 0; i < fIns.SceneItems.Count; i++)
        {
            GameObject objMesh = new GameObject();
            bool addIflComponent = false;
            int iflParam = -1;
            int iflMatIndex = 0;
            objMesh.name = fIns.SceneItems[i].name;
            objMesh.transform.localRotation = Quaternion.identity;
            objMesh.transform.localPosition = Vector3.zero;
            objMesh.transform.localScale = Vector3.one;
            bool realObject = false;//是不是正常物体，虚拟体无需设置材质球之类
            if (i < fModel.SceneObjectsCount)
            {
            //for (int j = 0; j < fModel.SceneObjectsCount; j++)
            //{
                //if (fModel.mesh[j].name == objMesh.name)
                {
                    realObject = true;
                    Mesh w = new Mesh();
                    //前者子网格编号，后者 索引缓冲区
                    SortedDictionary<int, List<int>> tr = new SortedDictionary<int, List<int>>();
                    List<Vector3> ve = new List<Vector3>();
                    List<Vector2> uv = new List<Vector2>();
                    List<Vector3> nor = new List<Vector3>();
                    List<Color> col = new List<Color>();
                    for (int k = 0; k < fModel.mesh[i].faces.Count; k++)
                    {
                        int key = fModel.mesh[i].faces[k].material;
                        if (tr.ContainsKey(key))
                        {
                            tr[key].Add(fModel.mesh[i].faces[k].triangle[0]);
                            tr[key].Add(fModel.mesh[i].faces[k].triangle[1]);
                            tr[key].Add(fModel.mesh[i].faces[k].triangle[2]);
                        }
                        else
                        {
                            tr.Add(key, new List<int>());
                            tr[key].Add(fModel.mesh[i].faces[k].triangle[0]);
                            tr[key].Add(fModel.mesh[i].faces[k].triangle[1]);
                            tr[key].Add(fModel.mesh[i].faces[k].triangle[2]);
                        }

                    }
                    for (int k = 0; k < fModel.mesh[i].vertices.Count; k++)
                    {
                        ve.Add(fModel.mesh[i].vertices[k].pos);
                        uv.Add(fModel.mesh[i].vertices[k].uv);
                        col.Add(fModel.mesh[i].vertices[k].color);
                        nor.Add(fModel.mesh[i].vertices[k].normal);
                    }
                    w.SetVertices(ve);
                    w.uv = uv.ToArray();
                    w.subMeshCount = tr.Count;
                    int ss = 0;
                    List<Material> targetMat = new List<Material>();
                    foreach (var each in tr)
                    {
                        w.SetIndices(each.Value.ToArray(), MeshTopology.Triangles, ss++);
                        if (each.Key >= 0 && each.Key < fModel.shader.Length)
                        {
                            int materialIndex = fModel.shader[each.Key].TextureArg0;
                            if (materialIndex >= 0 && materialIndex < mat.Length)
                            {
                                if (mat[materialIndex] == null)
                                {
                                    targetMat.Add(new Material(Shader.Find("Unlit/Transparent")));
                                    addIflComponent = true;
                                    iflParam = materialIndex;
                                    iflMatIndex = targetMat.Count - 1;
                                }
                                else
                                    targetMat.Add(mat[materialIndex]);
                            }
                            else
                            {
                                //占位材质，代表原文件里用到了序号<0的材质(即空材质)，这里使用默认材质代替,一般武器加载不会触发这里，但是有极个别情况
                                Material defaults = new Material(Shader.Find("Unlit/Texture"));
                                defaults.name = string.Format("{0}_{1:D2}", objMesh.name, materialIndex);
                                targetMat.Add(defaults);
                            }
                        }
                    }
                    MeshRenderer mr = objMesh.AddComponent<MeshRenderer>();
                    MeshFilter mf = objMesh.AddComponent<MeshFilter>();
                    mf.mesh = w;
                    mf.mesh.colors = col.ToArray();
                    mf.mesh.normals = nor.ToArray();
                    mf.mesh.RecalculateBounds();
                    mf.mesh.RecalculateNormals();

                    mr.materials = targetMat.ToArray();
                    string vis = "";
                    if (fIns.SceneItems[i].ContainsKey("visible", out vis))
                    {
                        if (vis == "0")
                        {
                            mr.enabled = false;
                            BoxCollider box = mr.gameObject.AddComponent<BoxCollider>();
                            objMesh.tag = "weapon";
                            box.enabled = false;
                            box.isTrigger = true;
                            weaponDamage.Add(box);
                        }
                    }
                    else
                    if (fIns.SceneItems[i].name.EndsWith("Box"))
                    {
                        BoxCollider box = mr.gameObject.AddComponent<BoxCollider>();
                        box.enabled = false;
                        box.isTrigger = true;
                        weaponDamage.Add(box);
                    }
                }
            }
            
            objMesh.transform.SetParent(WR);
            objMesh.layer = WR.gameObject.layer;
            if (addIflComponent && iflMaterials.ContainsKey(iflParam))
            {
                IFLLoader iflL = objMesh.AddComponent<IFLLoader>();
                iflL.fileNameReadOnly = iflMaterials[iflParam];
                iflL.IFLFile = Resources.Load<TextAsset>(iflMaterials[iflParam]);
                iflL.matIndex = iflMatIndex;
                iflL.LoadIFL();
            }

            if (!realObject)
            {
                objMesh.transform.localRotation = fIns.SceneItems[i].quat;
                objMesh.transform.localPosition = fIns.SceneItems[i].pos;
            }
            else
            {
                objMesh.transform.localRotation = Quaternion.identity;
                objMesh.transform.localScale = Vector3.one;
                objMesh.transform.localPosition = Vector3.zero;
            }
        }

        WR.localScale = Vector3.one * scale;
        //WR.localRotation = new Quaternion(0.7071f, 0, 0, -0.7071f);//挂右手试试不旋转会怎样
        WeaponTrail wt = WR.gameObject.AddComponent<WeaponTrail>();
        if (rightHand)
        {
            //右手
            GameObject ctrlRs = NodeHelper.Find("d_wpnRS", WR.gameObject);
            if (ctrlRs != null)
                wt.AddTransform(ctrlRs.transform);
            GameObject ctrlRe = NodeHelper.Find("d_wpnRE", WR.gameObject);
            if (ctrlRe != null)
                wt.AddTransform(ctrlRe.transform);
        }
        else
        {
            //左手
            GameObject ctrlLs = NodeHelper.Find("d_wpnLS", WR.gameObject);
            if (ctrlLs != null)
                wt.AddTransform(ctrlLs.transform);
            GameObject ctrlLe = NodeHelper.Find("d_wpnLE", WR.gameObject);
            if (ctrlLe != null)
                wt.AddTransform(ctrlLe.transform);
        }
        if (owner != null)
        {
            wt.Init(owner);
            trail.Add(wt);
        }
        else
        {
            GameObject.Destroy(wt);
        }
    }

    //是否开启武器触发器，用于打碎瓶子罐子，等场景物件，与部分只响应攻击的
    public void ChangeAttack(bool open)
    {
        for (int i = 0; i < weaponDamage.Count; i++)
        {
            if (weaponDamage[i] == null)
                continue;
            if (weaponDamage[i].enabled != open) {
                weaponDamage[i].enabled = open;
                FightBox fb = weaponDamage[i].GetComponent<FightBox>();
                if (fb != null) {
                    fb.ChangeAttack(open);
                }
            }
        }
    }

    Color dragC;
    public void ChangeWeaponTrail(DragDes drag)
    {
        for (int i = 0; i < trail.Count; i++)
        {
            trail[i].Emit = drag == null ? false : true;
            if (drag != null)
            {
                dragC.r = (drag.Color.x / 255.0f);
                dragC.g = (drag.Color.y / 255.0f);
                dragC.b = (drag.Color.z / 255.0f);
                dragC.a = 0.5f;
            }
            trail[i]._colors[0] = drag == null ? Color.white : dragC;
            trail[i]._lifeTime = drag == null ? 0 : drag.Time;
        }
    }

    public void OpenWeaponTrail()
    {
        for (int i = 0; i < trail.Count; i++)
        {
            trail[i].Emit = true;
                dragC.r = 1;
                dragC.g = 1;
                dragC.b = 1;
                dragC.a = 0.5f;
            trail[i]._colors[0] = dragC;
            trail[i]._lifeTime = 0.8f;
        }
    }

    public void CloseWeaponTrail()
    {
        for (int i = 0; i < trail.Count; i++)
        {
            trail[i].Emit = false;
        }
    }
}
