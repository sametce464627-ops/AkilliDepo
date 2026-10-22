using AkilliDepo.Exceptions;
using AkilliDepo.Models;
using AkilliDepo.Repositories;

namespace AkilliDepo.Services;

public sealed class InventoryService
{
    public Repository<Category> Categories { get; } = new();
    public Repository<Supplier> Suppliers { get; } = new();
    public Repository<Product> Products { get; } = new();
    public Repository<StockMovement> Movements { get; } = new();

    // Sözlük, servis ömrü boyunca korunur ve CRUD işlemleri sırasında güncellenir.
    private readonly Dictionary<Category, List<Product>> _categoryProducts = new();
    public IReadOnlyDictionary<Category, List<Product>> CategoryProducts => _categoryProducts;
    private int _nextMovementId = 1;

    public Category AddCategory(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Kategori adı boş olamaz.");
        if (Categories.GetAll().Any(x => x.Name.Equals(name.Trim(), StringComparison.CurrentCultureIgnoreCase)))
            throw new InvalidOperationException("Aynı isimde kategori var.");
        int id = Categories.Count == 0 ? 1 : checked(Categories.GetAll().Max(x => x.Id) + 1);
        var result = new Category(id, name, description);
        Categories.Add(result);
        _categoryProducts.Add(result, new List<Product>());
        return result;
    }

    public Supplier AddSupplier(string company, string contact, string phone, string email)
    {
        if (string.IsNullOrWhiteSpace(company)) throw new ArgumentException("Firma adı boş olamaz.");
        if (Suppliers.GetAll().Any(x => x.CompanyName.Equals(company.Trim(), StringComparison.CurrentCultureIgnoreCase)))
            throw new InvalidOperationException("Aynı isimde tedarikçi var.");
        int id = Suppliers.Count == 0 ? 1 : checked(Suppliers.GetAll().Max(x => x.Id) + 1);
        var result = new Supplier(id, company, contact, phone, email);
        Suppliers.Add(result);
        return result;
    }

    public Product AddProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        ValidateRelations(product.Category, product.Supplier);
        Products.Add(product);
        _categoryProducts[product.Category].Add(product);
        UpdateSupplierCounts();
        return product;
    }

    public int NextProductId() => Products.Count == 0
        ? 1 : checked(Products.GetAll().Max(p => p.Id) + 1);

    public void UpdateProduct(int productId, string name, Category category, Supplier supplier,
        decimal purchasePrice, decimal salePrice, int criticalLevel)
    {
        Product product = RequireProduct(productId);
        ValidateRelations(category, supplier);
        Category previousCategory = product.Category;
        product.UpdateDetails(name, category, supplier, purchasePrice, salePrice, criticalLevel);
        if (!previousCategory.Equals(category))
        {
            _categoryProducts[previousCategory].Remove(product);
            _categoryProducts[category].Add(product);
        }
        UpdateSupplierCounts();
    }

    public void DeleteProduct(int productId)
    {
        Product product = RequireProduct(productId);
        // Stok hareketleri Product referansı içerir; silinen ürünlerin geçmişi kaybolmaz.
        _categoryProducts[product.Category].Remove(product);
        if (!Products.Remove(productId)) throw new InvalidOperationException("Silme başarısız.");
        UpdateSupplierCounts();
    }

    public Product RequireProduct(int id) => Products.GetById(id)
        ?? throw new KeyNotFoundException($"{id} Id numaralı ürün bulunamadı.");

    public void StockIn(int productId, int quantity, User user)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Miktar pozitif olmalıdır.");
        Product product = RequireProduct(productId);
        product.IncreaseStock(quantity);
        AddMovement(product, quantity, user, StockMovementType.StockIn);
    }

    public void StockOut(int productId, int quantity, User user)
    {
        if (!user.CanStockOut()) throw new UnauthorizedAccessException("Bu işlem için yetkiniz yok.");
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Miktar pozitif olmalıdır.");
        Product product = RequireProduct(productId);
        product.DecreaseStock(quantity);
        AddMovement(product, quantity, user, StockMovementType.StockOut);
    }

    public SaleReceipt Sell(IReadOnlyDictionary<int, int> cart, User user, decimal taxRate = 0.20m)
    {
        ArgumentNullException.ThrowIfNull(cart);
        ArgumentNullException.ThrowIfNull(user);
        if (cart.Count == 0) throw new InvalidOperationException("Sepet boş.");
        if (taxRate < 0m || taxRate > 1m) throw new ArgumentOutOfRangeException(nameof(taxRate));

        // Tüm satırlar ve tutarlar ÖNCE doğrulanır: başarısız onayda stok değişmez.
        var lines = new List<SaleLine>();
        foreach (var pair in cart)
        {
            Product product = RequireProduct(pair.Key);
            if (pair.Value <= 0) throw new ArgumentOutOfRangeException(nameof(cart), "Satış adedi pozitif olmalıdır.");
            if (pair.Value > product.Stock)
                throw new InsufficientStockException($"{product.Name}: stok {product.Stock}, sepette {pair.Value}.");
            lines.Add(new SaleLine(product, pair.Value, product.GetFinalPrice()));
        }

        List<Product> criticalAfterSale = lines
            .Where(x => x.Product.Stock - x.Quantity <= x.Product.CriticalLevel)
            .Select(x => x.Product)
            .ToList();
        SaleReceipt receipt = new(lines, criticalAfterSale, taxRate);
        foreach (SaleLine line in lines)
        {
            line.Product.DecreaseStock(line.Quantity);
            AddMovement(line.Product, line.Quantity, user, StockMovementType.Sale);
        }
        return receipt;
    }

    private void AddMovement(Product product, int quantity, User user, StockMovementType type)
    {
        Movements.Add(new StockMovement(_nextMovementId++, product, quantity, user, type));
    }

    private void ValidateRelations(Category category, Supplier supplier)
    {
        if (!ReferenceEquals(Categories.GetById(category.Id), category))
            throw new InvalidOperationException("Geçerli kategori seçilmelidir.");
        if (!ReferenceEquals(Suppliers.GetById(supplier.Id), supplier))
            throw new InvalidOperationException("Geçerli tedarikçi seçilmelidir.");
    }

    private void UpdateSupplierCounts()
    {
        foreach (Supplier supplier in Suppliers.GetAll())
            supplier.SetProductCount(Products.GetAll().Count(x => x.Supplier.Id == supplier.Id));
    }
}
