# Elle Test Senaryoları

.NET 8 SDK ile `dotnet run` başlatın.

1. **Giriş ve rol:** `admin/admin123` ile giriş yapın. Ürün ekleme/silme/raporlar görünür. Çıkış yapın, `staff/staff123` ile tekrar giriş yapın. 2/3/4 gibi yönetim seçeneklerinin menülerde gösterilmediğini ve girildiğinde yetki reddi geldiğini doğrulayın.
2. **Hatalı giriş:** Üç defa yanlış şifre yazın. Programın sonlandığını doğrulayın.
3. **Başlangıç:** 16 ürün ve 4 kategori/tedarikçi listesini kontrol edin; 101, 202, 301, 401 kritik seviyededir.
4. **Ürün CRUD / kategori sözlüğü:** Yeni ürün ekleyin ve kategori raporunda görünmesini doğrulayın. Kategorisini değiştirin; eski kategori listesinden kalkıp yeni kategoriye gelmeli. Ürünü silin; hiçbir grupta kalmamalı.
5. **Manuel vs. LINQ arama:** `Kablo` ifadesiyle iki aramanın aynı ürünü bulduğunu kontrol edin. `101` Id'siyle ikili ve LINQ aramalarının aynı sonucu verdiğini doğrulayın.
6. **Stok:** Ürün 101'e 10 adet stok girişi yapın; stok 12 olsun. 13 adet çıkış girişiminde stok değişmemeli ve hata mesajı görülmeli. 1 adet stok çıkışı sonrası kayda `StockOut` düşmeli.
7. **Sepet iptal:** 101'den 2, 102'den 1 adet ekleyin ve sepeti iptal edin. Hiçbir ürün stoğu değişmemeli, hareket geçmişi artmamalı.
8. **Satış:** Aynı iki ürünü ekleyip satışı onaylayın. Stoklardan 2/1 düşmeli; iki ayrı `Sale` hareketi yazılmalı ve ara toplam, %20 KDV, genel toplam fişte görünmeli.
9. **Kritik uyarı:** Stok 101'i kritik eşiğe getirecek miktarda satış yapın. Fiş sonunda kritik stok uyarısı çıkmalı.
10. **SKT:** Süt, Yoğurt gibi yakın tarihli ürünler raporda görünmeli; `GetFinalPrice()` üzerinden %10 indirim hesaplanmalı. Süresi uzak ürün indirim almamalı.
11. **Raporlar:** Envanter değeri stok × alış fiyatı toplamı olmalı; en çok satan 5 raporu satışlardan sonra dolmalı; tedarikçi raporu çeşidi doğru saymalı; polimorfik rapor ürün ve tedarikçi bilgilerini içermeli.
12. **Geçersiz giriş:** Negatif fiyat, olmayan Id, geçersiz tarih, metin girilen sayı alanı gibi durumlarda işlem hata mesajıyla kontrol edilmeli ve uygulama çalışmaya devam etmeli.

Not: SDK'nin bulunmadığı bir ortamda bu senaryolar işletilemez; statik kod kontrolü çalıştırılmıştır, çalışma zamanı testi kullanıcı ortamında yapılmalıdır.
