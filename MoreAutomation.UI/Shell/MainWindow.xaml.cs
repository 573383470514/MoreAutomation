using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreAutomation.Application.Services;
using MoreAutomation.Contracts.Models;
using MoreAutomation.UI.Modules.AccountManagement;

namespace MoreAutomation.UI.Shell
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly UiActionBus _uiActionBus;

        [ObservableProperty]
        private ObservableObject? _currentView;

        [ObservableProperty]
        private string _statusText = "就绪";

        public MainViewModel(UiActionBus uiActionBus, AccountViewModel accountVm)
        {
            _uiActionBus = uiActionBus;
            // 默认显示账号管理页面
            CurrentView = accountVm;
        }

        [RelayCommand]
        private void NavigateToAccounts(AccountViewModel vm) => CurrentView = vm;

        [RelayCommand]
        private void HideWindow() => _uiActionBus.Publish(UiActionType.HideWindow);

        [RelayCommand]
        private void StartAutomation() => _uiActionBus.Publish(UiActionType.StartAutomation);

        // 响应状态更新（由 Application 层触发）
        public void UpdateStatus(string message) => StatusText = message;
    }
}