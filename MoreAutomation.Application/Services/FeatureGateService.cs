using System;
using MoreAutomation.Contracts.Configuration;

namespace MoreAutomation.Application.Services
{
    public class FeatureGateService
    {
        private readonly AppConfig _config;
        private readonly ModuleRegistry _moduleRegistry;

        public FeatureGateService(AppConfig config, ModuleRegistry moduleRegistry)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _moduleRegistry = moduleRegistry ?? throw new ArgumentNullException(nameof(moduleRegistry));
        }

        public void RegisterDefaults()
        {
            _moduleRegistry.RegisterModule(FeatureToggleKeys.ModuleAccountManagement, defaultEnabled: true);
            EnsureDefault(FeatureToggleKeys.ActionStartAutomation, true);
            EnsureDefault(FeatureToggleKeys.ActionHideWindow, true);
            EnsureDefault(FeatureToggleKeys.ActionAdaptiveToggle, true);
        }

        public bool IsEnabled(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("开关键不能为空", nameof(key));
            }

            if (key.StartsWith("module:", StringComparison.OrdinalIgnoreCase))
            {
                return _moduleRegistry.IsModuleEnabled(key);
            }

            return !_config.FeatureToggles.TryGetValue(key, out bool enabled) || enabled;
        }

        private void EnsureDefault(string key, bool enabled)
        {
            if (!_config.FeatureToggles.ContainsKey(key))
            {
                _config.FeatureToggles[key] = enabled;
            }
        }
    }
}
