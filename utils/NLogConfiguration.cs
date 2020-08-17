using NLog;
using NLog.Config;
using NLog.Targets;

namespace csharp_to_json_converter.utils
{
    public static class NLogConfiguration
    {
        internal static void Configure(LogLevel logLevel)
        {
            LoggingConfiguration config = new LoggingConfiguration();
            ConsoleTarget consoleTarget = new ConsoleTarget("Log to Console");
            config.AddRule(logLevel, LogLevel.Fatal, consoleTarget);
            LogManager.Configuration = config;
        }
    }
}