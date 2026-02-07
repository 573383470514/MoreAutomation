using System;
using System.Threading;
using System.Threading.Tasks;
using MoreAutomation.Contracts.Models;
using MoreAutomation.Contracts.Interfaces;

namespace MoreAutomation.Automation.Scheduler
{
    public class AutomationScheduler : IAutomationEngine
    {
        private AutomationStatus _status = AutomationStatus.Idle;
        public AutomationStatus Status => _status;

        private CancellationTokenSource? _cts;
        private readonly int _frameDelay = 100; // 默认限频：10fps，降低CPU占用

        public void Start()
        {
            _status = AutomationStatus.Running;
            _cts = new CancellationTokenSource();
            Task.Run(() => Loop(_cts.Token));
        }

        public void Stop()
        {
            _status = AutomationStatus.Idle;
            _cts?.Cancel();
        }

        public void Pause() => _status = AutomationStatus.Paused;
        public void Resume() => _status = AutomationStatus.Running;

        private async Task Loop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_status == AutomationStatus.Paused)
                {
                    await Task.Delay(500, token);
                    continue;
                }

                // 这里执行具体的 FSM Update 或 脚本逻辑

                await Task.Delay(_frameDelay, token); // 强制限频
            }
        }
    }
}