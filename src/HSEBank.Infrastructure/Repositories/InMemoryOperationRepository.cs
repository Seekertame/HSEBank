using HSEBank.Domain.Entities;
using HSEBank.Domain.Repositories;
using HSEBank.Domain.ValueObjects;

namespace HSEBank.Infrastructure.Repositories
{
    public class InMemoryOperationRepository : IOperationRepository
    {
        private readonly Dictionary<OperationId, Operation> _store = [];
        public Operation? Get(OperationId id)
            => _store.TryGetValue(id, out var op) ? op : null;

        public IEnumerable<Operation> GetByAccount(AccountId accountId)
            => _store.Values.Where(o => o.AccountId.Equals(accountId));

        public void Add(Operation op) => _store[op.Id] = op;
        public void Update(Operation op) => _store[op.Id] = op;
        public void Remove(OperationId id) => _store.Remove(id);
    }
}