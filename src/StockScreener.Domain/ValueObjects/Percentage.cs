namespace StockScreener.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing a percentage value.
/// Supports both bounded (0–100) and unbounded (e.g. growth rates) use cases.
/// </summary>
public sealed record Percentage
{
    public decimal Value { get; }

    /// <summary>Creates a percentage with no bounds checking.</summary>
    private Percentage(decimal value)
    {
        Value = Math.Round(value, 4, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Creates a percentage value. No range restrictions — suitable for growth rates,
    /// profit margins, returns, etc. that can exceed 100 or be negative.
    /// </summary>
    public static Percentage Of(decimal value) => new(value);

    /// <summary>
    /// Creates a percentage value bounded between 0 and 100 inclusive.
    /// Use for indicators such as RSI, dividend yield cap checks, etc.
    /// </summary>
    public static Percentage Bounded(decimal value, decimal min = 0m, decimal max = 100m)
    {
        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(nameof(value),
                $"Percentage value {value} must be between {min} and {max}.");

        return new Percentage(value);
    }

    public bool IsGreaterThan(Percentage other) => Value > other.Value;
    public bool IsLessThan(Percentage other) => Value < other.Value;
    public bool IsGreaterThanOrEqualTo(Percentage other) => Value >= other.Value;
    public bool IsLessThanOrEqualTo(Percentage other) => Value <= other.Value;

    public Percentage Add(Percentage other) => new(Value + other.Value);
    public Percentage Subtract(Percentage other) => new(Value - other.Value);

    /// <summary>Returns the decimal fraction (e.g. 25% → 0.25).</summary>
    public decimal AsFraction() => Value / 100m;

    public override string ToString() => $"{Value:F2}%";

    public static implicit operator decimal(Percentage p) => p.Value;
}
