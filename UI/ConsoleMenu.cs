using System.Globalization;
using AkilliDepo.Exceptions;
using AkilliDepo.Interfaces;
using AkilliDepo.Models;
using AkilliDepo.Services;

namespace AkilliDepo.UI;

public sealed class ConsoleMenu
{
    private readonly AuthService _auth = new();
    private readonly InventoryService _inventory = new();
    private readonly SearchService _search;
    private readonly ReportService _reports;
    private bool _running = true;

    public ConsoleMenu()
    {
        _search = new SearchService(_inventory);
        _reports = new ReportService(_inventory);
        SeedData.Initialize(_inventory, _auth);
    }

    private User CurrentUser => _auth.CurrentUser
        ?? throw new UnauthorizedAccessException("Önce giriş yapın.");

    public void Run()
    {
        try
        {
            while (_running)
            {
                if (_auth.CurrentUser is null && !PromptLogin()) break;
                try
                {
                    MainMenu();
                }
                catch (Exception ex) when (ex is ArgumentException or InvalidOperationException
                    or KeyNotFoundException or UnauthorizedAccessException or OverflowException
                    or InsufficientStockException)
                {
                    Console.WriteLine($"İşlem gerçekleştirilemedi: {ex.Message}");
                }
            }
        }
        catch (InputClosedException)
        {
            Console.WriteLine("Giriş kapatıldı. Program sonlandırılıyor.");
        }
        Console.WriteLine("Akıllı Depo uygulamasından çıkıldı.");
    }

    private bool PromptLogin()
    {
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            Console.WriteLine("\n===== AKILLI DEPO SİSTEMİ / GİRİŞ =====");
            string username = ReadLine("Kullanıcı adı: ");
            string password = ReadPassword("Şifre: ");
            if (_auth.Login(username, password))
            {
                Console.WriteLine($"Hoş geldiniz, {CurrentUser.FullName} ({CurrentUser.RoleName})");
                return true;
            }
            Console.WriteLine($"Kullanıcı adı veya şifre hatalı. Kalan hak: {3 - attempt}");
        }
        Console.WriteLine("3 hatalı giriş. Program kapatılıyor.");
        return false;
    }

    private void MainMenu()
    {
        while (_running && _auth.CurrentUser is not null)
        {
            Console.WriteLine("\n===== AKILLI DEPO SİSTEMİ =====");
            Console.WriteLine("1) Ürün İşlemleri");
            Console.WriteLine("2) Kategori / Tedarikçi İşlemleri");
            Console.WriteLine("3) Arama ve Filtreleme");
            Console.WriteLine("4) Stok Hareketleri");
            Console.WriteLine("5) Satış Yap");
            if (CurrentUser.CanViewReports()) Console.WriteLine("6) Raporlar");
            Console.WriteLine("7) Oturumu Kapat\n0) Çıkış");
            int choice = ReadInt("Seçiminiz: ", 0, 7);
            try
            {
                switch (choice)
                {
                    case 1: ProductMenu(); break;
                    case 2: CategorySupplierMenu(); break;
                    case 3: SearchMenu(); break;
                    case 4: StockMenu(); break;
                    case 5: SaleMenu(); break;
                    case 6:
                        Ensure(CurrentUser.CanViewReports());
                        ReportMenu();
                        break;
                    case 7: _auth.Logout(); Console.WriteLine("Oturum kapatıldı."); break;
                    case 0: _running = false; break;
                }
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException
                or KeyNotFoundException or UnauthorizedAccessException or OverflowException
                    or InsufficientStockException)
            {
                Console.WriteLine($"İşlem gerçekleştirilemedi: {ex.Message}");
            }
        }
    }

    private void ProductMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- ÜRÜN İŞLEMLERİ ---");
            Console.WriteLine("1) Ürünleri Listele");
            if (CurrentUser.CanManageProducts())
                Console.WriteLine("2) Ürün Ekle\n3) Ürün Güncelle\n4) Ürün Sil");
            Console.WriteLine("0) Geri");
            int choice = ReadInt("Seçiminiz: ", 0, 4);
            switch (choice)
            {
                case 1: PrintProductTable(_inventory.Products.GetAll()); break;
                case 2: Ensure(CurrentUser.CanManageProducts()); AddProduct(); break;
                case 3: Ensure(CurrentUser.CanManageProducts()); UpdateProduct(); break;
                case 4: Ensure(CurrentUser.CanDeleteProduct()); DeleteProduct(); break;
                case 0: return;
            }
        }
    }

    private void AddProduct()
    {
        string name = ReadRequired("Ürün adı: ");
        Category category = SelectCategory();
        Supplier supplier = SelectSupplier();
        decimal purchase = ReadDecimal("Alış fiyatı: ");
        decimal sale = ReadDecimal("Satış fiyatı (KDV hariç): ");
        int stock = ReadInt("İlk stok: ", 0, int.MaxValue);
        int critical = ReadInt("Kritik stok eşiği: ", 0, int.MaxValue);
        Console.WriteLine("Ürün tipi: 1) Standart  2) Elektronik  3) Bozulabilir");
        int type = ReadInt("Tip: ", 1, 3);
        int id = _inventory.NextProductId();
        Product result;
        switch (type)
        {
            case 2:
                int warranty = ReadInt("Garanti süresi (ay): ", 0, 240);
                result = new ElectronicProduct(id, name, category, supplier, purchase, sale,
                    stock, critical, warranty);
                break;
            case 3:
                DateOnly expiration = ReadDate("Son kullanma tarihi (gg.aa.yyyy): ");
                result = new PerishableProduct(id, name, category, supplier, purchase, sale,
                    stock, critical, expiration);
                break;
            default:
                result = new StandardProduct(id, name, category, supplier, purchase, sale, stock, critical);
                break;
        }
        _inventory.AddProduct(result);
        Console.WriteLine($"Ürün eklendi. Yeni ürün Id: {result.Id}");
    }

    private void UpdateProduct()
    {
        PrintProductTable(_inventory.Products.GetAll());
        int id = ReadInt("Güncellenecek ürün Id: ", 1, int.MaxValue);
        Product p = _inventory.RequireProduct(id);
        Console.WriteLine("Değiştirmeyeceğiniz metinleri boş, kategori/tedarikçi Id'sini 0 bırakın.");
        string name = ReadLine($"Yeni ad [{p.Name}]: ");
        if (string.IsNullOrWhiteSpace(name)) name = p.Name;
        PrintCategories();
        int categoryId = ReadInt($"Yeni kategori Id (0 = {p.Category.Name}): ", 0, int.MaxValue);
        Category category = categoryId == 0 ? p.Category :
            _inventory.Categories.GetById(categoryId)
                ?? throw new KeyNotFoundException("Kategori bulunamadı.");
        PrintSuppliers();
        int supplierId = ReadInt($"Yeni tedarikçi Id (0 = {p.Supplier.CompanyName}): ", 0, int.MaxValue);
        Supplier supplier = supplierId == 0 ? p.Supplier :
            _inventory.Suppliers.GetById(supplierId)
                ?? throw new KeyNotFoundException("Tedarikçi bulunamadı.");
        decimal purchase = ReadOptionalDecimal($"Yeni alış fiyatı [{p.PurchasePrice:N2}]: ") ?? p.PurchasePrice;
        decimal sale = ReadOptionalDecimal($"Yeni satış fiyatı [{p.SalePrice:N2}]: ") ?? p.SalePrice;
        int critical = ReadOptionalInt($"Yeni kritik eşik [{p.CriticalLevel}]: ", 0, int.MaxValue)
            ?? p.CriticalLevel;

        // Kullanıcı tüm değerleri girmeden kalıcı değişiklik yapma.
        int? newWarranty = null;
        DateOnly? newExpiration = null;
        if (p is ElectronicProduct ep)
        {
            newWarranty = ReadOptionalInt($"Yeni garanti ayı [{ep.WarrantyMonths}]: ", 0, 240);
        }
        else if (p is PerishableProduct pp)
        {
            string newDate = ReadLine($"Yeni SKT [{pp.ExpirationDate:dd.MM.yyyy}], boş = aynı: ");
            if (!string.IsNullOrWhiteSpace(newDate))
            {
                if (!TryParseDate(newDate, out DateOnly date))
                    throw new ArgumentException("Tarih gg.aa.yyyy biçiminde olmalıdır.");
                newExpiration = date;
            }
        }
        _inventory.UpdateProduct(id, name, category, supplier, purchase, sale, critical);
        if (p is ElectronicProduct electronic && newWarranty.HasValue)
            electronic.UpdateWarrantyMonths(newWarranty.Value);
        if (p is PerishableProduct perishable && newExpiration.HasValue)
            perishable.UpdateExpirationDate(newExpiration.Value);
        Console.WriteLine("Ürün güncellendi.");
    }

    private void DeleteProduct()
    {
        PrintProductTable(_inventory.Products.GetAll());
        int id = ReadInt("Silinecek ürün Id: ", 1, int.MaxValue);
        Product p = _inventory.RequireProduct(id);
        if (ReadYesNo($"'{p.Name}' silinsin mi? (E/H): "))
        {
            _inventory.DeleteProduct(id);
            Console.WriteLine("Ürün silindi. Geçmiş stok hareketleri korunur.");
        }
        else Console.WriteLine("Silme iptal edildi.");
    }

    private void CategorySupplierMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- KATEGORİ / TEDARİKÇİ ---");
            Console.WriteLine("1) Kategorileri Listele\n2) Tedarikçileri Listele");
            if (CurrentUser.CanManageCategories()) Console.WriteLine("3) Kategori Ekle");
            if (CurrentUser.CanManageSuppliers()) Console.WriteLine("4) Tedarikçi Ekle");
            Console.WriteLine("5) Bir Tedarikçinin Ürünlerini Listele\n0) Geri");
            int choice = ReadInt("Seçiminiz: ", 0, 5);
            switch (choice)
            {
                case 1: PrintCategories(); break;
                case 2: PrintSuppliers(); break;
                case 3:
                    Ensure(CurrentUser.CanManageCategories());
                    Category category = _inventory.AddCategory(ReadRequired("Kategori adı: "),
                        ReadLine("Açıklama: "));
                    Console.WriteLine($"Kategori eklendi: {category}");
                    break;
                case 4:
                    Ensure(CurrentUser.CanManageSuppliers());
                    Supplier supplier = _inventory.AddSupplier(ReadRequired("Firma adı: "),
                        ReadRequired("İlgili kişi: "), ReadLine("Telefon: "), ReadLine("E-posta: "));
                    Console.WriteLine($"Tedarikçi eklendi: #{supplier.Id} {supplier.CompanyName}");
                    break;
                case 5:
                    PrintSuppliers();
                    int id = ReadInt("Tedarikçi Id: ", 1, int.MaxValue);
                    if (_inventory.Suppliers.GetById(id) is null)
                        throw new KeyNotFoundException("Tedarikçi bulunamadı.");
                    PrintProductTable(_search.BySupplier(id));
                    break;
                case 0: return;
            }
        }
    }

    private void SearchMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- ARAMA / FİLTRELEME ---");
            Console.WriteLine("1) Ada Göre Doğrusal Arama (Elle)");
            Console.WriteLine("2) Ada Göre LINQ Arama");
            Console.WriteLine("3) Id'ye Göre İkili Arama (Elle)");
            Console.WriteLine("4) Id'ye Göre LINQ Arama");
            Console.WriteLine("5) Kategori Filtresi\n6) Fiyat Aralığı Filtresi\n0) Geri");
            int choice = ReadInt("Seçiminiz: ", 0, 6);
            switch (choice)
            {
                case 1: PrintProductTable(_search.SearchByNameLinear(ReadRequired("Ürün adından ifade: "))); break;
                case 2: PrintProductTable(_search.SearchByNameLinq(ReadRequired("Ürün adından ifade: "))); break;
                case 3:
                    int binaryId = ReadInt("Ürün Id: ", 1, int.MaxValue);
                    PrintProductTable(AsSequence(_search.SearchByIdBinary(binaryId)));
                    break;
                case 4:
                    int linqId = ReadInt("Ürün Id: ", 1, int.MaxValue);
                    PrintProductTable(AsSequence(_search.SearchByIdLinq(linqId)));
                    break;
                case 5:
                    Category cat = SelectCategory();
                    PrintProductTable(_search.ByCategory(cat.Id));
                    break;
                case 6:
                    decimal min = ReadDecimal("Minimum fiyat: ");
                    decimal max = ReadDecimal("Maksimum fiyat: ");
                    PrintProductTable(_search.ByPriceRange(min, max));
                    break;
                case 0: return;
            }
        }
    }

    private static IEnumerable<Product> AsSequence(Product? product) =>
        product is null ? Array.Empty<Product>() : new[] { product };

    private void StockMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- STOK HAREKETLERİ ---");
            Console.WriteLine("1) Stok Girişi");
            if (CurrentUser.CanStockOut()) Console.WriteLine("2) Stok Çıkışı");
            if (CurrentUser.CanViewMovements()) Console.WriteLine("3) Hareket Geçmişi");
            Console.WriteLine("0) Geri");
            int choice = ReadInt("Seçiminiz: ", 0, 3);
            switch (choice)
            {
                case 1:
                    StockChange(false);
                    break;
                case 2:
                    Ensure(CurrentUser.CanStockOut());
                    StockChange(true);
                    break;
                case 3:
                    Ensure(CurrentUser.CanViewMovements());
                    PrintMovementHistory();
                    break;
                case 0: return;
            }
        }
    }

    private void StockChange(bool isOut)
    {
        PrintProductTable(_inventory.Products.GetAll());
        int id = ReadInt("Ürün Id: ", 1, int.MaxValue);
        Product product = _inventory.RequireProduct(id);
        int qty = ReadInt("Miktar: ", 1, int.MaxValue);
        if (isOut) _inventory.StockOut(id, qty, CurrentUser);
        else _inventory.StockIn(id, qty, CurrentUser);
        Console.WriteLine($"İşlem tamamlandı. {product.Name}, güncel stok: {product.Stock}");
        if (product.IsCritical) Console.WriteLine("UYARI: Ürün kritik stok seviyesinde veya altında.");
    }

    private void PrintMovementHistory()
    {
        Console.WriteLine("\n--- STOK HAREKET GEÇMİŞİ ---");
        Console.WriteLine($"{"Id",4} {"Tarih",19} {"Ürün",-24} {"Tür",-12} {"Adet",6} {"Kullanıcı",-17}");
        foreach (StockMovement m in _inventory.Movements.GetAll().OrderByDescending(x => x.Date))
        {
            Console.WriteLine($"{m.Id,4} {m.Date:dd.MM.yyyy HH:mm:ss} {Fit(m.Product.Name, 24),-24} " +
                $"{m.Type,-12} {m.Quantity,6} {Fit(m.PerformedBy.Username, 17),-17}");
        }
        if (_inventory.Movements.Count == 0) Console.WriteLine("Kayıt bulunamadı.");
    }

    private void SaleMenu()
    {
        var cart = new Dictionary<int, int>();
        while (true)
        {
            Console.WriteLine("\n--- SATIŞ SEPETİ ---");
            Console.WriteLine("1) Sepete Ürün Ekle\n2) Sepeti Görüntüle\n3) Satışı Onayla\n0) Satışı İptal Et");
            int choice = ReadInt("Seçiminiz: ", 0, 3);
            switch (choice)
            {
                case 1:
                    PrintProductTable(_inventory.Products.GetAll());
                    int id = ReadInt("Ürün Id: ", 1, int.MaxValue);
                    Product product = _inventory.RequireProduct(id);
                    int quantity = ReadInt("Adet: ", 1, int.MaxValue);
                    int currentlyInCart = cart.GetValueOrDefault(id);
                    if (quantity > product.Stock - currentlyInCart)
                    {
                        Console.WriteLine($"Yetersiz stok! Depo: {product.Stock}, sepette: {currentlyInCart}");
                    }
                    else
                    {
                        cart[id] = checked(currentlyInCart + quantity);
                        Console.WriteLine("Ürün sepete eklendi.");
                    }
                    break;
                case 2:
                    PrintCart(cart);
                    break;
                case 3:
                    if (cart.Count == 0)
                    {
                        Console.WriteLine("Sepet boş, satış gerçekleştirilemez.");
                        break;
                    }
                    PrintCart(cart);
                    if (!ReadYesNo("Satışı onaylıyor musunuz? (E/H): "))
                    {
                        Console.WriteLine("Satış onaylanmadı; stok değişmedi.");
                        break;
                    }
                    SaleReceipt receipt = _inventory.Sell(cart, CurrentUser);
                    PrintReceipt(receipt);
                    return;
                case 0:
                    Console.WriteLine("Sepet iptal edildi. Hiçbir stok değişmedi.");
                    return;
            }
        }
    }

    private void PrintCart(IReadOnlyDictionary<int, int> cart)
    {
        Console.WriteLine("\n--- SEPET ---");
        decimal subtotal = 0m;
        foreach (var item in cart)
        {
            Product p = _inventory.RequireProduct(item.Key);
            decimal unitPrice = p.GetFinalPrice();
            decimal lineTotal = unitPrice * item.Value;
            subtotal += lineTotal;
            Console.WriteLine($"{Fit(p.Name, 25),-25} {item.Value,4} adet × {unitPrice,10:N2} = {lineTotal,12:N2} TL");
        }
        Console.WriteLine($"KDV hariç ara toplam: {subtotal:N2} TL");
        Console.WriteLine($"Tahmini KDV (%20): {decimal.Round(subtotal * 0.20m, 2):N2} TL");
        Console.WriteLine($"Tahmini toplam: {(subtotal + decimal.Round(subtotal * 0.20m, 2)):N2} TL");
    }

    private static void PrintReceipt(SaleReceipt receipt)
    {
        Console.WriteLine("\n========== SATIŞ FİŞİ ==========");
        Console.WriteLine($"Tarih: {receipt.Date:dd.MM.yyyy HH:mm:ss}");
        foreach (SaleLine line in receipt.Lines)
        {
            Console.WriteLine($"{line.ProductName} | {line.Quantity} × {line.UnitPrice:N2} TL = {line.Subtotal:N2} TL");
        }
        Console.WriteLine(new string('-', 34));
        Console.WriteLine($"Ara toplam: {receipt.Subtotal:N2} TL");
        Console.WriteLine($"KDV (%{receipt.TaxRate * 100:0}): {receipt.Tax:N2} TL");
        Console.WriteLine($"GENEL TOPLAM: {receipt.Total:N2} TL");
        if (receipt.CriticalProducts.Count > 0)
        {
            Console.WriteLine("!!! KRİTİK STOK UYARISI !!!");
            foreach (Product p in receipt.CriticalProducts)
                Console.WriteLine($"{p.Name}: {p.Stock} adet (eşik: {p.CriticalLevel})");
        }
        Console.WriteLine("===============================");
    }

    private void ReportMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- RAPORLAR ---");
            Console.WriteLine("1) Kritik Stok\n2) Kategori Bazlı Ürünler\n3) Toplam Envanter Değeri");
            Console.WriteLine("4) En Çok Satan 5 Ürün\n5) Tedarikçi Raporu\n6) SKT Yaklaşan Ürünler");
            Console.WriteLine("7) Polimorfik Ürün / Tedarikçi Raporu\n8) Özet İstatistikler\n0) Geri");
            int choice = ReadInt("Seçiminiz: ", 0, 8);
            switch (choice)
            {
                case 1:
                    Console.WriteLine("\n===== KRİTİK STOK RAPORU =====");
                    var critical = _reports.CriticalStock();
                    PrintProductTable(critical);
                    Console.WriteLine($"Toplam {critical.Count} ürün kritik seviyede.");
                    break;
                case 2:
                    foreach (var pair in _reports.CategoryReport().OrderBy(x => x.Key.Name))
                    {
                        Console.WriteLine($"\n## {pair.Key.Name} ({pair.Value.Count} çeşit)");
                        PrintProductTable(pair.Value);
                    }
                    break;
                case 3:
                    Console.WriteLine($"Toplam envanter alış maliyeti: {_reports.TotalInventoryValue():N2} TL");
                    break;
                case 4:
                    Console.WriteLine("--- EN ÇOK SATAN 5 ÜRÜN ---");
                    var top = _reports.TopSelling();
                    if (top.Count == 0) Console.WriteLine("Henüz satış yok.");
                    foreach (ProductSalesStat item in top)
                        Console.WriteLine($"{item.Product.Id} | {item.Product.Name} | {item.QuantitySold} adet");
                    break;
                case 5:
                    Console.WriteLine("--- TEDARİKÇİ RAPORU ---");
                    foreach (SupplierStat item in _reports.SupplierReport())
                        Console.WriteLine($"{item.Supplier.CompanyName,-22} | Ürün çeşidi: {item.ProductCount,3} | " +
                            $"Envanter değeri: {item.StockValue:N2} TL");
                    break;
                case 6:
                    Console.WriteLine("--- 7 GÜN İÇİNDE SKT'Sİ YAKLAŞAN ÜRÜNLER ---");
                    var expiring = _reports.ExpiringWithinDays();
                    if (expiring.Count == 0) Console.WriteLine("Bu tarih aralığında ürün bulunamadı.");
                    foreach (PerishableProduct p in expiring)
                        Console.WriteLine($"{p.Name} | SKT: {p.ExpirationDate:dd.MM.yyyy} | Fiyat: {p.GetFinalPrice():N2} TL");
                    break;
                case 7:
                    List<IReportable> reportables = _reports.PolymorphicReports().ToList();
                    foreach (IReportable item in reportables) Console.WriteLine(item.GetReport());
                    break;
                case 8:
                    Console.WriteLine($"Ürün çeşidi: {_reports.TotalProducts()}");
                    Console.WriteLine($"Ortalama satış fiyatı: {_reports.AverageSalePrice():N2} TL");
                    Console.WriteLine($"Kritik stok var mı?: {(_reports.HasCriticalStock() ? "Evet" : "Hayır")}");
                    break;
                case 0: return;
            }
        }
    }

    // Tüm ürün listelemelerinin kullandığı tek ortak metot.
    public static void PrintProductTable(IEnumerable<Product> products)
    {
        List<Product> list = products.ToList();
        Console.WriteLine($"{"Id",4} {"Ürün",-24} {"Kategori",-13} {"Tedarikçi",-20} " +
            $"{"Alış",10} {"Satış",10} {"Stok",6} {"Kritik",7}");
        Console.WriteLine(new string('-', 104));
        foreach (Product p in list)
        {
            Console.WriteLine($"{p.Id,4} {Fit(p.Name, 24),-24} {Fit(p.Category.Name, 13),-13} " +
                $"{Fit(p.Supplier.CompanyName, 20),-20} {p.PurchasePrice,10:N2} " +
                $"{p.GetFinalPrice(),10:N2} {p.Stock,6} {p.CriticalLevel,7}");
        }
        if (list.Count == 0) Console.WriteLine("Kayıt bulunamadı.");
        Console.WriteLine(new string('-', 104));
        Console.WriteLine($"Toplam: {list.Count} ürün");
    }

    private void PrintCategories()
    {
        Console.WriteLine("\nKATEGORİLER");
        foreach (Category c in _inventory.Categories.GetAll())
            Console.WriteLine($"{c.Id,3}) {c.Name} - {c.Description}");
    }

    private void PrintSuppliers()
    {
        Console.WriteLine("\nTEDARİKÇİLER");
        foreach (Supplier s in _inventory.Suppliers.GetAll())
            Console.WriteLine($"{s.Id,3}) {s.CompanyName} | {s.ContactName} | Ürün sayısı: {s.ProductCount}");
    }

    public Category SelectCategory()
    {
        PrintCategories();
        int id = ReadInt("Kategori Id: ", 1, int.MaxValue);
        return _inventory.Categories.GetById(id)
            ?? throw new KeyNotFoundException("Kategori bulunamadı.");
    }

    public Supplier SelectSupplier()
    {
        PrintSuppliers();
        int id = ReadInt("Tedarikçi Id: ", 1, int.MaxValue);
        return _inventory.Suppliers.GetById(id)
            ?? throw new KeyNotFoundException("Tedarikçi bulunamadı.");
    }

    public static int ReadInt(string message, int min, int max)
    {
        while (true)
        {
            string input = ReadLine(message);
            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.CurrentCulture, out int number)
                && number >= min && number <= max)
                return number;
            Console.WriteLine($"Geçersiz giriş. {min} ile {max} arasında tam sayı girin.");
        }
    }

    public static decimal ReadDecimal(string message)
    {
        while (true)
        {
            string input = ReadLine(message);
            if (TryParseDecimal(input, out decimal value) && value >= 0m) return value;
            Console.WriteLine("Geçersiz fiyat. Negatif olmayan ondalık sayı girin (örn. 12,50).");
        }
    }

    private static decimal? ReadOptionalDecimal(string message)
    {
        while (true)
        {
            string input = ReadLine(message);
            if (string.IsNullOrWhiteSpace(input)) return null;
            if (TryParseDecimal(input, out decimal value) && value >= 0m) return value;
            Console.WriteLine("Geçersiz fiyat. Sıfır veya pozitif bir sayı girin.");
        }
    }

    private static bool TryParseDecimal(string input, out decimal number)
    {
        const NumberStyles styles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
        return decimal.TryParse(input, styles, CultureInfo.CurrentCulture, out number)
            || decimal.TryParse(input, styles, CultureInfo.InvariantCulture, out number);
    }

    private static int? ReadOptionalInt(string message, int min, int max)
    {
        while (true)
        {
            string input = ReadLine(message);
            if (string.IsNullOrWhiteSpace(input)) return null;
            if (int.TryParse(input, out int value) && value >= min && value <= max) return value;
            Console.WriteLine($"{min} ile {max} arası bir tam sayı girin veya boş bırakın.");
        }
    }

    private static bool TryParseDate(string input, out DateOnly date)
    {
        string[] patterns = { "dd.MM.yyyy", "d.M.yyyy", "yyyy-MM-dd" };
        return DateOnly.TryParseExact(input.Trim(), patterns, CultureInfo.GetCultureInfo("tr-TR"),
            DateTimeStyles.None, out date);
    }

    private static DateOnly ReadDate(string message)
    {
        while (true)
        {
            if (TryParseDate(ReadLine(message), out DateOnly value)) return value;
            Console.WriteLine("Geçersiz tarih. Örn: 25.12.2026");
        }
    }

    private static bool ReadYesNo(string message)
    {
        while (true)
        {
            string input = ReadLine(message).Trim();
            if (input.Equals("E", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("Evet", StringComparison.OrdinalIgnoreCase)) return true;
            if (input.Equals("H", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("Hayır", StringComparison.OrdinalIgnoreCase)) return false;
            Console.WriteLine("E veya H girin.");
        }
    }

    private static string ReadRequired(string message)
    {
        while (true)
        {
            string text = ReadLine(message).Trim();
            if (!string.IsNullOrWhiteSpace(text)) return text;
            Console.WriteLine("Bu alan boş olamaz.");
        }
    }

    private static string ReadLine(string message)
    {
        Console.Write(message);
        return Console.ReadLine() ?? throw new InputClosedException();
    }

    private static string ReadPassword(string message)
    {
        Console.Write(message);
        // Klavye tabanlı kullanımda şifre gizlenir, yönlendirilmiş girişte satır okunur.
        if (Console.IsInputRedirected) return Console.ReadLine() ?? throw new InputClosedException();
        var buffer = new System.Text.StringBuilder();
        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); return buffer.ToString(); }
            if (key.Key == ConsoleKey.Backspace)
            {
                if (buffer.Length > 0) { buffer.Length--; Console.Write("\b \b"); }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                buffer.Append(key.KeyChar);
                Console.Write('*');
            }
        }
    }

    private static string Fit(string input, int width) => input.Length <= width
        ? input : input[..(width - 1)] + "…";

    private static void Ensure(bool allowed)
    {
        if (!allowed) throw new UnauthorizedAccessException("Bu işlem için yetkiniz yok.");
    }
}
