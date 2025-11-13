namespace IdentityService.Domain.ValueObjects;

/// <summary>
/// Value object representing an idempotency key
/// </summary>
public class IdempotencyKey
{
    public string Value { get; }

    private IdempotencyKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Idempotency key cannot be empty", nameof(value));

        if (value.Length > 128)
            throw new ArgumentException("Idempotency key cannot exceed 128 characters", nameof(value));

        Value = value;
    }

    public static IdempotencyKey Create(string value)
    {
        return new IdempotencyKey(value);
    }

    public static IdempotencyKey? CreateOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return new IdempotencyKey(value);
    }

    public override bool Equals(object? obj)
    {
        return obj is IdempotencyKey other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }
}

