using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using HSEBank.Domain.Entities;
using HSEBank.Domain.Repositories;
using HSEBank.Domain.ValueObjects;

namespace HSEBank.Infrastructure.Import
{
    public sealed class CsvAccountImporter : ImporterBase<BankAccount>
    {
        private readonly IBankAccountRepository _accounts;
        public CsvAccountImporter(IBankAccountRepository accounts) => _accounts = accounts;

        private sealed class Row { public Guid Id { get; set; } public string Name { get; set; } = ""; public decimal Balance { get; set; } }

        protected override IEnumerable<BankAccount> Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) yield break;
            var cfg = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true, Delimiter = ",", TrimOptions = TrimOptions.Trim };
            using var csv = new CsvReader(new StringReader(raw), cfg);
            foreach (var r in csv.GetRecords<Row>())
                yield return new BankAccount(new AccountId(r.Id), r.Name, new Money(r.Balance));
        }

        protected override void Persist(IEnumerable<BankAccount> items)
        {
            foreach (var acc in items)
            {
                var existing = _accounts.Get(acc.Id);
                if (existing is null) _accounts.Add(acc); else _accounts.Update(acc);
            }
        }
    }
}
