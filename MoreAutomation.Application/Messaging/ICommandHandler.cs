using System.Threading.Tasks;

namespace MoreAutomation.Application.Messaging
{
    public interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        Task HandleAsync(TCommand command);
    }
}
