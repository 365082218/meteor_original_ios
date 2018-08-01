#ifdef STARLITE
#pragma once

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenUtility {
        public:
            RavenUtility() = delete;
            static UInt64 HashString(const String& str);
            static String CombineComponentTypeAndFunctionName(const String& componentType, const String& functionName);
            static std::string GetFunctionNameFromPackedFunctionName(const std::string& packedName);

        public:
            static const int c_MaxFunctionParameters;
            static const UInt64 s_InvalidHash;
        };
    } // namespace Raven
} // namespace Starlite
#endif