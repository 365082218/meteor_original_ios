using UnityEngine;
using System.Collections;

//吸收掉落物品
//物品掉落时候挂上此脚本

public class AbsorbDropThings : MonoBehaviour {


    enum EMoveState
    {
        None,
        Rise,
        Flying,
        Rounding,
    };

    [Tooltip("达到目标特效")]
    public string _arrived_effect_name = "FlyToTargetEffect_0001";
    private GameObject _arrived_effect = null;

    public string _toptip_effect_name = "FlyToTargetEffect_0002";
    private GameObject _toptip_effect = null;

    [Tooltip("圆周运动半径")]
    public float _radius_length = 2;
    private float _cur_radius_length;
    [Tooltip("圆周运动角速度")]
    public float _angle_speed = 5;

    private float temp_angle;

    private Vector3 _pos_new;

    [Tooltip("圆周运动中心点")]
    public Vector3 _center_pos;
    [Tooltip("圆周运动中心物体")]
    public GameObject _center_object = null;
    [Tooltip("是否以该物体目前位置为圆心")]
    public bool _center_round_its_init_pos;

    public MeteorUnit mTarget = null;

    //中心物体
    //[Tooltip("中心物体")]
    //public GameObject _target_object = null;
    [Tooltip("是否升高")]
    public bool _raise_its_y = true;
    [Tooltip("升高时间")]
    public float _raise_its_y_time = 2;
    private float _raiseing_its_y_time;
    [Tooltip("升高高度（可为负数）")]
    public float _raise_its_y_distance = 3;

    [Tooltip("是否向心运动")]
    public bool _go_center = true;
    [Tooltip("向心运动时间")]
    public float _go_center_time = 2;
    private float _going_center_time;
    [Tooltip("向心运动距离（可为负数）")]
    public float _go_center_distance = -1;
    [Tooltip("总朝向中心运行")]
    public bool _always_face_to_center = true;

    public delegate void OnFinished(AbsorbDropThings dropthing);
    OnFinished mOnFinished = null;

    [Tooltip("飞向目标旋转使用世界坐标")]
    public bool _fly_rotation_world_space = false;
    [Tooltip("飞向目标旋转角度")]
    public Vector3 _fly_rotation_value = new Vector3(360, 0, 0);
    [Tooltip("飞向目标旋转速度")]
    public float _fly_rotation_speed = 5;

    [Tooltip("飞向目标所用时间")]
    public float _fly_to_target_time = 0.5f;
    private float _flying_to_target_time = 0;

    [Tooltip("飞向目标高度")]
    public float _fly_to_target_height = 1;

    EMoveState mMoveState = EMoveState.None;

    [Tooltip("调试设置运行信息")]
    public bool _debug_update_info = false;

    public GameObject DropThing;

    // Use this for initialization
    void Start()
    {

    }

    void OnEnable()
    {
    }

    public void ldaBegin(int index, MeteorUnit target, Vector3 initpos, GameObject dropthing, OnFinished onFinished)
    {
        //debug 调速
        _fly_rotation_speed = 2;

        _center_object = target.gameObject;

        if (_center_round_its_init_pos)
        {
            _center_pos = transform.localPosition;
        }

        if (_center_object != null)
            _center_pos = _center_object.transform.position;

        _raiseing_its_y_time = _raise_its_y_time;

        _going_center_time = _go_center_time;

        _cur_radius_length = _radius_length;

        ObjectXZFaceToTarget(this.gameObject, _center_object);

        //_arrived_effect = GameObject.Instantiate(Resources.Load(_arrived_effect_name), Vector3.zero, Quaternion.identity) as GameObject;
        //_arrived_effect.transform.parent = this.transform;
        //_arrived_effect.transform.localPosition = Vector3.zero;
        //_arrived_effect.transform.localRotation = Quaternion.identity;
        //_arrived_effect.transform.localScale = Vector3.one;
        //if (_arrived_effect != null)
        //    _arrived_effect.SetActive(false);

        //_toptip_effect = GameObject.Instantiate(Resources.Load(_toptip_effect_name), Vector3.zero, Quaternion.identity) as GameObject;
        //_toptip_effect.transform.parent = this.transform;
        //_toptip_effect.transform.localPosition = Vector3.zero;
        //_toptip_effect.transform.localRotation = Quaternion.identity;
        //_toptip_effect.transform.localScale = Vector3.one;
        //if (_toptip_effect != null)
        //    _toptip_effect.SetActive(false);

        mTarget = target;
        DropThing = dropthing;

        transform.position = initpos;
        //_center_object = target;
        mOnFinished = onFinished;

        _flying_to_target_time = _fly_to_target_time;

        //随机一个时间物品飞行时长不一
        //float rvalue = Random.value * 2;
        //if (Random.Range(0, 10) > 5)
        //    _flying_to_target_time += rvalue;
        //else
        //    _flying_to_target_time -= rvalue;

        //随机一个时间物品分开起飞
        //rvalue = Random.value * 5;
        Invoke("FlyStart", index * 0.2f);
    }

    public void FlyStart()
    {
        mMoveState = EMoveState.Flying;
    }

    // Update is called once per frame
    void Update()
    {
        if (_debug_update_info)
        {
            _debug_update_info = false;
            _raiseing_its_y_time = _raise_its_y_time;
            _going_center_time = _go_center_time;
            _cur_radius_length = _radius_length;

            float rvalue = Random.value * 0.2f;

            if (Random.Range(0, 10) > 5)
            {
                _raiseing_its_y_time += rvalue;
                _going_center_time += rvalue;
                _cur_radius_length += rvalue;
            }
            else
            {
                _raiseing_its_y_time -= rvalue;
                _going_center_time -= rvalue;
                _cur_radius_length -= rvalue;
            }

            //if (_arrived_effect != null)
            //    _arrived_effect.SetActive(false);
        }

        //if (Input.GetKeyUp(KeyCode.Z))
        //{
        //    ObjectXZFaceToTarget(this.gameObject, _center_object);

        //    //ldaBegin(null, transform.position, delegate(AbsorbDropThings dropthing)
        //    //{
        //    //    ////UnitManager.Instance.DestroyCoin(ball.mCoin);
        //    //    ////GameObject.DestroyImmediate(ball.gameObject);
        //    //});
        //}

        //if (Input.GetKeyUp(KeyCode.X))
        //{
        //    mMoveState = EMoveState.Rounding;
        //}
        //if (Input.GetKeyUp(KeyCode.C))
        //{
        //    mMoveState = EMoveState.None;
        //    _debug_update_info = true;
        //}

        if (mMoveState == EMoveState.Flying)
        {
            //if (!_center_object.activeSelf)
            //    _center_object.SetActive(true);

            if (_flying_to_target_time > Time.deltaTime)
            {
                Vector3 targetPos = Vector3.zero;

                if (_center_object != null)
                    targetPos = new Vector3(_center_object.transform.position.x, _center_object.transform.position.y + _fly_to_target_height, _center_object.transform.position.z);

                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime / _flying_to_target_time);
                _flying_to_target_time -= Time.deltaTime;

                transform.Rotate(Time.deltaTime * _fly_rotation_value.x * _fly_rotation_speed,
                                 Time.deltaTime * _fly_rotation_value.y * _fly_rotation_speed,
                                 Time.deltaTime * _fly_rotation_value.z * _fly_rotation_speed,
                                 (_fly_rotation_world_space ? Space.World : Space.Self));
            }
            else
            {
                mMoveState = EMoveState.None;

                //transform.rotation = Quaternion.identity;
                Vector3 rEulerAngles = transform.rotation.eulerAngles;
                rEulerAngles.x = 0;
                rEulerAngles.z = 0;
                transform.rotation = Quaternion.Euler(rEulerAngles);

                //if (this.gameObject.activeSelf)
                //    this.gameObject.SetActive(false);
                //if (_arrived_effect != null)
                //    _arrived_effect.SetActive(true);

                //if (_toptip_effect != null)
                //    _toptip_effect.SetActive(true);


                _arrived_effect = GameObject.Instantiate(Resources.Load(_arrived_effect_name), Vector3.zero, Quaternion.identity) as GameObject;
                _arrived_effect.transform.parent = this.transform;
                _arrived_effect.transform.localPosition = Vector3.zero;
                _arrived_effect.transform.localRotation = Quaternion.identity;
                _arrived_effect.transform.localScale = Vector3.one;
                _arrived_effect.transform.parent = null;//调整好位置后放到世界坐标去，不和物品一起消失
                //if (_arrived_effect != null)
                //    _arrived_effect.SetActive(false);

                _toptip_effect = GameObject.Instantiate(Resources.Load(_toptip_effect_name), Vector3.zero, Quaternion.identity) as GameObject;
                _toptip_effect.transform.parent = this.transform;
                _toptip_effect.transform.localPosition = Vector3.zero;
                _toptip_effect.transform.localRotation = Quaternion.identity;
                _toptip_effect.transform.localScale = Vector3.one;
                _toptip_effect.transform.parent = null;//调整好位置后放到世界坐标去，不和物品一起消失
                //if (_toptip_effect != null)
                //    _toptip_effect.SetActive(false);



                if (mOnFinished != null)
                    mOnFinished(this);

            }
            return;
        }

        //if (mMoveState == EMoveState.Rounding)
        //{
        //    //if (!_center_object.activeSelf)
        //    //    _center_object.SetActive(true);

        //    if (_raise_its_y)
        //    {
        //        if (_raiseing_its_y_time > 0)
        //        {
        //            Vector3 targetPos = _center_object.transform.position;//_center_pos;
        //            if (_center_object != null)
        //                targetPos = new Vector3(_center_object.transform.position.x, _center_object.transform.position.y + _raise_its_y_distance, _center_object.transform.position.z);

        //            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime / _raiseing_its_y_time);
        //            Debug.Log("_raise_its_y_distance  " + _raise_its_y_distance);
        //            _raiseing_its_y_time -= Time.deltaTime;
        //        }
        //    }

        //    if (_go_center)
        //    {
        //        if (_going_center_time > 0)
        //        {
        //            _cur_radius_length = Mathf.Lerp(_cur_radius_length, _radius_length - _go_center_distance, Time.deltaTime / _going_center_time);
        //            _going_center_time -= Time.deltaTime;
        //        }
        //    }

        //    if (_always_face_to_center && _center_object != null)
        //    {
        //        ObjectXZFaceToTarget(this.gameObject, _center_object);
        //    }

        //    temp_angle += _angle_speed * Time.deltaTime; // 
        //    //_pos_new.x = _center_pos.x + Mathf.Cos(temp_angle) * _cur_radius_length;
        //    //_pos_new.y = _center_pos.y + Mathf.Sin(temp_angle) * _cur_radius_length;
        //    //_pos_new.z = transform.localPosition.z;
        //    _pos_new.x = _center_object.transform.position.x + Mathf.Cos(temp_angle) * _cur_radius_length;
        //    _pos_new.y = transform.localPosition.y;
        //    _pos_new.z = _center_object.transform.position.y + Mathf.Sin(temp_angle) * _cur_radius_length;
        //    transform.localPosition = _pos_new;

        //    //debug
        //    if (_raise_its_y && _go_center && _raiseing_its_y_time <= 0 && _going_center_time <= 0)
        //    {
        //    //    //自动跳转到吸取
        //    //    ObjectXZFaceToTarget(this.gameObject, _center_object);

        //    //    ldaBegin(null, transform.position, delegate(AbsorbDropThings dropthing)
        //    //    {
        //    //        ////UnitManager.Instance.DestroyCoin(ball.mCoin);
        //    //        ////GameObject.DestroyImmediate(ball.gameObject);
        //    //    });
        //        mMoveState = EMoveState.Flying;
        //    }

        //    return;
        //}
    }

    public void ObjectXZFaceToTarget(GameObject self, GameObject target)
    {
        if (self == null) return;

        float x, z;
        x = target.transform.position.x - self.transform.position.x;
        z = target.transform.position.z - self.transform.position.z;
        float dir = Mathf.Atan2(x, z);

        Vector3 eulerAngles = self.transform.eulerAngles;
        eulerAngles.y = dir * Mathf.Rad2Deg;
        self.transform.eulerAngles = eulerAngles;
    }
}
