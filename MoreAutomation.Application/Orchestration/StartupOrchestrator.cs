using System;
using System.Collections.Generic;
using System.IO;
using MoreAutomation.Contracts.Configuration;

namespace MoreAutomation.Application.Orchestration
{
    public class StartupOrchestrator
    {
        private readonly AppConfig _config;

        public StartupOrchestrator(AppConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public StartupStep GetNextStep()
        {
            if (!_config.IsAgreed)
            {
                return StartupStep.ShowAgreement;
            }

            if (string.IsNullOrWhiteSpace(_config.ClientPath) || !Directory.Exists(_config.ClientPath))
            {
                return StartupStep.ShowPathSelection;
            }

            return StartupStep.EnterMainShell;
        }

        public IReadOnlyList<StartupStep> BuildStartupPlan()
        {
            var plan = new List<StartupStep>();
            var step = GetNextStep();

            if (step == StartupStep.ShowAgreement)
            {
                plan.Add(StartupStep.ShowAgreement);
                return plan;
            }

            if (step == StartupStep.ShowPathSelection)
            {
                plan.Add(StartupStep.ShowPathSelection);
                return plan;
            }

            plan.Add(StartupStep.EnterMainShell);
            return plan;
        }
    }

    public enum StartupStep
    {
        ShowAgreement,
        ShowPathSelection,
        EnterMainShell
    }
}
