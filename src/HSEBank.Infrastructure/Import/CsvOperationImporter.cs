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
        public CsvOperationImporter(IOperationRepository ops) => _ops = ops;

        protected override IEnumerable<Operation> Parse(string raw)
        {
            using var reader = new StringReader(raw);
            var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null
            };
            using var csv = new CsvReader(reader, cfg);

            var rowIndex = 1;
            while (csv.Read())
            {
                rowIndex++;

                // 1) Type
                var typeStr = GetFieldTrimmed(csv, "Type");
                if (!Enum.TryParse<OperationType>(typeStr, ignoreCase: true, out var type))
                    throw new InvalidDataException($"Row {rowIndex}: invalid Type '{typeStr}'. Expected Income/Expense.");

                // 2) AccountId
                var accountIdStr = GetFieldTrimmed(csv, "AccountId");
                if (!Guid.TryParse(accountIdStr, out var accountGuid))
                    throw new InvalidDataException($"Row {rowIndex}: invalid AccountId '{accountIdStr}'.");
                var accountId = new AccountId(accountGuid);

                // 3) CategoryId
                var categoryIdStr = GetFieldTrimmed(csv, "CategoryId");
                if (!Guid.TryParse(categoryIdStr, out var categoryGuid))
                    throw new InvalidDataException($"Row {rowIndex}: invalid CategoryId '{categoryIdStr}'.");
                var categoryId = new CategoryId(categoryGuid);

                // 4) Amount
                var amountStr = GetFieldTrimmed(csv, "Amount");
                if (!decimal.TryParse(amountStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var amountDec))
                    throw new InvalidDataException($"Row {rowIndex}: invalid Amount '{amountStr}'.");
                var amount = new Money(amountDec);

                // 5) Date
                var dateStr = GetFieldTrimmed(csv, "Date");
                if (!DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date))
                    throw new InvalidDataException($"Row {rowIndex}: invalid Date '{dateStr}'. Use ISO-8601 or InvariantCulture formats.");

                // 6) Description (необязательно): пусть пустое/отсутствующее значение станет ""
                var descRaw = csv.GetField("Description");
                var desc = new Description(descRaw); // VO сам нормализует null/пробелы/длину

                yield return new Operation(OperationId.New(), type, accountId, categoryId, amount, date, desc);
            }
        }

        protected override void Persist(IEnumerable<Operation> items)
        {
            foreach (var op in items)
                _ops.Add(op);
        }

        private static string GetFieldTrimmed(CsvReader csv, string name)
        {
            // TryGetField возвращает false, если колонки нет — дадим своё сообщение
            if (!csv.TryGetField<string>(name, out var s))
                throw new InvalidDataException($"Missing required column '{name}'.");
            return (s ?? string.Empty).Trim();
        }
    }
}
