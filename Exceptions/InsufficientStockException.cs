namespace AkilliDepo.Exceptions;

public sealed class InsufficientStockException : Exception
{
    public InsufficientStockException(string message) : base(message) { }
}
