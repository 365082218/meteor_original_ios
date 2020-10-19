using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//播放一个sfxfile中的一个子特效
//自己追踪目标的位置和旋转。
//子Mesh设置自己的本地坐标和旋转。
[System.Serializable]
public class SFXUnit :NetBehaviour
{
    public SFXEffectPlay parentSfx;
    public int effectIndex;
    public Texture tex;
    public SfxEffect source;
    public string EffectType;
    MeshRenderer mRender;
    MeshFilter mFilter;
    public MeteorUnit mOwner;
    public Collider damageBox;
    public FightBox hitBox;
    public Transform PositionFollow;//同步移动
    public Transform RotateFollow;//同步旋转，若没有，则不用跟随旋转
    public string texture;
    //public Mesh[] mesh;
    public ParticleSystem particle;
    public IFLLoader iflTexture;
    public bool PlayDone = false;
    public bool pause = false;
    bool LookAtC = false;
    /* BOX 立体模型、
AUDIO 声音指向、Audio_3音效 Audio_0 UI音效 Audio_15 多一个字节？
PLANE 平面模型、
DONUT 圆环模型、
MODEL 物品模型、
SPHERE 球体模型、
PARTICLE 颗粒模型、
CYLINDER 圆柱模型、
BILLBOARD 连接模板、公告板
DRAG*/
    //第一个参数 控制层级关系
    //第二个参数，本地坐标系跟随，
    protected new void Awake()
    {
        base.Awake();
    }

    protected new void OnDestroy()
    {
        base.OnDestroy();
        if (mOwner != null)
            mOwner.OnSFXDestroy(this);
    }

    public void Hide()
    {
        if (mRender != null)
            mRender.enabled = false;
        if (particle != null) {
            ParticleSystemRenderer render = particle.GetComponent<ParticleSystemRenderer>();
            if (render != null)
                render.enabled = false;
        }
    }


    //打到的物件列表,0.15S后可再次攻击同个目标
    List<SceneItemAgent> keyS = new List<SceneItemAgent>();
    List<SceneItemAgent> removedS = new List<SceneItemAgent>();
    Dictionary<SceneItemAgent, float> HurtItems = new Dictionary<SceneItemAgent, float>();
    List<MeteorUnit> keyM = new List<MeteorUnit>();
    List<MeteorUnit> removedM = new List<MeteorUnit>();
    Dictionary<MeteorUnit, float> HurtUnit = new Dictionary<MeteorUnit, float>();
    public bool ExistDamage(SceneItemAgent item) {
        return HurtItems.ContainsKey(item);
    }

    public bool ExistDamage(MeteorUnit item) {
        return HurtUnit.ContainsKey(item);
    }

    public void Attack(SceneItemAgent item) {
        HurtItems.Add(item, CombatData.DamageDetectDelay);
    }

    public void Attack(MeteorUnit unit) {
        HurtUnit.Add(unit, CombatData.DamageDetectDelay);
    }
    //public void SaveParticle(string file)
    //{
    //    if (source != null && source.particle != null)
    //    {
    //        System.IO.FileStream fs = System.IO.File.OpenWrite(file);
    //        System.IO.BinaryWriter w = new System.IO.BinaryWriter(fs);
    //        w.Write(source.particle);
    //        w.Flush();
    //        w.Close();
    //        fs.Close();
    //    }
    //}

    //原粒子系统是右手坐标系的，转移到左手坐标系后非常多问题，特别是父子关系以及，跟随和子物体旋转叠加
    public void Init(SfxEffect effect, SFXEffectPlay parent, int index, float timePlayed, bool preLoad = false)
    {
        playedTime = timePlayed;
        PlayDone = false;
        transform.rotation = Quaternion.identity;
        effectIndex = index;
        parentSfx = parent;//当找不到跟随者的时候，向父询问其他特效是否包含此名称.
        name = effect.EffectName;
        EffectType = effect.EffectType;
            //一个是跟随移动，一个是跟随旋转
        mOwner = parent.GetComponent<MeteorUnit>();
        source = effect;
        if (effect.EffectType.Equals("PARTICLE"))
        {
            particle = Instantiate(Resources.Load<GameObject>("SFXParticles"), Vector3.zero, Quaternion.identity, transform).GetComponent<ParticleSystem>();
            ParticleSystem.MainModule mainModule = particle.main;
            ParticleSystem.EmissionModule emissionModule = particle.emission;
            ParticleSystem.ShapeModule shapeModule = particle.shape;
            ParticleSystem.ForceOverLifetimeModule force = particle.forceOverLifetime;
            
            emissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(effect.MaxParticles, effect.MaxParticles);
            mainModule.startLifetime = new ParticleSystem.MinMaxCurve(effect.StartLifetime, effect.StartLifetime2);
            mainModule.maxParticles = effect.MaxParticles;
            mainModule.startSize3D = false;
            mainModule.startSize = new ParticleSystem.MinMaxCurve(effect.startSizeMin, effect.startSizeMax);
            mainModule.startSpeed = new ParticleSystem.MinMaxCurve(effect.Speed, effect.Speed);
            mainModule.loop = effect.particleNotLoop == 0;
            mainModule.duration = effect.frames[effect.FrameCnt - 1].startTime;
            mainModule.gravityModifierMultiplier = effect.gravity;



            //96字节
            if (effect.ParticleBytes == 96) {

            }
            //124字节的，烟雾特效，旋转角度 0-360，shape使用box, render使用billboard
            else if (effect.ParticleBytes == 124)
            {
                mainModule.startRotation3D = effect.EnableParticleRotate == 1;
                if (mainModule.startRotation3D) {
                    mainModule.startRotationX = effect.RotateAxis.x;
                    mainModule.startRotationY = effect.RotateAxis.y;
                    mainModule.startRotationZ = effect.RotateAxis.z;
                }
                mainModule.startRotation = new ParticleSystem.MinMaxCurve(effect.HRotateSpeed, effect.VRotateSpeed);

                shapeModule.shapeType = ParticleSystemShapeType.Box;
                shapeModule.scale = new Vector3(effect.startSizeMin, 0, effect.startSizeMax);
                //shapeModule.box = new Vector3(effect.emitWidth, 0, effect.emitLong);
                ParticleSystemRenderer r = particle.GetComponent<ParticleSystemRenderer>();
                r.renderMode = ParticleSystemRenderMode.Billboard;
                r.minParticleSize = 0.1f;
                r.maxParticleSize = 0.1f;
            }
            //112字节的,
            else if (effect.ParticleBytes == 112) {

            }
        }

        if (string.IsNullOrEmpty(effect.Bone0))
            PositionFollow = mOwner == null ? parentSfx.transform : mOwner.transform;
        else
        if (effect.Bone0.Equals("ROOT"))
            PositionFollow = mOwner == null ? parent.transform : mOwner.ROOTNull;
        else if (effect.Bone0.Equals("Character"))//根骨骼上一级，角色，就是不随着b骨骼走，但是随着d_base走
            PositionFollow = mOwner == null ? parent.transform: mOwner.RootdBase;
        else
            PositionFollow = FindFollowed(effect.Bone0);

        if (string.IsNullOrEmpty(effect.Bone1))
            RotateFollow = mOwner == null ? parent.transform : mOwner.transform;
        else
        if (effect.Bone1.Equals("ROOT"))
            RotateFollow = mOwner == null ? parent.transform : mOwner.ROOTNull;
        else if (effect.Bone1.Equals("Character"))
            RotateFollow = mOwner == null ? parent.transform : mOwner.RootdBase;
        else
            RotateFollow = FindFollowed(effect.Bone1);

        if (PositionFollow != null)
            transform.position = PositionFollow.transform.position;

        mRender = gameObject.GetComponent<MeshRenderer>();
        mFilter = gameObject.GetComponent<MeshFilter>();
        int meshIndex = -1;
        //部分模型是要绕X旋转270的，这样他的缩放，和移动，都只能靠自己
        if (effect.EffectType.Equals("PLANE"))
        {
            meshIndex = 0;
        }
        else if (effect.EffectType.Equals("BOX"))
        {
            meshIndex = 1;
        }
        else if (effect.EffectType.Equals("DONUT"))
        {
            transform.localScale = effect.frames[0].scale;
            meshIndex = 2;
        }
        else if (effect.EffectType.Equals("MODEL"))//自行加载模型
        {
            transform.localScale = effect.frames[0].scale;
            transform.rotation = RotateFollow.rotation * effect.frames[0].quat;
            transform.position = PositionFollow.position + effect.frames[0].pos;
            if (!string.IsNullOrEmpty(effect.Tails[0]))
            {
                string[] des = effect.Tails[0].Split(new char[] { '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (des.Length != 0)
                    Utility.ShowMeteorObject(des[des.Length - 1], transform);
            }
            //这个时候,不要用自带的meshfilter了,让他自己处理显示问题,只要告诉他在哪个地方显示
            meshIndex = 100;
        }
        else if (effect.EffectType.Equals("SPHERE"))
        {
            meshIndex = 3;
        }
        else if (effect.EffectType.Equals("PARTICLE"))
        {
            if (!string.IsNullOrEmpty(effect.Tails[0]))
                PositionFollow = FindFollowed(effect.Tails[0]);
            if (!string.IsNullOrEmpty(effect.Tails[1]))
                RotateFollow = FindFollowed(effect.Tails[1]);
            if (PositionFollow != null)
                transform.position = PositionFollow.transform.position;
            meshIndex = 101;
        }
        else if (effect.EffectType.Equals("CYLINDER"))
        {
            meshIndex = 4;
        }
        else if (effect.EffectType.Equals("BILLBOARD"))
        {
            LookAtC = true;
            meshIndex = 5;
        }
        else if (effect.EffectType.Equals("DRAG"))
        {
            meshIndex = 6;
        }
        //决定模型
        if (meshIndex != -1 && meshIndex < GamePool.Instance.MeshMng.Meshes.Length)
        {
            if (meshIndex == 4)
                mFilter.mesh = SfxMeshGenerator.Ins.CreateCylinder(source.origAtt.y, source.origAtt.x, source.origScale.x);
            else if (meshIndex == 3)
                mFilter.mesh = SfxMeshGenerator.Ins.CreateSphere(source.SphereRadius);
            else if (meshIndex == 0)
                mFilter.mesh = SfxMeshGenerator.Ins.CreatePlane(source.origScale.x, source.origScale.y);
            //else if (meshIndex == 1)
            //    mFilter.mesh = SfxMeshGenerator.Ins.CreateBox(source.origScale.x, source.origScale.y, source.origScale.z);
            else
                mFilter.mesh = GamePool.Instance.MeshMng.Meshes[meshIndex];
        }
        if (effect.Texture.ToLower().EndsWith(".bmp") || effect.Texture.ToLower().EndsWith(".jpg") || effect.Texture.ToLower().EndsWith(".tga"))
        {
            texture = effect.Texture.Substring(0, effect.Texture.Length - 4);
            tex = Resources.Load<Texture>(texture);
        }
        else if (effect.Texture.ToLower().EndsWith(".ifl"))
        {
            iflTexture = gameObject.AddComponent<IFLLoader>();
            iflTexture.IFLFile = Resources.Load<TextAsset>(effect.Texture);
            if (effect.EffectType.Equals("PARTICLE"))
                iflTexture.SetTargetMeshRenderer(particle.GetComponent<ParticleSystemRenderer>());
            iflTexture.LoadIFL();//传递false表示由特效控制每一帧的切换时间.
            tex = iflTexture.GetTexture(0);
        }
        //else
        //    print("effect contains other prefix:" + effect.Texture == null ? " texture is null" : effect.Texture);
        if (tex != null)
        {
            if (effect.EffectType.Equals("PARTICLE"))
            {
                ParticleSystemRenderer render = particle.GetComponent<ParticleSystemRenderer>();
                if (effect.BlendType == 0)
                    render.material.shader = Shader.Find("Unlit/Texture");//普通不透明无光照
                else
                if (effect.BlendType == 1)
                    render.material.shader = Shader.Find("UnlitAdditive");//滤色 加亮 不透明
                else
                if (effect.BlendType == 2)
                    render.material.shader = Shader.Find("UnlitAdditive");//反色+透明度
                else if (effect.BlendType == 3)
                    render.material.shader = Shader.Find("Mobile/Particles/Alpha Blended");//不滤色,支持透明
                else if (effect.BlendType == 4)
                    render.material.shader = Shader.Find("Custom/MeteorBlend4");//滤色，透明
                else
                    render.material.shader = Shader.Find("Unlit/Texture");//普通不透明无光照
                if (texture.Equals("AItem09"))
                {
                    render.enabled = false;
                    particle.Stop();
                    /* 应该是有一个地裂的粒子特效，但是这个特效参数没分析出来.
                     *  tex = Resources.Load<Texture>("AItemParticle");
                        render.material.SetTexture("_MainTex", tex);
                     */
                }
                else
                    render.material.SetTexture("_MainTex", tex);
                render.material.SetColor("_Color", effect.frames[0].colorRGB);
                render.material.SetColor("_TintColor", effect.frames[0].colorRGB);
                render.material.SetFloat("_Intensity", effect.frames[0].TailFlags[9]);
                if (GameStateMgr.Ins.gameStatus != null && !GameStateMgr.Ins.gameStatus.DisableParticle)
                    particle.Play();
            }
            else
            {
                if (effect.BlendType == 0)
                    mRender.material.shader = Shader.Find("Unlit/Texture");//普通不透明无光照
                else
                if (effect.BlendType == 1)
                    mRender.material.shader = Shader.Find("UnlitAdditive");//滤色 加亮 不透明
                else
                if (effect.BlendType == 2)
                    mRender.material.shader = Shader.Find("UnlitAdditive");//反色+透明度
                else if (effect.BlendType == 3)
                    mRender.material.shader = Shader.Find("Mobile/Particles/Alpha Blended");//不滤色,支持透明
                else if (effect.BlendType == 4)
                    mRender.material.shader = Shader.Find("Custom/MeteorBlend4");//滤色，透明
                else
                    mRender.material.shader = Shader.Find("Unlit/Texture");//普通不透明无光照

                if (effect.BlendType == 2)
                {
                    mRender.material.SetColor("_InvColor", effect.frames[0].colorRGB);
                }
                else
                {
                    mRender.material.SetColor("_Color", effect.frames[0].colorRGB);
                    mRender.material.SetColor("_TintColor", effect.frames[0].colorRGB);
                }
                mRender.material.SetFloat("_Intensity", effect.frames[0].TailFlags[9]);
                mRender.material.SetTexture("_MainTex", tex);
                if (effect.uSpeed != 0.0f || effect.vSpeed != 0.0f)
                    tex.wrapMode = TextureWrapMode.Repeat;
                else
                    tex.wrapMode = TextureWrapMode.Clamp;
                mRender.material.SetFloat("_u", effect.uSpeed);
                mRender.material.SetFloat("_v", effect.vSpeed);
            }
        }
        else
        {
            mRender.enabled = false;
        }
        //drag不知道有什么作用，可能只是定位用的挂载点
        if (effect.Hidden == 1 || meshIndex == 6)
        {
            if (particle != null)
            {
                ParticleSystemRenderer render = particle.GetComponent<ParticleSystemRenderer>();
                if (render != null)
                    render.enabled = false;
            }
            mRender.enabled = false;
        }
        if (effect.EffectName.StartsWith("Attack"))
        {
            if (effect.EffectType.ToUpper() == "BOX") {
                BoxCollider bo = gameObject.AddComponent<BoxCollider>();
                //bo.center = Vector3.zero;
                //bo.size = Vector3.one;
                damageBox = bo;
                damageBox.isTrigger = true;
            }
            //} else if (effect.EffectType.ToUpper() == "PLANE") {
            //    BoxCollider bo = gameObject.AddComponent<BoxCollider>();
            //    //bo.center = Vector3.zero;
            //    //bo.size = source.origScale;
            //    damageBox = bo;
            //} 
            else {
                //SphereCollider sph = gameObject.AddComponent<SphereCollider>();
                //damageBox = sph;
                //sph.radius = 1.0f;
                //sph.center = Vector3.zero;
                //Debug.LogError("Attack Effect Create MeshCollider");
                MeshCollider co = gameObject.AddComponent<MeshCollider>();
                co.convex = true;
                co.isTrigger = true;
                damageBox = co;
                //Debug.LogError("attackBox not Box:" + name);
            }
            hitBox = gameObject.AddComponent<FightBox>();
            //多个子特效是可以打到多次的，由子特效存储攻击到的目标，决定后续是否能再击中
            hitBox.Init(mOwner, null);
            damageBox.enabled = false;
            if (U3D.showBox) {
                BoundsGizmos.Instance.AddCollider(damageBox);
                BoundsGizmos.Instance.AddTransform(transform);
            }
        }

        if (preLoad)
        {
            PlayDone = true;
            if (parentSfx != null)
                parentSfx.OnPlayDone(this);
            else
                Destroy(gameObject);
            return;
        }
    }

    public Transform FindFollowed(string bone)
    {
        GameObject bindBone = NodeHelper.Find(bone, parentSfx.gameObject);
        if (bindBone != null)
            return bindBone.transform;
        //不能跨特效搜索，只能在本特效中
        Transform tr = parentSfx.FindEffectByName(bone);
        if (tr != null)
            bindBone = tr.gameObject;
        return bindBone == null ? null : bindBone.transform;
    }

    float playedTime = 0.0f;
    int playedIndex = 1;
    Vector3 temp = new Vector3(0, 0, 0);
    Vector3 temp2 = new Vector3(0, 0, 0);

    // Update is called once per frame
    public override void NetUpdate()
    {
        keyS.Clear();
        keyS.AddRange(HurtItems.Keys);
        removedS.Clear();
        foreach (var each in keyS) {
            HurtItems[each] -= FrameReplay.deltaTime;
            if (HurtItems[each] < 0.0f)
                removedS.Add(each);
        }

        for (int i = 0; i < removedS.Count; i++)
            HurtItems.Remove(removedS[i]);


        keyM.Clear();
        keyM.AddRange(HurtUnit.Keys);
        removedM.Clear();
        foreach (var each in keyM) {
            HurtUnit[each] -= FrameReplay.deltaTime;
            if (HurtUnit[each] < 0.0f)
                removedM.Add(each);
        }

        for (int i = 0; i < removedM.Count; i++)
            HurtUnit.Remove(removedM[i]);

        if (pause || PlayDone) {
            return;
        }
        playedTime += FrameReplay.deltaTime;
        if (playedIndex < source.frames.Count)
        {
            if (playedTime < source.frames[0].startTime && mRender.enabled)
                mRender.enabled = false;
            else if (playedTime >= source.frames[0].startTime && !mRender.enabled && source.Hidden == 0 && EffectType != "DRAG" && tex != null)
                mRender.enabled = true;

            if (playedTime < source.frames[0].startTime)
                return;
            float timeRatio2 = (playedTime - source.frames[playedIndex - 1].startTime) / (source.frames[playedIndex].startTime - source.frames[playedIndex - 1].startTime);

            string vertexColor = "_TintColor";
            if (source.BlendType == 2)
                vertexColor = "_InvColor";

            if (source.EffectType.Equals("BOX"))
            {
                if (source.frames[playedIndex].startTime <= playedTime)
                {
                    OnNewFrame(vertexColor);
                    temp.x = source.origScale.x * source.frames[playedIndex - 1].scale.x;
                    temp.y = source.origScale.y * source.frames[playedIndex - 1].scale.y;
                    temp.z = source.origScale.z * source.frames[playedIndex - 1].scale.z;
                    transform.localScale = temp;
                }
                else
                {
                    OnLastFrame(timeRatio2, vertexColor);
                    temp.x = source.origScale.x * source.frames[playedIndex - 1].scale.x;
                    temp.y = source.origScale.y * source.frames[playedIndex - 1].scale.y;
                    temp.z = source.origScale.z * source.frames[playedIndex - 1].scale.z;
                    temp2.x = source.origScale.x * source.frames[playedIndex].scale.x;
                    temp2.y = source.origScale.y * source.frames[playedIndex].scale.y;
                    temp2.z = source.origScale.z * source.frames[playedIndex].scale.z;
                    transform.localScale = Vector3.Lerp(temp, temp2, timeRatio2);
                }
            }
            else if (source.EffectType.Equals("CYLINDER"))
            {
                if (source.frames[playedIndex].startTime <= playedTime)
                {
                    OnNewFrame(vertexColor);
                    transform.localScale = source.frames[playedIndex - 1].scale;
                }
                else
                {
                    OnLastFrame(timeRatio2, vertexColor);
                    transform.localScale = Vector3.Lerp(source.frames[playedIndex - 1].scale, source.frames[playedIndex].scale, timeRatio2);
                }
            }
            else if (source.EffectType.Equals("PLANE"))
            {
                if (source.frames[playedIndex].startTime <= playedTime)
                {
                    OnNewFrame(vertexColor);
                    transform.localScale = source.frames[playedIndex - 1].scale;
                }
                else
                {
                    OnLastFrame(timeRatio2, vertexColor);
                    transform.localScale = Vector3.Lerp(source.frames[playedIndex - 1].scale, source.frames[playedIndex].scale, timeRatio2);
                }
            }
            else if (source.EffectType.Equals("SPHERE"))
            {
                if (source.frames[playedIndex].startTime <= playedTime)
                {
                    OnNewFrame(vertexColor);
                    transform.localScale = source.frames[playedIndex - 1].scale;
                }
                else
                {
                    OnLastFrame(timeRatio2, vertexColor);
                    transform.localScale = Vector3.Lerp(source.frames[playedIndex - 1].scale, source.frames[playedIndex].scale, timeRatio2);
                }
            }
            else if (source.EffectType.Equals("BILLBOARD"))
            {
                if (source.frames[playedIndex].startTime <= playedTime)
                {
                    OnNewFrame(vertexColor);
                    if (Camera.main != null)
                        transform.LookAt(Camera.main.transform);
                    temp.x = source.origScale.x * source.frames[playedIndex - 1].scale.x;
                    temp.y = source.origScale.y * source.frames[playedIndex - 1].scale.y;
                    temp.z = source.origScale.z * source.frames[playedIndex - 1].scale.z;
                    transform.localScale = temp;
                }
                else
                {
                    OnLastFrame(timeRatio2, vertexColor);
                    if (Camera.main != null)
                        transform.LookAt(Camera.main.transform);
                    temp.x = source.origScale.x * source.frames[playedIndex - 1].scale.x;
                    temp.y = source.origScale.y * source.frames[playedIndex - 1].scale.y;
                    temp.z = source.origScale.z * source.frames[playedIndex - 1].scale.z;
                    temp2.x = source.origScale.x * source.frames[playedIndex].scale.x;
                    temp2.y = source.origScale.y * source.frames[playedIndex].scale.y;
                    temp2.z = source.origScale.z * source.frames[playedIndex].scale.z;
                    transform.localScale = Vector3.Lerp(temp, temp2, timeRatio2);
                }
            }
            else if (source.EffectType.Equals("MODEL"))
            {
                //第一帧开始时间为-1.表示无限时播放
                if (source.frames[playedIndex].startTime <= playedTime)
                {
                    OnNewFrame(vertexColor);
                    transform.localScale = source.frames[playedIndex - 1].scale;
                }
                else
                {
                    OnLastFrame(timeRatio2, vertexColor);
                    transform.localScale = Vector3.Lerp(source.frames[playedIndex - 1].scale, source.frames[playedIndex].scale, timeRatio2);
                }
            }
            else if (source.EffectType.Equals("PARTICLE"))
            {
                if (source.frames[playedIndex].startTime <= playedTime)
                {
                    OnNewFrame(vertexColor);
                    transform.localScale = source.frames[playedIndex - 1].scale;
                    ParticleSystemRenderer render = particle.GetComponent<ParticleSystemRenderer>();
                    render.material.SetColor("_Color", source.frames[playedIndex - 1].colorRGB);
                    render.material.SetColor(vertexColor, source.frames[playedIndex - 1].colorRGB);
                    //粒子不要设置强度了，原版本的粒子实现和UNITY的不一样
                    render.material.SetFloat("_Intensity", (source.FadeOut == 1 ? source.FadeOutAlpha * source.frames[playedIndex - 1].TailFlags[9] : source.frames[playedIndex - 1].TailFlags[9]));
                    //particle.Play();
                    //particle.Emit(source.MaxParticles);
                    //particle.Simulate();
                    playedIndex++;
                    //playedTime = 0.0f;
                }
                else
                {
                    OnLastFrame(timeRatio2, vertexColor);
                    transform.localScale = Vector3.Lerp(source.frames[playedIndex - 1].scale, source.frames[playedIndex].scale, timeRatio2);
                }
            }
            else if (source.EffectType.Equals("DONUT"))
            {
                if (source.frames[playedIndex].startTime <= playedTime)
                {
                    OnNewFrame(vertexColor);
                    transform.localScale = source.frames[playedIndex - 1].scale;
                }
                else
                {
                    OnLastFrame(timeRatio2, vertexColor);
                    transform.localScale = Vector3.Lerp(source.frames[playedIndex - 1].scale, source.frames[playedIndex].scale, timeRatio2);
                }
            }
            else if (source.EffectType.Equals("DRAG"))
            {
                if (source.frames[playedIndex].startTime <= playedTime)
                {
                    OnNewFrame(vertexColor);
                    transform.localScale = source.frames[playedIndex - 1].scale;
                }
                else
                {
                    OnLastFrame(timeRatio2, vertexColor);
                    transform.localScale = Vector3.Lerp(source.frames[playedIndex - 1].scale, source.frames[playedIndex].scale, timeRatio2);
                }
            }
        }
        else
        {
            PlayDone = true;
            if (parentSfx != null) {
                if (parentSfx.loop) {
                    RePlay();
                    return;
                }
                parentSfx.OnPlayDone(this);
            }
            else
                Destroy(gameObject);
        }
    }

    //重复播放
    public void RePlay()
    {
        playedTime = 0.0f;
        playedIndex = 1;
        PlayDone = false;
    }

    public void ShowKeyFrame(string vertexColor)
    {
        if (source.localSpace == 0 && RotateFollow == null && PositionFollow == null) {

        } else {
            if (RotateFollow != null)
                transform.rotation = (RotateFollow.rotation * source.frames[playedIndex].quat);
            else
                transform.rotation = source.frames[playedIndex].quat;

            //if (source.localSpace == 1)
            //{
            if (RotateFollow != null) {
                Vector3 targetPos = RotateFollow.TransformPoint(source.frames[playedIndex].pos) - RotateFollow.position;
                if (PositionFollow != null)
                    transform.position = PositionFollow.position + targetPos;
                else
                    transform.position = targetPos;
            }
            //}
            else {
                if (PositionFollow != null)
                    transform.position = PositionFollow.position + source.frames[playedIndex].pos;
                else
                    transform.position = source.frames[playedIndex].pos;
            }

        }
        mRender.material.SetFloat("_Intensity", source.frames[playedIndex].TailFlags[9]);
        mRender.material.SetColor(vertexColor, source.frames[playedIndex].colorRGB);
    }

    public void OnNewFrame(string vertexColor)
    {
        ShowKeyFrame(vertexColor);
        playedIndex++;
    }

    public void OnLastFrame(float timeRatio2, string vertexColor)
    {
        if (source.localSpace == 0 && RotateFollow == null && PositionFollow == null) {

        } else {
            if (RotateFollow != null)
                transform.rotation = (RotateFollow.rotation * Quaternion.Slerp(source.frames[playedIndex - 1].quat, source.frames[playedIndex].quat, timeRatio2));
            else
                transform.rotation = Quaternion.Slerp(source.frames[playedIndex - 1].quat, source.frames[playedIndex].quat, timeRatio2);

            //if (source.localSpace == 1)
            //{
            if (RotateFollow != null) {
                Vector3 targetPos = RotateFollow.TransformPoint(Vector3.Lerp(source.frames[playedIndex - 1].pos, source.frames[playedIndex].pos, timeRatio2)) - RotateFollow.position;
                if (PositionFollow != null)
                    transform.position = PositionFollow.position + targetPos;
                else
                    transform.position = targetPos;
            }
        //}
        else {
                if (PositionFollow != null)
                    transform.position = PositionFollow.position + Vector3.Lerp(source.frames[playedIndex - 1].pos, source.frames[playedIndex].pos, timeRatio2);
                else
                    transform.position = Vector3.Lerp(source.frames[playedIndex - 1].pos, source.frames[playedIndex].pos, timeRatio2);
            }
        }
        mRender.material.SetFloat("_Intensity", Mathf.Lerp(source.FadeOut == 1 ? source.FadeOutAlpha * source.frames[playedIndex - 1].TailFlags[9] : source.frames[playedIndex - 1].TailFlags[9], source.FadeOut == 1 ? source.FadeOutAlpha * source.frames[playedIndex].TailFlags[9] : source.frames[playedIndex].TailFlags[9], timeRatio2));
        mRender.material.SetColor(vertexColor, Color.Lerp(source.frames[playedIndex - 1].colorRGB, source.frames[playedIndex].colorRGB, timeRatio2));
    }

    //这个只碰撞瓶罐和建筑。与角色的碰撞由其他地方处理.
    public void ChangeAttack(bool open)
    {
        //播放完毕不允许再开开，否则特效重复攻击
        if (PlayDone && open)
            return;
        ChangeAttackCore(open);
    }

    void ChangeAttackCore(bool open)
    {
        if (damageBox != null)
        {
            if (damageBox.enabled != open)
                damageBox.enabled = open;
        }

        if (hitBox != null) {
            hitBox.ChangeAttack(open);
        }
    }
}
