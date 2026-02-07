using MoreAutomation.Domain.Entities;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;

namespace MoreAutomation.Contracts.Interfaces
{
    public interface IAccountRepository
    {
        Task<List<Account>> GetAllAsync();
        Task AddAsync(Account account);
        Task DeleteAsync(long accountNumber);
        Task UpdateAsync(Account account);
    }
}