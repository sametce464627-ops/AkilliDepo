using System.Security.Cryptography;
using System.Text;

namespace AkilliDepo.Models;

public abstract class User
{
    private readonly byte[] _passwordSalt;
    private readonly byte[] _passwordHash;

    public int Id { get; }
    public string Username { get; }
    public string FullName { get; }
    public abstract string RoleName { get; }

    protected User(int id, string username, string password, string fullName)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password) ||
            string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Kullanıcı bilgileri eksik.");
        Id = id;
        Username = username.Trim();
        FullName = fullName.Trim();
        _passwordSalt = RandomNumberGenerator.GetBytes(16);
        _passwordHash = HashPassword(password, _passwordSalt);
    }

    private static byte[] HashPassword(string password, byte[] salt)
    {
        byte[] input = Encoding.UTF8.GetBytes(password);
        byte[] mixed = new byte[salt.Length + input.Length];
        Buffer.BlockCopy(salt, 0, mixed, 0, salt.Length);
        Buffer.BlockCopy(input, 0, mixed, salt.Length, input.Length);
        return SHA256.HashData(mixed);
    }

    public bool CheckPassword(string input)
    {
        if (input is null) return false;
        return CryptographicOperations.FixedTimeEquals(_passwordHash, HashPassword(input, _passwordSalt));
    }

    public abstract bool CanDeleteProduct();
    public abstract bool CanManageSuppliers();
    public abstract bool CanManageCategories();
    public abstract bool CanManageProducts();
    public abstract bool CanStockOut();
    public abstract bool CanViewReports();
    public abstract bool CanViewMovements();
}
