using System.Collections.Generic;

namespace MoreAutomation.Application.Services
{
    public class ModuleRegistry
    {
        private readonly Dictionary<string, bool> _moduleStates = new();

        public void RegisterModule(string moduleName, bool defaultEnabled = true)
        {
            if (!_moduleStates.ContainsKey(moduleName))
                _moduleStates[moduleName] = defaultEnabled;
        }

        public bool IsModuleEnabled(string moduleName)
        {
            return _moduleStates.TryGetValue(moduleName, out bool enabled) && enabled;
        }

        public void SetModuleState(string moduleName, bool isEnabled)
        {
            _moduleStates[moduleName] = isEnabled;
        }
    }
}