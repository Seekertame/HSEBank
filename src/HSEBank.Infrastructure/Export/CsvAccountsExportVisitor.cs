using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using HSEBank.Domain.Entities;

namespace HSEBank.Infrastructure.Export;

public sealed class CsvAccountsExportVisitor : IExportVisitor
{
    private readonly List<BankAccount> _accs = [];

    public void Visit(BankAccount acc) => _accs.Add(acc);
    public void Visit(Category cat) { }
    public void Visit(Operation op) { }

    public string GetResult()
    {
        var sb = new StringBuilder();
        using var sw = new StringWriter(sb);
        var cfg = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true, Delimiter = "," };
        using var csv = new CsvWriter(sw, cfg);

        csv.WriteField("Id"); csv.WriteField("Name"); csv.WriteField("Balance");
        csv.NextRecord();
        foreach (var a in _accs)
        {
            csv.WriteField(a.Id.Value);
            csv.WriteField(a.Name);
            csv.WriteField(a.Balance.Value.ToString(CultureInfo.InvariantCulture));
            csv.NextRecord();
        }
        return sb.ToString();
    }
}
