using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreAutomation.Application.Services;
using MoreAutomation.Contracts.Configuration;
using MoreAutomation.Contracts.Interfaces;
using MoreAutomation.Contracts.Models;
using MoreAutomation.UI.Modules.AccountManagement;

namespace MoreAutomation.UI.Shell
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly UiActionBus _uiActionBus;
        private readonly FeatureGateService _featureGateService;
        private readonly MoreAutomation.Application.Messaging.ICommandBus _commandBus;
        private readonly MoreAutomation.Application.Services.ILogService _log;
        private readonly AccountViewModel _accountVm;
        private readonly MoreAutomation.UI.Modules.DailyTasks.DailyTasksViewModel _dailyTasksVm;
        private readonly MoreAutomation.UI.Modules.MirrorMode.MirrorModeViewModel _mirrorModeVm;
        private readonly IAutomationEngine _automationEngine;

        [ObservableProperty]
        private ObservableObject? _currentView;

        [ObservableProperty]
        private string _statusText = "就绪";

        [ObservableProperty]
        private string _logConsole = string.Empty;

        [ObservableProperty]
        private string _currentMode = "普通";

        [ObservableProperty]
        private System.Collections.ObjectModel.ObservableCollection<string> _loggedInAccounts = new();

        public MainViewModel(UiActionBus uiActionBus, FeatureGateService featureGateService, AccountViewModel accountVm, MoreAutomation.Application.Messaging.ICommandBus commandBus, MoreAutomation.Application.Services.ILogService log, IAutomationEngine automationEngine, MoreAutomation.UI.Modules.DailyTasks.DailyTasksViewModel dailyTasksVm, MoreAutomation.UI.Modules.MirrorMode.MirrorModeViewModel mirrorModeVm)
        {
            _uiActionBus = uiActionBus;
            _featureGateService = featureGateService;
            _commandBus = commandBus;
            _log = log;
            _accountVm = accountVm;
            _dailyTasksVm = dailyTasksVm;
            _mirrorModeVm = mirrorModeVm;
            _automationEngine = automationEngine;
            CurrentView = accountVm;
            CurrentMode = "普通";
            // initialize log console
            LogConsole = string.Join("\n", _log.GetAll());
            
            // 订阅自动化引擎的登录/登出事件
            if (automationEngine is MoreAutomation.Automation.Scheduler.AutomationScheduler scheduler)
            {
                scheduler.AccountLoggedIn += (s, args) => AddLoggedInAccount(args.accountNumber.ToString());
                scheduler.AccountLoggedOut += (s, args) => RemoveLoggedInAccount(args.accountNumber.ToString());
            }
        }

        [RelayCommand]
        private void NavigateToAccounts()
        {
            CurrentView = _accountVm;
        }

        [RelayCommand]
        private void NavigateToDailyTasks()
        {
            CurrentView = _dailyTasksVm;
        }

        [RelayCommand]
        private void NavigateToMirrorMode()
        {
            CurrentView = _mirrorModeVm;
        }

        [RelayCommand]
        private void SwitchToWashPetMode()
        {
            CurrentMode = "洗宠";
            UpdateStatus("已切换到洗宠模式");
        }

        [RelayCommand]
        private void SwitchToNormalMode()
        {
            CurrentMode = "普通";
            UpdateStatus("已切换到普通模式");
        }

        [RelayCommand]
        private async Task LoginWithAccount(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber)) return;
            try
            {
                if (long.TryParse(accountNumber, out long accNum))
                {
                    // 从已登录账号字符串中解析组号和区服号，格式如 "12345(G2S1)"
                    int groupId = 1, serverId = 1;
                    var match = System.Text.RegularExpressions.Regex.Match(accountNumber, @"G(\d+)S(\d+)");
                    if (match.Success)
                    {
                        int.TryParse(match.Groups[1].Value, out groupId);
                        int.TryParse(match.Groups[2].Value, out serverId);
                    }
                    await _commandBus.SendAsync(new MoreAutomation.Application.Commands.LoginAccountCommand(accNum, groupId, serverId));
                    UpdateStatus($"已使用账号 {accountNumber} 登录");
                }
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"登录失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SetMasterAccount(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber)) return;
            try
            {
                if (long.TryParse(accountNumber, out long accNum))
                {
                    await _commandBus.SendAsync(new MoreAutomation.Application.Commands.SetMasterCommand(accNum));
                    UpdateStatus($"账号 {accountNumber} 已设为主控");
                }
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"设为主控失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task LogoutAccount(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber)) return;
            try
            {
                if (long.TryParse(accountNumber, out long accNum))
                {
                    await _commandBus.SendAsync(new MoreAutomation.Application.Commands.LogoutAccountCommand(accNum));
                    RemoveLoggedInAccount(accountNumber);
                    UpdateStatus($"账号 {accountNumber} 已退出登录");
                }
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"退出登录失败: {ex.Message}");
            }
        }

        public void AddLoggedInAccount(string accountNumber)
        {
            if (!LoggedInAccounts.Contains(accountNumber))
            {
                LoggedInAccounts.Add(accountNumber);
            }
        }

        public void RemoveLoggedInAccount(string accountNumber)
        {
            LoggedInAccounts.Remove(accountNumber);
        }

        public void ClearLoggedInAccounts()
        {
            LoggedInAccounts.Clear();
        }

        [RelayCommand]
        private async Task HideWindow()
        {
            try
            {
                await _commandBus.SendAsync(new MoreAutomation.Application.Commands.HideWindowCommand());
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"操作失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task StartAutomation()
        {
            try
            {
                await _commandBus.SendAsync(new MoreAutomation.Application.Commands.StartAutomationCommand());
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"启动失败: {ex.Message}");
            }
        }

        private bool CheckActionEnabled(string key, string disabledMessage)
        {
            // 保留但不在 UI 流程中强依赖
            if (_featureGateService.IsEnabled(key))
            {
                return true;
            }

            UpdateStatus(disabledMessage);
            return false;
        }

        public void UpdateStatus(string message) => StatusText = message;

        [RelayCommand]
        private void SaveLog()
        {
            try
            {
                var dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.Filter = "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*";
                if (dlg.ShowDialog() == true)
                {
                    System.IO.File.WriteAllText(dlg.FileName, LogConsole);
                    UpdateStatus("日志已保存");
                }
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"保存失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private void ClearLog()
        {
            try
            {
                _log.Clear();
                LogConsole = string.Empty;
                UpdateStatus("日志已清空");
            }
            catch
            {
                UpdateStatus("清空日志失败");
            }
        }
    }
}
