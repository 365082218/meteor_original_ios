
using Excel2Json;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public enum FlyStatus
{
    FlySpline,//发射-以贝塞尔曲线方式运动
    FlyGoto,//直线追随
    FlyReturn,//撞击到目标后直线返回，不会被墙壁遮挡
}

//血滴子，敌方无法逃脱，除了防御/或用地形躲避.
public class FlyWheel :NetBehaviour {

    public static bool FindFlyWheel(MeteorUnit owner)
    {
        for (int i = 0; i < FlyWheel.wheels.Count; i++)
        {
            if (FlyWheel.wheels[i].owner == owner)
                return true;
        }
        return false;
    }
    static List<FlyWheel> wheels = new List<FlyWheel>();
    static void Register(FlyWheel wheel)
    {
        wheels.Add(wheel);
    }
    static void UnRegister(FlyWheel wheel)
    {
        wheels.Remove(wheel);
    }
    Spline spline = new Spline(3);
    MeteorUnit owner;
    MeteorUnit auto_target;
    AttackDes _attack;
    Transform WeaponRoot;
    Transform R;//武器
    InventoryItem Weapon;
    WeaponTrail Trail;
    FlyStatus status = FlyStatus.FlySpline;
    //射程-无限-跟踪，不受时间限制
    float totalTime = 3.0f;
    float playedTime = 0.0f;
    float speed;
    float normal_speed = 150.0f;
    float middle_speed = 300.0f;
    float super_speed = 450.0f;
    float max_speed = 550.0f;
    float returnspeed = 550.0f;
    bool outofArea = false;

    protected new void Awake()
    {
        base.Awake();
    }
    protected new void OnDestroy()
    {
        base.OnDestroy();
        FlyWheel.UnRegister(this);
    }

    // Update is called once per frame
    public override void NetUpdate() {
        List<MeteorUnit> deleted = new List<MeteorUnit>();
		foreach (var each in attackTick)
        {
            if (each.Value - FrameReplay.deltaTime <= 0.0f)
                deleted.Add(each.Key);
        }
        for (int i = 0; i < deleted.Count; i++)
            attackTick.Remove(deleted[i]);
        Fly();
	}

    //力量感很差，仅仅运行轨迹是一条曲线还是不行的。要有冲击感，要做变速/匀加速按固定轨迹运动，才行
    //因为贝塞尔通过时间计算曲线的某一位置，可以把时间参数模拟加快，做为变量
    //但是怎么去控制
    //LineRenderer line;
    public void LoadAttack(InventoryItem weapon, MeteorUnit target, AttackDes attackdef, MeteorUnit Owner)
    {
        //line = gameObject.AddComponent<LineRenderer>();
        //line.startWidth = 1f;
        //line.endWidth = 1f;
        //line.numPositions = 200;
        if (attackdef != null) {
            if (attackdef.PoseIdx == 218) {
                //轻力度
                speed = (normal_speed + max_speed) / 2;
            } else if (attackdef.PoseIdx == 220) {
                //中等力度
                speed = (middle_speed + max_speed)/2;
            } else if (attackdef.PoseIdx == 222) {
                //超级力度
                speed = (super_speed + max_speed) / 2;
            }
        }
        owner = Owner;
        _attack = attackdef;
        auto_target = target;
        WeaponRoot = new GameObject().transform;
        WeaponRoot.SetParent(transform);
        WeaponRoot.localPosition = Vector3.zero;
        WeaponRoot.localScale = Vector3.one;
        WeaponRoot.localRotation = Quaternion.Euler(0, 0, 0);
        WeaponRoot.name = "WeaponRoot";
        WeaponRoot.gameObject.layer = gameObject.layer;
        Weapon = weapon;
        //计算控制点
        spline.SetControlPoint(0, transform.position);
        InitSpline();
        //List<Vector3> veclst = spline.GetEquiDistantPointsOnCurve(200);
        //line.SetPositions(veclst.ToArray());
        totalTime = spline.GetLength() / speed;
        //float a = (max_speed - speed) / totalTime;
        //Debug.LogError("tTotal Init:" + tTotal + " length:" + spline.GetLength() + " speed:" + speed);
        LoadWeapon();
        //增加拖尾
        GameObject trailS = NodeHelper.Find("d_wpnRS", this.gameObject);
        GameObject trailE = NodeHelper.Find("d_wpnRE", this.gameObject);
        Trail = gameObject.AddComponent<WeaponTrail>();
        if (trailS != null)
            Trail.AddTransform(trailS.transform);
        if (trailE != null)
            Trail.AddTransform(trailE.transform);
        Trail.Init(Owner);
        Trail.Open();
        MeshRenderer mr = gameObject.GetComponentInChildren<MeshRenderer>();
        if (mr != null)
        {
            BoxCollider hitBox = mr.gameObject.AddComponent<BoxCollider>();
            hitBox.isTrigger = true;
            hitBox.size = new Vector3(10, 5, 10);
            if (U3D.showBox) {
                BoundsGizmos.Instance.AddCollider(hitBox);
            }
        }
    }

    float refreshDelay = 0.1f;//0.1秒刷新一次目标缓存.
    Vector3 TargetPosCache = Vector3.zero;
    void InitSpline()
    {
        if (auto_target != null)
        {
            bool struggle = (auto_target.ActionMgr.mActiveAction.Idx == CommonAction.Struggle || auto_target.ActionMgr.mActiveAction.Idx == CommonAction.Struggle0);
            struggle = struggle || auto_target.Crouching;
            TargetPosCache = auto_target.transform.position + (struggle ? Vector3.up * 0 : Vector3.up * 25.0f);
            //计算一个坐标，终点，作为贝塞尔曲线的控制点.
            Vector3 vecforw = (new Vector3(auto_target.transform.position.x, 0, auto_target.transform.position.z) - new Vector3(owner.transform.position.x, 0, owner.transform.position.z)).normalized;
            //主角面向向量与（主角朝目标向量）的夹角的一半
            float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(-owner.transform.forward, vecforw), -1.0f, 1.0f));
            if (angle * Mathf.Rad2Deg > 90.0f)
            {
                //比较特殊。这种情况说明是背对敌人，飞轮应该往前飞行一段时间后返回
                outofArea = true;
                spline.SetControlPoint(1, owner.WeaponR.position + 50 * (-owner.transform.forward));
                spline.SetControlPoint(2, owner.WeaponR.position + 100 * (-owner.transform.forward));
                return;
            }
            //看下是左侧还是右侧
            bool isLeft = Vector3.Dot(owner.transform.right, vecforw) < 0;
            //左侧
            Vector3 vec = Quaternion.AngleAxis((isLeft ? -angle / 2.0f : angle / 2.0f) * Mathf.Rad2Deg, Vector3.up) * (-owner.transform.forward);
            //Debug.LogError("vec:" + vec);
            Vector3 vecPosition = vec * (Vector3.Distance(new Vector3(auto_target.transform.position.x, 0, auto_target.transform.position.z), new Vector3(owner.transform.position.x, 0, owner.transform.position.z))) + owner.WeaponR.position - 0.5f * Vector3.up * (owner.transform.position.y - auto_target.transform.position.y);
            //Debug.LogError("vecPosition:" + vecPosition);
            spline.SetControlPoint(1, vecPosition);
            spline.SetControlPoint(2, TargetPosCache);
        }
        else
        {
            outofArea = true;
            spline.SetControlPoint(1, owner.WeaponR.position + 50 * (-owner.transform.forward));
            spline.SetControlPoint(2, owner.WeaponR.position + 100 * (-owner.transform.forward));
        }
    }

    void RefreshSpline()
    {
        //出击时就确定反向的，不能刷新跟踪
        if (outofArea)
            return;
        bool struggle = (auto_target.ActionMgr.mActiveAction.Idx == CommonAction.Struggle || auto_target.ActionMgr.mActiveAction.Idx == CommonAction.Struggle0);
        struggle = struggle || auto_target.Crouching;

        TargetPosCache = auto_target.transform.position + (struggle ? Vector3.up * 0 : Vector3.up * 25.0f);
        spline.SetControlPoint(2, TargetPosCache);
        //tTotal = tTick + spline.GetLength() / speed;
    }
    Vector3 vecRotate = new Vector3(0, 15.0f, 0);
    void Fly()
    {
        if (status == FlyStatus.FlySpline)
        {
            //Debug.LogError("status == 0");
            //发射,随机自转
            playedTime += FrameReplay.deltaTime;
            refreshDelay -= FrameReplay.deltaTime;
            if (refreshDelay <= 0.0f && !outofArea)
            {
                RefreshSpline();
                refreshDelay = 0.1f;
            }

            if (playedTime > totalTime)
            {
                status = outofArea ? FlyStatus.FlyReturn : FlyStatus.FlyGoto;//如果还没有撞到敌人.则在接下来的时间里，直线追击敌人
                //tTotal = Vector3.Distance(transform.position, owner.WeaponR.position) / returnspeed;
                //tTick = 0.0f;
                return ;
            }

            transform.Rotate(vecRotate, Space.Self);
            //Debug.LogError("tTick:" + tTick + " tTotal:" + tTotal);
            Vector3 v = spline.Eval(playedTime / totalTime);
            //Debug.LogError(v.ToString());
            transform.position = v;
        }
        else if (status == FlyStatus.FlyGoto)
        {
            transform.Rotate(vecRotate, Space.Self);
            Vector3 dir = auto_target.mSkeletonPivot - transform.position;
            //Debug.LogError("dir.magnitude:" + dir.sqrMagnitude + " speed*time.deltatime:" + speed * Time.deltaTime);
            //WsGlobal.AddDebugLine(transform.position, transform.position + dir, Color.red, "dir");
            if (dir.magnitude <= speed * FrameReplay.deltaTime)//下一帧能够超过目标.
                status = FlyStatus.FlyReturn;
            transform.position = (transform.position + dir.normalized * speed * FrameReplay.deltaTime);
        }
        else if (status == FlyStatus.FlyReturn)
        {
            //Debug.LogError("status == 1");
            //回收-直线回转-穿墙
            transform.Rotate(vecRotate, Space.Self);
            Vector3 dir = owner.WeaponR.position - transform.position;
            if (dir.magnitude <= speed * FrameReplay.deltaTime)//速度太大的时候，可能会2边跑，而且距离都大于5，这样要看夹角是否改变了方向.
            {
                //Debug.LogError("WeaponReturned");
                owner.WeaponReturned(_attack.PoseIdx);
                owner.weaponLoader.ShowWeapon();
                MeshRenderer mr = gameObject.GetComponentInChildren<MeshRenderer>();
                if (mr != null) {
                    BoxCollider bc = mr.gameObject.GetComponent<BoxCollider>();
                    bc.enabled = false;
                }
                GameObject.Destroy(gameObject);
                return;
            }
            transform.position = (transform.position + dir.normalized * speed * FrameReplay.deltaTime);
        }
    }

    public static void Init(Vector3 spawn, MeteorUnit autoTarget, InventoryItem weapon, AttackDes attackDef, MeteorUnit unit)
    {
        GameObject flyWheelObj = GameObject.Instantiate(Resources.Load("FlyWheel"), spawn, Quaternion.identity, null) as GameObject;
        flyWheelObj.layer = LayerManager.Flight;
        FlyWheel wheel = flyWheelObj.GetComponent<FlyWheel>();
        FlyWheel.Register(wheel);
        wheel.LoadAttack(weapon, autoTarget, attackDef, unit);
    }

    Dictionary<MeteorUnit, float> attackTick = new Dictionary<MeteorUnit, float>();

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.root.gameObject.layer == LayerManager.Scene)
        {
            if (status == FlyStatus.FlySpline || status == FlyStatus.FlyGoto)
            {
                status = FlyStatus.FlyReturn;
                playedTime = 0.0f;
                totalTime = Vector3.Distance(transform.position, owner.WeaponR.position) / returnspeed;
                spline.SetControlPoint(2, transform.position);
            }
        }
        else
        {
            MeteorUnit unit = other.GetComponentInParent<MeteorUnit>();
            if (unit == null)
                return;
            if (unit.Dead)
                return;
            if (attackTick.ContainsKey(unit))
                return;
            //同队忽略攻击
            if (unit.SameCamp(owner))
                return;
            if (unit == owner)
                return;
            unit.OnAttack(owner, _attack);
            attackTick.Add(unit, 0.2f);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.gameObject.layer == LayerManager.Scene)
        {
            if (status == FlyStatus.FlySpline || status == FlyStatus.FlyGoto)
            {
                status = FlyStatus.FlyReturn;
                playedTime = 0.0f;
                totalTime = Vector3.Distance(transform.position, owner.WeaponR.position) / returnspeed;
                spline.SetControlPoint(2, transform.position);
            }
        }
        else
        {
            MeteorUnit unit = other.GetComponentInParent<MeteorUnit>();
            if (unit == null)
                return;
            if (unit == owner && status == FlyStatus.FlyReturn)
            {
                //Debug.LogError("WeaponReturned");
                owner.WeaponReturned(_attack.PoseIdx);
                owner.weaponLoader.ShowWeapon();
                MeshRenderer mr = gameObject.GetComponentInChildren<MeshRenderer>();
                if (mr != null) {
                    BoxCollider bc = mr.gameObject.GetComponent<BoxCollider>();
                    bc.enabled = false;
                }
                GameObject.Destroy(gameObject);
                return;
            }
            if (unit.Dead)
                return;
            if (attackTick.ContainsKey(unit))
                return;
            //同队忽略攻击
            if (unit.SameCamp(owner))
                return;
            if (unit == owner)
                return;
            unit.OnAttack(owner, _attack);
            attackTick.Add(unit, 0.2f);
        }
    }

    public void LoadWeapon()
    {
        InventoryItem item = Weapon;
        if (item.Info().MainType == (int)UnitType.Weapon)
        {
            float scale = 1.0f;
            WeaponData weaponProperty = U3D.GetWeaponProperty(item.Info().UnitId);
            string weaponR = "";
            weaponR = weaponProperty.WeaponR;

            if (!string.IsNullOrEmpty(weaponR))
            {
                GameObject weaponPrefab = Resources.Load<GameObject>(weaponR);
                if (weaponPrefab == null)
                {
                    GMCFile fGmcL = GMCLoader.Ins.Load(weaponR);
                    DesFile fDesL = DesLoader.Ins.Load(weaponR);

                    if (fGmcL != null && fDesL != null)
                        GenerateWeaponModel(weaponR, fGmcL, fDesL, scale, weaponProperty.TextureL);
                    else if (fGmcL == null && fDesL != null)
                    {
                        GMBFile fGmbL = GMBLoader.Ins.Load(weaponR);
                        GenerateWeaponModel(weaponR, fGmbL, fDesL, scale, weaponProperty.TextureL);
                    }
                }
                else
                {
                    if (R != null)
                        DestroyImmediate(R);
                    GameObject objWeapon = GameObject.Instantiate(weaponPrefab);
                    objWeapon.layer = LayerManager.Flight;
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
                        box.gameObject.layer = LayerManager.Flight;
                        //weaponDamage.Add(box);
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
        R.gameObject.layer = LayerManager.Flight;
        R.SetParent(WeaponRoot);
        R.localRotation = Quaternion.identity;
        R.localPosition = Vector3.zero;
        R.localScale = Vector3.one;
        R.name = matIden;
        WR = R;

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
        R.gameObject.layer = LayerManager.Flight;
        R.SetParent(WeaponRoot);
        R.localRotation = Quaternion.identity;
        R.localPosition = Vector3.zero;
        R.localScale = Vector3.one;
        R.name = matIden;
        WR = R;

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