using UnityEngine;
using System.Collections;


public class IFLLoader :NetBehaviour {
    public TextAsset IFLFile;
    public int matIndex;
    public string fileNameReadOnly;
    Renderer mesh;
    public Texture[] tex;
    
    int nIndex = 0;
    float delay = 0;//1.0f / 30.0f;//所有帧都是
    float run = 0.0f;
    public bool AutoPlay;
    public bool useSharedMaterial = false;//真则使用材质球的共享材质，使用同一材质球的均会被修改. 否则使用自身的新增材质
    // Use this for initialization
    //影响了共用材质的贴图，这样MAX里读IFL文件的贴图序列就可以正常使用了.IFL存储了一系列图片名
    private new void Awake()
    {
        base.Awake();
        mesh = GetComponent<MeshRenderer>();
        if (mesh == null)
            mesh = GetComponentInChildren<MeshRenderer>();
        if (mesh != null) {
            mesh.sharedMaterial.renderQueue = 3100;
        }
    }

    protected new void OnDestroy()
    {
        base.OnDestroy();
    }

    void Start () {
        delay = 1.0f / 30.0f;
        
        if (AutoPlay)
            LoadIFL();
	}
	
	// Update is called once per frame
	public override void NetUpdate() {
        run += FrameReplay.deltaTime;
        if (PlayAnimation)
            Play();
    }

    public void SetTargetMeshRenderer(Renderer mr)
    {
        mesh = mr;
    }

    bool PlayAnimation = false;
    public void LoadIFL(bool autoPlay = true)
    {
        AutoPlay = autoPlay;
        if (IFLFile != null)
        {
            string[] file = IFLFile.text.Split(new char[] { '\r','\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            tex = new Texture[file.Length];
            for (int i = 0; i < file.Length; i++)
            {
                string fileName = "";
                int nExt = file[i].LastIndexOf('.');
                if (nExt != -1)
                    fileName = file[i].Substring(0, nExt);
                Texture texture = Resources.Load<Texture>(fileName);
                if (texture != null)
                    tex[i] = texture;
            }

            if (tex != null && tex.Length != 0 && autoPlay)
                PlayAnimation = true;
        }
        else
        {
            if (mesh != null)
            {
                if (useSharedMaterial)
                    mesh.sharedMaterials[matIndex] = Resources.Load<Material>("missifl");//全透明材质
                else
                    mesh.materials[matIndex] = Resources.Load<Material>("missifl");
            }
        }
    }

    public void Play()
    {
        if (run > delay)
        {
            if (mesh != null && tex != null)
            {
                nIndex++;
                nIndex %= tex.Length;
                if (tex[nIndex] != null)
                {
                    if (useSharedMaterial)
                        mesh.sharedMaterials[matIndex].SetTexture("_MainTex", tex[nIndex]);
                    else
                        mesh.materials[matIndex].SetTexture("_MainTex", tex[nIndex]);
                }
            }
            run = 0.0f;
        }
    }

    public void PlayNextFrame()
    {
        if (mesh != null && tex != null)
        {
            nIndex++;
            nIndex %= tex.Length;
            if (tex[nIndex] != null)
            {
                if (useSharedMaterial)
                    mesh.sharedMaterials[matIndex].SetTexture("_MainTex", tex[nIndex]);
                else
                    mesh.materials[matIndex].SetTexture("_MainTex", tex[nIndex]);
            }
        }
    }

    public Texture GetTexture(int index)
    {
        if (tex == null)
            return null;
        if (index >= 0 && index < tex.Length)
            return tex[index];
        return null;
    }
}
