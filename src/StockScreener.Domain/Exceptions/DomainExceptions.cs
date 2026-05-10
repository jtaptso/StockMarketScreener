namespace StockScreener.Domain.Exceptions;

/// <summary>Thrown when a requested resource does not exist.</summary>
public class NotFoundException(string message) : Exception(message)
{
    public NotFoundException(string resourceName, object key)
        : this($"{resourceName} with key '{key}' was not found.") { }
}

/// <summary>Thrown when a request violates a business rule.</summary>
public class DomainValidationException(string message) : Exception(message);

/// <summary>Thrown when an operation conflicts with the current state (e.g. duplicate).</summary>
public class ConflictException(string message) : Exception(message);
