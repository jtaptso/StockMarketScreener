using StockScreener.Domain.Entities;
using StockScreener.Domain.ValueObjects;

namespace StockScreener.Domain.Interfaces.Services;

public interface IScreenerEngine
{
    /// <summary>
    /// Applies all active criteria in <paramref name="filter"/> (AND logic)
    /// to the supplied stock collection and returns only those that match.
    /// Sorting and pagination are also applied.
    /// </summary>
    IEnumerable<Stock> Filter(IEnumerable<Stock> stocks, ScreenerFilter filter);
}
