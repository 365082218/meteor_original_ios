using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoClass;

public class DamageRecord
{
    public MeteorUnit target;
    public float tick;
}

//飞镖飞行实现-Flight层
public class DartLoader : MonoBehaviour {
    InventoryItem weapon;
    public const float MaxDistance = 500.0f;
    List<DamageRecord> rec = new List<DamageRecord>();
    // Use this for initialization
    private void Awake()
    {
        rig = GetComponent<Rigidbody>();
    }
    void Start () {
		
	}

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < rec.Count && i >= 0; i++)
        {
            rec[i].tick -= Time.deltaTime;
            if (rec[i].tick <= 0.0f)
            {
                rec.RemoveAt(i);
                i--;
            }
        }
    }
    
    AttackDes _attack;
    Vector3 _direction;
    float _speed = 300.0f;//普通速度.
    float gspeed = 100.0f;//加速度.
    float maxLength = 800;//跑多远开始开启重力.
    float maxDistance = 1500;//最远射程
    Rigidbody rig;
    Coroutine fly;

    public void LoadAttack(InventoryItem weapon, Vector3 forward, AttackDes att, MeteorUnit Owner)
    {
        owner = Owner;
        _attack = att;
        WeaponRoot = new GameObject().transform;
        WeaponRoot.SetParent(transform);
        WeaponRoot.localPosition = Vector3.zero;
        WeaponRoot.localScale = Vector3.one;
        WeaponRoot.localRotation = Quaternion.Euler(0, 180, 90);
        WeaponRoot.name = "WeaponRoot";
        WeaponRoot.gameObject.layer = gameObject.layer;
        Weapon = weapon;
        LoadWeapon();
        transform.LookAt(transform.position + forward);
        MeshRenderer mr = gameObject.GetComponentInChildren<MeshRenderer>();
        if (mr != null)
        {
            BoxCollider bc = mr.gameObject.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(2, 1, 6);
        }
        if (fly != null)
            StopCoroutine(fly);
        fly = StartCoroutine(Fly());
    }

    IEnumerator Fly()
    {
        while (true)
        {
            _speed += gspeed * Time.deltaTime;
            float dis = Time.deltaTime * _speed;
            transform.Translate(transform.forward * dis, Space.World);
            maxLength -= dis;
            maxDistance -= dis;
            if (maxLength <= 0.0f && !rig.useGravity)
                rig.useGravity = true;
            if (maxDistance <= 0.0f)
                DestroyObject(gameObject);
            yield return 0;
        }
    }

    public static void Init(Vector3 spawn, Vector3 forw, InventoryItem weapon, AttackDes att, MeteorUnit owner)
    {
        GameObject dartObj = GameObject.Instantiate(ResMng.LoadPrefab("DartLoader"), spawn, Quaternion.identity, null) as GameObject;
        dartObj.layer = LayerMask.NameToLayer("Flight");
        DartLoader dart = dartObj.GetComponent<DartLoader>();
        dart.LoadAttack(weapon, forw, att, owner);
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.gameObject.layer == LayerMask.NameToLayer("Scene"))
        {
            GameObject.Destroy(gameObject);
        }
        else
        {
            MeteorUnit unit = other.GetComponentInParent<MeteorUnit>();
            if (unit == null)
                return;
            if (unit.Dead)
                return;
            //同队忽略攻击
            if (unit.SameCamp(owner))
                return;
            if (rec.Find(m => m.target.Equals(unit)) != null)
                return;
            //Debug.LogError("dart attack start");
            unit.OnAttack(owner, _attack);
            DamageRecord record = new DamageRecord();
            record.target = unit;
            record.tick = 0.2f;
            rec.Add(record);
            GameObject.Destroy(gameObject);
            //Debug.LogError("dart attack end");
        }
    }

    InventoryItem Weapon;
    public InventoryItem GetCurrentWeapon() { return Weapon; }
    MeteorUnit owner;
    Transform WeaponRoot;//武器父-除非角色整个换了，否则不会删除。武器挂载点
    Transform R;//武器

    //public List<BoxCollider> weaponDamage = new List<BoxCollider>();
    public int WeaponType()
    {
        return Weapon == null ? -1 : Weapon.Info().SubType;
    }

    //把飞镖显示出来.尺寸*2，否则看不清
    public void LoadWeapon()
    {
        InventoryItem item = Weapon;
        if (item.Info().MainType == (int)EquipType.Weapon)
        {
            float scale = 2.0f;
            WeaponBase weaponProperty = WeaponMng.Instance.GetItem(item.Info().UnitId);
            string weaponR = "";
            weaponR = weaponProperty.WeaponR;

            if (!string.IsNullOrEmpty(weaponR))
            {
                GameObject weaponPrefab = ResMng.LoadPrefab(weaponR) as GameObject;
                if (weaponPrefab == null)
                {
                    GMCFile fGmcL = GMCLoader.Instance.Load(weaponR);
                    DesFile fDesL = DesLoader.Instance.Load(weaponR);

                    if (fGmcL != null && fDesL != null)
                        GenerateWeaponModel(weaponR, fGmcL, fDesL, scale, weaponProperty.TextureL);
                    else if (fGmcL == null && fDesL != null)
                    {
                        GMBFile fGmbL = GMBLoader.Instance.Load(weaponR);
                        GenerateWeaponModel(weaponR, fGmbL, fDesL, scale, weaponProperty.TextureL);
                    }
                }
                else
                {
                    if (R != null)
                        DestroyImmediate(R);
                    GameObject objWeapon = GameObject.Instantiate(weaponPrefab);
                    objWeapon.layer = LayerMask.NameToLayer("Flight");
                    R = objWeapon.transform;
                    //L = new GameObject().transform;
                    R.SetParent(WeaponRoot);
                    R.localPosition = Vector3.zero;
                    //这种导入来的模型，需要Y轴旋转180，与原系统的物件坐标系存在一些问题
                    R.localRotation = new Quaternion(0, 1, 0, 0);
                    R.name = weaponR;
                    R.localScale = Vector3.one;

                    //每个武器只能有一个碰撞盒
                    BoxCollider box = R.GetComponentInChildren<BoxCollider>();
                    if (box != null)
                    {
                        box.enabled = false;
                        //box.gameObject.tag = "Flight";
                        box.gameObject.layer = LayerMask.NameToLayer("Flight");
                    }
                    else
                    {
                        Debug.LogError("新增武器上找不到碰撞盒[除了暗器火枪飞轮外都应该有碰撞盒]");
                    }
                }
            }
        }
    }

    //通过描述文件生成实际文件.描述文件类似使用一系列模板一样，指定
    //装备要发武器姿态来。否则不知道预先加载哪个材质，有些装备需要先生成材质后调整的
    public void GenerateWeaponModel(string DesFile, GMCFile fModel, DesFile fIns, float scale = 1.0f, string textureOverrite = "")
    {
        if (string.IsNullOrEmpty(DesFile) || fModel == null || fIns == null)
            return;
        string matIden = DesFile;
        Transform WR = null;
        if (R != null)
            DestroyImmediate(R);
        R = new GameObject().transform;
        R.gameObject.layer = LayerMask.NameToLayer("Flight");
        R.SetParent(WeaponRoot);
        R.localRotation = Quaternion.identity;
        R.localPosition = Vector3.zero;
        R.localScale = Vector3.one;
        R.name = matIden;
        WR = R;

        //武器挂点必须在缩放正确后才能到指定的位置
        WR.localScale = Vector3.one;
        Material[] mat = new Material[fModel.TexturesCount];
        Dictionary<int, string> iflMaterials = new Dictionary<int, string>();
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
                    Dictionary<int, List<int>> tr = new Dictionary<int, List<int>>();
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
                            box.enabled = false;
                        }
                    }
                    else
                    if (fIns.SceneItems[i].name.EndsWith("Box"))
                    {
                        BoxCollider box = mr.gameObject.AddComponent<BoxCollider>();
                        box.enabled = false;
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
    }

    //GMB文件合并了网格，应该是不用设置位置和旋转了
    public void GenerateWeaponModel(string DesFile, GMBFile fModel, DesFile fIns, float scale = 1.0f, string textureOverrite = "")
    {
        if (string.IsNullOrEmpty(DesFile) || fModel == null || fIns == null)
            return;
        string matIden = DesFile;
        Transform WR = null;

        if (R != null)
            DestroyImmediate(R);
        R = new GameObject().transform;
        R.gameObject.layer = LayerMask.NameToLayer("Flight");
        R.SetParent(WeaponRoot);
        R.localRotation = Quaternion.identity;
        R.localPosition = Vector3.zero;
        R.localScale = Vector3.one;
        R.name = matIden;
        WR = R;

        //武器挂点必须在缩放正确后才能到指定的位置
        WR.localScale = Vector3.one;
        Material[] mat = new Material[fModel.TexturesCount];
        Dictionary<int, string> iflMaterials = new Dictionary<int, string>();
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
                    Dictionary<int, List<int>> tr = new Dictionary<int, List<int>>();
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
                            box.enabled = false;
                            //weaponDamage.Add(box);
                        }
                    }
                    else
                    if (fIns.SceneItems[i].name.EndsWith("Box"))
                    {
                        BoxCollider box = mr.gameObject.AddComponent<BoxCollider>();
                        box.enabled = false;
                        //weaponDamage.Add(box);
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
    }
}
