using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MoreAutomation.Contracts.Configuration;

namespace MoreAutomation.Infrastructure.Config
{
    public class JsonConfigService
    {
        private readonly string _configPath;
        private readonly SemaphoreSlim _configLock = new(1, 1);
        private AppConfig _cache;

        public JsonConfigService()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NIGHTHAVEN");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            _configPath = Path.Combine(folder, "config.json");
            _cache = new AppConfig();
        }

        public AppConfig GetConfig()
        {
            if (!File.Exists(_configPath))
            {
                return _cache;
            }

            try
            {
                string json = File.ReadAllText(_configPath);
                _cache = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            catch (JsonException)
            {
                // 容错处理：配置损坏时保持默认值，避免阻断主流程。
                _cache = new AppConfig();
            }
            catch (IOException)
            {
                // 文件读写冲突场景下回退缓存值，保证稳定性。
            }

            return _cache;
        }

        public async Task SaveConfigAsync(AppConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);

            await _configLock.WaitAsync();
            try
            {
                _cache = config;
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_configPath, json);
            }
            finally
            {
                _configLock.Release();
            }
        }

        /// <summary>
        /// 同步保存配置到磁盘（UI 快速持久化）。
        /// </summary>
        public void SaveConfig(AppConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);

            try
            {
                _cache = config;
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[JsonConfigService] 配置保存失败: {ex.Message}");
                throw;
            }
        }
    }
}
