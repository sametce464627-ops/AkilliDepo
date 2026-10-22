namespace AkilliDepo.Exceptions;

public sealed class InputClosedException : Exception
{
    public InputClosedException() : base("Giriş akışı kapatıldı.") { }
}
