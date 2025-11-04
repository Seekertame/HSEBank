namespace HSEBank.Domain.ValueObjects
{
    public readonly record struct Money(decimal Value)
    {

        public static Money Zero => new(0m);

        public static Money operator +(Money left, Money right) 
            => new(left.Value + right.Value);

        public static Money operator -(Money left, Money right)
            => new(left.Value - right.Value);

        public override string ToString() => Value.ToString("0.00");

    }
}