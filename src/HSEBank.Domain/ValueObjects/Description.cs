public readonly record struct Description
{
    public string Value { get; }
    public bool IsEmpty => Value.Length == 0;
    public static Description Empty => new(string.Empty);
    private const int MaxLength = 200;
    public Description(string? value)
    {
        value = value?.Trim() ?? string.Empty;
        if (value.Length > MaxLength)
        {
            throw new ArgumentException($"Description cannot exceed {MaxLength} characters", nameof(value));
        }
        Value = value;
    }
    public override string ToString() => Value;
    public static explicit operator Description(string? s) => new(s);
}

