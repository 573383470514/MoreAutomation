using System;
using MoreAutomation.Domain.Rules;

namespace MoreAutomation.Domain.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public int SortIndex { get; set; }

        private long _accountNumber;
        public long AccountNumber
        {
            get => _accountNumber;
            set
            {
                if (value < AccountValidationRules.MinAccountValue || value > AccountValidationRules.MaxAccountValue)
                {
                    throw new ArgumentException($"账号必须在 {AccountValidationRules.MinAccountValue} ~ {AccountValidationRules.MaxAccountValue} 之间");
                }

                _accountNumber = value;
            }
        }

        private int _groupId = AccountValidationRules.MinGroupId;
        public int GroupId
        {
            get => _groupId;
            set
            {
                if (value < AccountValidationRules.MinGroupId || value > AccountValidationRules.MaxGroupId)
                {
                    throw new ArgumentOutOfRangeException(nameof(GroupId), $"组号必须在 {AccountValidationRules.MinGroupId} ~ {AccountValidationRules.MaxGroupId} 之间");
                }

                _groupId = value;
            }
        }

        public string Password { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public bool IsMaster { get; set; }
        public int ProxyPort { get; set; }

        public Account() { }

        public Account(long accountNumber)
        {
            AccountNumber = accountNumber;
        }
    }
}
