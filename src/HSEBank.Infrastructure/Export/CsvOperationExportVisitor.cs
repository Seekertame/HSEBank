using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using HSEBank.Domain.Entities;

namespace HSEBank.Infrastructure.Export
{
    public sealed class CsvOperationExportVisitor : IExportVisitor
    {
        private readonly List<Operation> _ops = [];

        public void Visit(BankAccount acc) { }
        public void Visit(Category cat) { }
        public void Visit(Operation op) => _ops.Add(op);

        public string GetResult()
        {
            var sb = new StringBuilder();
            using var sw = new StringWriter(sb);
            var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                TrimOptions = TrimOptions.Trim
            };
            using var csv = new CsvWriter(sw, cfg);

            // header
            csv.WriteField("Type");
            csv.WriteField("AccountId");
            csv.WriteField("CategoryId");
            csv.WriteField("Amount");
            csv.WriteField("Date");
            csv.WriteField("Description");
            csv.NextRecord();

            // rows
            foreach (var o in _ops)
            {
                csv.WriteField(o.Type.ToString());
                csv.WriteField(o.AccountId.Value);
                csv.WriteField(o.CategoryId.Value);
                csv.WriteField(o.Amount.Value.ToString(CultureInfo.InvariantCulture));
                csv.WriteField(o.Date.ToString("o", CultureInfo.InvariantCulture)); 
                csv.WriteField(o.Description.Value);
                csv.NextRecord();
            }

            return sb.ToString();
        }
    }
}
