using HSEBank.Domain.ValueObjects;

namespace HSEBank.Domain.Entities
{
    public class Operation
    {
        public OperationId Id { get; }
        public OperationType Type { get; private set; }
        public AccountId AccountId { get; }
        public CategoryId CategoryId { get; private set; }
        public Money Amount { get; private set; }
        public DateTime Date { get; private set; }
        public Description Description { get; private set; }
        public Operation(
            OperationId id,
            OperationType type,
            AccountId accountId,
            CategoryId categoryId,
            Money amount,
            DateTime date,
            Description description)
        {
            if (amount.Value <= 0)
            {
                throw new ArgumentException("Amount must be positive", nameof(amount));
            }
            // Если вдруг пришёл default(Description), который может проскочить, нормализуем к пустому:
            if (description.Value is null) description = Description.Empty;

            Id = id;
            Type = type;
            AccountId = accountId;
            CategoryId = categoryId;
            Amount = amount;
            Date = date;
            Description = description;
        }
        public void Update(OperationType type, CategoryId categoryId, Money amount, DateTime date, Description description)
        {
            if (amount.Value <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));

            Type = type;
            CategoryId = categoryId;
            Amount = amount;
            Date = date;
            Description = description;
        }
    }
}