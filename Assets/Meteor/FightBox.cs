using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//逻辑上就是一个触发器
//专门处理设置攻击受击之间的碰撞，受击盒和受击盒，无法碰撞,
//但是受击和攻击盒是同一个，所以只能在逻辑上做区分
//角色的，永远是可以攻击的盒子，和所有可以受击的盒子上做碰撞检测
//其他的所有盒子，互相都没有碰撞检测，这样就可以避免，数量太大的碰撞检测
//盒子一定是一个Trigger，不会阻碍角色的

//碰撞设置
//受击盒-只能与攻击盒碰撞
//攻击盒层-可以和受击盒和攻击盒碰撞
//当一个攻击盒生成以后，只需要把攻击盒的层设置在攻击层
//当一个受击盒生成以后，需要把受击盒设置在受击层
//当攻击结束时，如这个盒子是原来的受击盒中的，需要再调整为受击层
public class FightBox : NetBehaviour {
    public MeteorUnit Target { get { return Owner; } }
    public AttackDes Attack { get { return AttackDef; } }
    MeteorUnit Owner;
    SFXUnit Attacker;
    AttackDes AttackDef;
    static bool Initialize = false;
    public Collider Collider { get { return box; } }
    Collider box;
    private new void Awake() {
        base.Awake();
        box = GetComponent<Collider>();
        if (Initialize)
            return;
        Initialize = true;
    }

    public bool detectCollsion;
    Rigidbody rig;
    public Vector3 center;
    Vector3 half;
    //Vector3 boxCenter;
    //Vector3 boxSize;
    public void ChangeAttack(bool check) {
        detectCollsion = check;
        if (!detectCollsion) {
            if (rig != null) {
                GameObject.Destroy(rig);
                rig = null;
            }
        }
    }
    public override void NetUpdate() {
        if (detectCollsion) {
            bool processed = false;
            if (box is BoxCollider) {
                BoxCollider b = box as BoxCollider;
                center = box.transform.TransformPoint(b.center);
                half = Vector3.Scale(b.size, b.transform.localScale) / 2.0f;
                processed = true;
            } else if (box is MeshCollider) {
                //MeshCollider m = box as MeshCollider;
                //center = box.transform.TransformPoint(m.sharedMesh.bounds.center);
                //half = Vector3.Scale(m.sharedMesh.bounds.size, m.transform.localScale) / 2;
                processed = false;
                //这种面片的情况，不要用近似obb盒，否则不准确，还是用系统带的触发器
                if (rig == null) {
                    rig = box.gameObject.GetComponent<Rigidbody>();
                    if (rig == null)
                        rig = box.gameObject.AddComponent<Rigidbody>();
                    rig.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    rig.useGravity = false;
                    rig.isKinematic = true;
                }
            }

            if (processed) {
                Collider[] colliders = Physics.OverlapBox(center, half, box.transform.rotation, 1 << LayerManager.Bone | 1 << LayerManager.DetectAll | 1 << LayerManager.Trigger);
                for (int i = 0; i < colliders.Length; i++) {
                    if (colliders[i] is BoxCollider) {
                        BoxCollider boxBone = colliders[i] as BoxCollider;
                        if (Owner != null && Owner.HurtList.Contains(boxBone))
                            continue;
                    }
                    DetectDamage(colliders[i]);
                }
            }
        }    
    }

    void OnDrawGizmos() {

        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
        if (detectCollsion) {
            var cacheMatrix = Gizmos.matrix;
            //Gizmos.color = Color.red;
            //Gizmos.matrix = transform.localToWorldMatrix;
            //Gizmos.DrawWireCube(boxCenter, boxSize);
            Gizmos.color = Color.blue;
            Matrix4x4 m = new Matrix4x4();
            m.SetTRS(transform.position, box.transform.rotation, Vector3.one);
            Gizmos.matrix = m;
            Gizmos.DrawWireCube(transform.InverseTransformPoint(center), half * 2);
            Gizmos.matrix = cacheMatrix;
        }
    }

    public new void OnDestroy() {
        base.OnDestroy();
        if (Owner != null) {
            if (Owner.HitList.Contains(this)) {
                Owner.HitList.Remove(this);
            }
        }
        if (Attacker != null) {
            if (Attacker.mOwner.HitList.Contains(this))
                Attacker.mOwner.HitList.Remove(this);
        }
    }

    public void Init(SFXUnit SfxAttack, AttackDes attDef) {
        AttackDef = attDef;
        Attacker = SfxAttack;
        if (Attacker.mOwner != null) {
            Attacker.mOwner.IgnoreOthers(box);
            if (!Attacker.mOwner.HitList.Contains(this))
                Attacker.mOwner.HitList.Add(this);
        }
    }

    public void Init(MeteorUnit owner, AttackDes attDef) {
        AttackDef = attDef;
        Owner = owner;
        if (Owner != null) {
            Owner.IgnoreOthers(box);
            if (!Owner.HitList.Contains(this))
                Owner.HitList.Add(this);
        }
    }

    void DetectDamage(Collider other) {
        if (Owner == null) {
            if (Attacker == null)
                return;
            //角色发射出的特效攻击盒
            SceneItemAgent target = other.gameObject.GetComponentInParent<SceneItemAgent>();
            if (target != null) {
                if (Attacker.ExistDamage(target))
                    return;
                Attacker.Attack(target);
                target.OnDamage(Attacker.mOwner, AttackDef);
            } else {
                MeteorUnit unit = other.gameObject.GetComponentInParent<MeteorUnit>();
                if (unit != null) {
                    if (Attacker.mOwner == unit)
                        return;
                    if (Attacker.mOwner.SameCamp(unit))
                        return;
                    if (Attacker.ExistDamage(unit))
                        return;
                    //Debug.LogError("name:" + name + " hit other:" + other.name);
                    Attacker.Attack(unit);
                    unit.OnAttack(Attacker.mOwner, AttackDef == null ? Attacker.mOwner.CurrentDamage : AttackDef);
                }
            }
            return;
        }
        //角色身体上的攻击盒/武器上的攻击盒
        SceneItemAgent agent = other.gameObject.GetComponentInParent<SceneItemAgent>();
        if (agent != null) {
            if (Owner.ExistDamage(agent))
                return;
            Owner.Attack(agent);
            agent.OnDamage(Owner, AttackDef);
        } else {
            MeteorUnit unit = other.gameObject.GetComponentInParent<MeteorUnit>();
            if (unit != null) {
                if (Owner == unit)
                    return;
                if (Owner.SameCamp(unit))
                    return;
                if (Owner.ExistDamage(unit))
                    return;
                //Debug.LogError("name:" + name + " hit other:" + other.name);
                Owner.Attack(unit);
                unit.OnAttack(Owner, AttackDef == null ? Owner.CurrentDamage : AttackDef);
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (detectCollsion) {
            //Debug.LogError("enter");
            DetectDamage(other);
        }
    }

    private void OnTriggerStay(Collider other) {
        if (detectCollsion) {
            //Debug.LogError("stay");
            DetectDamage(other);
        }
    }
}
