using System.Collections.Generic;
using System.Linq;
using MoreAutomation.Contracts.Interfaces;
using MoreAutomation.Domain.Entities;

namespace MoreAutomation.Infrastructure.Platform
{
    public class ProxyManager
    {
        private readonly IAccountRepository _repo;
        private readonly Dictionary<long, int> _map = new();

        public ProxyManager(IAccountRepository repo)
        {
            _repo = repo;
        }

        public void Initialize()
        {
            var all = _repo.GetAllAsync().GetAwaiter().GetResult();
            foreach (var a in all.Where(x => x.ProxyPort > 0))
            {
                _map[a.AccountNumber] = a.ProxyPort;
            }
        }

        public int? GetProxyPort(long accountNumber)
        {
            if (_map.TryGetValue(accountNumber, out int p)) return p;
            return null;
        }

        public void SetProxy(long accountNumber, int port)
        {
            if (port <= 0)
            {
                _map.Remove(accountNumber);
                return;
            }

            _map[accountNumber] = port;
        }
    }
}
