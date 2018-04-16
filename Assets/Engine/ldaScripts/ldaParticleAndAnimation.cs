using UnityEngine;
using System.Collections;

public class ldaParticleAndAnimation : MonoBehaviour
{
	void Start () 
	{	
		//PlayOnce();
	}
	
	[ContextMenu("Play Loop")]
	public void PlayLoop()
	{
		ParticleSystem[] pss = GetComponentsInChildren<ParticleSystem>(true);
		foreach(ParticleSystem ps in pss)
		{
			ps.loop = true;
			ps.Play();
		}

		ParticleRenderer[] prs = GetComponentsInChildren<ParticleRenderer>(true);
		foreach(ParticleRenderer pr in prs)
		{
			if(pr.GetComponent<Animation>()!=null){
			pr.GetComponent<Animation>().wrapMode = WrapMode.Loop;
			pr.GetComponent<Animation>().Play();
			}
		}

		ParticleAnimator[] pas = GetComponentsInChildren<ParticleAnimator>(true);
		foreach(ParticleAnimator pa in pas)
		{
			if(pa.GetComponent<Animation>()!=null){
				pa.GetComponent<Animation>().wrapMode = WrapMode.Loop;
				pa.GetComponent<Animation>().Play();
			}
		}

		Animation[] anis = GetComponentsInChildren<Animation>(true);
		foreach(Animation an in anis)
		{
			an.wrapMode = WrapMode.Loop;
			an.Play();
		}
	}

    [ContextMenu("Stop Play")]
    public void StopPlay()
    {
        ParticleSystem[] pss = GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in pss)
        {
            ps.loop = false;
            ps.Stop();
        }

        ParticleRenderer[] prs = GetComponentsInChildren<ParticleRenderer>(true);
        foreach (ParticleRenderer pr in prs)
        {
            if (pr.GetComponent<Animation>() != null)
            {
                pr.GetComponent<Animation>().wrapMode = WrapMode.Once;
                pr.GetComponent<Animation>().Stop();
            }
        }

        ParticleAnimator[] pas = GetComponentsInChildren<ParticleAnimator>(true);
        foreach (ParticleAnimator pa in pas)
        {
            if (pa.GetComponent<Animation>() != null)
            {
                pa.GetComponent<Animation>().wrapMode = WrapMode.Once;
                pa.GetComponent<Animation>().Stop();
            }
        }

        Animation[] anis = GetComponentsInChildren<Animation>(true);
        foreach (Animation an in anis)
        {
            an.wrapMode = WrapMode.Once;
            an.Stop();
        }
    }
	
	[ContextMenu("Play Once")]
	public void PlayOnce () 
	{	
		ParticleSystem[] pss = GetComponentsInChildren<ParticleSystem>(true);
		foreach(ParticleSystem ps in pss)
		{
			ps.loop = false;
			ps.Play();
		}
		Animation[] anis = GetComponentsInChildren<Animation>(true);
		foreach(Animation an in anis)
		{
			an.wrapMode = WrapMode.Once;
			an.Play();
		}
	}
}
