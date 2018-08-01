#ifdef STARLITE
#pragma once

#include <Enums/RavenEaseType.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenEaseUtility {
        public:
            static double BounceEaseIn(double time, double duration, double unusedOvershootOrAmplitude, double unusedPeriod);
            static double BounceEaseInOut(double time, double duration, double unusedOvershootOrAmplitude, double unusedPeriod);
            static double BounceEaseOut(double time, double duration, double unusedOvershootOrAmplitude, double unusedPeriod);
            static double Evaluate(ERavenEaseType easeType, double time, double duration, double overshootOrAmplitude, double period);

        public:
            static const double c_PiOver2;
            static const double c_TwoPi;
        };
    } // namespace Raven
} // namespace Starlite
#endif