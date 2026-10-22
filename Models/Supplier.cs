using AkilliDepo.Interfaces;

namespace AkilliDepo.Models;

public sealed class Supplier : IEntity, IReportable
{
    public int Id { get; }
    public string CompanyName { get; }
    public string ContactName { get; }
    public string Phone { get; }
    public string Email { get; }
    public int ProductCount { get; private set; }

    public Supplier(int id, string companyName, string contactName, string phone, string email)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        if (string.IsNullOrWhiteSpace(companyName)) throw new ArgumentException("Firma adı boş olamaz.");
        Id = id;
        CompanyName = companyName.Trim();
        ContactName = contactName.Trim();
        Phone = phone.Trim();
        Email = email.Trim();
    }

    internal void SetProductCount(int value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        ProductCount = value;
    }

    public string GetReport() =>
        $"TEDARİKÇİ #{Id}: {CompanyName} | Yetkili: {ContactName} | Tel: {Phone} | " +
        $"E-posta: {Email} | Sağladığı ürün çeşidi: {ProductCount}";
}
