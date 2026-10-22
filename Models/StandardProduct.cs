namespace AkilliDepo.Models;

public sealed class StandardProduct : Product
{
    public StandardProduct(int id, string name, Category category, Supplier supplier,
        decimal purchasePrice, decimal salePrice, int stock, int criticalLevel)
        : base(id, name, category, supplier, purchasePrice, salePrice, stock, criticalLevel) { }

    public override string GetReport() =>
        $"STANDART #{Id}: {Name} | {Category.Name} | Stok: {Stock} | Fiyat: {GetFinalPrice():N2} TL";
}
