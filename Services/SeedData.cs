using AkilliDepo.Models;

namespace AkilliDepo.Services;

public static class SeedData
{
    public static void Initialize(InventoryService inventory, AuthService auth)
    {
        auth.Register(new Admin(1, "admin", "admin123", "Ayşe Yılmaz"));
        auth.Register(new Staff(2, "staff", "staff123", "Mehmet Demir"));

        Category electronic = inventory.AddCategory("Elektronik", "Elektronik ürünler");
        Category food = inventory.AddCategory("Gıda", "Bozulabilir gıda ürünleri");
        Category stationary = inventory.AddCategory("Kırtasiye", "Ofis ve kırtasiye");
        Category cleaning = inventory.AddCategory("Temizlik", "Temizlik ürünleri");

        Supplier tech = inventory.AddSupplier("Tekno Dağıtım", "Ali Kaya", "0212 555 0101", "tekno@example.com");
        Supplier market = inventory.AddSupplier("Gıda Toptan", "Deniz Ak", "0212 555 0102", "gida@example.com");
        Supplier paper = inventory.AddSupplier("Kağıt A.Ş.", "Fatma Koç", "0212 555 0103", "kagit@example.com");
        Supplier clean = inventory.AddSupplier("Temiz Ltd.", "Okan Yılmaz", "0212 555 0104", "temiz@example.com");
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        inventory.AddProduct(new ElectronicProduct(101, "USB-C Kablo", electronic, tech, 55m, 95m, 2, 8, 24));
        inventory.AddProduct(new ElectronicProduct(102, "Kablosuz Mouse", electronic, tech, 180m, 299m, 28, 5, 24));
        inventory.AddProduct(new ElectronicProduct(103, "Bluetooth Kulaklık", electronic, tech, 420m, 699m, 14, 4, 24));
        inventory.AddProduct(new ElectronicProduct(104, "USB Bellek 64 GB", electronic, tech, 160m, 250m, 35, 10, 12));

        inventory.AddProduct(new PerishableProduct(201, "Süt 1 L", food, market, 28m, 42m, 23, 6, today.AddDays(3)));
        inventory.AddProduct(new PerishableProduct(202, "Yoğurt 1 KG", food, market, 48m, 72m, 9, 10, today.AddDays(5)));
        inventory.AddProduct(new PerishableProduct(203, "Peynir 500 G", food, market, 90m, 139m, 20, 5, today.AddDays(22)));
        inventory.AddProduct(new PerishableProduct(204, "Yumurta 10'lu", food, market, 42m, 65m, 12, 4, today.AddDays(9)));

        inventory.AddProduct(new StandardProduct(301, "A4 Fotokopi Kağıdı", stationary, paper, 100m, 149m, 3, 10));
        inventory.AddProduct(new StandardProduct(302, "Tükenmez Kalem", stationary, paper, 6m, 12m, 80, 20));
        inventory.AddProduct(new StandardProduct(303, "Defter", stationary, paper, 22m, 39m, 45, 15));
        inventory.AddProduct(new StandardProduct(304, "Zımba", stationary, paper, 55m, 89m, 11, 5));

        inventory.AddProduct(new StandardProduct(401, "Bulaşık Deterjanı", cleaning, clean, 54m, 89m, 5, 15));
        inventory.AddProduct(new StandardProduct(402, "Yüzey Temizleyici", cleaning, clean, 45m, 75m, 34, 10));
        inventory.AddProduct(new StandardProduct(403, "Çamaşır Suyu", cleaning, clean, 21m, 38m, 16, 5));
        inventory.AddProduct(new StandardProduct(404, "Kağıt Havlu", cleaning, clean, 62m, 99m, 9, 8));
    }
}
