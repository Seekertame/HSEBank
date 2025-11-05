using HSEBank.Domain.Entities;
using HSEBank.Domain.Repositories;
using HSEBank.Domain.ValueObjects;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HSEBank.Infrastructure.Import
{
    public sealed class YamlAllImporter : ImporterBase<object>
    {
        private readonly IBankAccountRepository _accounts;
        private readonly ICategoryRepository _cats;
        private readonly IOperationRepository _ops;

        private Snapshot? _snapshot;

        public YamlAllImporter(
            IBankAccountRepository accounts,
            ICategoryRepository cats,
            IOperationRepository ops)
        {
            _accounts = accounts; _cats = cats; _ops = ops;
        }

        private sealed class Snapshot
        {
            public List<AccountRow> Accounts { get; set; } = [];
            public List<CategoryRow> Categories { get; set; } = [];
            public List<OperationRow> Operations { get; set; } = [];
        }
        private sealed class AccountRow
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = "";
            public decimal Balance { get; set; }
        }
        private sealed class CategoryRow
        {
            public Guid Id { get; set; }
            public string Type { get; set; } = "";
            public string Name { get; set; } = "";
        }
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
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            _snapshot = deserializer.Deserialize<Snapshot>(raw) ?? new Snapshot();
            yield break;
        }

        protected override void Persist(IEnumerable<object> _)
        {
            if (_snapshot is null) return;

            foreach (var r in _snapshot.Accounts)
            {
                var id = new AccountId(r.Id);
                var acc = _accounts.Get(id);
                var upd = new BankAccount(id, r.Name, new Money(r.Balance));
                if (acc is null) _accounts.Add(upd); else _accounts.Update(upd);
            }

            foreach (var r in _snapshot.Categories)
            {
                var id = new CategoryId(r.Id);
                var type = Enum.Parse<CategoryType>(r.Type, true);
                var cat = new Category(id, type, r.Name);
                if (_cats.Get(id) is null) _cats.Add(cat); else _cats.Update(cat);
            }

            foreach (var r in _snapshot.Operations)
            {
                var op = new Operation(
                    new OperationId(r.Id),
                    Enum.Parse<OperationType>(r.Type, true),
                    new AccountId(r.AccountId),
                    new CategoryId(r.CategoryId),
                    new Money(r.Amount),
                    r.Date,
                    new Description(r.Description)
                );
                _ops.Add(op);
            }

            foreach (var acc in _accounts.GetAll())
            {
                var ops = _ops.GetByAccount(acc.Id);
                acc.RecomputeBalance(ops);
                _accounts.Update(acc);
            }
        }
    }
}
