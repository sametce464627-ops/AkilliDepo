using AkilliDepo.Interfaces;
using AkilliDepo.Models;

namespace AkilliDepo.Services;

public sealed class ReportService
{
    private readonly InventoryService _inventory;
    public ReportService(InventoryService inventory) => _inventory = inventory;

    public IReadOnlyList<Product> CriticalStock() =>
        _inventory.Products.GetAll().Where(p => p.IsCritical)
            .OrderBy(p => p.Stock).ToList();

    // Her çağrıda GroupBy ile yeni kategori sözlüğü oluşturulmaz.
    public IReadOnlyDictionary<Category, List<Product>> CategoryReport() => _inventory.CategoryProducts;

    public decimal TotalInventoryValue() =>
        _inventory.Products.GetAll().Sum(p => p.Stock * p.PurchasePrice);

    public IReadOnlyList<ProductSalesStat> TopSelling(int count = 5) =>
        _inventory.Movements.GetAll()
            .Where(m => m.Type == StockMovementType.Sale)
            .GroupBy(m => m.Product)
            .Select(group => new ProductSalesStat(group.Key, group.Sum(m => m.Quantity)))
            .OrderByDescending(x => x.QuantitySold)
            .Take(count)
            .ToList();

    public IReadOnlyList<SupplierStat> SupplierReport() =>
        _inventory.Suppliers.GetAll()
            .Select(s => new SupplierStat(s,
                _inventory.Products.GetAll().Count(p => p.Supplier.Id == s.Id),
                _inventory.Products.GetAll()
                    .Where(p => p.Supplier.Id == s.Id)
                    .Sum(p => p.Stock * p.PurchasePrice)))
            .OrderBy(s => s.Supplier.CompanyName)
            .ToList();

    public IReadOnlyList<PerishableProduct> ExpiringWithinDays(int days = 7)
    {
        if (days < 0) throw new ArgumentOutOfRangeException(nameof(days));
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        return _inventory.Products.GetAll()
            .OfType<PerishableProduct>()
            .Where(p => p.ExpirationDate >= today && p.ExpirationDate <= today.AddDays(days))
            .OrderBy(p => p.ExpirationDate).ToList();
    }

    // Gerçek polimorfik raporlama; tek listede farklı türler.
    public IReadOnlyList<IReportable> PolymorphicReports()
    {
        var reportables = new List<IReportable>();
        reportables.AddRange(_inventory.Products.GetAll());
        reportables.AddRange(_inventory.Suppliers.GetAll());
        return reportables;
    }

    public int TotalProducts() => _inventory.Products.GetAll().Count();
    public decimal AverageSalePrice() => _inventory.Products.Count == 0
        ? 0m : _inventory.Products.GetAll().Average(p => p.GetFinalPrice());
    public bool HasCriticalStock() => _inventory.Products.GetAll().Any(p => p.IsCritical);
}
