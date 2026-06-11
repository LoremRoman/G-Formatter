using System;
using System.IO;
using System.Text.Json;

namespace G_Formatter.Services
{
    public static class ConfigManager
    {
        private class ConfigData
        {
            public bool IsSuspended { get; set; }
            public float IdleOpacity { get; set; }
        }

        private static readonly string CacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");
        private static readonly string ConfigFile = Path.Combine(CacheDir, "cache.json");

        public static void Save(bool isSuspended, float idleOpacity)
        {
            try
            {
                if (!Directory.Exists(CacheDir)) Directory.CreateDirectory(CacheDir);

                var data = new ConfigData { IsSuspended = isSuspended, IdleOpacity = idleOpacity };
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(ConfigFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ConfigManager] Error al guardar config: {ex.Message}");
            }
        }

        public static (bool IsSuspended, float IdleOpacity) Load()
        {
            if (File.Exists(ConfigFile))
            {
                try
                {
                    string json = File.ReadAllText(ConfigFile);
                    var data = JsonSerializer.Deserialize<ConfigData>(json);

                    if (data != null) return (data.IsSuspended, data.IdleOpacity);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ConfigManager] Error al leer config: {ex.Message}");
                }
            }
            return (false, 0.5f);
        }
    }
}