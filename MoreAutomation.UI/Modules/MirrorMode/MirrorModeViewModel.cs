using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreAutomation.Application.Messaging;
using MoreAutomation.Application.Services;
using MoreAutomation.Contracts.Automation;
using MoreAutomation.Contracts.Configuration;
using MoreAutomation.Contracts.Monitoring;
using MoreAutomation.Infrastructure.Config;

namespace MoreAutomation.UI.Modules.MirrorMode
{
    public partial class MirrorModeViewModel : ObservableObject
    {
        private readonly IMirrorAgent _mirrorAgent;
        private readonly ILogService _log;
        private readonly AppConfig _config;
        private readonly JsonConfigService _configService;
        private readonly ICoordinateExecutor _executor;

        [ObservableProperty]
        private string _currentRole = "未初始化";

        [ObservableProperty]
        private string _clientId = "unknown";

        [ObservableProperty]
        private ObservableCollection<string> _connectedFollowers = new();

        [ObservableProperty]
        private string _mirrorLog = string.Empty;

        // 坐标采集相关属性
        [ObservableProperty]
        private bool _isCapturing;

        [ObservableProperty]
        private string _actionName = string.Empty;

        [ObservableProperty]
        private string _actionDescription = string.Empty;

        [ObservableProperty]
        private string _captureStatus = "未开始采集";

        [ObservableProperty]
        private int _capturedCount;

        [ObservableProperty]
        private ObservableCollection<string> _capturedCoordinates = new();

        [ObservableProperty]
        private ObservableCollection<string> _savedActions = new();

        [ObservableProperty]
        private string _selectedActionName = string.Empty;

        // 后台自动化配置属性
        [ObservableProperty]
        private bool _configBackgroundMode = true;

        [ObservableProperty]
        private bool _configUseVirtualDisplay = true;

        [ObservableProperty]
        private int _configDisplayWidth = 1920;

        [ObservableProperty]
        private int _configDisplayHeight = 1080;

        [ObservableProperty]
        private bool _isInputNative = true;

        [ObservableProperty]
        private bool _isInputSimulated = false;

        // 执行相关属性
        [ObservableProperty]
        private bool _isExecuting;

        [ObservableProperty]
        private int _executionProgress; // 0-100

        [ObservableProperty]
        private string _executionStatus = "就绪";

        private CancellationTokenSource? _executionCts;

        private ObservableCollection<ProportionalCoordinate> _currentCoordinateList = new();

        public MirrorModeViewModel(IMirrorAgent mirrorAgent, ILogService log, AppConfig config, JsonConfigService configService, ICoordinateExecutor executor)
        {
            _mirrorAgent = mirrorAgent;
            _log = log;
            _config = config;
            _configService = configService;
            _executor = executor;
            ClientId = "client_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            UpdateRoleDisplay();
            LoadSavedActions();
            LoadBackgroundConfig();
        }

        [RelayCommand]
        private void SetMaster()
        {
            _mirrorAgent.SetMasterMode(true);
            UpdateRoleDisplay();
            RefreshMirrorLog();
        }

        [RelayCommand]
        private void SetFollower()
        {
            _mirrorAgent.SetMasterMode(false);
            UpdateRoleDisplay();
            RefreshMirrorLog();
        }

        [RelayCommand]
        private void ClearLog()
        {
            MirrorLog = string.Empty;
        }

        [RelayCommand]
        private void StartCapture()
        {
            if (string.IsNullOrWhiteSpace(ActionName))
            {
                CaptureStatus = "❌ 请输入操作名称";
                return;
            }

            IsCapturing = true;
            _currentCoordinateList.Clear();
            CapturedCoordinates.Clear();
            CapturedCount = 0;
            CaptureStatus = "采集中... 在下方预览框中点击坐标";
            _log.Append($"[Capture] 开始采集操作: {ActionName}");
        }

        [RelayCommand]
        private void EndCapture()
        {
            IsCapturing = false;
            CaptureStatus = $"✓ 采集完成 ({CapturedCount} 个点)";
            _log.Append($"[Capture] 结束采集，共 {CapturedCount} 个点");
        }

        [RelayCommand]
        private void SaveCapture()
        {
            if (CapturedCount == 0)
            {
                CaptureStatus = "❌ 没有采集到坐标";
                return;
            }

            try
            {
                var action = new MirrorModeAction
                {
                    Name = ActionName,
                    Description = ActionDescription,
                    Coordinates = new System.Collections.Generic.List<ProportionalCoordinate>(_currentCoordinateList),
                    DelayBetweenClicksMs = 100 // 默认点击间隔 100ms
                };

                // 检查是否已存在同名任务，如有则替换
                var existingIndex = _config.MirrorModeActions.FindIndex(a => a.Name == ActionName);
                if (existingIndex >= 0)
                {
                    _config.MirrorModeActions[existingIndex] = action;
                    CaptureStatus = $"✓ 已更新 '{ActionName}' ({CapturedCount} 个点)";
                }
                else
                {
                    _config.MirrorModeActions.Add(action);
                    CaptureStatus = $"✓ 已保存 '{ActionName}' ({CapturedCount} 个点)";
                }

                // 持久化到磁盘
                _configService.SaveConfig(_config);
                _log.Append($"[Capture] 保存操作: {ActionName} ({CapturedCount} 点)");
                
                // 刷新已保存动作列表
                LoadSavedActions();
                
                // 重置
                ActionName = string.Empty;
                ActionDescription = string.Empty;
                _currentCoordinateList.Clear();
                CapturedCoordinates.Clear();
                CapturedCount = 0;
            }
            catch (System.Exception ex)
            {
                CaptureStatus = $"❌ 保存失败: {ex.Message}";
                _log.Append($"[Capture] 保存失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private void ClearCapture()
        {
            _currentCoordinateList.Clear();
            CapturedCoordinates.Clear();
            CapturedCount = 0;
            CaptureStatus = "已清除采集数据";
        }

        public void AddCapturedCoordinate(double xPercent, double yPercent)
        {
            if (!IsCapturing) return;

            var coord = new ProportionalCoordinate(xPercent, yPercent);
            _currentCoordinateList.Add(coord);
            CapturedCoordinates.Add($"Point {CapturedCount + 1}: {coord}");
            CapturedCount++;
        }

        private void UpdateRoleDisplay()
        {
            CurrentRole = _mirrorAgent.IsMaster ? "主控 (Master)" : "从应 (Follower)";
        }

        private void RefreshMirrorLog()
        {
            var allLogs = _log.GetAll();
            var mirrorLogs = allLogs.Where(x => x.Contains("[Mirror]")).ToList();
            MirrorLog = string.Join("\n", mirrorLogs.TakeLast(50));
        }

        private void LoadSavedActions()
        {
            SavedActions.Clear();
            foreach (var action in _config.MirrorModeActions)
            {
                SavedActions.Add($"{action.Name} ({action.Coordinates.Count} 点) - {action.Description}");
            }
        }

        [RelayCommand]
        private void LoadAction()
        {
            if (string.IsNullOrWhiteSpace(SelectedActionName))
                return;

            // 从 SavedActions 列表中选择对应的实际 Action
            var actionIndex = SavedActions.IndexOf(SelectedActionName);
            if (actionIndex >= 0 && actionIndex < _config.MirrorModeActions.Count)
            {
                var action = _config.MirrorModeActions[actionIndex];
                ActionName = action.Name;
                ActionDescription = action.Description;
                _currentCoordinateList = new ObservableCollection<ProportionalCoordinate>(action.Coordinates);
                CapturedCount = _currentCoordinateList.Count;
                CapturedCoordinates.Clear();
                for (int i = 0; i < _currentCoordinateList.Count; i++)
                {
                    var coord = _currentCoordinateList[i];
                    CapturedCoordinates.Add($"Point {i + 1}: {coord}");
                }
                CaptureStatus = $"✓ 已加载 '{action.Name}' ({CapturedCount} 个点)";
                _log.Append($"[Capture] 加载操作: {action.Name}");
            }
        }

        [RelayCommand]
        private void DeleteAction()
        {
            if (string.IsNullOrWhiteSpace(SelectedActionName))
                return;

            var actionIndex = SavedActions.IndexOf(SelectedActionName);
            if (actionIndex >= 0 && actionIndex < _config.MirrorModeActions.Count)
            {
                var actionName = _config.MirrorModeActions[actionIndex].Name;
                _config.MirrorModeActions.RemoveAt(actionIndex);
                _configService.SaveConfig(_config);
                LoadSavedActions();
                CaptureStatus = $"✓ 已删除 '{actionName}'";
                _log.Append($"[Capture] 删除操作: {actionName}");
            }
        }

        private void LoadBackgroundConfig()
        {
            ConfigBackgroundMode = _config.BackgroundAutomation.EnableBackgroundMode;
            ConfigUseVirtualDisplay = _config.BackgroundAutomation.UseVirtualDisplayCapture;
            ConfigDisplayWidth = _config.BackgroundAutomation.VirtualDisplayWidth;
            ConfigDisplayHeight = _config.BackgroundAutomation.VirtualDisplayHeight;
            
            IsInputNative = _config.BackgroundAutomation.InputMethod == "Native";
            IsInputSimulated = _config.BackgroundAutomation.InputMethod == "Simulated";
        }

        [RelayCommand]
        private void SaveBackgroundConfig()
        {
            _config.BackgroundAutomation.EnableBackgroundMode = ConfigBackgroundMode;
            _config.BackgroundAutomation.UseVirtualDisplayCapture = ConfigUseVirtualDisplay;
            _config.BackgroundAutomation.VirtualDisplayWidth = ConfigDisplayWidth;
            _config.BackgroundAutomation.VirtualDisplayHeight = ConfigDisplayHeight;
            _config.BackgroundAutomation.InputMethod = IsInputNative ? "Native" : "Simulated";

            _configService.SaveConfig(_config);
            _log.Append("[BackgroundConfig] 后台配置已保存");
        }

        [RelayCommand]
        private async Task ExecuteAction()
        {
            if (string.IsNullOrWhiteSpace(SelectedActionName))
            {
                ExecutionStatus = "❌ 请先选择要执行的动作";
                return;
            }

            var actionIndex = SavedActions.IndexOf(SelectedActionName);
            if (actionIndex < 0 || actionIndex >= _config.MirrorModeActions.Count)
            {
                ExecutionStatus = "❌ 动作未找到";
                return;
            }

            var action = _config.MirrorModeActions[actionIndex];

            IsExecuting = true;
            ExecutionProgress = 0;
            ExecutionStatus = $"准备执行 '{action.Name}' ({action.Coordinates.Count} 个点)...";
            _executionCts = new CancellationTokenSource();

            try
            {
                _log.Append($"[ExecuteAction] 开始执行: {action.Name}");

                // 获取目标窗口
                IntPtr hwnd = _executor.GetActiveWindowHandle();
                if (hwnd == IntPtr.Zero && _config.BackgroundAutomation.EnableBackgroundMode)
                {
                    ExecutionStatus = "⚠ 无法获取活跃窗口，尝试后台模式...";
                }

                var result = await _executor.ExecuteActionAsync(action, hwnd);

                if (result.Success)
                {
                    ExecutionStatus = $"✅ {result.Message}";
                    ExecutionProgress = 100;
                    _log.Append($"[ExecuteAction] 执行成功: {result.Message}");
                }
                else
                {
                    ExecutionStatus = $"❌ {result.Message}";
                    _log.Append($"[ExecuteAction] 执行失败: {result.Message}");
                }
            }
            catch (OperationCanceledException)
            {
                ExecutionStatus = "⏹ 执行已取消";
                _log.Append("[ExecuteAction] 执行已取消");
            }
            catch (Exception ex)
            {
                ExecutionStatus = $"❌ 执行异常: {ex.Message}";
                _log.Append($"[ExecuteAction] 异常: {ex.Message}");
            }
            finally
            {
                IsExecuting = false;
                _executionCts?.Dispose();
            }
        }

        [RelayCommand]
        private void StopExecution()
        {
            _executionCts?.Cancel();
            ExecutionStatus = "停止中...";
        }
    }
}
