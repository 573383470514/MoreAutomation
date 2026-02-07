using System;
using System.Collections.Generic;

namespace MoreAutomation.Contracts.Configuration
{
    public class AppConfig
    {
        public string ClientPath { get; set; } = string.Empty;
        public bool IsAgreed { get; set; }
        public string LastMode { get; set; } = "Normal";

        /// <summary>
        /// 运行参数默认值 + 用户覆盖入口。
        /// </summary>
        public AutomationTuningConfig AutomationTuning { get; set; } = new();

        /// <summary>
        /// 模块级/动作级开关矩阵；key 推荐使用 "module" 或 "module:action"。
        /// </summary>
        public Dictionary<string, bool> FeatureToggles { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 回滚到默认配置，保留用户协议与客户端路径。
        /// </summary>
        public void ResetToDefaults()
        {
            LastMode = "Normal";
            AutomationTuning = new AutomationTuningConfig();
            FeatureToggles = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public class AutomationTuningConfig
    {
        public int FrameDelayMs { get; set; } = 100;
        public int PausePollingDelayMs { get; set; } = 500;
    }
}
