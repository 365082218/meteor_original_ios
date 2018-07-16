using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//管理角色的动画帧，用自己的方式实现动画
public class PoseStatusDebug : MonoBehaviour
{
    public static Dictionary<int, List<Pose>> ActionList = new Dictionary<int, List<Pose>>();
    public Pose mActiveAction = null;
    static Dictionary<int, TextAsset> PosFile = new Dictionary<int, TextAsset>();
    MeteorUnitDebug _Self;
    public EAIStatus Status = EAIStatus.Idle;
    public Transform FollowBone;
    public float FollowBoneScale = 1.0f;
    //[Description("待机=0，移动=1，攻击=2，受伤=3，防御=4")]
    int UnitId;
    public static uint FPK = 6;//FPK = FRAME PER KEY FRAME/表示2个关键帧间含有多少个普通帧,如果和timeScale相乘为负，那么动画应该反向。
    void Awake()
    {
        
    }

    void Start()
    {
    }
    public bool CanMove;
    public bool CanControl;
    public bool CanRotate;
    public bool CanAttack;
    public bool CanJump;
    public bool CanChangeWeapon;
    public bool CanDefence;//僵直或者收招前不可抵御。只可由IDle转入
    public bool CanSkill;
    public bool Jumping = false;
    public float JumpOne = 0.5f;//跳由3个动作组成，第一个动作往上，第二个动作是到达顶点后调整姿势，第三个动作是落地(落地)
    public float JumpTwo = 0.13f;//第二部分调整姿势 4帧 0.13秒
    public float JumpThree = 0.5f;//回落 15帧 0.5S
    public bool onDefence = false;
    public void OnDefence()
    {
        CanDefence = false;
        onDefence = true;
    }
    public void OnReleaseDefence()
    {
        CanDefence = true;
        onDefence = false;
    }
    public void OnJump()
    {
        Jumping = true;
    }

    public void Init()
    {
        _Self = GetComponent<MeteorUnitDebug>();
        load = GetComponent<CharacterLoader>();
        UnitId = _Self == null ? 0 : _Self.UnitId;
        CanMove = CanRotate = CanAttack = CanJump = CanDefence = CanSkill = CanChangeWeapon = true;
        if (!PosFile.ContainsKey(UnitId))
        {
            if (UnitId >= 20)
                PosFile.Add(UnitId, PosFile[0]);
            else
                PosFile.Add(UnitId, Resources.Load<TextAsset>("9.07/" + "P" + UnitId + ".pos"));
            ActionList.Add(UnitId, new List<Pose>());
            ReadPose();
        }
    }
    // Update is called once per frame
    void Update()
    {
    }

    public void StopAction()
    {
        if (load != null)
            load.SetPosData(null);
    }

    public Dictionary<int, int> LinkInput = new Dictionary<int, int>();
    //当前动作响应了连招输入指令，但是还得等到融合帧再去调用该连招动作.否则动作无连贯感和着力感
    public void LinkAction(int idx)
    {
        //找到当前正在跑的动作序列，查看该动作是否含有尾部融合，必须后摇的时候才能切换动作
        if (mActiveAction != null)
        {
            if (mActiveAction.ActionList.Count >=2)
            {
                int blendCount = 0;
                PosAction act = null;
                for (int i = 0; i < mActiveAction.ActionList.Count; i++)
                {
                    if (mActiveAction.ActionList[i].Type == "Blend")
                    {
                        blendCount++;
                        act = mActiveAction.ActionList[i];
                    }
                }
                //有前摇和后摇的.
                if (blendCount >= 2)
                {
                    //当前正处于后摇中，可以立即切换动画
                    int curIndex = load.GetCurrentFrameIndex();
                    if (act.Start < curIndex && act.End > curIndex)
                    {
                        ChangeAction(idx);
                        return;
                    }
                    else
                    {
                        LinkInput[mActiveAction.Idx] = idx;//指明当到达了可融合帧时，要切换到目标动作去.
                        return;
                    }
                }
            }
        }
        //如果动作无后摇之类的，就立即切换动作吧.
        ChangeAction(idx);
    }

    public void TickAction()
    {
        //在上一个动作完成后，播放下一个动作.比如切换武器。完毕后，播放IDle，有默认连接动作的，切换过去，没有的播放默认
        //Debug.Log("TickAction");
        if (!waitPause)
            ChangeAction(mActiveAction.Link);
    }

    bool waitPause = false;
    public void WaitPause()
    {
        waitPause = true;
    }

    public void OnDead()
    {
        CanControl = CanSkill = CanRotate = CanMove = CanJump = CanDefence = CanChangeWeapon = false;
    }
    //根据动作号开始动画.
    public CharacterLoader AnimalCtrlEx { get { return load; } }
    CharacterLoader load;
    //动作在地面还是空中
    //动作是移动 防守 还是攻击 受伤 待机 
    public void ChangeAction(int idx = CommonAction.Idle)
    {
        if (ActionList[UnitId].Count > idx && load != null)
        {
            CanRotate = CanMove = CanJump = CanDefence = CanChangeWeapon = false;
            if (idx == CommonAction.Defence)
            {
                //switch ((EquipWeaponType)_Self.GetCurEquipType())
                //{
                //    case EquipWeaponType.Knife: idx = Data.CommonActionI.KnifeDefence;break;
                //    case EquipWeaponType.Sword: idx = Data.CommonActionI.SwordDefence; break;
                //    case EquipWeaponType.Blade: idx = Data.CommonActionI.BladeDefence; break;
                //    case EquipWeaponType.Lance: idx = Data.CommonActionI.LanceDefence; break;
                //    case EquipWeaponType.Brahchthrust: idx = Data.CommonActionI.BrahchthrustDefence;break;
                //    case EquipWeaponType.Dart: idx = Data.CommonActionI.DartDefence;break;
                //    case EquipWeaponType.Gloves: return;//没找到
                //    case EquipWeaponType.Guillotines: idx = Data.CommonActionI.GuillotinesDefence;break;
                //    case EquipWeaponType.Hammer: idx = Data.CommonActionI.HammerDefence;break;
                //    case EquipWeaponType.NinjaSword:return;
                //    case EquipWeaponType.HeavenLance:return;//乾坤3个都没找到防御姿势
                //    case EquipWeaponType.Gun: return;//火枪是没有防御动作的
                //}
            }
            else
            if (idx == CommonAction.Jump)
            {
                CanChangeWeapon = true;//空中可换武器
                CanMove = true;//跳跃后，可以移动
            }
            //else if (idx == Data.CommonActionI.ShortJump)
            //{
            //    CanChangeWeapon = false;//空中不可换武器
            //    CanMove = true;
            //}
            else if (idx == CommonAction.WalkBackward)
            {
                CanMove = true;
            }
            else if (idx == CommonAction.WalkLeft)
            {
                CanMove = true;
            }
            else if (idx == CommonAction.WalkRight)
            {
                CanMove = true;
            }
            else if (idx == CommonAction.Run)
            {
                CanMove = true;
                CanJump = true;
            }
            else if (idx == CommonAction.Idle)
            {
                CanRotate = CanMove = CanJump = CanDefence = CanChangeWeapon = true;
            }
            load.SetPosData(ActionList[UnitId][idx]);
            mActiveAction = ActionList[UnitId][idx];
            if (idx != 0)
                CanDefence = false;
            else
                CanDefence = true;
            
            
        }
    }


    void ReadPose()
    {
        if (PosFile[UnitId] != null)
        {
            Pose current = null;
            PosAction curAct = null;
            AttackDes att = null;
            DragDes dra = null;
            NextPose nex = null;
            int left = 0;
            int leftAct = 0;
            int leftAtt = 0;
            int leftDra = 0;
            int leftNex = 0;
            string text = System.Text.Encoding.ASCII.GetString(PosFile[UnitId].bytes);
            string[] pos = text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < pos.Length; i++)
            {
                if (current != null && current.Idx == 573)
                {
                    //Debug.Log("get");
                }
                string line = pos[i];
                string[] lineObject = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (lineObject.Length == 0)
                {
                    //Debug.Log("line i:" + i);
                    //空行跳过
                    continue;
                }
                else if (lineObject[0].StartsWith("#"))
                    continue;
                else
                if (lineObject[0] == "Pose" && left == 0 && leftAct == 0)
                {
                    Pose insert = new Pose();
                    ActionList[UnitId].Add(insert);
                    int idx = int.Parse(lineObject[1]);
                    insert.Idx = idx;
                    current = insert;
                }
                else if (lineObject[0] == "{")
                {
                    if (nex != null)
                        leftNex++;
                    else
                    if (dra != null)
                        leftDra++;
                    else
                    if (att != null)
                    {
                        leftAtt++;
                    }
                    else
                        if (curAct != null)
                        leftAct++;
                    else
                        left++;
                }
                else if (lineObject[0] == "}")
                {
                    if (nex != null)
                    {
                        leftNex--;
                        if (leftNex == 0)
                            nex = null;
                    }
                    else
                    if (dra != null)
                    {
                        leftDra--;
                        if (leftDra == 0)
                            dra = null;
                    }
                    else
                    if (att != null)
                    {
                        leftAtt--;
                        if (leftAtt == 0)
                            att = null;
                    }
                    else
                    if (curAct != null)
                    {
                        leftAct--;
                        if (leftAct == 0)
                            curAct = null;
                    }
                    else
                    {
                        left--;
                        if (left == 0)
                            current = null;
                    }

                }
                else if (lineObject[0] == "link" || lineObject[0] == "Link" || lineObject[0] == "Link\t" || lineObject[0] == "link\t")
                {
                    current.Link = int.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "source" || lineObject[0] == "Source")
                {
                    current.SourceIdx = int.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "Start" || lineObject[0] == "start")
                {
                    if (nex != null)
                    {
                        nex.Start = int.Parse(lineObject[1]);
                    }
                    else
                    if (dra != null)
                    {
                        dra.Start = int.Parse(lineObject[1]);
                    }
                    else
                    if (att != null)
                    {
                        att.Start = int.Parse(lineObject[1]);
                    }
                    else
                    if (curAct != null)
                        curAct.Start = int.Parse(lineObject[1]);
                    else
                        current.Start = int.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "End" || lineObject[0] == "end")
                {
                    if (nex != null)
                    {
                        nex.End = int.Parse(lineObject[1]);
                    }
                    else
                    if (dra != null)
                    {
                        dra.End = int.Parse(lineObject[1]);
                    }
                    else
                    if (att != null)
                    {
                        att.End = int.Parse(lineObject[1]);
                    }
                    else
                    if (curAct != null)
                        curAct.End = int.Parse(lineObject[1]);
                    else
                        current.End = int.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "Speed" || lineObject[0] == "speed")
                {
                    if (curAct != null)
                        curAct.Speed = float.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "LoopStart")
                {
                    current.LoopStart = int.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "LoopEnd")
                {
                    current.LoopEnd = int.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "EffectType")
                {
                    current.EffectType = int.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "EffectID")
                {
                    current.EffectID = lineObject[1];
                }
                else if (lineObject[0] == "Blend")
                {
                    PosAction act = new PosAction();
                    act.Type = "Blend";
                    current.ActionList.Add(act);
                    curAct = act;
                }
                else if (lineObject[0] == "Action")
                {
                    PosAction act = new PosAction();
                    act.Type = "Action";
                    current.ActionList.Add(act);
                    curAct = act;
                }
                else if (lineObject[0] == "Attack")
                {
                    att = new AttackDes();
                    att.PoseIdx = current.Idx;
                    current.Attack.Add(att);
                }
                else if (lineObject[0] == "bone")
                {
                    //重新分割，=号分割，右边的,号分割
                    lineObject = line.Split(new char[] { '=' }, System.StringSplitOptions.RemoveEmptyEntries);
                    string bones = lineObject[1];
                    while (bones.EndsWith(","))
                    {
                        i++;
                        lineObject = new string[1];
                        lineObject[0] = pos[i];
                        bones += lineObject[0];
                    }
                    //bones = bones.Replace(' ', '_');
                    string[] bonesstr = bones.Split(new char[] { ',' });
                    for (int j = 0; j < bonesstr.Length; j++)
                    {
                        string b = bonesstr[j].TrimStart(new char[] { ' ', '\"' });
                        b = b.TrimEnd(new char[] { '\"', ' ' });
                        b = b.Replace(' ', '_');
                        att.bones.Add(b);
                    }
                }
                else if (lineObject[0] == "AttackType")
                {
                    att._AttackType = int.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "CheckFriend")
                {
                    att.CheckFriend = int.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "DefenseValue")
                {
                    att.DefenseValue = float.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "DefenseMove")
                {
                    att.DefenseMove = float.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "TargetValue")
                {
                    att.TargetValue = float.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "TargetMove")
                {
                    att.TargetMove = float.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "TargetPose")
                {
                    att.TargetPose = int.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "TargetPoseFront")
                {
                    att.TargetPoseFront = int.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "TargetPoseBack")
                {
                    att.TargetPoseBack = int.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "TargetPoseLeft")
                {
                    att.TargetPoseLeft = int.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "TargetPoseRight")
                {
                    att.TargetPoseRight = int.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "Drag")
                {
                    dra = new DragDes();
                    current.Drag = dra;
                }
                else if (lineObject[0] == "Time")
                {
                    if (nex != null)
                        nex.Time = float.Parse(lineObject[1]);
                    else
                        dra.Time = float.Parse(lineObject[1]);
                }
                else if (lineObject[0] == "Color")
                {
                    string[] rgb = lineObject[1].Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                    dra.Color.x = int.Parse(rgb[0]);
                    dra.Color.y = int.Parse(rgb[1]);
                    dra.Color.z = int.Parse(rgb[2]);
                }
                else if (lineObject[0] == "NextPose")
                {
                    current.Next = new NextPose();
                    nex = current.Next;
                }
                else if (lineObject[0] == "{}")
                {
                    current = null;
                    continue;
                }
                else
                {
                    Debug.Log("line :" + i + " can t understand：" + pos[i]);
                    break;
                }
            }
        }
    }
}



