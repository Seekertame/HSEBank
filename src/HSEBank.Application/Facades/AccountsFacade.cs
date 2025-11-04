using HSEBank.Domain.ValueObjects;
using HSEBank.Domain.Repositories;
using HSEBank.Domain.Entities;

namespace HSEBank.Application.Facades
{
    public class AccountsFacade
    {
        private readonly IBankAccountRepository _accounts;
        public AccountsFacade(IBankAccountRepository accounts) 
            => _accounts = accounts;
        public IEnumerable<BankAccount> GetAll()
            => _accounts.GetAll();
        public AccountId Create(string name, Money initialBalance)
        {
            var account = new BankAccount(AccountId.New(), name, initialBalance);
            _accounts.Add(account);
            return account.Id;
        }
        public void Rename(AccountId id, string newName)
        {
            var acc = _accounts.Get(id) ?? throw new InvalidOperationException("Account not found");
            acc.Rename(newName);
            _accounts.Update(acc);
        }

        public void Remove(AccountId id) => _accounts.Remove(id);
    }
}
