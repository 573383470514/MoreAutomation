using System.Collections.Generic;
using MoreAutomation.Domain.Entities;
using MoreAutomation.Application.Messaging;

namespace MoreAutomation.Application.Commands
{
    public record DeleteAccountCommand(long AccountNumber) : ICommand;

    public record LoadAccountsQuery() : IQuery<List<Account>>;

    public record AddAccountCommand(Account Account) : ICommand;

    public record SetMasterCommand(long AccountNumber) : ICommand;

    public record UpdateAccountCommand(Account Account) : ICommand;
}
