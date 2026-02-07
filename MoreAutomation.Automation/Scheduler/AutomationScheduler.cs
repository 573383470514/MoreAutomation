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

        // 事件：账户登录/登出
        public event EventHandler<(long accountNumber, string accountName)>? AccountLoggedIn;
        public event EventHandler<(long accountNumber, string accountName)>? AccountLoggedOut;

        private AutomationStatus _status = AutomationStatus.Idle;
        private CancellationTokenSource? _cts;
        private readonly int _frameDelayMs;
        private readonly int _pausePollingDelayMs;
        private readonly MoreAutomation.Automation.Recovery.FailureRecoveryPolicy _recoveryPolicy;
        private readonly AppConfig? _appConfig;
        private readonly MoreAutomation.Contracts.Monitoring.IMetricsService? _metrics;

        // 运行时控制标志（可被外部强停触发）
        private volatile bool _forceStopRequested;

        // 简单运行指标
        private long _tickCount;
        private long _failureCount;
        private int _consecutiveFailures;

        public AutomationStatus Status => _status;

        public AutomationScheduler() : this(new AppConfig(), new MoreAutomation.Automation.Recovery.FailureRecoveryPolicy(), null)
        {
        }

        public AutomationScheduler(AppConfig appConfig, MoreAutomation.Automation.Recovery.FailureRecoveryPolicy recoveryPolicy, MoreAutomation.Contracts.Monitoring.IMetricsService? metrics)
        {
            ArgumentNullException.ThrowIfNull(appConfig);
            _frameDelayMs = Math.Max(MinDelayMs, appConfig.AutomationTuning.FrameDelayMs);
            _pausePollingDelayMs = Math.Max(MinDelayMs, appConfig.AutomationTuning.PausePollingDelayMs);
            _recoveryPolicy = recoveryPolicy ?? new MoreAutomation.Automation.Recovery.FailureRecoveryPolicy();
            _appConfig = appConfig;
            _metrics = metrics;

            // 根据配置初始化恢复策略参数
            if (_appConfig?.AutomationTuning != null)
            {
                _recoveryPolicy.MaxRetries = _appConfig.AutomationTuning.MaxRetries;
                _recoveryPolicy.RetryDelayMs = _appConfig.AutomationTuning.RetryDelayMs;
                _recoveryPolicy.RetryBackoffFactor = _appConfig.AutomationTuning.RetryBackoffFactor;
                _recoveryPolicy.CircuitTripThreshold = _appConfig.AutomationTuning.CircuitTripThreshold;
            }
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

        public void ForceStop()
        {
            _forceStopRequested = true;
            _cts?.Cancel();
            _status = AutomationStatus.Idle;
        }

        public void Pause() => _status = AutomationStatus.Paused;

        public void Resume()
        {
            // 只在处于 Paused 时恢复为 Running；避免从 Idle 无意启动
            if (_status == AutomationStatus.Paused)
            {
                _status = AutomationStatus.Running;
            }
        }

        public void NotifyAccountLoggedIn(long accountNumber, string accountName = "")
        {
            AccountLoggedIn?.Invoke(this, (accountNumber, accountName));
        }

        public void NotifyAccountLoggedOut(long accountNumber, string accountName = "")
        {
            AccountLoggedOut?.Invoke(this, (accountNumber, accountName));
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

                // 在每次 tick 之前检查 circuit-breaker
                if (_appConfig?.FeatureToggles != null && _appConfig.FeatureToggles.TryGetValue(FeatureToggleKeys.RuntimeCircuitBreaker, out bool circuit) && circuit)
                {
                    _status = AutomationStatus.Error;
                    break;
                }

                int currentRetry = 0;
                bool tickSucceeded = false;

                while (!tickSucceeded && !token.IsCancellationRequested)
                {
                    if (_forceStopRequested)
                    {
                        // 强停请求到来，直接退出循环
                        break;
                    }

                    try
                    {
                        System.Threading.Interlocked.Increment(ref _tickCount);
                        _metrics?.IncrementTick();
                        // 执行单次 tick（占位逻辑，可扩展为 FSM.Update 或脚本调度）
                        await Task.Delay(_frameDelayMs, token);
                        tickSucceeded = true;
                        // 成功则重置连续失败计数
                        _consecutiveFailures = 0;
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        if (_forceStopRequested)
                        {
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        System.Threading.Interlocked.Increment(ref _failureCount);
                        _metrics?.IncrementFailure();
                        currentRetry++;
                        // 连续失败计数
                        _consecutiveFailures++;
                        var action = _recoveryPolicy.OnRecognitionFailed(currentRetry);

                        switch (action)
                        {
                            case MoreAutomation.Automation.Recovery.RecoveryAction.Retry:
                                // 使用策略提供的退避延迟
                                var delay = _recoveryPolicy.GetDelayForRetry(currentRetry);
                                await Task.Delay(delay, token);
                                continue;
                            case MoreAutomation.Automation.Recovery.RecoveryAction.Skip:
                                // 跳过本次 tick，继续主循环
                                tickSucceeded = true;
                                break;
                            case MoreAutomation.Automation.Recovery.RecoveryAction.Pause:
                                _status = AutomationStatus.Paused;
                                tickSucceeded = true;
                                break;
                            case MoreAutomation.Automation.Recovery.RecoveryAction.Alert:
                                // 记录告警并暂停执行，实际可扩展为上报或 UI 提示
                                _status = AutomationStatus.Paused;
                                tickSucceeded = true;
                                break;
                            default:
                                throw;
                        }

                        // 检查是否达到熔断阈值
                        if (_recoveryPolicy.ShouldTripCircuit(_consecutiveFailures))
                        {
                            _status = AutomationStatus.Error;
                            break;
                        }
                    }
                }
            }
        }
    }
}
