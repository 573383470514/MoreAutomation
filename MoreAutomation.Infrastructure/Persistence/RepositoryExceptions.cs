using System;

namespace MoreAutomation.Infrastructure.Persistence
{
    public class RepositoryException : Exception
    {
        public RepositoryException(string message, Exception? inner = null) : base(message, inner) { }
    }

    public class DuplicateAccountException : RepositoryException
    {
        public DuplicateAccountException(string message, Exception? inner = null) : base(message, inner) { }
    }
}
