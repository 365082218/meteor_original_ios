using UnityEngine;
using System.Collections.Generic;

public class OnHitEffect : MonoBehaviour
{
	public Color EffectColor = new Color(0.8f, 0, 0,1);
    public float EffectTime = 0.2f;

    List<KeyValuePair<Material, Color>> mBaseColor = new List<KeyValuePair<Material, Color>>();

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
		{
            Transform child = gameObject.transform.GetChild(i);
            Renderer renderer = child.GetComponent<Renderer>();
            if (!renderer)
                continue;

            foreach (Material mat in renderer.materials)
            {
				if(!mat.HasProperty("_kcolor"))continue;
                Color NomalColor = new Color(0,0,0,0);
                mBaseColor.Add(new KeyValuePair<Material, Color>(mat, NomalColor));
				mat.SetColor("_kcolor",EffectColor);
            }
        }
        Invoke("Finished", EffectTime);
    }

    void Finished()
    {
        foreach (KeyValuePair<Material, Color> pair in mBaseColor)
            pair.Key.SetColor("_kcolor", pair.Value);

        Destroy(this);
    }

    public void Reset()
    {
        CancelInvoke();
        Invoke("Finished", EffectTime);
    }

    public static void Attach(GameObject obj)
    {
		if (!obj)
			return;
		
        OnHitEffect effect = obj.GetComponent<OnHitEffect>();
        if (effect)
            effect.Reset();
        else
            obj.AddComponent<OnHitEffect>();
    }
}
