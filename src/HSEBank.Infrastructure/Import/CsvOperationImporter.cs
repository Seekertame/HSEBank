using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using HSEBank.Domain.Entities;
using HSEBank.Domain.Repositories;
using HSEBank.Domain.ValueObjects;

namespace HSEBank.Infrastructure.Import
{
    public sealed class CsvOperationImporter : ImporterBase<Operation>
    {
        private readonly IOperationRepository _ops;
        private readonly IBankAccountRepository _accounts;
        private readonly ICategoryRepository _cats;

        public CsvOperationImporter(
            IOperationRepository ops,
            IBankAccountRepository accounts,
            ICategoryRepository cats)
        {
            _ops = ops;
            _accounts = accounts;
            _cats = cats;
        }
        private sealed class Row
        {
            public string Type { get; set; } = "";
            public string AccountId { get; set; } = "";
            public string CategoryId { get; set; } = "";
            public string Amount { get; set; } = "";
            public string Date { get; set; } = "";
            public string? Description { get; set; }
        }

        protected override IEnumerable<Operation> Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                yield break;

            var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null,
                BadDataFound = null,
                PrepareHeaderForMatch = args => args.Header?.Trim() 
            };

            using var csv = new CsvReader(new StringReader(raw), cfg);

            var rows = csv.GetRecords<Row>();
            var i = 1; // 1 — шапка, далее данные
            foreach (var r in rows)
            {
                i++;

                if (!Enum.TryParse<OperationType>(r.Type, true, out var type))
                    throw new InvalidDataException($"Row {i}: invalid Type '{r.Type}' (use Income/Expense).");

                if (!Guid.TryParse(r.AccountId, out var accGuid))
                    throw new InvalidDataException($"Row {i}: invalid AccountId '{r.AccountId}'.");
                var accountId = new AccountId(accGuid);

                if (!Guid.TryParse(r.CategoryId, out var catGuid))
                    throw new InvalidDataException($"Row {i}: invalid CategoryId '{r.CategoryId}'.");
                var categoryId = new CategoryId(catGuid);

                if (!decimal.TryParse(r.Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amountDec))
                    throw new InvalidDataException($"Row {i}: invalid Amount '{r.Amount}' (use dot).");
                var amount = new Money(amountDec);

                if (!DateTime.TryParse(r.Date, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date))
                    throw new InvalidDataException($"Row {i}: invalid Date '{r.Date}' (use ISO-8601).");

                var desc = new Description(r.Description);
                yield return new Operation(OperationId.New(), type, accountId, categoryId, amount, date, desc);
            }
        }

        protected override void Persist(IEnumerable<Operation> items)
        {
            var list = items.ToList();

            foreach (var g in list.GroupBy(o => o.CategoryId))
            {
                if (_cats.Get(g.Key) is null)
                {
                    var catType = g.First().Type == OperationType.Income ? CategoryType.Income : CategoryType.Expense;
                    var shortId = g.Key.Value.ToString()[..8];
                    var name = catType == CategoryType.Income ? $"Imported Income {shortId}" : $"Imported Expense {shortId}";
                    _cats.Add(new Category(g.Key, catType, name));
                }
            }

            foreach (var g in list.GroupBy(o => o.AccountId))
            {
                var acc = _accounts.Get(g.Key);
                if (acc is null)
                {
                    var shortId = g.Key.Value.ToString()[..8];
                    acc = new BankAccount(g.Key, $"Imported {shortId}", Money.Zero);
                    _accounts.Add(acc);
                }

                foreach (var op in g)
                    _ops.Add(op);

                var opsForAcc = _ops.GetByAccount(g.Key);
                acc.RecomputeBalance(opsForAcc);
                _accounts.Update(acc);
            }
        }
    }
}
