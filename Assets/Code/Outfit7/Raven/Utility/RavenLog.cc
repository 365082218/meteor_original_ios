#include "RavenLog.h"

namespace Starlite {
    namespace Raven {
        namespace Internal {
            SSINGLETON_IMPLEMENT(RavenLog);

            void RavenLog::SetLogCallback(const LogDelegate& callback) {
                e_LogCallback = callback;
            }

            void RavenLog::ClearLogCallback() {
                e_LogCallback.Clear();
            }

            void RavenLog::SetLogLevel(const LogLevel& logLevel) {
                m_LogLevel = logLevel;
            }
        } // namespace Internal
    }     // namespace Raven
} // namespace Starlite
