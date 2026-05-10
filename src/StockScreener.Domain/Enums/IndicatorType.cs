namespace StockScreener.Domain.Enums;

public enum IndicatorType
{
    RSI, // Relative Strength Index
    SMA, // Simple Moving Average
    EMA, // Exponential Moving Average
    MACD, // Moving Average Convergence Divergence
    EMACD, // Exponential Moving Average Convergence Divergence
    BollingerBands, // Bollinger Bands
    ATR, // Average True Range
    Unknown
}