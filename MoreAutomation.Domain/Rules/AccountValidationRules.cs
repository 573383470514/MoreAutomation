namespace MoreAutomation.Domain.Rules
{
    public static class AccountValidationRules
    {
        public const int MaxTotalAccounts = 10000;
        public const int MaxAccountsPerGroup = 5;
        public const int MinAccountsPerGroup = 1;
        public const long MinAccountValue = 55555;
        public const long MaxAccountValue = 999999999;
    }
}