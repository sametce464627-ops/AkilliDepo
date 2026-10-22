namespace AkilliDepo.Models;

public sealed class ElectronicProduct : Product
{
    public int WarrantyMonths { get; private set; }

    public ElectronicProduct(int id, string name, Category category, Supplier supplier,
        decimal purchasePrice, decimal salePrice, int stock, int criticalLevel, int warrantyMonths)
        : base(id, name, category, supplier, purchasePrice, salePrice, stock, criticalLevel)
    {
        UpdateWarrantyMonths(warrantyMonths);
    }

    public void UpdateWarrantyMonths(int value)
    {
        if (value < 0 || value > 240) throw new ArgumentOutOfRangeException(nameof(value));
        WarrantyMonths = value;
    }

    public override string GetReport() =>
        $"ELEKTRONİK #{Id}: {Name} | Stok: {Stock} | Garanti: {WarrantyMonths} ay | Fiyat: {GetFinalPrice():N2} TL";
}
