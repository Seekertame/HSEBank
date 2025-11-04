namespace HSEBank.Domain.ValueObjects
{
    public readonly record struct AccountId(Guid Value)
    {
        public static AccountId New() => new(Guid.NewGuid());
        public override string ToString() => Value.ToString();
    }
    public readonly record struct CategoryId(Guid Value)
    {
        public static CategoryId New() => new(Guid.NewGuid());
        public override string ToString() => Value.ToString();
    }
    public readonly record struct OperationId(Guid Value)
    {
        public static OperationId New() => new(Guid.NewGuid());
        public override string ToString() => Value.ToString();
    }

}