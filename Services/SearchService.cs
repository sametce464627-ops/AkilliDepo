using AkilliDepo.Models;

namespace AkilliDepo.Services;

public sealed class SearchService
{
    private readonly InventoryService _inventory;
    public SearchService(InventoryService inventory) => _inventory = inventory;

    // LINQ KULLANILMADAN elle doğrusal arama: O(n).
    public IReadOnlyList<Product> SearchByNameLinear(string query)
    {
        var result = new List<Product>();
        IReadOnlyList<Product> products = _inventory.Products.GetAll();
        for (int i = 0; i < products.Count; i++)
        {
            if (products[i].Name.Contains(query, StringComparison.CurrentCultureIgnoreCase))
                result.Add(products[i]);
        }
        return result;
    }

    // Karşılık gelen LINQ araması.
    public IReadOnlyList<Product> SearchByNameLinq(string query) =>
        _inventory.Products.GetAll()
            .Where(p => p.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase))
            .ToList();

    // Listenin Id'ye göre sıralanmış olması ikili aramanın ön koşuludur.
    // Arama döngüsü elle yazılmıştır; List<T>.BinarySearch kullanılmaz.
    public Product? SearchByIdBinary(int id)
    {
        List<Product> sorted = new(_inventory.Products.GetAll());
        sorted.Sort((a, b) => a.Id.CompareTo(b.Id));
        int left = 0;
        int right = sorted.Count - 1;
        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            if (sorted[mid].Id == id) return sorted[mid];
            if (sorted[mid].Id < id) left = mid + 1;
            else right = mid - 1;
        }
        return null;
    }

    public Product? SearchByIdLinq(int id) =>
        _inventory.Products.GetAll().FirstOrDefault(p => p.Id == id);

    public IReadOnlyList<Product> ByCategory(int categoryId) =>
        _inventory.Products.GetAll().Where(p => p.Category.Id == categoryId).ToList();

    public IReadOnlyList<Product> ByPriceRange(decimal min, decimal max)
    {
        if (min < 0m || max < min) throw new ArgumentException("Geçersiz fiyat aralığı.");
        return _inventory.Products.GetAll()
            .Where(p => p.GetFinalPrice() >= min && p.GetFinalPrice() <= max)
            .OrderBy(p => p.GetFinalPrice()).ToList();
    }

    public IReadOnlyList<Product> BySupplier(int supplierId) =>
        _inventory.Products.GetAll().Where(p => p.Supplier.Id == supplierId).ToList();
}
