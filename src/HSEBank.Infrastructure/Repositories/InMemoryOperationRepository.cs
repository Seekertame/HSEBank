using HSEBank.Domain.Entities;
using HSEBank.Domain.Repositories;
using HSEBank.Domain.ValueObjects;

namespace HSEBank.Infrastructure.Repositories
{
    public sealed class InMemoryOperationRepository : IOperationRepository
    {
        private readonly List<Operation> _items = [];

        public Operation? Get(OperationId id) => _items.FirstOrDefault(o => o.Id.Equals(id));

        public IEnumerable<Operation> GetByAccount(AccountId id)
            => _items.Where(o => o.AccountId.Equals(id)).OrderBy(o => o.Date).ToList();

        public IEnumerable<Operation> GetAll() => _items.ToList();

        public void Add(Operation op) => _items.Add(op);

        public void Update(Operation op)
        {
            var i = _items.FindIndex(x => x.Id.Equals(op.Id));
            if (i >= 0) _items[i] = op;
        }

        public void Remove(OperationId id)
            => _items.RemoveAll(o => o.Id.Equals(id));
    }
}