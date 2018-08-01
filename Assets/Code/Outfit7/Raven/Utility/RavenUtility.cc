#ifdef STARLITE
#include "RavenUtility.h"

#include <sstream>

namespace Starlite {
    namespace Raven {
        const UInt64 RavenUtility::s_InvalidHash = RavenUtility::HashString("INVALID_PROPERTY");
        const int RavenUtility::c_MaxFunctionParameters = 10;

        static void Split(const std::string& s, char delim, std::vector<std::string>& elems) {
            std::stringstream ss;
            ss.str(s);
            std::string item;
            while (std::getline(ss, item, delim)) {
                elems.push_back(item);
            }
        }

        static std::vector<std::string> Split(const std::string& s, char delim) {
            std::vector<std::string> elems;
            Split(s, delim, elems);
            return elems;
        }

        UInt64 RavenUtility::HashString(const String& str) {
            return Hash64WithSeed(str.GetCString(), str.Length(), 0x1337LLU);
        }

        String RavenUtility::CombineComponentTypeAndFunctionName(const String& componentType, const String& functionName) {
            return String::GetFormatted("%s|%s", componentType, functionName);
        }

        std::string RavenUtility::GetFunctionNameFromPackedFunctionName(const std::string& packedName) {
            auto split = Split(packedName, '|');
            return split[0];
        }
    } // namespace Raven
} // namespace Starlite
#endif