using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreAutomation.Contracts.Interfaces;
using MoreAutomation.Domain.Entities;
using MoreAutomation.Domain.Rules;
using MoreAutomation.Application.Services;

namespace MoreAutomation.Application.UseCases.AccountManagement
{
    public class AccountService
    {
        private readonly IAccountRepository _repository;
        private readonly MoreAutomation.Infrastructure.Platform.ProxyManager _proxyManager;
        private readonly MoreAutomation.Application.Services.ILogService _log;

        public AccountService(IAccountRepository repository, MoreAutomation.Infrastructure.Platform.ProxyManager proxyManager, MoreAutomation.Application.Services.ILogService log)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _proxyManager = proxyManager;
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<List<Account>> GetAccountsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task AddAccountAsync(long accNum, string pwd, int groupId, string note)
        {
            if (string.IsNullOrWhiteSpace(pwd))
            {
                throw new ArgumentException("账号密码不能为空", nameof(pwd));
            }

            if (groupId < AccountValidationRules.MinGroupId || groupId > AccountValidationRules.MaxGroupId)
            {
                throw new ArgumentOutOfRangeException(nameof(groupId), $"组号必须在 {AccountValidationRules.MinGroupId} ~ {AccountValidationRules.MaxGroupId} 之间");
            }

            var all = await _repository.GetAllAsync();

            if (all.Count >= AccountValidationRules.MaxTotalAccounts)
            {
                throw new InvalidOperationException("账号总量已达上限");
            }

            var groupCount = all.Count(a => a.GroupId == groupId);
            if (groupCount >= AccountValidationRules.MaxAccountsPerGroup)
            {
                throw new InvalidOperationException($"组 {groupId} 已满（上限 {AccountValidationRules.MaxAccountsPerGroup} 个）");
            }

            if (all.Any(a => a.AccountNumber == accNum))
            {
                throw new InvalidOperationException("该账号已存在");
            }

            bool isMaster = groupCount == 0;

            var account = new Account(accNum)
            {
                Password = pwd,
                GroupId = groupId,
                Note = note?.Trim() ?? string.Empty,
                IsMaster = isMaster,
                SortIndex = all.Count + 1
            };

            await _repository.AddAsync(account);
            try { _log.Append($"添加账号 {accNum} 到组 {groupId}"); } catch {}
        }

        public async Task DeleteAccountAsync(long accNum)
        {
            var all = await _repository.GetAllAsync();
            var account = all.Find(a => a.AccountNumber == accNum);
            if (account == null) return;

            var groupCount = all.Count(a => a.GroupId == account.GroupId);
            if (groupCount - 1 < AccountValidationRules.MinAccountsPerGroup)
            {
                throw new InvalidOperationException($"组 {account.GroupId} 中账号数不能少于 {AccountValidationRules.MinAccountsPerGroup}。");
            }

            await _repository.DeleteAsync(accNum);
            try { _log.Append($"删除账号 {accNum}"); } catch {}
        }

        public async Task SetMasterAsync(long accNum)
        {
            var all = await _repository.GetAllAsync();
            var account = all.Find(a => a.AccountNumber == accNum);
            if (account == null) throw new InvalidOperationException("账号不存在");

            var groupId = account.GroupId;
            // 将本组其他账号取消主控
            var groupMembers = all.FindAll(a => a.GroupId == groupId);
            foreach (var m in groupMembers)
            {
                m.IsMaster = m.AccountNumber == accNum;
                await _repository.UpdateAsync(m);
            }
            try { _log.Append($"设置账号 {accNum} 为主控（组 {groupId}）"); } catch {}
        }

        public async Task UpdateAccountAsync(MoreAutomation.Domain.Entities.Account account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
            try
            {
                await _repository.UpdateAsync(account);

                // sync proxy mapping into in-memory manager
                try
                {
                    _proxyManager?.SetProxy(account.AccountNumber, account.ProxyPort);
                }
                catch
                {
                    // swallow proxy mapping errors to avoid breaking account update
                }
            }
            catch (MoreAutomation.Infrastructure.Persistence.RepositoryException rex)
            {
                throw new BusinessException(BusinessErrorCode.RepositoryError, "仓储操作失败", rex);
            }
        }

        public async Task MoveAccountAsync(long accNum, int targetGroupId)
        {
            if (targetGroupId < AccountValidationRules.MinGroupId || targetGroupId > AccountValidationRules.MaxGroupId)
            {
                throw new ArgumentOutOfRangeException(nameof(targetGroupId));
            }
            var all = await _repository.GetAllAsync();
            var account = all.Find(a => a.AccountNumber == accNum);
            if (account == null) throw new InvalidOperationException("账号不存在");

            if (account.GroupId == targetGroupId) return; // no-op

            var sourceGroupId = account.GroupId;
            var sourceGroupCount = all.Count(a => a.GroupId == sourceGroupId);
            var targetGroupCount = all.Count(a => a.GroupId == targetGroupId);
            var wasMaster = account.IsMaster;

            if (sourceGroupCount - 1 < AccountValidationRules.MinAccountsPerGroup)
            {
                throw new BusinessException(BusinessErrorCode.GroupSizeViolation, $"迁移后源组 {sourceGroupId} 的账号数不能少于 {AccountValidationRules.MinAccountsPerGroup}");
            }

            if (targetGroupCount + 1 > AccountValidationRules.MaxAccountsPerGroup)
            {
                throw new BusinessException(BusinessErrorCode.GroupSizeViolation, $"目标组 {targetGroupId} 已达上限 {AccountValidationRules.MaxAccountsPerGroup}");
            }

            try
            {
                // 更新移动账号的组号与 IsMaster 标记（若目标组为空则为主控）
                account.GroupId = targetGroupId;
                account.IsMaster = targetGroupCount == 0;
                await _repository.UpdateAsync(account);
                try { _log.Append($"将账号 {accNum} 从组 {sourceGroupId} 移动到组 {targetGroupId}"); } catch {}

                // 如果被移动的是主控且源组仍有成员，需在源组选举新的主控
                if (wasMaster && sourceGroupCount - 1 >= AccountValidationRules.MinAccountsPerGroup)
                {
                    var refreshed = await _repository.GetAllAsync();
                    var sourceMembers = refreshed.FindAll(a => a.GroupId == sourceGroupId);
                    if (sourceMembers.Count > 0)
                    {
                        var newMaster = sourceMembers.OrderBy(a => a.SortIndex).First();
                        newMaster.IsMaster = true;
                        await _repository.UpdateAsync(newMaster);
                    }
                }
            }
            catch (MoreAutomation.Infrastructure.Persistence.DuplicateAccountException dex)
            {
                throw new BusinessException(BusinessErrorCode.DuplicateAccount, "账号冲突", dex);
            }
            catch (MoreAutomation.Infrastructure.Persistence.RepositoryException rex)
            {
                throw new BusinessException(BusinessErrorCode.RepositoryError, "仓储操作失败", rex);
            }
        }
    }
}
