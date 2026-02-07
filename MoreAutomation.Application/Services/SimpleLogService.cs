using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

namespace MoreAutomation.Application.Services
{
    public class SimpleLogService : ILogService
    {
        private readonly List<string> _lines = new();

        public void Append(string message)
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            lock (_lines)
            {
                _lines.Add(line);
                if (_lines.Count > 5000) _lines.RemoveAt(0);
            }
        }

        public IReadOnlyList<string> GetAll()
        {
            lock (_lines) return _lines.AsReadOnly();
        }

        public void Clear()
        {
            lock (_lines) _lines.Clear();
        }
    }
}
