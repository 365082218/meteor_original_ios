using UnityEngine;
using System.Collections;
using System;
using System.Linq;

namespace Outfit7.Sequencer {
    public class EaseManager : MonoBehaviour {

        public static float _PiOver2 = 1.570796f;
        public static float _TwoPi = 6.2831853f;

        public enum Ease {
            Linear,
            InSine,
            OutSine,
            InOutSine,
            InQuad,
            OutQuad,
            InOutQuad,
            InCubic,
            OutCubic,
            InOutCubic,
            InQuart,
            OutQuart,
            InOutQuart,
            InQuint,
            OutQuint,
            InOutQuint,
            InExpo,
            OutExpo,
            InOutExpo,
            InCirc,
            OutCirc,
            InOutCirc,
            InElastic,
            OutElastic,
            InOutElastic,
            InBack,
            OutBack,
            InOutBack,
            InBounce,
            OutBounce,
            InOutBounce,
            Hard
            //EaseIn,
            //EaseOut,
            //EaseInO
        }

        public static Ease EaseHotkeys(Ease currentEaseType) {
            UnityEngine.Event evnt = UnityEngine.Event.current;
            if (EventType.KeyDown == evnt.type && KeyCode.DownArrow == evnt.keyCode) {
                try {
                    return  System.Enum.GetValues(typeof(EaseManager.Ease)).Cast<EaseManager.Ease>()
                        .SkipWhile(e => e != currentEaseType).Skip(1).First();
                } catch (InvalidOperationException e) {
                    e.GetType();
                    return (EaseManager.Ease) 0;
                }
            } else if (EventType.KeyDown == evnt.type && KeyCode.UpArrow == evnt.keyCode) {
                try {
                    return System.Enum.GetValues(typeof(EaseManager.Ease)).Cast<EaseManager.Ease>().Reverse()
                        .SkipWhile(e => e != currentEaseType).Skip(1).First();
                } catch (InvalidOperationException e) {
                    e.GetType();
                    return (EaseManager.Ease) System.Enum.GetNames(typeof(EaseManager.Ease)).Length - 1;
                }
            } else {
                return currentEaseType;
            }
        }

        public static float Evaluate(Ease easeType, float time, float duration, float overshootOrAmplitude, float period) {
            switch (easeType) {
                case Ease.Linear:
                    return time / duration;
                case Ease.InSine:
                    return -(float) Math.Cos(time / duration * _PiOver2) + 1;
                case Ease.OutSine:
                    return (float) Math.Sin(time / duration * _PiOver2);
                case Ease.InOutSine:
                    return -0.5f * ((float) Math.Cos(Mathf.PI * time / duration) - 1);
                case Ease.InQuad:
                    return (time /= duration) * time;
                case Ease.OutQuad:
                    return -(time /= duration) * (time - 2);
                case Ease.InOutQuad:
                    if ((time /= duration * 0.5f) < 1)
                        return 0.5f * time * time;
                    return -0.5f * ((--time) * (time - 2) - 1);
                case Ease.InCubic:
                    return (time /= duration) * time * time;
                case Ease.OutCubic:
                    return ((time = time / duration - 1) * time * time + 1);
                case Ease.InOutCubic:
                    if ((time /= duration * 0.5f) < 1)
                        return 0.5f * time * time * time;
                    return 0.5f * ((time -= 2) * time * time + 2);
                case Ease.InQuart:
                    return (time /= duration) * time * time * time;
                case Ease.OutQuart:
                    return -((time = time / duration - 1) * time * time * time - 1);
                case Ease.InOutQuart:
                    if ((time /= duration * 0.5f) < 1)
                        return 0.5f * time * time * time * time;
                    return -0.5f * ((time -= 2) * time * time * time - 2);
                case Ease.InQuint:
                    return (time /= duration) * time * time * time * time;
                case Ease.OutQuint:
                    return ((time = time / duration - 1) * time * time * time * time + 1);
                case Ease.InOutQuint:
                    if ((time /= duration * 0.5f) < 1)
                        return 0.5f * time * time * time * time * time;
                    return 0.5f * ((time -= 2) * time * time * time * time + 2);
                case Ease.InExpo:
                    return (time == 0) ? 0 : (float) Math.Pow(2, 10 * (time / duration - 1));
                case Ease.OutExpo:
                    if (time == duration)
                        return 1;
                    return (-(float) Math.Pow(2, -10 * time / duration) + 1);
                case Ease.InOutExpo:
                    if (time == 0)
                        return 0;
                    if (time == duration)
                        return 1;
                    if ((time /= duration * 0.5f) < 1)
                        return 0.5f * (float) Math.Pow(2, 10 * (time - 1));
                    return 0.5f * (-(float) Math.Pow(2, -10 * --time) + 2);
                case Ease.InCirc:
                    return -((float) Math.Sqrt(1 - (time /= duration) * time) - 1);
                case Ease.OutCirc:
                    return (float) Math.Sqrt(1 - (time = time / duration - 1) * time);
                case Ease.InOutCirc:
                    if ((time /= duration * 0.5f) < 1)
                        return -0.5f * ((float) Math.Sqrt(1 - time * time) - 1);
                    return 0.5f * ((float) Math.Sqrt(1 - (time -= 2) * time) + 1);
                case Ease.InElastic:
                    float s0;
                    if (time == 0)
                        return 0;
                    if ((time /= duration) == 1)
                        return 1;
                    if (period == 0)
                        period = duration * 0.3f;
                    if (overshootOrAmplitude < 1) {
                        overshootOrAmplitude = 1;
                        s0 = period / 4;
                    } else
                        s0 = period / _TwoPi * (float) Math.Asin(1 / overshootOrAmplitude);
                    return -(overshootOrAmplitude * (float) Math.Pow(2, 10 * (time -= 1)) * (float) Math.Sin((time * duration - s0) * _TwoPi / period));
                case Ease.OutElastic:
                    float s1;
                    if (time == 0)
                        return 0;
                    if ((time /= duration) == 1)
                        return 1;
                    if (period == 0)
                        period = duration * 0.3f;
                    if (overshootOrAmplitude < 1) {
                        overshootOrAmplitude = 1;
                        s1 = period / 4;
                    } else
                        s1 = period / _TwoPi * (float) Math.Asin(1 / overshootOrAmplitude);
                    return (overshootOrAmplitude * (float) Math.Pow(2, -10 * time) * (float) Math.Sin((time * duration - s1) * _TwoPi / period) + 1);
                case Ease.InOutElastic:
                    float s;
                    if (time == 0)
                        return 0;
                    if ((time /= duration * 0.5f) == 2)
                        return 1;
                    if (period == 0)
                        period = duration * (0.3f * 1.5f);
                    if (overshootOrAmplitude < 1) {
                        overshootOrAmplitude = 1;
                        s = period / 4;
                    } else
                        s = period / _TwoPi * (float) Math.Asin(1 / overshootOrAmplitude);
                    if (time < 1)
                        return -0.5f * (overshootOrAmplitude * (float) Math.Pow(2, 10 * (time -= 1)) * (float) Math.Sin((time * duration - s) * _TwoPi / period));
                    return overshootOrAmplitude * (float) Math.Pow(2, -10 * (time -= 1)) * (float) Math.Sin((time * duration - s) * _TwoPi / period) * 0.5f + 1;
                case Ease.InBack:
                    return (time /= duration) * time * ((overshootOrAmplitude + 1) * time - overshootOrAmplitude);
                case Ease.OutBack:
                    return ((time = time / duration - 1) * time * ((overshootOrAmplitude + 1) * time + overshootOrAmplitude) + 1);
                case Ease.InOutBack:
                    if ((time /= duration * 0.5f) < 1)
                        return 0.5f * (time * time * (((overshootOrAmplitude *= (1.525f)) + 1) * time - overshootOrAmplitude));
                    return 0.5f * ((time -= 2) * time * (((overshootOrAmplitude *= (1.525f)) + 1) * time + overshootOrAmplitude) + 2);
                case Ease.InBounce:
                    return BounceEaseIn(time, duration, overshootOrAmplitude, period);
                case Ease.OutBounce:
                    return BounceEaseOut(time, duration, overshootOrAmplitude, period);
                case Ease.InOutBounce:
                    return BounceEaseInOut(time, duration, overshootOrAmplitude, period);
                case Ease.Hard:
                    if (time / duration == 1)
                        return 1;
                    else
                        return 0;
            //case Ease.INTERNAL_Custom:
            //    return customEase(time, duration, overshootOrAmplitude, period);
            //case Ease.INTERNAL_Zero:
            //    // 0 duration tween
            //    return 1;
            //
            //    // Extra custom eases ////////////////////////////////////////////////////
            //case Ease.Flash:
            //    return Flash.Ease(time, duration, overshootOrAmplitude, period);
            //case Ease.InFlash:
            //    return Flash.EaseIn(time, duration, overshootOrAmplitude, period);
            //case Ease.OutFlash:
            //    return Flash.EaseOut(time, duration, overshootOrAmplitude, period);
            //case Ease.InOutFlash:
            //    return Flash.EaseInOut(time, duration, overshootOrAmplitude, period);

            // Default
                default:
                    // OutQuad
                    return -(time /= duration) * (time - 2);
            }
        }

        public static float BounceEaseIn(float time, float duration, float unusedOvershootOrAmplitude, float unusedPeriod) {
            return 1 - BounceEaseOut(duration - time, duration, -1, -1);
        }

        public static float BounceEaseOut(float time, float duration, float unusedOvershootOrAmplitude, float unusedPeriod) {
            if ((time /= duration) < (1 / 2.75f)) {
                return (7.5625f * time * time);
            }
            if (time < (2 / 2.75f)) {
                return (7.5625f * (time -= (1.5f / 2.75f)) * time + 0.75f);
            }
            if (time < (2.5f / 2.75f)) {
                return (7.5625f * (time -= (2.25f / 2.75f)) * time + 0.9375f);
            }
            return (7.5625f * (time -= (2.625f / 2.75f)) * time + 0.984375f);
        }

        public static float BounceEaseInOut(float time, float duration, float unusedOvershootOrAmplitude, float unusedPeriod) {
            if (time < duration * 0.5f) {
                return BounceEaseIn(time * 2, duration, -1, -1) * 0.5f;
            }
            return BounceEaseOut(time * 2 - duration, duration, -1, -1) * 0.5f + 0.5f;
        }
    }
}
