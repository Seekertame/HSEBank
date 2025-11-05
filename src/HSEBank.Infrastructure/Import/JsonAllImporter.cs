using System.Text.Json;
using HSEBank.Domain.Entities;
using HSEBank.Domain.Repositories;
using HSEBank.Domain.ValueObjects;

namespace HSEBank.Infrastructure.Import
{
    public sealed class JsonAllImporter : ImporterBase<object>
    {
        private readonly IBankAccountRepository _accounts;
        private readonly ICategoryRepository _cats;
        private readonly IOperationRepository _ops;

        public JsonAllImporter(IBankAccountRepository accounts, ICategoryRepository cats, IOperationRepository ops)
        { _accounts = accounts; _cats = cats; _ops = ops; }

        private sealed class Snapshot
        {
            public List<AccountRow> Accounts { get; set; } = [];
            public List<CategoryRow> Categories { get; set; } = [];
            public List<OperationRow> Operations { get; set; } = [];
        }
        private sealed class AccountRow { public Guid Id { get; set; } public string Name { get; set; } = ""; public decimal Balance { get; set; } }
        private sealed class CategoryRow { public Guid Id { get; set; } public string Type { get; set; } = ""; public string Name { get; set; } = ""; }
        private sealed class OperationRow
        {
            public Guid Id { get; set; }
            public string Type { get; set; } = "";
            public Guid AccountId { get; set; }
            public Guid CategoryId { get; set; }
            public decimal Amount { get; set; }
            public DateTime Date { get; set; }
            public string? Description { get; set; }
        }

        protected override IEnumerable<object> Parse(string raw)
        {
            var snap = JsonSerializer.Deserialize<Snapshot>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                       ?? new Snapshot();
            yield break;
        }

        protected override void Persist(IEnumerable<object> _)
        {
            throw new NotSupportedException("Use ImportJson(string raw) overload");
        }

        public (int accs, int cats, int ops) ImportJsonString(string raw)
        {
            var snap = JsonSerializer.Deserialize<Snapshot>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                       ?? new Snapshot();

            int a = 0, c = 0, o = 0;

            foreach (var r in snap.Accounts)
            {
                var id = new AccountId(r.Id);
                var acc = _accounts.Get(id);
                if (acc is null) { _accounts.Add(new BankAccount(id, r.Name, new Money(r.Balance))); a++; }
                else { acc = new BankAccount(id, r.Name, new Money(r.Balance)); _accounts.Update(acc); }
            }

            foreach (var r in snap.Categories)
            {
                var id = new CategoryId(r.Id);
                var type = Enum.Parse<CategoryType>(r.Type, true);
                var cat = _cats.Get(id);
                if (cat is null) { _cats.Add(new Category(id, type, r.Name)); c++; }
                else { _cats.Update(new Category(id, type, r.Name)); }
            }

            foreach (var r in snap.Operations)
            {
                var type = Enum.Parse<OperationType>(r.Type, true);
                var op = new Operation(new OperationId(r.Id), type,
                    new AccountId(r.AccountId), new CategoryId(r.CategoryId),
                    new Money(r.Amount), r.Date, new Description(r.Description));
                _ops.Add(op); o++;
            }

            foreach (var acc in _accounts.GetAll())
            {
                var ops = _ops.GetByAccount(acc.Id);
                acc.RecomputeBalance(ops);
                _accounts.Update(acc);
            }

            return (a, c, o);
        }
    }
}
