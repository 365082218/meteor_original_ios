#ifdef STARLITE
#pragma once

namespace Starlite {
    namespace Raven {
        SENUM() enum class ERavenParameterType { Int, Float, Bool, Enum, BoolTrigger, IntTrigger, EnumTrigger, EnumBitMask, Vector4, Object, ActorList, Gradient };
    }
} // namespace Starlite
#endif