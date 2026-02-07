using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoreAutomation.Application.Services;
using MoreAutomation.Contracts.Configuration;

namespace MoreAutomation.Application.UseCases.DeveloperTools
{
    /// <summary>
    /// 开发者工具服务：管理坐标捕捉、参数调整、识别阈值等配置。
    /// </summary>
    public class DeveloperToolsService
    {
        private readonly MoreAutomation.Infrastructure.Config.JsonConfigService _configService;
        private readonly ILogService _log;

        public DeveloperToolsService(MoreAutomation.Infrastructure.Config.JsonConfigService configService, ILogService log)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// 获取当前的自动化调优配置。
        /// </summary>
        public AutomationTuningConfig GetTuningConfig()
        {
            var cfg = _configService.GetConfig();
            return cfg.AutomationTuning;
        }

        /// <summary>
        /// 保存自动化调优参数。
        /// </summary>
        public async Task SaveTuningConfigAsync(AutomationTuningConfig tuning)
        {
            if (tuning == null) throw new ArgumentNullException(nameof(tuning));
            
            var cfg = _configService.GetConfig();
            cfg.AutomationTuning = tuning;
            await _configService.SaveConfigAsync(cfg);
            
            try { _log.Append("更新自动化调优参数"); } catch { }
        }

        /// <summary>
        /// 获取功能开关当前状态。
        /// </summary>
        public Dictionary<string, bool> GetFeatureToggles()
        {
            var cfg = _configService.GetConfig();
            return cfg.FeatureToggles;
        }

        /// <summary>
        /// 切换单个功能开关。
        /// </summary>
        public async Task SetFeatureToggleAsync(string key, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("功能开关键不能为空");
            
            var cfg = _configService.GetConfig();
            cfg.FeatureToggles[key] = enabled;
            await _configService.SaveConfigAsync(cfg);
            
            try { _log.Append($"设置功能开关 {key} 为 {enabled}"); } catch { }
        }
    }
}
