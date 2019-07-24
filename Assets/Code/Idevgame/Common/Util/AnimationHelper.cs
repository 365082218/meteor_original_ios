using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class AnimationHelper
{
    public static IEnumerator PlayAnimation(this Animation animation, string clipName, bool useTimeScale, Action onComplete)
    {
        if (!useTimeScale)
        {
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
                if (_progressTime >= _currState.length)
                {
                    if (_currState.wrapMode != WrapMode.Loop)
                        isPlaying = false;
                    else
                        _progressTime = 0.0f;
                }
                yield return new WaitForEndOfFrame();
            }
            if (onComplete != null)
                onComplete();
        }
        else
            animation.Play(clipName);
    }
}

