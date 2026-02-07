using System;
using System.IO;
using MoreAutomation.Contracts.Configuration;

namespace MoreAutomation.Application.Orchestration
{
    public class StartupOrchestrator
    {
        private readonly AppConfig _config;

        public StartupOrchestrator(AppConfig config)
        {
            _config = config;
        }

        public StartupStep GetNextStep()
        {
            // 1) 检查协议
            if (!_config.IsAgreed) return StartupStep.ShowAgreement;

            // 2) 检查客户端路径
            if (string.IsNullOrWhiteSpace(_config.ClientPath) || !Directory.Exists(_config.ClientPath))
                return StartupStep.ShowPathSelection;

            // 3) 进入主模式
            return StartupStep.EnterMainShell;
        }
    }

    public enum StartupStep
    {
        ShowAgreement,
        ShowPathSelection,
        EnterMainShell
    }
}