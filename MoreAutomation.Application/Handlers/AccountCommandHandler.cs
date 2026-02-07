using System.Collections.Generic;
using System.Threading.Tasks;
using MoreAutomation.Application.Messaging;
using MoreAutomation.Application.Commands;
using MoreAutomation.Application.UseCases.AccountManagement;
using MoreAutomation.Domain.Entities;

namespace MoreAutomation.Application.Handlers
{
    public class AccountCommandHandler : ICommandHandler<DeleteAccountCommand>, ICommandHandler<AddAccountCommand>, ICommandHandler<SetMasterCommand>, ICommandHandler<UpdateAccountCommand>, IQueryHandler<LoadAccountsQuery, List<Account>>
    {
        private readonly AccountService _service;

        public AccountCommandHandler(AccountService service)
        {
            _service = service;
        }

        public async Task HandleAsync(DeleteAccountCommand command)
        {
            await _service.DeleteAccountAsync(command.AccountNumber);
        }

        public async Task HandleAsync(AddAccountCommand command)
        {
            var a = command.Account;
            await _service.AddAccountAsync(a.AccountNumber, a.Password, a.GroupId, a.Note);
        }

        public async Task HandleAsync(SetMasterCommand command)
        {
            await _service.SetMasterAsync(command.AccountNumber);
        }

        public async Task HandleAsync(UpdateAccountCommand command)
        {
            await _service.UpdateAccountAsync(command.Account);
        }

        public async Task<List<Account>> HandleAsync(LoadAccountsQuery query)
        {
            return await _service.GetAccountsAsync();
        }
    }
}
