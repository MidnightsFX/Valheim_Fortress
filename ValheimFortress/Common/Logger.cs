using BepInEx.Logging;
using System;

namespace ValheimFortress.common
{
    internal static class Logger
    {

        public static LogLevel Level = LogLevel.Info;

        public static void enableDebugLogging(object sender, EventArgs e)
        {
            if (VFConfig.EnableDebugMode.Value)
            {
                Level = LogLevel.Debug;
            }
            else
            {
                Level = LogLevel.Info;
            }
            // set log level
        }

        public static void toggleDebug()
        {
            if (VFConfig.EnableDebugMode.Value)
            {
                Level = LogLevel.Debug;
            }
            else
            {
                Level = LogLevel.Info;
            }
            // set log level
        }

        public static void LogDebug(string message)
        {
            if (Level >= LogLevel.Debug)
            {
                ValheimFortress.Log.LogInfo(message);
            }
        }
        public static void LogInfo(string message)
        {
            if (Level >= LogLevel.Info)
            {
                ValheimFortress.Log.LogInfo(message);
            }
        }

        public static void LogWarning(string message)
        {
            if (Level >= LogLevel.Warning)
            {
                ValheimFortress.Log.LogWarning(message);
            }
        }

        public static void LogError(string message)
        {
            if (Level >= LogLevel.Error)
            {
                ValheimFortress.Log.LogError(message);
            }
        }
    }
}
