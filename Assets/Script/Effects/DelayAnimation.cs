using UnityEngine;
using System.Collections;

public class DelayAnimation : MonoBehaviour
{
    public float DelayTime = 0;
	public float LoopMin = 2;
	public float LoopMax = 3;
    public AnimationClip Animation;

    // Use this for initialization
    void Start()
    {
        Invoke("PlayAnim", DelayTime);
    }

    void PlayAnim()
    {
        GetComponent<Animation>().Play(Animation.name);
        Invoke("PlayAnim", Random.Range(LoopMin, LoopMax));
    }
}
