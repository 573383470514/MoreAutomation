using System;
using System.Threading;
using System.Threading.Tasks;
using MoreAutomation.Contracts.Configuration;
using MoreAutomation.Contracts.Interfaces;
using MoreAutomation.Contracts.Models;

namespace MoreAutomation.Automation.Scheduler
{
    public class AutomationScheduler : IAutomationEngine
    {
        private const int MinDelayMs = 50;

        private AutomationStatus _status = AutomationStatus.Idle;
        private CancellationTokenSource? _cts;
        private readonly int _frameDelayMs;
        private readonly int _pausePollingDelayMs;

        public AutomationStatus Status => _status;

        public AutomationScheduler() : this(new AppConfig())
        {
        }

        public AutomationScheduler(AppConfig appConfig)
        {
            ArgumentNullException.ThrowIfNull(appConfig);

            _frameDelayMs = Math.Max(MinDelayMs, appConfig.AutomationTuning.FrameDelayMs);
            _pausePollingDelayMs = Math.Max(MinDelayMs, appConfig.AutomationTuning.PausePollingDelayMs);
        }

        public void Start()
        {
            if (_status == AutomationStatus.Running)
            {
                return;
            }

            _status = AutomationStatus.Running;
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            Task.Run(() => Loop(_cts.Token));
        }

        public void Stop()
        {
            _status = AutomationStatus.Idle;
            _cts?.Cancel();
        }

        public void Pause() => _status = AutomationStatus.Paused;

        public void Resume()
        {
            if (_status != AutomationStatus.Idle)
            {
                _status = AutomationStatus.Running;
            }
        }

        private async Task Loop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_status == AutomationStatus.Paused)
                {
                    await Task.Delay(_pausePollingDelayMs, token);
                    continue;
                }

                if (_status == AutomationStatus.Idle)
                {
                    break;
                }

                // 执行具体 FSM update 或脚本调度。
                await Task.Delay(_frameDelayMs, token);
            }
        }
    }
}
