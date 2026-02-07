using System;

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
                if (value < 55555 || value > 999999999)
                    throw new ArgumentException("账号必须在 55555 ~ 999999999 之间");
                _accountNumber = value;
            }
        }

        public string Password { get; set; } = string.Empty;
        public int GroupId { get; set; }
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