#ifdef STARLITE
#pragma once

namespace Starlite {
    namespace Raven {
        SENUM()
        enum class ERavenConditionMode {
            Equal,
            Less,
            LessOrEqual,
            GreaterOrEqual,
            Greater,
            NotEqual,
            BitSet,
            BitNotSet,
            BitMaskSet,
            BitMaskNotSet,
        };
    } // namespace Raven
} // namespace Starlite
#endif