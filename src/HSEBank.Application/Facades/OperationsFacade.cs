using HSEBank.Domain.Entities;
using HSEBank.Domain.Factories;
using HSEBank.Domain.Repositories;
using HSEBank.Domain.ValueObjects;

namespace HSEBank.Application.Facades;

public class OperationsFacade(IOperationRepository ops, IBankAccountRepository accounts, OperationFactory factory)
{
    private readonly IOperationRepository _ops = ops;
    private readonly IBankAccountRepository _accounts = accounts;
    private readonly OperationFactory _factory = factory;

    public OperationId Add(AccountId accountId, CategoryId categoryId, OperationType type, Money amount, DateTime date, Description description)
    {
        var account = _accounts.Get(accountId) ?? throw new InvalidOperationException("Account not found");
        var op = _factory.CreateOperation(type, accountId, categoryId, amount, date, description);
        _ops.Add(op);

        account.RecomputeBalance(_ops.GetByAccount(accountId));
        _accounts.Update(account);
        return op.Id;
    }

    public IEnumerable<Operation> GetByAccount(AccountId accountId) => _ops.GetByAccount(accountId);

    public void Edit(OperationId id, OperationType type, CategoryId categoryId, Money amount, DateTime date, Description description)
    {
        var op = _ops.Get(id) ?? throw new InvalidOperationException("Operation not found");
        var account = _accounts.Get(op.AccountId) ?? throw new InvalidOperationException("Account not found");

        op.Update(type, categoryId, amount, date, description);
        _ops.Update(op);

        account.RecomputeBalance(_ops.GetByAccount(account.Id));
        _accounts.Update(account);
    }

    public void Remove(OperationId id)
    {
        var op = _ops.Get(id) ?? throw new InvalidOperationException("Operation not found");
        _ops.Remove(id);

        var account = _accounts.Get(op.AccountId);
        if (account is not null)
        {
            account.RecomputeBalance(_ops.GetByAccount(account.Id));
            _accounts.Update(account);
        }
    }
}
