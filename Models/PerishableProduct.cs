namespace AkilliDepo.Models;

public sealed class PerishableProduct : Product
{
    public DateOnly ExpirationDate { get; private set; }

    public PerishableProduct(int id, string name, Category category, Supplier supplier,
        decimal purchasePrice, decimal salePrice, int stock, int criticalLevel, DateOnly expirationDate)
        : base(id, name, category, supplier, purchasePrice, salePrice, stock, criticalLevel)
    {
        UpdateExpirationDate(expirationDate);
    }

    public void UpdateExpirationDate(DateOnly date) => ExpirationDate = date;

    // SKT'sine kalan süre 0..6 gün ise %10 indirim. Süresi geçmiş ürün indirim kapsamında değildir.
    public override decimal GetFinalPrice()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        int daysLeft = ExpirationDate.DayNumber - today.DayNumber;
        return daysLeft >= 0 && daysLeft < 7
            ? decimal.Round(SalePrice * 0.90m, 2, MidpointRounding.AwayFromZero)
            : base.GetFinalPrice();
    }

    public override string GetReport() =>
        $"GIDA #{Id}: {Name} | Stok: {Stock} | SKT: {ExpirationDate:dd.MM.yyyy} | Fiyat: {GetFinalPrice():N2} TL";
}
