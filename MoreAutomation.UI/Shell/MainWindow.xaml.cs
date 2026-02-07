using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreAutomation.Application.Services;
using MoreAutomation.Contracts.Configuration;
using MoreAutomation.Contracts.Models;
using MoreAutomation.UI.Modules.AccountManagement;

namespace MoreAutomation.UI.Shell
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly UiActionBus _uiActionBus;
        private readonly FeatureGateService _featureGateService;

        [ObservableProperty]
        private ObservableObject? _currentView;

        [ObservableProperty]
        private string _statusText = "就绪";

        public MainViewModel(UiActionBus uiActionBus, FeatureGateService featureGateService, AccountViewModel accountVm)
        {
            _uiActionBus = uiActionBus;
            _featureGateService = featureGateService;
            _featureGateService.RegisterDefaults();

            CurrentView = accountVm;
        }

        [RelayCommand]
        private void NavigateToAccounts(AccountViewModel vm)
        {
            if (!_featureGateService.IsEnabled(FeatureToggleKeys.ModuleAccountManagement))
            {
                UpdateStatus("账号管理模块已禁用");
                return;
            }

            CurrentView = vm;
        }

        [RelayCommand]
        private void HideWindow()
        {
            if (!CheckActionEnabled(FeatureToggleKeys.ActionHideWindow, "隐藏窗口功能已禁用"))
            {
                return;
            }

            _uiActionBus.Publish(UiActionType.HideWindow);
        }

        [RelayCommand]
        private void StartAutomation()
        {
            if (!CheckActionEnabled(FeatureToggleKeys.ActionStartAutomation, "启动自动化功能已禁用"))
            {
                return;
            }

            _uiActionBus.Publish(UiActionType.StartAutomation);
        }

        private bool CheckActionEnabled(string key, string disabledMessage)
        {
            if (_featureGateService.IsEnabled(key))
            {
                return true;
            }

            UpdateStatus(disabledMessage);
            return false;
        }

        public void UpdateStatus(string message) => StatusText = message;
    }
}
