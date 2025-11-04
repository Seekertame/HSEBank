using System.Text.Json;
using System.Text.Json.Serialization;
using HSEBank.Domain.Entities;
using HSEBank.Domain.ValueObjects;

namespace HSEBank.Infrastructure.Export
{

    public sealed class JsonOperationExportVisitor : IExportVisitor
    {
        private readonly List<OperationRow> _items = new();

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        private sealed record OperationRow(
            Guid Id,
            OperationType Type,
            Guid AccountId,
            Guid CategoryId,
            decimal Amount,
            DateTime DateUtc,
            string Description
        );

        public void Visit(BankAccount acc) { }
        public void Visit(Category cat) { }

        public void Visit(Operation op)
        {
            _items.Add(new OperationRow(
                Id: op.Id.Value,
                Type: op.Type,
                AccountId: op.AccountId.Value,
                CategoryId: op.CategoryId.Value,
                Amount: op.Amount.Value,
                DateUtc: op.Date.ToUniversalTime(),
                Description: op.Description.Value
            ));
        }

        public string GetResult() => JsonSerializer.Serialize(_items, JsonOptions);

        public void WriteTo(Stream target) => JsonSerializer.Serialize(target, _items, JsonOptions);

        public void Reset() => _items.Clear();
    }
}
