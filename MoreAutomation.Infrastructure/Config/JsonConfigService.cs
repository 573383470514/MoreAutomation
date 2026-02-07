using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MoreAutomation.Contracts.Configuration;

namespace MoreAutomation.Infrastructure.Config
{
    public class JsonConfigService
    {
        private readonly string _configPath;
        private AppConfig _cache;

        public JsonConfigService()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NIGHTHAVEN");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            _configPath = Path.Combine(folder, "config.json");
            _cache = new AppConfig();
        }

        public AppConfig GetConfig()
        {
            if (File.Exists(_configPath))
            {
                try
                {
                    string json = File.ReadAllText(_configPath);
                    _cache = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                }
                catch { /* 容错处理：返回默认值 */ }
            }
            return _cache;
        }

        public async Task SaveConfigAsync(AppConfig config)
        {
            _cache = config;
            string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_configPath, json);
        }
    }
}