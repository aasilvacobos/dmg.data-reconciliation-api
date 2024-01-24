using DMG.ProviderInvoicing.IO.Logging;

namespace DMG.ProviderInvoicing.IO.Utility;

/// Handles logging for the  adapter.
internal static class UtilityLogger
{
    private const string LogMessagePrefix = @"[IO-UTL]"; 

    internal static void Debug(string message) => Logger.Debug($"{LogMessagePrefix} {message}");
    internal static void Info(string message) => Logger.Info($"{LogMessagePrefix} {message}");
    internal static void Warning(string message) => Logger.Warning($"{LogMessagePrefix} {message}");
    internal static void Error(string message) => Logger.Error($"{LogMessagePrefix} {message}");
    internal static void Exception(Exception ex, string message) => Logger.Exception(ex, $"{LogMessagePrefix} {message}");
    internal static void Emergency(string message) => Logger.Emergency($"{LogMessagePrefix} {message}");
}