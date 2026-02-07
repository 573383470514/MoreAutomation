using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MoreAutomation.Contracts.Configuration;

namespace MoreAutomation.UI.Modules.DeveloperTools
{
    /// <summary>
    /// 开发者工具视图模型。
    /// </summary>
    public partial class DeveloperToolsViewModel : ObservableObject
    {
        private readonly MoreAutomation.Application.UseCases.DeveloperTools.DeveloperToolsService _service;

        /// <summary>
        /// 自动化调优参数。
        /// </summary>
        [ObservableProperty]
        private int frameDelayMs;

        [ObservableProperty]
        private int pausePollingDelayMs;

        [ObservableProperty]
        private int maxRetries;

        [ObservableProperty]
        private int retryDelayMs;

        [ObservableProperty]
        private double retryBackoffFactor;

        [ObservableProperty]
        private int circuitTripThreshold;

        /// <summary>
        /// 功能开关列表。
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<FeatureToggleItem> featureToggles = new();

        /// <summary>
        /// 坐标捕捉状态（用于开发者工具展示）。
        /// </summary>
        [ObservableProperty]
        private string coordinateCaptureStatus = "未激活";

        [ObservableProperty]
        private string? lastCapturedCoordinate;

        public DeveloperToolsViewModel(MoreAutomation.Application.UseCases.DeveloperTools.DeveloperToolsService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            LoadConfig();
        }

        private void LoadConfig()
        {
            var tuning = _service.GetTuningConfig();
            FrameDelayMs = tuning.FrameDelayMs;
            PausePollingDelayMs = tuning.PausePollingDelayMs;
            MaxRetries = tuning.MaxRetries;
            RetryDelayMs = tuning.RetryDelayMs;
            RetryBackoffFactor = tuning.RetryBackoffFactor;
            CircuitTripThreshold = tuning.CircuitTripThreshold;

            var toggles = _service.GetFeatureToggles();
            FeatureToggles.Clear();
            foreach (var kv in toggles)
            {
                FeatureToggles.Add(new FeatureToggleItem { Key = kv.Key, Enabled = kv.Value });
            }
        }

        /// <summary>
        /// 保存调优参数。
        /// </summary>
        [RelayCommand]
        private async Task SaveTuningParams()
        {
            var tuning = new AutomationTuningConfig
            {
                FrameDelayMs = FrameDelayMs,
                PausePollingDelayMs = PausePollingDelayMs,
                MaxRetries = MaxRetries,
                RetryDelayMs = RetryDelayMs,
                RetryBackoffFactor = RetryBackoffFactor,
                CircuitTripThreshold = CircuitTripThreshold
            };

            await _service.SaveTuningConfigAsync(tuning);
            System.Windows.MessageBox.Show("调优参数已保存");
        }

        /// <summary>
        /// 启用坐标捕捉模式。
        /// </summary>
        [RelayCommand]
        private void EnableCoordinateCapture()
        {
            CoordinateCaptureStatus = "已激活 - 在游戏窗口按 [Ctrl+Alt+C] 捕捉坐标";
            // TODO: 在此调用实际的坐标捕捉逻辑
        }

        /// <summary>
        /// 禁用坐标捕捉模式。
        /// </summary>
        [RelayCommand]
        private void DisableCoordinateCapture()
        {
            CoordinateCaptureStatus = "未激活";
            LastCapturedCoordinate = null;
        }
    }

    /// <summary>
    /// 功能开关项。
    /// </summary>
    public class FeatureToggleItem : ObservableObject
    {
        private string _key = string.Empty;
        public string Key
        {
            get => _key;
            set => SetProperty(ref _key, value);
        }

        private bool _enabled;
        public bool Enabled
        {
            get => _enabled;
            set => SetProperty(ref _enabled, value);
        }
    }
}
