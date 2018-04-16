using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/**
 * 挂在动作节点上即可
 * 目前只支持max导出的Legacy动画--Animation
 * 通过K帧做出来的Mecanim动画--Animator暂时不行
 * 
 */

public class ldaAnimationManualSpeed : MonoBehaviour
{
    private List<Animation> animations = new List<Animation>();

    void OnEnable()
    {
        if (animations == null)
            animations = new List<Animation>();
        animations.Clear();
        Animation[] anims = this.GetComponentsInChildren<Animation>(true);

        foreach (Animation a in anims)
        {
            if(a!=null)
                animations.Add(a);
        }

        Animator[] animators = this.GetComponentsInChildren<Animator>(true);
        foreach (Animator ator in animators)
        {
            //if (ator != null)
            //    animations.Add(ator.animation);
            //在 animator组件里把 UpdateMode 修改为UnscaledTime，这样就可以在timescale=0时继续动le
            ator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        //遍历所有子节点动作
        foreach (Animation anim in animations)
        {
            foreach (AnimationState animstate in anim)
            {
                StartCoroutine(ldaAnimationCtrl.PlayAnimation(anim, animstate.name, false, () => Debug.Log("onComplete")));
            }
        }
    }

    //// Use this for initialization
    //void Start()
    //{
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //}
}
    public static class ldaAnimationCtrl
    {

        public static IEnumerator PlayAnimation(this Animation animation, string clipName, bool useTimeScale, Action onComplete)
        {
            Debug.Log("Overwritten Play animation, useTimeScale? " + useTimeScale);
            //We Don't want to use timeScale, so we have to animate by frame..
            if (!useTimeScale)
            {
                Debug.Log("Started this animation! ( " + clipName + " ) ");
                AnimationState _currState = animation[clipName];
                bool isPlaying = true;
                //float _startTime = 0F;
                float _progressTime = 0F;
                float _timeAtLastFrame = 0F;
                float _timeAtCurrentFrame = 0F;
                float deltaTime = 0F;

                animation.Play(clipName);

                _timeAtLastFrame = Time.realtimeSinceStartup;
                while (isPlaying)
                {
                    _timeAtCurrentFrame = Time.realtimeSinceStartup;
                    deltaTime = _timeAtCurrentFrame - _timeAtLastFrame;
                    _timeAtLastFrame = _timeAtCurrentFrame;

                    _progressTime += deltaTime;
                    _currState.normalizedTime = _progressTime / _currState.length;
                    animation.Sample();

                    //Debug.Log(_progressTime);
                    if (_progressTime >= _currState.length)
                    {
                        //Debug.Log("Bam! Done animating");
                        if (_currState.wrapMode != WrapMode.Loop)
                        {
                            //Debug.Log("Animation is not a loop anim, kill it.");
                            //_currState.enabled = false;
                            isPlaying = false;
                        }
                        else
                        {
                            //Debug.Log("Loop anim, continue.");
                            _progressTime = 0.0f;
                        }
                    }
                    yield return new WaitForEndOfFrame();
                }
                //yield return null;
                if (onComplete != null)
                {
                    Debug.Log("Start onComplete");
                    onComplete();
                }
            }
            else
            {
                animation.Play(clipName);
            }
        }
    }

