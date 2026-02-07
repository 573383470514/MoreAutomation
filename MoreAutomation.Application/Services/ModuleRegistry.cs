using System;
using System.Collections.Concurrent;

namespace MoreAutomation.Application.Services
{
    public class ModuleRegistry
    {
        private readonly ConcurrentDictionary<string, bool> _moduleStates = new(StringComparer.OrdinalIgnoreCase);

        public void RegisterModule(string moduleName, bool defaultEnabled = true)
        {
            ValidateModuleName(moduleName);
            _moduleStates.TryAdd(moduleName, defaultEnabled);
        }

        public bool IsModuleEnabled(string moduleName)
        {
            ValidateModuleName(moduleName);
            return _moduleStates.TryGetValue(moduleName, out bool enabled) && enabled;
        }

        public void SetModuleState(string moduleName, bool isEnabled)
        {
            ValidateModuleName(moduleName);
            _moduleStates[moduleName] = isEnabled;
        }

        private static void ValidateModuleName(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                throw new ArgumentException("模块名称不能为空", nameof(moduleName));
            }
        }
    }
}
