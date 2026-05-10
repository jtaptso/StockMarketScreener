namespace StockScreener.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing an inclusive date range.
/// </summary>
public sealed record DateRange
{
    public DateOnly Start { get; }
    public DateOnly End { get; }

    public DateRange(DateOnly start, DateOnly end)
    {
        if (start > end)
            throw new ArgumentException(
                $"Start date ({start}) must be on or before end date ({end}).",
                nameof(start));

        Start = start;
        End = end;
    }

    /// <summary>Convenience constructor accepting <see cref="DateTime"/> values.</summary>
    public DateRange(DateTime start, DateTime end)
        : this(DateOnly.FromDateTime(start), DateOnly.FromDateTime(end)) { }

    /// <summary>Creates a range covering the last <paramref name="days"/> calendar days up to today.</summary>
    public static DateRange LastDays(int days)
    {
        if (days <= 0)
            throw new ArgumentOutOfRangeException(nameof(days), "Days must be positive.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new DateRange(today.AddDays(-days), today);
    }

    /// <summary>Creates a range from the start of the current year to today.</summary>
    public static DateRange YearToDate()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new DateRange(new DateOnly(today.Year, 1, 1), today);
    }

    /// <summary>Creates a range for a full calendar year.</summary>
    public static DateRange ForYear(int year) =>
        new(new DateOnly(year, 1, 1), new DateOnly(year, 12, 31));

    /// <summary>Number of calendar days in the range (inclusive).</summary>
    public int TotalDays => End.DayNumber - Start.DayNumber + 1;

    /// <summary>Returns true if the given date falls within this range (inclusive).</summary>
    public bool Contains(DateOnly date) => date >= Start && date <= End;

    /// <summary>Returns true if the given date falls within this range (inclusive).</summary>
    public bool Contains(DateTime dateTime) => Contains(DateOnly.FromDateTime(dateTime));

    /// <summary>Returns true if this range overlaps with another.</summary>
    public bool Overlaps(DateRange other) => Start <= other.End && End >= other.Start;

    public override string ToString() => $"{Start:yyyy-MM-dd} → {End:yyyy-MM-dd}";
}
