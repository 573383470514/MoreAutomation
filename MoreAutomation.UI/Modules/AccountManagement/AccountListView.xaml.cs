using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreAutomation.Domain.Entities;
using MoreAutomation.Application.Messaging;
using MoreAutomation.Application.Commands;

namespace MoreAutomation.UI.Modules.AccountManagement
{
    public partial class AccountViewModel : ObservableObject
    {
        private readonly ICommandBus _commandBus;
        private readonly MoreAutomation.Infrastructure.Config.JsonConfigService _cfgService;
        [ObservableProperty]
        private ObservableCollection<Account> _accounts = new();
        [ObservableProperty]
        private bool _showAddPanel;

        [ObservableProperty]
        private string _newAccountNumber = string.Empty;

        [ObservableProperty]
        private string _newPassword = string.Empty;

        [ObservableProperty]
        private bool _showNewPassword;

        [ObservableProperty]
        private string _newGroup = "1";

        [ObservableProperty]
        private string _newNote = string.Empty;

        [ObservableProperty]
        private bool _hideSensitive;

        public System.ComponentModel.ICollectionView AccountsView { get; private set; }

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string? _selectedGroup;

        [ObservableProperty]
        private int _selectedServerId = 1;

        public System.Collections.Generic.List<int> Groups { get; } = new();
        public System.Collections.Generic.List<int> ServerIds { get; } = new();

        public AccountViewModel(ICommandBus commandBus, MoreAutomation.Infrastructure.Config.JsonConfigService cfgService)
        {
            _commandBus = commandBus;
            AccountsView = System.Windows.Data.CollectionViewSource.GetDefaultView(Accounts);
            AccountsView.Filter = FilterAccounts;
            _cfgService = cfgService ?? throw new System.ArgumentNullException(nameof(cfgService));
            // load persisted hide-sensitive preference
            var cfg = _cfgService.GetConfig();
            HideSensitive = cfg.HideSensitiveAccounts;
            SelectedServerId = cfg.CurrentServer;
            // initialize server list 1~10
            for (int i = 1; i <= 10; i++)
            {
                ServerIds.Add(i);
            }
            _ = LoadAccountsAsync();
        }

        [RelayCommand]
        private void ToggleAddPanel()
        {
            ShowAddPanel = !ShowAddPanel;
        }

        [RelayCommand]
        private void CancelAdd()
        {
            ShowAddPanel = false;
            NewAccountNumber = string.Empty;
            NewPassword = string.Empty;
            NewGroup = "1";
            NewNote = string.Empty;
        }

        [RelayCommand]
        private async Task ConfirmAdd()
        {
            if (!long.TryParse(NewAccountNumber, out long acc))
            {
                System.Windows.MessageBox.Show("请输入有效账号");
                return;
            }

            if (!int.TryParse(NewGroup, out int gid)) gid = 1;

            try
            {
                await _commandBus.SendAsync(new AddAccountCommand(new Account(acc)
                {
                    Password = NewPassword ?? string.Empty,
                    GroupId = gid,
                    Note = NewNote ?? string.Empty
                }));
                await RefreshViewAsync();
                CancelAdd();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"添加失败: {ex.Message}");
            }
        }

        partial void OnHideSensitiveChanged(bool value)
        {
            try
            {
                var cfg = _cfgService.GetConfig();
                cfg.HideSensitiveAccounts = value;
                _cfgService.SaveConfigAsync(cfg).GetAwaiter().GetResult();
            }
            catch
            {
                // ignore persistence failures to avoid UI disruption
            }
        }

        private bool FilterAccounts(object obj)
        {
            if (obj is not Account a) return false;
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                if (!(a.Note?.IndexOf(SearchText, System.StringComparison.OrdinalIgnoreCase) >= 0 || a.AccountNumber.ToString().Contains(SearchText)))
                    return false;
            }
            if (!string.IsNullOrWhiteSpace(SelectedGroup))
            {
                if (int.TryParse(SelectedGroup, out int g))
                {
                    if (a.GroupId != g) return false;
                }
            }
            return true;
        }

        private async Task RefreshViewAsync()
        {
            var list = await _commandBus.QueryAsync<LoadAccountsQuery, System.Collections.Generic.List<Account>>(new LoadAccountsQuery());
            Accounts = new ObservableCollection<Account>(list);
            Groups.Clear();
            foreach (var g in Accounts.Select(a => a.GroupId).Distinct().OrderBy(x => x)) Groups.Add(g);
            AccountsView = System.Windows.Data.CollectionViewSource.GetDefaultView(Accounts);
            AccountsView.Filter = FilterAccounts;
            AccountsView.Refresh();
        }

        private async Task LoadAccountsAsync() => await RefreshViewAsync();

        

        [RelayCommand]
        private async Task DeleteAccount(Account account)
        {
            if (account == null) return;
            await _commandBus.SendAsync(new DeleteAccountCommand(account.AccountNumber));
            await RefreshViewAsync();
        }

        [RelayCommand]
        private async Task SetMaster(Account account)
        {
            if (account == null) return;
            await _commandBus.SendAsync(new SetMasterCommand(account.AccountNumber));
            await RefreshViewAsync();
        }

        [RelayCommand]
        private async Task MoveUp(Account account)
        {
            if (account == null) return;
            var idx = Accounts.IndexOf(account);
            if (idx > 0)
            {
                Accounts.Move(idx, idx - 1);
                // update sort indexes
                for (int i = 0; i < Accounts.Count; i++)
                {
                    Accounts[i].SortIndex = i + 1;
                    await _commandBus.SendAsync(new UpdateAccountCommand(Accounts[i]));
                }
                AccountsView.Refresh();
            }
        }

        [RelayCommand]
        private async Task MoveDown(Account account)
        {
            if (account == null) return;
            var idx = Accounts.IndexOf(account);
            if (idx < Accounts.Count - 1 && idx >= 0)
            {
                Accounts.Move(idx, idx + 1);
                for (int i = 0; i < Accounts.Count; i++)
                {
                    Accounts[i].SortIndex = i + 1;
                    await _commandBus.SendAsync(new UpdateAccountCommand(Accounts[i]));
                }
                AccountsView.Refresh();
            }
        }

        partial void OnSelectedServerIdChanged(int value)
        {
            try
            {
                var cfg = _cfgService.GetConfig();
                cfg.CurrentServer = value;
                _cfgService.SaveConfigAsync(cfg).GetAwaiter().GetResult();
            }
            catch
            {
                // ignore persistence failures
            }
        }

        [RelayCommand]
        private async Task LoginAccount(Account account)
        {
            if (account == null) return;
            try
            {
                await _commandBus.SendAsync(new LoginAccountCommand(account.AccountNumber, account.GroupId, SelectedServerId));
                System.Windows.MessageBox.Show($"已发起登录: 账号 {account.AccountNumber}, 组号 {account.GroupId}, 区服 {SelectedServerId}");
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"登录失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ConfigureProxy(Account account)
        {
            if (account == null) return;
            
            // 获取当前配置
            var cfg = new MoreAutomation.Infrastructure.Config.JsonConfigService().GetConfig();
            
            // 如果尚未设置代理密钥，需要先设置一个
            if (string.IsNullOrWhiteSpace(cfg.ProxySecretKey))
            {
                var setKeyDlg = new MoreAutomation.UI.Shell.ProxyKeyPrompt();
                setKeyDlg.Owner = System.Windows.Application.Current.MainWindow;
                setKeyDlg.Title = "首次设置代理密钥";
                setKeyDlg.ShowDialog();
                
                if (!setKeyDlg.Accepted || string.IsNullOrWhiteSpace(setKeyDlg.Key))
                {
                    System.Windows.MessageBox.Show("取消设置代理");
                    return;
                }
                
                // 保存新密钥
                cfg.ProxySecretKey = setKeyDlg.Key;
                await new MoreAutomation.Infrastructure.Config.JsonConfigService().SaveConfigAsync(cfg);
                System.Windows.MessageBox.Show("代理密钥已设置");
            }
            else
            {
                // 验证密钥
                var keyDlg = new MoreAutomation.UI.Shell.ProxyKeyPrompt();
                keyDlg.Owner = System.Windows.Application.Current.MainWindow;
                keyDlg.Title = "输入代理密钥";
                keyDlg.ShowDialog();
                
                if (!keyDlg.Accepted)
                {
                    return;
                }
                
                if (keyDlg.Key != cfg.ProxySecretKey)
                {
                    System.Windows.MessageBox.Show("密钥错误");
                    return;
                }
            }
            
            // 密钥验证成功，打开代理配置窗口
            var dlg = new MoreAutomation.UI.Shell.ConfigureProxyWindow(account);
            dlg.Owner = System.Windows.Application.Current.MainWindow;
            dlg.ShowDialog();
            
            if (dlg.Saved)
            {
                // 代理已保存，可在此处执行后续操作
                System.Windows.MessageBox.Show("代理配置已保存");
            }
        }
    }
}