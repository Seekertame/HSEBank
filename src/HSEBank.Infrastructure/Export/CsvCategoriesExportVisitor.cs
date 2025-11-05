using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using HSEBank.Domain.Entities;

namespace HSEBank.Infrastructure.Export;

public sealed class CsvCategoriesExportVisitor : IExportVisitor
{
    private readonly List<Category> _cats = [];
    public void Visit(BankAccount acc) { }
    public void Visit(Category cat) => _cats.Add(cat);
    public void Visit(Operation op) { }

    public string GetResult()
    {
        var sb = new StringBuilder();
        using var sw = new StringWriter(sb);
        var cfg = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true, Delimiter = "," };
        using var csv = new CsvWriter(sw, cfg);

        csv.WriteField("Id"); csv.WriteField("Type"); csv.WriteField("Name");
        csv.NextRecord();
        foreach (var c in _cats)
        {
            csv.WriteField(c.Id.Value);
            csv.WriteField(c.Type.ToString());
            csv.WriteField(c.Name);
            csv.NextRecord();
        }
        return sb.ToString();
    }
}
