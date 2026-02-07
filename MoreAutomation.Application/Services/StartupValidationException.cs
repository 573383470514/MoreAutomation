using System;

namespace MoreAutomation.Application.Services
{
    public class StartupValidationException : Exception
    {
        public StartupValidationException(string message) : base(message) { }
        public StartupValidationException(string message, Exception inner) : base(message, inner) { }
    }
}
