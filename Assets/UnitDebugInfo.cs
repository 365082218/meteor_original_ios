using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UnitDebugInfo : MonoBehaviour {
    [SerializeField]
    private Text scriptTemplate;
    [SerializeField]
    private Text ID;
    [SerializeField]
    private Text HP;
    [SerializeField]
    private Text Position;
    [SerializeField]
    private Text Angry;
    [SerializeField]
    private Text Att;
    [SerializeField]
    private Text Def;
    [SerializeField]
    private Text Speed;
    [SerializeField]
    private Text Target;
    MeteorUnit target;
    [SerializeField]
    private Text ActionFrame;
    public void SetOwner(MeteorUnit owner)
    {
        target = owner;
    }
    // Use this for initialization
    Coroutine refresh;
    IEnumerator Refresh()
    {
        while (true)
        {
            if (target != null)
            {
                scriptTemplate.text = "脚本文件:" + target.Attr.NpcTemplate + ".pst";
                ID.text = "角色编号:" + target.InstanceId;
                HP.text = "气血:" + target.Attr.hpCur + "/" + target.Attr.HpMax;
                Position.text = "坐标:" + string.Format("{0:f2},{1:f2}, {2:f2}", target.transform.position.x, target.transform.position.y, target.transform.position.z);
                Angry.text = "怒气:" + target.AngryValue;
                Att.text = "攻击力:" + (target.CalcDamage() + target.Attr.CalcBuffDamage());
                Def.text = "防御力:" + (target.CalcDef() + target.Attr.CalcBuffDef());
                Speed.text = "速度:" + target.Speed;
                Target.text = "锁定目标:" + (target.GetLockedTarget() != null ? target.GetLockedTarget().name : "-");
                ActionFrame.text = string.Format("动作源{0:d1}-动作ID{1}-帧:{2}", target.posMng.mActiveAction.SourceIdx, target.posMng.mActiveAction.Idx, target.charLoader.curIndex);
            }
            yield return 0;
        }
    }

	void Start () {
        if (refresh == null)
            refresh = StartCoroutine(Refresh());
    }

    private void OnEnable()
    {
        if (refresh == null)
            refresh = StartCoroutine(Refresh());
    }
    private void OnDisable()
    {
        if (refresh != null)
        {
            StopCoroutine(refresh);
            refresh = null;
        }
    }
}
