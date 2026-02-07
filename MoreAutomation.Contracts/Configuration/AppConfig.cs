using System;
using System.Collections.Generic;
using MoreAutomation.Contracts.Monitoring;

namespace MoreAutomation.Contracts.Configuration
{
    public partial class AppConfig
    {
        public string ClientPath { get; set; } = string.Empty;
        public bool IsAgreed { get; set; }
        public string LastMode { get; set; } = "Normal";
        public int CurrentServer { get; set; } = 1; // 区服号 1~10
        
        /// <summary>
        /// 代理配置密钥：需要输入正确的密钥才能访问代理设置面板。
        /// </summary>
        public string ProxySecretKey { get; set; } = string.Empty;

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
        // 当识别或执行失败时的恢复策略参数
        public int MaxRetries { get; set; } = 3;
        public int RetryDelayMs { get; set; } = 200;
        public double RetryBackoffFactor { get; set; } = 1.5;
        // 连续失败达到阈值时触发 circuit-breaker
        public int CircuitTripThreshold { get; set; } = 10;
    }

    // UI preferences persisted locally
    public partial class AppConfig
    {
        public bool HideSensitiveAccounts { get; set; } = false;
        
        /// <summary>
        /// 镜像模式保存的自动化动作列表。每个动作包含动作名、描述、采集的比例坐标序列。
        /// </summary>
        public List<MirrorModeAction> MirrorModeActions { get; set; } = new();

        /// <summary>
        /// 后台自动化配置：是否支持后台/最小化状态下完整自动化（虚拟显存截取）。
        /// </summary>
        public BackgroundAutomationConfig BackgroundAutomation { get; set; } = new();
    }

    /// <summary>
    /// 后台自动化运行配置。
    /// </summary>
    public class BackgroundAutomationConfig
    {
        /// <summary>
        /// 是否启用后台自动化（允许最小化/隐藏窗口时继续执行）。
        /// </summary>
        public bool EnableBackgroundMode { get; set; } = true;

        /// <summary>
        /// 是否使用虚拟显存截图（GPU 加速），用于后台截图分析。
        /// </summary>
        public bool UseVirtualDisplayCapture { get; set; } = true;

        /// <summary>
        /// 虚拟显存分辨率（宽）。
        /// </summary>
        public int VirtualDisplayWidth { get; set; } = 1920;

        /// <summary>
        /// 虚拟显存分辨率（高）。
        /// </summary>
        public int VirtualDisplayHeight { get; set; } = 1080;

        /// <summary>
        /// 后台模式下的鼠标/键盘操作方式："Native" = 系统原生（低级 Hook），"Simulated" = 模拟（中级）。
        /// </summary>
        public string InputMethod { get; set; } = "Native";
    }
}
