#pragma once

namespace Starlite {
    namespace Raven {
        namespace Internal {
            class RavenLog : public StaticSingleton {
                typedef Delegate<String, LogLevel> LogDelegate;

            public:
                RavenLog() = default;

                void SetLogCallback(const LogDelegate& callback);
                void ClearLogCallback();
                void SetLogLevel(const LogLevel& logLevel);

                template <typename... Ts> void Verbose(const char* format, Ts... ts);

                template <typename... Ts> void Info(const char* format, Ts... ts);

                template <typename... Ts> void Warning(const char* format, Ts... ts);

                template <typename... Ts> void Error(const char* format, Ts... ts);

                template <typename... Ts> void VerboseT(const char* tag, const char* format, Ts... ts);

                template <typename... Ts> void InfoT(const char* tag, const char* format, Ts... ts);

                template <typename... Ts> void WarningT(const char* tag, const char* format, Ts... ts);

                template <typename... Ts> void ErrorT(const char* tag, const char* format, Ts... ts);

                template <typename... Ts> void Print(LogLevel logLevel, const char* tag, const char* format, Ts... ts);

            private:
                LogDelegate e_LogCallback;
                LogLevel m_LogLevel = LogLevel::kVerbose;
            };
            SSINGLETON_INSTANCE(RavenLog);

            template <typename... Ts> void RavenLog::Verbose(const char* format, Ts... ts) {
                Print(LogLevel::kVerbose, nullptr, format, std::forward<Ts>(ts)...);
            }

            template <typename... Ts> void RavenLog::Info(const char* format, Ts... ts) {
                Print(LogLevel::kInfo, nullptr, format, std::forward<Ts>(ts)...);
            }

            template <typename... Ts> void RavenLog::Warning(const char* format, Ts... ts) {
                Print(LogLevel::kWarning, nullptr, format, std::forward<Ts>(ts)...);
            }

            template <typename... Ts> void RavenLog::Error(const char* format, Ts... ts) {
                Print(LogLevel::kError, nullptr, format, std::forward<Ts>(ts)...);
            }

            template <typename... Ts> void RavenLog::VerboseT(const char* tag, const char* format, Ts... ts) {
                Print(LogLevel::kVerbose, tag, format, std::forward<Ts>(ts)...);
            }

            template <typename... Ts> void RavenLog::InfoT(const char* tag, const char* format, Ts... ts) {
                Print(LogLevel::kInfo, tag, format, std::forward<Ts>(ts)...);
            }

            template <typename... Ts> void RavenLog::WarningT(const char* tag, const char* format, Ts... ts) {
                Print(LogLevel::kWarning, tag, format, std::forward<Ts>(ts)...);
            }

            template <typename... Ts> void RavenLog::ErrorT(const char* tag, const char* format, Ts... ts) {
                Print(LogLevel::kError, tag, format, std::forward<Ts>(ts)...);
            }

            template <typename... Ts> void RavenLog::Print(LogLevel logLevel, const char* tag, const char* format, Ts... ts) {
                // not thread safe!
                if (m_LogLevel > logLevel) {
                    return;
                }

                if (e_LogCallback) {
                    e_LogCallback(String::GetFormatted(format, std::forward<Ts>(ts)...), logLevel);
                } else {
                    pLog->Print(logLevel, tag, format, std::forward<Ts>(ts)...);
                }
            }
        } // namespace Internal
    }     // namespace Raven
} // namespace Starlite
