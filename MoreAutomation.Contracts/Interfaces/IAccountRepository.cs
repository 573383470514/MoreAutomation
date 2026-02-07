using System.Collections.Generic;
using System.Threading.Tasks;
using MoreAutomation.Domain.Entities;

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
