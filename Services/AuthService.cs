using AkilliDepo.Models;

namespace AkilliDepo.Services;

public sealed class AuthService
{
    private readonly List<User> _users = new();
    public User? CurrentUser { get; private set; }

    public void Register(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (_users.Any(x => x.Id == user.Id ||
            x.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Kullanıcı Id'si veya adı zaten var.");
        _users.Add(user);
    }

    public bool Login(string username, string password)
    {
        User? user = _users.FirstOrDefault(x =>
            x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        if (user is null || !user.CheckPassword(password)) return false;
        CurrentUser = user;
        return true;
    }

    public void Logout() => CurrentUser = null;
}
