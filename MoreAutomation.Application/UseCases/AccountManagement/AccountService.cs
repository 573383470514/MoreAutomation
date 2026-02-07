using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreAutomation.Contracts.Interfaces;
using MoreAutomation.Domain.Entities;
using MoreAutomation.Domain.Rules;

namespace MoreAutomation.Application.UseCases.AccountManagement
{
    public class AccountService
    {
        private readonly IAccountRepository _repository;

        public AccountService(IAccountRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<List<Account>> GetAccountsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task AddAccountAsync(long accNum, string pwd, int groupId, string note)
        {
            if (string.IsNullOrWhiteSpace(pwd))
                throw new ArgumentException("账号密码不能为空", nameof(pwd));

            if (groupId < 0)
                throw new ArgumentOutOfRangeException(nameof(groupId), "分组编号不能小于 0");

            var all = await _repository.GetAllAsync();

            // 1. 落实 Domain 硬约束：总量上限
            if (all.Count >= AccountValidationRules.MaxTotalAccounts)
                throw new InvalidOperationException("账号总量已达上限");

            // 2. 落实 Domain 硬约束：每组数量上限
            var groupCount = all.Count(a => a.GroupId == groupId);
            if (groupCount >= AccountValidationRules.MaxAccountsPerGroup)
                throw new InvalidOperationException($"组 {groupId} 已满（上限 {AccountValidationRules.MaxAccountsPerGroup} 个）");

            if (all.Any(a => a.AccountNumber == accNum))
                throw new InvalidOperationException("该账号已存在");

            // 3. 业务逻辑：如果是组内第一个账号，设为主控
            bool isMaster = groupCount == 0;

            var account = new Account(accNum)
            {
                Password = pwd, // 实际建议在此处调用加密服务
                GroupId = groupId,
                Note = note?.Trim() ?? string.Empty,
                IsMaster = isMaster,
                SortIndex = all.Count + 1
            };

            await _repository.AddAsync(account);
        }

        public async Task DeleteAccountAsync(long accNum)
        {
            await _repository.DeleteAsync(accNum);
        }
    }
}
