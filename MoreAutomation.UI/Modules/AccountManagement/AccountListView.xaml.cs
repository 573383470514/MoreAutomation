using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreAutomation.Application.UseCases.AccountManagement;
using MoreAutomation.Domain.Entities;

namespace MoreAutomation.UI.Modules.AccountManagement
{
    public partial class AccountViewModel : ObservableObject
    {
        private readonly AccountService _accountService;

        [ObservableProperty]
        private ObservableCollection<Account> _accounts = new();

        public AccountViewModel(AccountService accountService)
        {
            _accountService = accountService;
            LoadAccountsCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadAccounts()
        {
            var list = await _accountService.GetAccountsAsync();
            Accounts = new ObservableCollection<Account>(list);
        }

        [RelayCommand]
        private async Task DeleteAccount(Account account)
        {
            if (account == null) return;
            await _accountService.DeleteAccountAsync(account.AccountNumber);
            await LoadAccounts();
        }
    }
}