namespace Loupedeck.TarkovKeybindPlugin
{
    using System;
    using System.IO;

    internal static class PluginLog
    {
        private static readonly String LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TarkovKeybindPlugin.log");

        public static void Write(String message)
        {
            try
            {
                var line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
                File.AppendAllText(LogPath, line + Environment.NewLine);
            }
            catch { }
        }

        public static void Clear()
        {
            try { File.WriteAllText(LogPath, ""); }
            catch { }
        }
    }
}
