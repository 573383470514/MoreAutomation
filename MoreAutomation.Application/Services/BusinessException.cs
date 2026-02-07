using System;

namespace MoreAutomation.Application.Services
{
    public enum BusinessErrorCode
    {
        None = 0,
        DuplicateAccount = 1,
        RepositoryError = 2,
        GroupSizeViolation = 3
    }

    public class BusinessException : Exception
    {
        public BusinessErrorCode Code { get; }

        public BusinessException(BusinessErrorCode code, string message) : base(message)
        {
            Code = code;
        }

        public BusinessException(BusinessErrorCode code, string message, Exception inner) : base(message, inner)
        {
            Code = code;
        }
    }
}
