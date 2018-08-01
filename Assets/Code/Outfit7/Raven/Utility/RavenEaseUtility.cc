#ifdef STARLITE
#include "RavenEaseUtility.h"

namespace Starlite {
    namespace Raven {
        const double RavenEaseUtility::c_PiOver2 = 1.5707963267948966192313216916398;
        const double RavenEaseUtility::c_TwoPi = 6.283185307179586476925286766559;

        double RavenEaseUtility::BounceEaseIn(double time, double duration, double unusedOvershootOrAmplitude, double unusedPeriod) {
            return 1 - BounceEaseOut(duration - time, duration, -1, -1);
        }

        double RavenEaseUtility::BounceEaseInOut(double time, double duration, double unusedOvershootOrAmplitude, double unusedPeriod) {
            if (time < duration * 0.5) {
                return BounceEaseIn(time * 2, duration, -1, -1) * 0.5;
            }
            return BounceEaseOut(time * 2 - duration, duration, -1, -1) * 0.5 + 0.5;
        }

        double RavenEaseUtility::BounceEaseOut(double time, double duration, double unusedOvershootOrAmplitude, double unusedPeriod) {
            if ((time /= duration) < (1 / 2.75)) {
                return (7.5625 * time * time);
            }
            if (time < (2 / 2.75)) {
                return (7.5625 * (time -= (1.5 / 2.75)) * time + 0.75);
            }
            if (time < (2.5 / 2.75)) {
                return (7.5625 * (time -= (2.25 / 2.75)) * time + 0.9375);
            }
            return (7.5625 * (time -= (2.625 / 2.75)) * time + 0.984375);
        }

        double RavenEaseUtility::Evaluate(ERavenEaseType easeType, double time, double duration, double overshootOrAmplitude, double period) {
            switch (easeType) {
            case ERavenEaseType::Linear:
                return time / duration;

            case ERavenEaseType::InSine:
                return -Math::Cos(time / duration * c_PiOver2) + 1;

            case ERavenEaseType::OutSine:
                return Math::Sin(time / duration * c_PiOver2);

            case ERavenEaseType::InOutSine:
                return -0.5 * (Math::Cos(Math::kPI * time / duration) - 1);

            case ERavenEaseType::InQuad:
                return (time /= duration) * time;

            case ERavenEaseType::OutQuad:
                return -(time /= duration) * (time - 2);

            case ERavenEaseType::InOutQuad:
                if ((time /= duration * 0.5) < 1)
                    return 0.5 * time * time;
                return -0.5 * ((--time) * (time - 2) - 1);

            case ERavenEaseType::InCubic:
                return (time /= duration) * time * time;

            case ERavenEaseType::OutCubic:
                return ((time = time / duration - 1) * time * time + 1);

            case ERavenEaseType::InOutCubic:
                if ((time /= duration * 0.5) < 1)
                    return 0.5 * time * time * time;
                return 0.5 * ((time -= 2) * time * time + 2);

            case ERavenEaseType::InQuart:
                return (time /= duration) * time * time * time;

            case ERavenEaseType::OutQuart:
                return -((time = time / duration - 1) * time * time * time - 1);

            case ERavenEaseType::InOutQuart:
                if ((time /= duration * 0.5) < 1)
                    return 0.5 * time * time * time * time;
                return -0.5 * ((time -= 2) * time * time * time - 2);

            case ERavenEaseType::InQuint:
                return (time /= duration) * time * time * time * time;

            case ERavenEaseType::OutQuint:
                return ((time = time / duration - 1) * time * time * time * time + 1);

            case ERavenEaseType::InOutQuint:
                if ((time /= duration * 0.5) < 1)
                    return 0.5 * time * time * time * time * time;
                return 0.5 * ((time -= 2) * time * time * time * time + 2);

            case ERavenEaseType::InExpo:
                return (time == 0) ? 0 : Math::Pow(2, 10 * (time / duration - 1));

            case ERavenEaseType::OutExpo:
                if (time == duration)
                    return 1;
                return (-Math::Pow(2, -10 * time / duration) + 1);

            case ERavenEaseType::InOutExpo:
                if (time == 0)
                    return 0;
                if (time == duration)
                    return 1;
                if ((time /= duration * 0.5) < 1)
                    return 0.5 * Math::Pow(2, 10 * (time - 1));
                return 0.5 * (-Math::Pow(2, -10 * --time) + 2);

            case ERavenEaseType::InCirc:
                return -(Math::Sqrt(1 - (time /= duration) * time) - 1);

            case ERavenEaseType::OutCirc:
                return Math::Sqrt(1 - (time = time / duration - 1) * time);

            case ERavenEaseType::InOutCirc:
                if ((time /= duration * 0.5) < 1)
                    return -0.5 * (Math::Sqrt(1 - time * time) - 1);
                return 0.5 * (Math::Sqrt(1 - (time -= 2) * time) + 1);

            case ERavenEaseType::InElastic:
                double s0;
                if (time == 0)
                    return 0;
                if ((time /= duration) == 1)
                    return 1;
                if (period == 0)
                    period = duration * 0.3;
                if (overshootOrAmplitude < 1) {
                    overshootOrAmplitude = 1;
                    s0 = period / 4;
                } else
                    s0 = period / c_TwoPi * Math::ASin(1 / overshootOrAmplitude);
                return -(overshootOrAmplitude * Math::Pow(2, 10 * (time -= 1)) * Math::Sin((time * duration - s0) * c_TwoPi / period));

            case ERavenEaseType::OutElastic:
                double s1;
                if (time == 0)
                    return 0;
                if ((time /= duration) == 1)
                    return 1;
                if (period == 0)
                    period = duration * 0.3;
                if (overshootOrAmplitude < 1) {
                    overshootOrAmplitude = 1;
                    s1 = period / 4;
                } else
                    s1 = period / c_TwoPi * Math::ASin(1 / overshootOrAmplitude);
                return (overshootOrAmplitude * Math::Pow(2, -10 * time) * Math::Sin((time * duration - s1) * c_TwoPi / period) + 1);

            case ERavenEaseType::InOutElastic:
                double s;
                if (time == 0)
                    return 0;
                if ((time /= duration * 0.5) == 2)
                    return 1;
                if (period == 0)
                    period = duration * (0.3 * 1.5);
                if (overshootOrAmplitude < 1) {
                    overshootOrAmplitude = 1;
                    s = period / 4;
                } else
                    s = period / c_TwoPi * Math::ASin(1 / overshootOrAmplitude);
                if (time < 1)
                    return -0.5 * (overshootOrAmplitude * Math::Pow(2, 10 * (time -= 1)) * Math::Sin((time * duration - s) * c_TwoPi / period));
                return overshootOrAmplitude * Math::Pow(2, -10 * (time -= 1)) * Math::Sin((time * duration - s) * c_TwoPi / period) * 0.5 + 1;

            case ERavenEaseType::InBack:
                return (time /= duration) * time * ((overshootOrAmplitude + 1) * time - overshootOrAmplitude);

            case ERavenEaseType::OutBack:
                return ((time = time / duration - 1) * time * ((overshootOrAmplitude + 1) * time + overshootOrAmplitude) + 1);

            case ERavenEaseType::InOutBack:
                if ((time /= duration * 0.5) < 1)
                    return 0.5 * (time * time * (((overshootOrAmplitude *= (1.525)) + 1) * time - overshootOrAmplitude));
                return 0.5 * ((time -= 2) * time * (((overshootOrAmplitude *= (1.525)) + 1) * time + overshootOrAmplitude) + 2);

            case ERavenEaseType::InBounce:
                return BounceEaseIn(time, duration, overshootOrAmplitude, period);

            case ERavenEaseType::OutBounce:
                return BounceEaseOut(time, duration, overshootOrAmplitude, period);

            case ERavenEaseType::InOutBounce:
                return BounceEaseInOut(time, duration, overshootOrAmplitude, period);

            case ERavenEaseType::Hard:
                if (time / duration == 1)
                    return 1;
                else
                    return 0;
            default:
                // OutQuad
                return -(time /= duration) * (time - 2);
            }
        }
    } // namespace Raven
} // namespace Starlite
#endif