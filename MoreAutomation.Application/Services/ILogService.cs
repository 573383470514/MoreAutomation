using System;
using System.Collections.Generic;

namespace MoreAutomation.Application.Services
{
    public interface ILogService
    {
        void Append(string message);
        IReadOnlyList<string> GetAll();
        void Clear();
    }
}
