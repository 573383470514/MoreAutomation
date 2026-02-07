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
        // 强制中止当前运行并尽快返回到安全状态
        void ForceStop();
    }
}