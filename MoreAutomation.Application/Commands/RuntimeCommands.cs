using MoreAutomation.Application.Messaging;

namespace MoreAutomation.Application.Commands
{
    public record StartAutomationCommand() : ICommand;
    public record StopAutomationCommand() : ICommand;
    public record ForceStopCommand() : ICommand;
    public record HideWindowCommand() : ICommand;
}
