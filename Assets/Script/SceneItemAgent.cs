using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Excel2Json;
using protocol;

//using DG.Tweening;
//damage为物件是否可以伤害玩家角色,需要des文件里带伤害值
//damagevalue为物件是否可以被玩家伤害,并且每次伤害值是多少.
//collision为物件是否可以阻塞角色
//active为物件是否激活
//pose x y为播放ID为x的动画片段，并依据y设定的值决定是否循环.
[System.Serializable]
public class SceneItemProperty
{
    //作为默认场景物品才有.
    public Dictionary<string, int> names = new Dictionary<string, int>();
    public Dictionary<string, int> attribute = new Dictionary<string, int>();//是否激活active 是否有伤害(damage) 是否有碰撞(collision) pose做动作.
}
//原版内不需要序列化存储的机关，关卡固有机关,尖刺,摆斧
public class SceneItemAgent :NetBehaviour {
    // Use this for initialization
    List<Collider> collisions = new List<Collider>();
    [SerializeField]
    private AnimationCurve curve;
    //public bool registerCollision;
    public int InstanceId;//从0序号的实例ID
    FMCPlayer player;
    System.Reflection.MethodInfo MethodOnAttack;
    System.Reflection.MethodInfo MethodOnIdle;
    System.Func<int, int, int, int> OnAttackCallBack;
    System.Action<int, int> OnIdle;
    MethodInfo OnTouch;
    MethodInfo OnPickUp;
    float initializeY;
    bool billBoard = false;
    bool animate = false;
    public bool TakeUp { get { return Refresh; } }//是否被占据，如果在刷新中，则被占据

    int replaceQueue = -1;
    public void SetAlpha(float alpha) {
        MeshRenderer[] mr = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < mr.Length; i++) {
            if (mr[i].sharedMaterials.Length == 0)
                return;
            if (replaceQueue == -1)
                replaceQueue = mr[i].material.renderQueue;
            mr[i].material.renderQueue = 3000;
            mr[i].material.SetFloat("_Alpha", alpha);
        }
    }

    public void RestoreAlpha() {
        MeshRenderer[] mr = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < mr.Length; i++) {
            if (mr[i].sharedMaterials.Length == 0)
                return;
            mr[i].material.SetFloat("_Alpha", 1.0f);
            if (replaceQueue != -1)
                mr[i].material.renderQueue = replaceQueue;
        }
        replaceQueue = -1;
    }

    //场景初始化调用，或者爆出物品，待物品落地时调用
    public void OnStart(LevelScriptBase script = null)
    {
        initializeY = transform.position.y + 2;
        //自转+高度转
        if (!property.names.ContainsKey("machine") && !billBoard)
        {
            if (curve == null)
            {
                Keyframe[] ks = new Keyframe[2];
                ks[0] = new Keyframe(0, -1);
                ks[1] = new Keyframe(1, 1);
                curve = new AnimationCurve(ks);
            }
            curve.postWrapMode = WrapMode.PingPong;
            curve.preWrapMode = WrapMode.PingPong;
            animate = true;
        }

        if (script != null)
        {
            MethodOnAttack = script.GetType().GetMethod(gameObject.name + "_OnAttack");
            MethodOnIdle = script.GetType().GetMethod(gameObject.name + "_OnIdle");
            OnTouch = CombatData.Ins.GScriptType.GetMethod(name + "_OnTouch", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (OnTouch == null)
            {
                System.Type typeParent = CombatData.Ins.GScriptType.BaseType;
                while (typeParent != null && OnTouch == null)
                {
                    OnTouch = typeParent.GetMethod(name + "_OnTouch", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    typeParent = typeParent.BaseType;
                }
            }
            OnPickUp = CombatData.Ins.GScriptType.GetMethod(name + "_OnPickUp", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (OnPickUp == null)
            {
                System.Type typeParent = CombatData.Ins.GScriptType.BaseType;
                while (typeParent != null && OnPickUp == null)
                {
                    OnPickUp = typeParent.GetMethod(name + "_OnPickUp", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    typeParent = typeParent.BaseType;
                }
            }
        }

        RefreshCollision();
    }

    //bool up = true;
    //float yHeight = 5.0f;
    void yMove()
    {
        float y = curve.Evaluate(FrameReplay.Ins.time);
        transform.position = new Vector3(transform.position.x, (initializeY + 5 * y), transform.position.z);
        transform.Rotate(new Vector3(0, 90 * FrameReplay.deltaTime, 0));
    }

    protected new void Awake()
    {
        base.Awake();
        root = transform;
        root.tag = "SceneItemAgent";
        Refresh = false;
        MeteorManager.Ins.OnGenerateSceneItem(this);
    }

    protected new void OnDestroy()
    {
        base.OnDestroy();
        MeteorManager.Ins.OnDestroySceneItem(this);
    }

    float refresh_tick;
    bool Refresh;

    public override void NetUpdate()
    {
        if (MethodOnIdle != null)
            MethodOnIdle.Invoke(CombatData.Ins.GScript, new object[] { InstanceId });
        else if (OnIdle != null)
            OnIdle.Invoke(InstanceId, Index);

        if (animate && root != null && root.gameObject.activeInHierarchy)
            yMove();

        if (ItemInfo != null && (ItemInfo.IsItem() || ItemInfo.IsWeapon()) && Refresh) {
            refresh_tick -= FrameReplay.deltaTime;
            if (refresh_tick <= 0) {
                OnRefresh();
            }
        }
    }

    public void OnNetPickuped(MeteorUnit unit) {
        if (unit != null) {
            OnPickUped(unit);
            UnityEngine.Debug.Log("item:" + name + " pick up by:" + unit.name);
        }
    }

    public void OnPickUped(MeteorUnit unit)
    {
        //场景上的模型物件捡取
        if (unit.Dead)
            return;
        if (ItemInfo != null && ItemInfo.Type != 0 && root != null)
        {
            if (ItemInfo.IsWeapon())
            {
                //满武器，不能捡
                if (unit.Attr.Weapon != 0 && unit.Attr.Weapon2 != 0)
                    return;
                //相同武器，不能捡
                ItemData ib0 = GameStateMgr.Ins.FindItemByIdx(unit.Attr.Weapon);
                WeaponData wb0 = U3D.GetWeaponProperty(ib0.UnitId);
                if (wb0 != null && wb0.WeaponR == ItemInfo.model)
                    return;

                if (unit.Attr.Weapon2 != 0)
                {
                    ItemData ib1 = GameStateMgr.Ins.FindItemByIdx(unit.Attr.Weapon2);
                    WeaponData wb1 = U3D.GetWeaponProperty(ib1.UnitId);
                    if (wb1 != null && wb1.WeaponR == ItemInfo.model)
                        return;
                }

                //同类武器不能捡
                int weaponPickup = GameStateMgr.Ins.GetWeaponCode(ItemInfo.model);
                ItemData wb = GameStateMgr.Ins.FindItemByIdx(weaponPickup);
                if (wb == null)
                    return;

                ItemData wbl = GameStateMgr.Ins.FindItemByIdx(unit.Attr.Weapon);
                if (wbl == null)
                    return;

                ItemData wbr = GameStateMgr.Ins.FindItemByIdx(unit.Attr.Weapon2);
                if (wb.SubType == wbl.SubType)
                    return;

                if (wbr != null && wb.SubType == wbr.SubType)
                    return;
                //可以捡取
                unit.Attr.Weapon2 = weaponPickup;
                SFXLoader.Ins.PlayEffect(672, unit.gameObject, true);
                refresh_tick = ItemInfo.first[1].flag[1];
                if (asDrop)
                    GameObject.Destroy(gameObject);
                else
                {
                    if (refresh_tick > 0)
                        Refresh = true;
                    GameObject.Destroy(root.gameObject);
                    root = null;
                }
            }
            else if (ItemInfo.IsItem())
            {
                //表明此为Buff,会叠加某个属性，且挂上一个持续的特效。
                unit.GetItem(ItemInfo);
                refresh_tick = ItemInfo.first[1].flag[1];
                if (refresh_tick > 0)
                    Refresh = true;
                GameObject.Destroy(root.gameObject);
                root = null;
            }
            else if (ItemInfo.IsFlag())
            {
                GameObject.Destroy(root.gameObject);
                root = null;
                if (ItemInfo.first[1].flag[1] != 0)
                    SFXLoader.Ins.PlayEffect(ItemInfo.first[1].flag[1], unit.gameObject, true);
                U3D.InsertSystemMsg(unit.name + " 夺得镖物");
                unit.SetFlag(ItemInfo, ItemInfo.first[2].flag[1]);
            }

            if (OnPickUp != null)
                OnPickUp.Invoke(CombatData.Ins.GScript, null);
        }
        else if (ItemInfoEx != null)
        {
            if (ItemInfoEx.MainType == (int)UnitType.Weapon)
            {
                string weaponModel = "";
                WeaponData wp = U3D.GetWeaponProperty(ItemInfoEx.UnitId);
                if (wp != null)
                    weaponModel = wp.WeaponR;
                //满武器，不能捡
                if (unit.Attr.Weapon != 0 && unit.Attr.Weapon2 != 0)
                    return;
                //相同武器，不能捡
                ItemData ib0 = GameStateMgr.Ins.FindItemByIdx(unit.Attr.Weapon);
                WeaponData wb0 = U3D.GetWeaponProperty(ib0.UnitId);
                if (wb0 != null && wb0.WeaponR == weaponModel)
                    return;

                if (unit.Attr.Weapon2 != 0)
                {
                    ItemData ib1 = GameStateMgr.Ins.FindItemByIdx(unit.Attr.Weapon2);
                    WeaponData wb1 = U3D.GetWeaponProperty(ib1.UnitId);
                    if (wb1 != null && wb1.WeaponR == weaponModel)
                        return;
                }

                //同类武器不能捡
                int weaponPickup = GameStateMgr.Ins.GetWeaponCode(weaponModel);
                ItemData wb = GameStateMgr.Ins.FindItemByIdx(weaponPickup);
                if (wb == null)
                    return;

                ItemData wbl = GameStateMgr.Ins.FindItemByIdx(unit.Attr.Weapon);
                if (wbl == null)
                    return;

                ItemData wbr = GameStateMgr.Ins.FindItemByIdx(unit.Attr.Weapon2);
                if (wb.SubType == wbl.SubType)
                    return;

                if (wbr != null && wb.SubType == wbr.SubType)
                    return;
                //可以捡取
                unit.Attr.Weapon2 = weaponPickup;
                SFXLoader.Ins.PlayEffect(672, unit.gameObject, true);
                
                if (asDrop)
                    GameObject.Destroy(gameObject);
            }
        }
        else
        {
            if (OnTouch != null)
                OnTouch.Invoke(CombatData.Ins.GScript, new object[] { 0, unit.InstanceId });
            else if (OnPickUp != null)
                OnPickUp.Invoke(CombatData.Ins.GScript, null);
        }
        if (unit.Attr.IsPlayer && CombatData.Ins.GLevelMode == LevelMode.MultiplyPlayer) {
            GetItemMsg msg = new GetItemMsg();
            msg.instance = (uint)InstanceId;
            msg.playerId = (uint)unit.InstanceId;
            msg.type = (int)GetItemType.SceneItem;
            FrameSyncServer.Ins.NetEvent(protocol.MeteorMsg.Command.GetItem, msg);
        }
    }

    void OnRefresh()
    {
        //只要是刷新，那么根一定是子节点。否则人物接到的时候，就删除了。
        if (root != null)
            GameObject.Destroy(root.gameObject);
        root = new GameObject("root").transform;
        root.SetParent(transform);
        root.gameObject.layer = gameObject.layer;
        root.localScale = Vector3.one;
        root.localPosition = Vector3.zero;
        root.localRotation = Quaternion.identity;
        root.tag = "SceneItemAgent";
        if (ItemInfo != null)
        {
            Load(ItemInfo.model);
            ApplyPost();
        }
        if (ItemInfo.IsFlag())
            U3D.InsertSystemMsg("镖物重置");
        Refresh = false;
        if (ItemInfo.IsWeapon() || ItemInfo.IsItem())
            refresh_tick = ItemInfo.first[1].flag[1];
        //要处理一下碰撞网格太多的问题
        RefreshCollision();
    }

    public Transform root;
    public Option ItemInfo;
    public ItemData ItemInfoEx;
    void ApplyPrev(Option op)
    {
        if (ItemInfo == null)
            ItemInfo = op;
        //如果是物品，那么是物品生成器，会一段时间刷新一个。如果刷新时间小于0，那么不刷.
        if (ItemInfo.IsItem() || ItemInfo.IsFlag() || ItemInfo.IsWeapon())
        {
            root = new GameObject("root").transform;
            root.SetParent(transform);
            root.gameObject.layer = gameObject.layer;
            root.localScale = Vector3.one;
            root.localPosition = Vector3.zero;
            root.localRotation = Quaternion.identity;
            root.tag = "SceneItemAgent";
        }
    }

    public void ApplyPost()
    {
        if (ItemInfo == null || root == null)
        {
            if (ItemInfoEx != null)
            {
                for (int i = 0; i < root.transform.childCount; i++)
                {
                    Collider co = root.transform.GetChild(i).GetComponent<Collider>();
                    if (co != null && co.enabled)
                    {
                        if (co is MeshCollider)
                            (co as MeshCollider).convex = true;
                        co.isTrigger = true;
                    }
                }
            }
            return;
        }
        if (ItemInfo.IsFlag() || ItemInfo.IsItem() || ItemInfo.IsWeapon())
        {
            for (int i = 0; i < root.transform.childCount; i++)
            {
                Collider co = root.transform.GetChild(i).GetComponent<Collider>();
                if (co != null && co.enabled)
                {
                    if (co is MeshCollider)
                        (co as MeshCollider).convex = true;
                    co.isTrigger = true;
                }
            }
        }
    }
    public void Load(string file)
    {
        string s = file;
        if (!string.IsNullOrEmpty(file))
        {
            //查看此物体属于什么，A：武器 B：道具 C：镖物
            if (ItemInfo == null)
            {
                for (int i = 0; i < MenuResLoader.Ins.Info.Count; i++)
                {
                    if (MenuResLoader.Ins.Info[i].model != "0" && 0 == string.Compare(MenuResLoader.Ins.Info[i].model, s, true))
                    {
                        ApplyPrev(MenuResLoader.Ins.Info[i]);
                        break;
                    }
                    string rh = s.ToUpper();
                    string rh2 = MenuResLoader.Ins.Info[i].model.ToUpper();
                    if (rh2.StartsWith(rh))
                    {
                        s = MenuResLoader.Ins.Info[i].model;
                        ApplyPrev(MenuResLoader.Ins.Info[i]);
                        break;
                    }
                }
                //不是一个Meteor.res里的物件
                if (ItemInfo == null)
                {
                    List<ItemData> its = DataMgr.Ins.GetItemDatas();
                    for (int i = 0; i < its.Count; i++)
                    {
                        if (its[i].MainType == (int)UnitType.Weapon)
                        {
                            WeaponData weapon = U3D.GetWeaponProperty(its[i].UnitId);
                            if (weapon.WeaponR == s)
                            {
                                ItemInfoEx = its[i];
                                break;
                            }
                        }
                    }
                }
            }

            //证明此物品不是可拾取物品
            //gameObject.layer = (ItemInfo != null || ItemInfoEx != null) ?  LayerManager.Trigger : LayerManager.Scene;
            gameObject.layer = LayerManager.Trigger;
            root.gameObject.layer = gameObject.layer;
            //箱子椅子桌子酒坛都不允许为场景物品.
            Utility.ShowMeteorObject(s, root);
            DesFile fIns = DesLoader.Ins.Load(s);
            //把子物件的属性刷到一起.
            for (int i = 0; i < fIns.SceneItems.Count; i++)
                LoadCustom(fIns.SceneItems[i].name, fIns.SceneItems[i].custom);

            //雪人不能合并材质，不然没法动画
            if (CombatData.Ins.GLevelItem.Scene != "Meteor_21")
            {
                if (name.StartsWith("D_Item") || name.StartsWith("D_RJug") || name.StartsWith("D_itRJug") || name.StartsWith("D_BBox") ||
                    name.StartsWith("D_BBBox") || name.StartsWith("D_Box"))
                {
                    if (root != null)
                    {
                        CombineChildren combine = root.gameObject.AddComponent<CombineChildren>();
                        combine.generateTriangleStrips = false;
                    }
                    else
                    {
                        CombineChildren combine = gameObject.AddComponent<CombineChildren>();
                        combine.generateTriangleStrips = false;
                    }
                }
            }

            FMCFile f = FMCLoader.Ins.Load(s);
            if (f != null)
            {
                player = GetComponent<FMCPlayer>();
                if (player == null)
                    player = gameObject.AddComponent<FMCPlayer>();
                player.Init(s, f);
            }
            else
            {
                player = GetComponent<FMCPlayer>();
                if (player != null)
                {
                    Destroy(player);
                    player = null;
                }
            }
        }
    }

    public void LoadCustom(string na, List<string> custom_feature)
    {
        if (custom_feature != null) {
            for (int i = 0; i < custom_feature.Count; i++) {
                string[] kv = custom_feature[i].Split(new char[] { '=' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (kv.Length == 2) {
                    string k = kv[0].Trim(new char[] { ' ' });
                    string v = kv[1].Trim(new char[] { ' ' });
                    if (k == "name") {
                        string[] varray = v.Split(new char[] { '\"' }, System.StringSplitOptions.RemoveEmptyEntries);
                        string value = varray[0].Trim(new char[] { ' ' });
                        if (value.StartsWith("damage")) {
                            value = value.Substring(6);
                            property.names["damage"] = int.Parse(value);
                        }
                    } else if (k == "model") {

                    } else if (k == "ticket") {

                    } else if (k == "visible") {
                        GameObject obj = NodeHelper.Find(na, root.gameObject);
                        MeshRenderer mr = obj.GetComponent<MeshRenderer>();
                        if (mr == null) {
                            mr = obj.GetComponentInChildren<MeshRenderer>();
                        }
                        if (mr != null)
                            mr.enabled = (int.Parse(v) == 1);
                    } else if (k == "collision") {
                        //是否包含碰撞检测.1:阻碍角色，2:Trigger
                        GameObject obj = NodeHelper.Find(na, root.gameObject);
                        Collider[] co = obj.GetComponentsInChildren<Collider>(true);
                        int vv = int.Parse(v);
                        for (int q = 0; q < co.Length; q++) {
                            MeshCollider c = co[q] as MeshCollider;
                            if (c != null) {
                                bool enable = c.enabled;
                                c.convex = vv == 1;
                                c.enabled = !enable;//重置碰撞体和角色碰撞的bug
                                c.enabled = enable;
                            }
                            co[q].isTrigger = vv == 0;
                            if (property.attribute.ContainsKey("blockplayer") && property.attribute["blockplayer"] == 0) {
                                co[q].isTrigger = vv == 1;
                            }
                            //co[q].enabled = vv == 1;
                        }
                        property.attribute["collision"] = vv;
                    } else if (k == "blockplayer") {
                        //不要阻碍角色-Trigger为1
                        GameObject obj = NodeHelper.Find(na, root.gameObject);
                        if (v == "no") {
                            Collider[] co = obj.GetComponentsInChildren<Collider>(true);
                            for (int c = 0; c < co.Length; c++) {
                                MeshCollider m = co[c] as MeshCollider;
                                if (m != null) {
                                    if (!na.StartsWith("Plane")) {
                                        m.convex = true;
                                        m.isTrigger = true;
                                        co[c].enabled = true;
                                    } else
                                        Destroy(m);
                                } else {
                                    co[c].isTrigger = true;
                                    co[c].enabled = true;
                                }
                            }
                            property.attribute["blockplayer"] = 0;
                        }
                    } else if (k == "billboard") {
                        LookAtCamara cam = GetComponent<LookAtCamara>();
                        if (cam == null)
                            gameObject.AddComponent<LookAtCamara>();
                        billBoard = true;
                    } else if (k == "effect") {

                    } else {
                        //Debug.LogError(na + " can not alysis:" + custom_feature[i]);
                    }
                }
            }
        }
    }
    public SceneItemProperty property = new SceneItemProperty();
    public void SetSceneItem(string features, int value)
    {
        if (features == "frame")
        {
            if (player != null)
            {
                player.ChangeFrame(value);
            }
        }
        else
            Debug.LogError("setSceneItem:" + features + " v1:" + value);
    }

    public void SetSceneItem(string features, string value)
    {
        if (features == "name")
        {
            property.names[value] = 1;
            if (value == "damage")
            {

            }
            if (value == "machine")
            {

            }
        }
    }
    public void SetSceneItem(string features, string sub_features, int value)
    {
        if (features == "attribute")
        {
            property.attribute[sub_features] = value;
            if (sub_features == "damage")
            {
                if (value == 1)
                {
                    if (!property.attribute.ContainsKey("collision"))
                    {
                        Collider[] co = GetComponentsInChildren<Collider>();
                        for (int i = 0; i < co.Length; i++)
                        {
                            co[i].enabled = true;
                            if (co[i] as MeshCollider != null)
                                (co[i] as MeshCollider).convex = true;
                            co[i].isTrigger = true;
                        }
                    }
                    else
                    if (property.attribute.ContainsKey("collision") && property.attribute["collision"] == 0)
                    {
                        Collider[] co = GetComponentsInChildren<Collider>();
                        for (int i = 0; i < co.Length; i++)
                        {
                            co[i].enabled = true;
                            if (co[i] as MeshCollider != null)
                                (co[i] as MeshCollider).convex = true;
                            co[i].isTrigger = true;
                        }
                    }
                }
                else
                {

                }
            }
            else if (sub_features == "collision")
            {
                SetCollision(value == 1);
            }
            else if (sub_features == "damagevalue")
            {
            }
            else if (sub_features == "active")
            {
                gameObject.SetActive(value != 0);
                if (root != null)
                    root.gameObject.SetActive(value != 0);
                //删除受击框
                if (value == 0) {
                    for (int i = 0; i < collisions.Count; i++) {
                        Destroy(collisions[i]);
                    }
                    collisions.Clear();
                }
                if (OnIdle != null && value == 0)
                {
                    GameObject.Destroy(gameObject);
                }
                else if (MethodOnIdle != null && value == 0)
                {
                    GameObject.Destroy(gameObject);
                }

                //如果在刷新倒计时中，立即刷新物件.
                if (Refresh)
                    OnRefresh();
            }
            else if (sub_features == "interactive")
            {
                //与其他不发生交互，无法接触
                Collider[] co = GetComponentsInChildren<Collider>();
                for (int i = 0; i < co.Length; i++)
                    co[i].enabled = value != 0;
                //交互切换
                if (MethodOnAttack != null || OnAttackCallBack != null)
                {
                    if (value == 0) {

                    }
                    else if (value == 1)
                    {
                        //刷新受击框.
                        RefreshCollision();
                        
                    }
                }
            }
        }
        else if (features == "name")
        {
            property.names[sub_features] = value;
            if (sub_features == "damage")
            {

            }
            if (sub_features == "machine")
            {

            }
        }
    }

    //setSceneItem("xx", "pos", posid, loop)
    public void SetSceneItem(string features, int value1, int value2)
    {
        if (features == "pose")
        {
            if (player != null)
            {
                player.ChangePose(value1, value2);
            }
        }
        else
            Debug.LogError("setSceneItem:" + features + " v1:" + value1 + " v2:" + value2);
    }

    //这个对象是否拥有击伤角色的能力.
    public bool HasDamage()
    {
        if (property.attribute.ContainsKey("damage") && property.attribute["damage"] == 1)
            return true;
        return false;
    }

    //这个对象的击伤能力值
    public int DamageValue()
    {
        if (HasDamage())
        {
            if (property.attribute.ContainsKey("damagevalue"))
                return property.attribute["damagevalue"];
            else if (property.names.ContainsKey("damage"))
                return property.names["damage"];
        }
        return 0;
    }

    //受击框
    public void RefreshCollision()
    {
        collisions.Clear();
        if (MethodOnAttack != null || OnAttackCallBack != null)
        {
            ///针对能击碎的物件，合并碰撞盒放到顶级
            if (name.StartsWith("D_BBox") || name.StartsWith("D_BBBox") || name.StartsWith("D_RJug")) {
                CombineChildren combine = gameObject.GetComponent<CombineChildren>();
                if (combine != null) {
                    combine.Combine();
                }
                BoxCollider b = gameObject.GetComponent<BoxCollider>();
                if (b == null) {
                    b = gameObject.AddComponent<BoxCollider>();
                }
                //部分官卡不会合并网格，圣诞夜，宝箱材质是多个无法合并
                if (combine == null) {
                    if (name.StartsWith("D_BBox")) {
                        b.size = new Vector3(40, 40, 40);
                        b.center = new Vector3(0, 20, 0);
                    }
                }
                MeshCollider[] co = GetComponentsInChildren<MeshCollider>();
                for (int i = 0; i < co.Length; i++) {
                    Destroy(co[i]);
                }
            }
            if (name.ToLower().Contains("chair") || name.ToLower().Contains("desk")) {

            } else {
                Collider[] co = GetComponentsInChildren<Collider>();
                for (int i = 0; i < co.Length; i++) {
                    //不显示出来的都没有受击框
                    MeshRenderer mr = co[i].GetComponent<MeshRenderer>();
                    if (mr != null && mr.enabled)
                        collisions.Add(co[i]);
                }
            }
            
        } else {//不能受击的物件，可能可以拾取，或者
            if (ItemInfo != null && ItemInfo.IsItem() && root != null) {
                //炼化
                if (ItemInfo.Idx == 9 || ItemInfo.Idx == 23 || ItemInfo.Idx == 24 || ItemInfo.Idx == 25) {
                    BoxCollider b = root.GetComponent<BoxCollider>();
                    if (b == null) {
                        b = root.gameObject.AddComponent<BoxCollider>();
                    }
                    b.center = new Vector3(0, 7.5f, 0);
                    b.size = new Vector3(15, 15, 15);
                    b.isTrigger = true;
                    MeshCollider[] co = GetComponentsInChildren<MeshCollider>();
                    for (int i = 0; i < co.Length; i++) {
                        Destroy(co[i]);
                    }
                }
                else {
                    //其他道具类
                    ReplaceMeshCollider();
                }
            } else if (name.StartsWith("D_itBBox")){
                ReplaceMeshCollider();
            } else if (name.StartsWith("D_wpBBox")) {
                ProcessWeaponCollider();
            } else if (name.StartsWith("D_wpBBBox")) {
                ProcessWeaponCollider();
            } else if (name.StartsWith("D_Wpn") || name.StartsWith("D_wpn") || name.StartsWith("D_Wp") || name.StartsWith("D_wp")) {
                ProcessWeaponCollider();
            } else if (name.StartsWith("D_sn03")){
                //尖刺-如果有box的对象，取消box对象上的触发器
                ProcessSn03Collider();
            } else if (name.StartsWith("D_ston")){
                //可击碎的石头-击碎后可攻击角色
            } else if (name.StartsWith("D_tp")){
                
            } /*else*/
                //Debug.LogError("not processed:" + name);
        }
    }

    //如果有子对象名称为box，取消碰撞盒上的istrigger
    void ProcessSn03Collider() {
        if (transform.childCount >= 2) {
            for (int i = 0; i < transform.childCount; i++) {
                Transform tri = transform.GetChild(i);
                if (tri.name.StartsWith("Box")) {
                    BoxCollider b = tri.gameObject.GetComponent<BoxCollider>();
                    if (b != null) {
                        b.isTrigger = false;
                    }
                }
            }
        }
    }

    //只要有没开渲染的碰撞器，把其他网格上的碰撞器去掉
    void ProcessWeaponCollider() {
        if (ItemInfo.model == "W2_0") {
            Transform tri = root.Find("Object01");
            if (tri != null)
                GameObject.Destroy(tri.gameObject);
            Transform wpnFA03 = root.Find("wpnFA03");
            MeshCollider mr = wpnFA03.gameObject.GetComponent<MeshCollider>();
            if (mr != null)
                GameObject.Destroy(mr);
            BoxCollider box = wpnFA03.gameObject.GetComponent<BoxCollider>();
            if (box == null)
                box = wpnFA03.gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            return;
        }
        Transform weapon = null;//实际武器
        Transform atbox = null;//碰撞范围
        for (int i = 0; i < root.childCount; i++) {
            Transform tr = root.GetChild(i);
            MeshRenderer mr = tr.GetComponent<MeshRenderer>();
            if (mr != null) {
                if (mr.enabled)
                    weapon = tr;
                else
                    atbox = tr;
            }
        }

        //一些武器并没有攻击范围盒，自身就是
        if (atbox == null) {
            if (weapon != null) {
                MeshCollider me = weapon.GetComponent<MeshCollider>();
                if (me != null)
                    GameObject.Destroy(me);
                BoxCollider b = weapon.GetComponent<BoxCollider>();
                if (b == null) {
                    b = weapon.gameObject.AddComponent<BoxCollider>();
                }
                b.isTrigger = true;
            }
        }
        else {
            if (weapon != null) {
                MeshCollider me = weapon.GetComponent<MeshCollider>();
                if (me != null)
                    GameObject.Destroy(me);
            }
        }
    }

    void ReplaceMeshCollider() {
        if (root.childCount == 1) {
            //D_itSt31
            BoxCollider b = root.GetChild(0).GetComponent<BoxCollider>();
            if (b == null) {
                b = root.GetChild(0).gameObject.AddComponent<BoxCollider>();
            }
            b.isTrigger = true;
            MeshCollider[] co = root.GetComponentsInChildren<MeshCollider>();
            for (int i = 0; i < co.Length; i++) {
                Destroy(co[i]);
            }
        } else {
            //Debug.LogError("多个子网格的需要自己处理");
            CombineItem();
        }
    }

    //检查root下的材质能否合并，能则尝试合并
    void CombineItem() {
        bool combine = true;
        Material mat = null;
        //金华城 D_itBBox33
        for (int i = 0; i < root.childCount; i++) {
            MeshRenderer mr = root.GetChild(i).GetComponent<MeshRenderer>();
            if (mr != null && mr.sharedMaterial != mat && mat != null) {
                combine = false;
                break;
            }
            if (mat == null)
                mat = mr.sharedMaterial;
        }
        if (combine) {
            CombineChildren cm = root.gameObject.AddComponent<CombineChildren>();
            cm.Combine();
            BoxCollider b = root.gameObject.AddComponent<BoxCollider>();
            b.isTrigger = true;
            MeshCollider[] co = GetComponentsInChildren<MeshCollider>();
            for (int i = 0; i < co.Length; i++) {
                Destroy(co[i]);
            }
        } else {
            Debug.LogError("无法合并材质");
        }
    }

    //场景物件按照0点防御来算.
    int CalcDamage(MeteorUnit attacker, AttackDes attack = null)
    {
        //(((武器攻击力 + buff攻击力) x 招式攻击力） / 100) - （敌方武器防御力 + 敌方buff防御力） / 10
        //你的攻击力，和我的防御力之间的计算
        //attacker.damage.PoseIdx;
        if (GameStateMgr.Ins.gameStatus.EnableGodMode)
            return 100000;
        int DefTmp = 0;
        AttackDes atk = attacker.CurrentDamage;
        if (atk == null)
            atk = attack;
        if (atk == null) {
            Debug.LogError("CalcDamage can not find attackdes");
            return 1;
        }
        int WeaponDamage = attacker.CalcDamage();
        int PoseDamage = MenuResLoader.Ins.FindOpt(atk.PoseIdx, 3).second[0].flag[6];
        int BuffDamage = attacker.Attr.CalcBuffDamage();
        int realDamage = Mathf.FloorToInt((((WeaponDamage + BuffDamage) * PoseDamage) / 100.0f - (DefTmp)));
        return realDamage;
    }

    public void OnDamage(MeteorUnit attacker, AttackDes attck = null)
    {
        int realDamage = CalcDamage(attacker, attck);
        //非铁箱子， 木箱子，酒坛， 桌子 椅子的受击处理
        if (MethodOnAttack != null)
            MethodOnAttack.Invoke(CombatData.Ins.GScript, new object[] { InstanceId, attacker.InstanceId, realDamage });
        else if (OnAttackCallBack != null)
        {
            OnAttackCallBack.Invoke(InstanceId, Index, realDamage);
        }
    }

    void SetCollision(bool en)
    {
        Collider[] co = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < co.Length; i++)
        {
            if (en)
                co[i].isTrigger = false;
            else
            {
                MeshCollider mc = co[i] as MeshCollider;
                if (mc != null)
                {
                    if (OnIdle != null)
                        mc.enabled = false;
                    else
                    {
                        mc.convex = true;
                        mc.inflateMesh = true;
                        mc.skinWidth = 0.2f;
                        mc.isTrigger = true;
                        mc.enabled = true;
                    }
                }
                else
                {
                    if (OnIdle != null)
                        co[i].enabled = false;
                    else
                    {
                        co[i].isTrigger = true;
                        co[i].enabled = true;
                    }
                }
            }
        }
    }

    public bool HasCollision()
    {
        return collisions.Count != 0;
    }

    public List<Collider> GetCollsion()
    {
        return collisions;
    }

    public int GetSceneItem(string feature)
    {
        if (feature == "pose")
        {
            if (player != null)
                return player.GetPose();
            return -1;
        }
        else if (feature == "state")
        {
            if (player != null)
                return player.GetStatus();
        }
        else if (feature == "index")
        {
            return InstanceId;
        }
        return -1;
    }

    bool asDrop = false;
    public void SetAsDrop()
    {
        asDrop = true;
    }

    public void SetAutoDestroy(float t)
    {
        Invoke("AutoDestroy", t);
    }

    public void AutoDestroy()
    {
        //如果某人拾取了flag，不要再刷新镖物
        if (!U3D.GetFlag())
        {
            for (int i = 0; i < MeteorManager.Ins.SceneItems.Count; i++)
            {
                if (MeteorManager.Ins.SceneItems[i] != this
                    && MeteorManager.Ins.SceneItems[i].ItemInfo != null
                    && MeteorManager.Ins.SceneItems[i].ItemInfo.IsFlag())
                {
                    MeteorManager.Ins.SceneItems[i].OnRefresh();
                    break;
                }
            }
        }
        MeteorManager.Ins.OnDestroySceneItem(this);
        GameObject.Destroy(gameObject);
    }

    int Index;
    public void OnAttack(System.Func<int, int, int, int> act, System.Action<int, int> idle)
    {
        OnAttackCallBack = act;
        OnIdle = idle;
        RefreshCollision();
        Index = int.Parse(name.Substring(name.Length - 2));
        Utility.SetObjectLayer(gameObject, LayerManager.Trigger);
    }

    public bool CanPickup()
    {
        if (ItemInfo != null && (ItemInfo.IsItem()))
            if (root != null && root.gameObject.activeInHierarchy && ItemInfo.second.Length != 0)
                return true;
        if (ItemInfo != null && ItemInfo.IsFlag())
            if (root != null)
                return true;
        return false;
    }
}
