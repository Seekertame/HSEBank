using HSEBank.Domain.Entities;
using HSEBank.Domain.Repositories;
using HSEBank.Domain.ValueObjects;

namespace HSEBank.Infrastructure.Repositories
{
    public sealed class InMemoryBankAccountRepository : IBankAccountRepository
    {
        private readonly Dictionary<AccountId, BankAccount> _store = [];

        public BankAccount? Get(AccountId id)
            => _store.TryGetValue(id, out var acc) ? acc : null;

        public IEnumerable<BankAccount> GetAll()
            => _store.Values;

        public void Add(BankAccount account)
            => _store[account.Id] = account;

        public void Update(BankAccount account)
            => _store[account.Id] = account;

        public void Remove(AccountId id)
            => _store.Remove(id);
    }
}