namespace MoreAutomation.Contracts.Configuration
{
    public static class FeatureToggleKeys
    {
        public const string ModuleAccountManagement = "module:account-management";
        public const string ActionStartAutomation = "action:start-automation";
        public const string ActionHideWindow = "action:hide-window";
        public const string ActionAdaptiveToggle = "action:adaptive-toggle";
        // 常见动作
        public const string ActionAddAccount = "action:add-account";
        public const string ActionDeleteAccount = "action:delete-account";
        public const string ActionStopAutomation = "action:stop-automation";

        // 运行时控制器与全局熔断
        public const string RuntimeForceStop = "runtime:force-stop";
        public const string RuntimeCircuitBreaker = "runtime:circuit-breaker";
    }
}
