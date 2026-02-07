using MoreAutomation.Contracts.Models;

namespace MoreAutomation.Contracts.Interfaces
{
    public interface IAutomationEngine
    {
        AutomationStatus Status { get; }
        void Start();
        void Stop();
        void Pause();
        void Resume();
    }
}