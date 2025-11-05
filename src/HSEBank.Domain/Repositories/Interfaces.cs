using HSEBank.Domain.Entities;
using HSEBank.Domain.ValueObjects;

namespace HSEBank.Domain.Repositories
{
    public interface IBankAccountRepository
    {
        BankAccount? Get(AccountId id);
        IEnumerable<BankAccount> GetAll();
        void Add(BankAccount bankAccount);
        void Update(BankAccount bankAccount);
        void Remove(AccountId id);
    }

    public interface ICategoryRepository
    {
        Category? Get(CategoryId id);
        IEnumerable<Category> GetAll();
        void Add(Category category);
        void Update(Category category);
        void Remove(CategoryId id);
    }

    public interface IOperationRepository
    {
        Operation? Get(OperationId id);
        IEnumerable<Operation> GetByAccount(AccountId accountId);
        void Add(Operation op);
        void Update(Operation op);
        void Remove(OperationId id);
        IEnumerable<Operation> GetAll();
    }
}