using System.Text.Json;
using HSEBank.Domain.Entities;

namespace HSEBank.Infrastructure.Export;

public sealed class JsonAllExportVisitor : IExportVisitor
{
    private readonly List<BankAccount> _accs = [];
    private readonly List<Category> _cats = [];
    private readonly List<Operation> _ops = [];

    public void Visit(BankAccount acc) => _accs.Add(acc);
    public void Visit(Category cat) => _cats.Add(cat);
    public void Visit(Operation op) => _ops.Add(op);

    public string GetResult()
    {
        var payload = new
        {
            accounts = _accs.Select(a => new { id = a.Id.Value, name = a.Name, balance = a.Balance.Value }),
            categories = _cats.Select(c => new { id = c.Id.Value, type = c.Type.ToString(), name = c.Name }),
            operations = _ops.Select(o => new {
                id = o.Id.Value,
                type = o.Type.ToString(),
                accountId = o.AccountId.Value,
                categoryId = o.CategoryId.Value,
                amount = o.Amount.Value,
                date = o.Date,
                description = o.Description.Value
            })
        };
        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    }
}
