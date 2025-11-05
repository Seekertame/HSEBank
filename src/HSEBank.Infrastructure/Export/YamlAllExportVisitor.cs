using HSEBank.Domain.Entities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HSEBank.Infrastructure.Export;

public sealed class YamlAllExportVisitor : IExportVisitor
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
            accounts = _accs.Select(a => new { id = a.Id.Value, name = a.Name, balance = a.Balance.Value }).ToList(),
            categories = _cats.Select(c => new { id = c.Id.Value, type = c.Type.ToString(), name = c.Name }).ToList(),
            operations = _ops.Select(o => new {
                id = o.Id.Value,
                type = o.Type.ToString(),
                accountId = o.AccountId.Value,
                categoryId = o.CategoryId.Value,
                amount = o.Amount.Value,
                date = o.Date,
                description = o.Description.Value
            }).ToList()
        };

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
            .Build();

        return serializer.Serialize(payload);
    }
}
