using System;
using System.Threading.Tasks;
using MoreAutomation.Application.Messaging;
using MoreAutomation.Application.Commands;
using MoreAutomation.Contracts.Configuration;
using MoreAutomation.Application.Services;
using MoreAutomation.Contracts.Interfaces;

namespace MoreAutomation.Application.Handlers
{
    public class RuntimeCommandHandler : ICommandHandler<StartAutomationCommand>, ICommandHandler<StopAutomationCommand>, ICommandHandler<ForceStopCommand>, ICommandHandler<HideWindowCommand>, ICommandHandler<LoginAccountCommand>, ICommandHandler<LogoutAccountCommand>
    {
        private readonly IAutomationEngine _engine;
        private readonly FeatureGateService _featureGate;
        private readonly UiActionBus _uiActionBus;

        public RuntimeCommandHandler(IAutomationEngine engine, FeatureGateService featureGate, UiActionBus uiActionBus)
        {
            _engine = engine;
            _featureGate = featureGate;
            _uiActionBus = uiActionBus;
        }

        public Task HandleAsync(StartAutomationCommand command)
        {
            if (!_featureGate.IsEnabled(FeatureToggleKeys.ActionStartAutomation))
                throw new InvalidOperationException("启动自动化被禁用");

            _engine.Start();
            return Task.CompletedTask;
        }

        public Task HandleAsync(StopAutomationCommand command)
        {
            _engine.Stop();
            return Task.CompletedTask;
        }

        public Task HandleAsync(ForceStopCommand command)
        {
            // 强停优先级更高，但受功能开关控制
            if (!_featureGate.IsEnabled(FeatureToggleKeys.RuntimeForceStop))
            {
                throw new InvalidOperationException("强停被功能开关禁用");
            }

            try
            {
                _engine.ForceStop();
            }
            catch
            {
                _engine.Stop();
            }

            return Task.CompletedTask;
        }

        public Task HandleAsync(HideWindowCommand command)
        {
            // UI 操作通过 UiActionBus 发布
            _uiActionBus.Publish(MoreAutomation.Application.Services.UiActionType.HideWindow);
            return Task.CompletedTask;
        }

        public Task HandleAsync(LoginAccountCommand command)
        {
            // 触发自动化引擎登录指定账户（含组号和区服号）
            _engine.Start();

            // 如果引擎支持事件通知，将账号转换为字符串并通知
            if (_engine is MoreAutomation.Automation.Scheduler.AutomationScheduler scheduler)
            {
                scheduler.NotifyAccountLoggedIn(command.AccountNumber, $"{command.AccountNumber}(G{command.GroupId}S{command.ServerId})");
            }

            return Task.CompletedTask;
        }

        public Task HandleAsync(LogoutAccountCommand command)
        {
            // 触发自动化引擎登出账户
            if (_engine is MoreAutomation.Automation.Scheduler.AutomationScheduler scheduler)
            {
                scheduler.NotifyAccountLoggedOut(command.AccountNumber, command.AccountNumber.ToString());
            }

            return Task.CompletedTask;
        }
    }
}
