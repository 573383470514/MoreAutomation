using MoreAutomation.Application.Messaging;

namespace MoreAutomation.Application.Commands
{
    public record LoginAccountCommand(long AccountNumber, int GroupId = 1, int ServerId = 1) : ICommand;

    public record LogoutAccountCommand(long AccountNumber) : ICommand;
}
