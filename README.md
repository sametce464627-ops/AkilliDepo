# Akıllı Depo ve Envanter Otomasyonu

Ödev tanımına uygun, **C# / .NET 8 Console Application**. Harici NuGet paketi ve veritabanı kullanılmaz; veriler `List<T>` ve `Dictionary<TKey,TValue>` içinde, yalnızca program çalışırken tutulur.

## Başlatma

.NET 8 SDK (veya daha yenisi) kuruluyken bu klasörde terminal açın:

```bash
dotnet restore
dotnet run
```

### Demo kullanıcıları

| Rol | Kullanıcı adı | Şifre |
|---|---|---|
| Yönetici | `admin` | `admin123` |
| Personel | `staff` | `staff123` |

Şifreler sadece örnek kullanım içindir; uygulama içerisinde rastgele tuzlanmış SHA-256 özetleri tutulur. Gerçek bir kimlik doğrulama sistemi veya kalıcı kullanıcı deposu yerine geçmez. Üç başarısız giriş sonrası program sonlanır; çıkış yapıp başka hesapla giriş yapılabilir.

## Kullanım akışı

1. Yöneticiyle giriş yapın ve **Ürün İşlemleri** alt menüsünden listeleme/ekleme/güncelleme/silme işlemlerini deneyin.
2. **Arama** menüsünde hem manuel arama hem LINQ karşılıkları bulunur.
3. **Stok Hareketleri** menüsünden giriş/çıkış işlemleri yapın. Her başarılı değişiklik tarih, kullanıcı, ürün, tür ve miktarla kaydedilir.
4. **Satış Yap** ile birden fazla ürün sepete ekleyin. Satış onayında stok yeniden doğrulanır; iptal edilirse hareket eklenmez ve stok değişmez. Onaylanan işlem sonunda ürün bazında satış hareketleri ve KDV'li fiş yazdırılır.
5. **Raporlar** menüsünde kritik stok, kategori grupları, envanter maliyeti, çok satanlar, tedarikçiler, SKT yaklaşanlar ve `List<IReportable>` polimorfik raporları bulunur.

İlk açılışta 4 kategori, 4 tedarikçi, 16 ürün (USB-C Kablo, Yoğurt, A4 Fotokopi Kağıdı, Bulaşık Deterjanı kritik dahil) ve 2 kullanıcı otomatik oluşturulur. İlk etapta satış hareketi bulunmadığından en çok satanlar raporu satış yapıldıktan sonra dolacaktır.

KDV oranı simülasyonda **%20** sabit kabul edilir; `SalePrice` KDV hariçtir. 0–6 gün içerisinde son kullanma tarihi gelecek bozulabilir ürünlere sanal **%10 indirim** uygulanır. SKT'si geçmiş ürünler yaklaşanlar raporuna girmez ve bu indirimden yararlanmaz. Bu kurallar öğretici örnektir; ticari/fiskal mevzuata göre kullanılmamalıdır.

## Klasör yapısı

```text
AkilliDepo/
├── AkilliDepo.csproj
├── Program.cs
├── Interfaces/   IEntity, IReportable
├── Models/       User, Admin, Staff, Product türleri, Category, Supplier,
│                StockMovement, SaleLine, SaleReceipt, rapor DTO'ları
├── Repositories/ Repository<T>
├── Services/     AuthService, InventoryService, SearchService,
│                ReportService, SeedData
├── Exceptions/   InsufficientStockException, InputClosedException
└── UI/           ConsoleMenu
```

`Program.cs` yalnızca kültürü/çıktıyı ayarlar ve `ConsoleMenu` nesnesini başlatır. Veri modelleri ve servislerde `Console` kullanılmaz. Her sınıf ayrı `.cs` dosyasında tanımlıdır.

## Ödev gereksinimleri karşılığı

| Gereksinim | Dosya / metot |
|---|---|
| `while`, `switch`, `if/else`, `for/foreach` | `ConsoleMenu` ana/alt menüleri ve sepet döngüsü; `SearchService.SearchByNameLinear`, `SearchByIdBinary` |
| `PrintProductTable`, `ReadInt`, `ReadDecimal`, `SelectCategory`, `SelectSupplier` | `UI/ConsoleMenu.cs` |
| Admin / Staff, parola kontrolü, 3 yanlış deneme ve logout | `Models/User.cs`, `Admin.cs`, `Staff.cs`; `Services/AuthService.cs`; `ConsoleMenu.PromptLogin` |
| `IEntity` / `IReportable` | `Interfaces/`; `Category`, `Supplier`, `Product` uygulamaları |
| Abstract `Product` / `User`, kalıtım | `Models/` altındaki türler |
| Virtual/override fiyat ve polimorfik rapor | `PerishableProduct.GetFinalPrice`, `GetReport`; `ReportService.PolymorphicReports` |
| Stok kapsülleme, yetersiz stok exception | `Product.IncreaseStock/DecreaseStock`, `InsufficientStockException` |
| Generic repo ve aynı Id kontrolü | `Repositories/Repository.cs`, dört farklı türde kullanım |
| **Yaşayan** `Dictionary<Category,List<Product>>` | `InventoryService._categoryProducts`, `AddProduct`, `UpdateProduct`, `DeleteProduct` |
| Kategori eşitliği | `Category.Equals` ve `Category.GetHashCode` |
| Manuel doğrusal ve ikili arama + LINQ | `Services/SearchService.cs` |
| Satış, sepet, stok ön kontrolü, fiş, KDV, iptal | `ConsoleMenu.SaleMenu`, `InventoryService.Sell`, `SaleReceipt` |
| Tarih/kullanıcı/tür/miktarlı hareket | `StockMovement`, `InventoryService.AddMovement` |
| 6 zorunlu rapor | `ReportService` ve `ConsoleMenu.ReportMenu` |
| Girdi kontrolü ve hata yakalama | `ConsoleMenu` TryParse metotları, `Run` / `MainMenu` try-catch |

### LINQ kullanım tablosu

| LINQ metodu | Gerçek kullanım |
|---|---|
| `Where` | Kritik stok listesi, SKU filtreleri, satış hareketlerini seçme |
| `Select` | `TopSelling` ve tedarikçi raporu model dönüşümleri |
| `OrderBy` | Fiyat ve SKT sıralama |
| `OrderByDescending` | En çok satan ürünleri ve hareket geçmişini sıralama |
| `GroupBy` | Satış hareketlerini ürün bazında gruplama |
| `Sum` | Stok maliyeti, toplam satılan miktar ve sepet tutarı |
| `Count` | Kategori/tedarikçi ürün adedi; toplam ürün sayısı |
| `Any` | `HasCriticalStock`, kategori/tedarikçi adı tekrar kontrolü |
| `FirstOrDefault` | Repo Id arama, LINQ Id arama, kullanıcı bulma |
| `Take` | En çok satan ilk 5 ürün |
| `Average` | Ortalama satış fiyatı |
| `Max` | Yeni Id üretimi |
| `OfType` | SKT yaklaşan bozulabilir ürünler |

### Arama algoritmaları ve karşılaştırması

- **Manuel doğrusal arama:** `for` döngüsüyle tüm ürünleri dolaşır ve `string.Contains(query, StringComparison.CurrentCultureIgnoreCase)` ile kısmi ad eşleşmelerini toplar. Zaman karmaşıklığı **O(n)**. Sıralama gerektirmez.
- **LINQ ad araması:** Aynı filtreyi `Where` ile ifade eder; okunabilirliği yüksektir, yine **O(n)**.
- **Manuel ikili arama:** Önce `List<T>.Sort` ile sıralı bir liste hazırlanır; `left`, `right`, `mid` indeksleri kullanılarak döngüyle arama yapılır. **İkili aramanın ön koşulu: liste aranan anahtara göre sıralı olmalıdır.** Sıralı bir listede arama **O(log n)**; bu örnekte **her çağrı için sıralama** yapıldığından toplam süre **O(n log n)**'dir. Hazır `List<T>.BinarySearch` kullanılmaz.
- **LINQ Id araması:** `FirstOrDefault(p => p.Id == id)` kısadır, ancak sıralanmamış listede **O(n)**. Çok sık Id sorgusunda önceden sıralı tutulan liste veya sözlük daha uygundur.

### Neden `Category.Equals` ve `GetHashCode` override edildi?

`Dictionary<Category, List<Product>>` anahtar eşitliğini ve hash kodunu kullanır. Yalnızca nesne referansına göre eşitlik olsaydı aynı Id'yi temsil eden iki kategori nesnesi farklı anahtar gibi değerlendirilebilirdi. Bu yüzden eşitlik **değiştirilemez `Id`** üzerinden tanımlanmıştır ve hash kodu da aynı Id üzerinden hesaplanır. `Id` sonradan değişmediğinden sözlüğün anahtarları tutarlıdır. Kategori değiştiğinde `InventoryService.UpdateProduct` önce eski grubundan ürünü çıkarır, sonra yeni gruba ekler. Silinen ürünler de sözlükten çıkarılır; rapor için sözlük sıfırdan kurulmaz.

## Notlar

- Veriler bellektedir: uygulama kapanınca ürün değişiklikleri, fişler ve stok hareketleri **kaybolur**.
- Şifre girişinde etkileşimli konsolda karakterler yıldızla gizlenir.
- Kullanıcı sonlandırılmış giriş akışı (EOF) gönderirse program güvenli şekilde kapanır.
- Ödevde belirtilen sınıf ve yetki davranışları örnekleme amaçlıdır; üretim ortamı için rol/yetki, parola saklama, fatura, zaman ve veri kalıcılığı ayrıca ele alınmalıdır.
