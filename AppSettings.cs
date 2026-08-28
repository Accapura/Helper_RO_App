using System;
using System.IO;
using System.Text.Json;

namespace HelperGos
{
    /// <summary>
    /// Small persisted-settings blob: window position/size, theme, font size
    /// and both hotkeys, written to %LocalAppData%\HelperGos\settings.json and
    /// reloaded on the next launch, so the app comes back the way the user
    /// left it instead of resetting to defaults every time.
    /// </summary>
    public sealed class AppSettings
    {
        public double? WindowLeft { get; set; }
        public double? WindowTop { get; set; }
        public double WindowWidth { get; set; } = 560;
        public double WindowHeight { get; set; } = 750;

        public string ThemeName { get; set; } = AppData.DefaultTheme.Name;
        public double FontSizePx { get; set; } = 13;

        public string HotkeyKey { get; set; } = "D9";
        public string LockHotkeyKey { get; set; } = "D0";

        // Set once the user checks "Больше не показывать при запуске" on
        // DownloadLinksDialog -- suppresses that dialog on future launches
        // (see MainWindow_Loaded in MainWindow.xaml.cs).
        public bool HideDownloadLinksDialog { get; set; } = false;

        private static string SettingsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HelperGos", "settings.json");

        public static AppSettings Load()
        {
            try
            {
                string path = SettingsPath;
                if (!File.Exists(path)) return new AppSettings();
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                // Corrupt/unreadable settings file -- fall back to defaults rather
                // than fail startup over a cosmetic persistence feature.
                return new AppSettings();
            }
        }

        public void Save()
        {
            try
            {
                string path = SettingsPath;
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch
            {
                // Best-effort -- never let a failed settings write crash shutdown.
            }
        }
    }
}
