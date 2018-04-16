using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class AnimationWizard : ScriptableWizard
{
    public GameObject Object;
    public AnimationClip Animation;
    public float PlaySpeed = 1.0f;
    public GameObject Effect;
    public float EffectTime = 0.5f;

    bool mPlaying = false;
    float mAnimTime = 0.0f;
	ParticleSystem[] mParticleSystemArray;

    [MenuItem("GameObject/Engine/AnimationWizard")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<AnimationWizard>("AnimationWizard", "Close", "Play");
    }

    void OnWizardCreate()
    {
    }

    void OnWizardOtherButton()
    {
        mPlaying = !mPlaying;
        if (mPlaying)
        {
            mAnimTime = 0.0f;

            if (Effect)
			{
                Selection.activeGameObject = Effect;
				mParticleSystemArray = Effect.GetComponentsInChildren<ParticleSystem>();
			}

            Object.GetComponent<Animation>().Play(Animation.name);
        }
    }

    void Update()
    {
        if (mPlaying && Object && Animation)
        {
            mAnimTime += PlaySpeed / 60;
            Animation.SampleAnimation(Object, mAnimTime);

            if (mAnimTime < EffectTime)
			{
				foreach (ParticleSystem ps in mParticleSystemArray)
					ps.Simulate(mAnimTime);
			}
			else
            {
                Selection.activeGameObject = Object;
            }

            if (mAnimTime > Animation.length)
            {
                Selection.activeGameObject = Object;
                mPlaying = false;
            }
        }
    }
}
