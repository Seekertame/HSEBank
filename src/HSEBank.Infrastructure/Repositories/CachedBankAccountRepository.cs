using HSEBank.Domain.Entities;
using HSEBank.Domain.Repositories;
using HSEBank.Domain.ValueObjects;

namespace HSEBank.Infrastructure.Repositories
{
    public sealed class CachedBankAccountRepository(IBankAccountRepository inner) : IBankAccountRepository
    {
        private readonly IBankAccountRepository _inner = inner;
        private readonly Dictionary<AccountId, BankAccount> _cache = [];
        private bool _initialized;

        private void EnsureInit()
        {
            if (_initialized) return;
            foreach (var acc in _inner.GetAll()) _cache[acc.Id] = acc;
            _initialized = true;
        }

        public BankAccount? Get(AccountId id)
        {
            EnsureInit();
            return _cache.TryGetValue(id, out var acc) ? acc : null;
        }

        public IEnumerable<BankAccount> GetAll()
        {
            EnsureInit();
            return _cache.Values;
        }

        public void Add(BankAccount account)
        {
            _inner.Add(account);
            _cache[account.Id] = account;
        }

        public void Update(BankAccount account)
        {
            _inner.Update(account);
            _cache[account.Id] = account;
        }

        public void Remove(AccountId id)
        {
            _inner.Remove(id);
            _cache.Remove(id);
        }
    }
}
