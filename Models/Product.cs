using AkilliDepo.Exceptions;
using AkilliDepo.Interfaces;

namespace AkilliDepo.Models;

public abstract class Product : IEntity, IReportable
{
    public int Id { get; }
    public string Name { get; private set; }
    public Category Category { get; private set; }
    public Supplier Supplier { get; private set; }
    public decimal PurchasePrice { get; private set; }
    public decimal SalePrice { get; private set; }
    public int Stock { get; private set; }
    public int CriticalLevel { get; private set; }
    public bool IsCritical => Stock <= CriticalLevel;

    protected Product(int id, string name, Category category, Supplier supplier,
        decimal purchasePrice, decimal salePrice, int stock, int criticalLevel)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        Id = id;
        Name = "";
        Category = category ?? throw new ArgumentNullException(nameof(category));
        Supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
        UpdateDetails(name, category, supplier, purchasePrice, salePrice, criticalLevel);
        IncreaseStock(stock);
    }

    public void UpdateDetails(string name, Category category, Supplier supplier,
        decimal purchasePrice, decimal salePrice, int criticalLevel)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Ürün adı boş olamaz.");
        if (purchasePrice < 0m || salePrice < 0m)
            throw new ArgumentException("Fiyat negatif olamaz.");
        if (criticalLevel < 0) throw new ArgumentOutOfRangeException(nameof(criticalLevel));
        Name = name.Trim();
        Category = category ?? throw new ArgumentNullException(nameof(category));
        Supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
        PurchasePrice = purchasePrice;
        SalePrice = salePrice;
        CriticalLevel = criticalLevel;
    }

    public void IncreaseStock(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Miktar negatif olamaz.");
        Stock = checked(Stock + amount);
    }

    public void DecreaseStock(int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Miktar pozitif olmalıdır.");
        if (Stock < amount)
            throw new InsufficientStockException($"{Name}: stok {Stock}, istenen {amount}.");
        Stock -= amount;
    }

    public virtual decimal GetFinalPrice() => SalePrice;
    public abstract string GetReport();
}
