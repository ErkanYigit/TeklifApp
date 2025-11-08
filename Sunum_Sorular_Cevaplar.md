# Teklif Yönetim Sistemi - Sunum Soruları ve Cevapları

## 1. Neden .NET Minimal API kullandınız? Controller-based API yerine Minimal API'yi tercih etmenizin nedeni nedir?

**Cevap:**
- **Daha az boilerplate kod**: Controller sınıfları, action method'ları ve route attribute'larına gerek yok
- **Daha hızlı geliştirme**: Endpoint'ler doğrudan `Program.cs` veya extension method'larla tanımlanıyor
- **.NET 8'in modern yaklaşımı**: Minimal API, .NET 6+ ile gelen ve .NET 8'de olgunlaşan bir özellik
- **Performans**: Daha hafif, daha az overhead
- **Okunabilirlik**: Endpoint'ler daha okunabilir ve anlaşılır

**Alternatif:** MVC Controller'lar da kullanılabilirdi, ancak Minimal API projenin gereksinimleri için yeterli ve daha verimli.

---

## 2. Neden Angular Standalone Components kullandınız? NgModules yaklaşımı yerine?

**Cevap:**
- **Daha hafif bundle boyutları**: Her component kendi bağımlılıklarını yönetiyor, gereksiz import'lar yok
- **Lazy loading kolaylığı**: Her feature modülü bağımsız yüklenebiliyor
- **Modern Angular yaklaşımı**: Angular 17+ ile Standalone Components önerilen yaklaşım
- **Daha az yapılandırma**: NgModule tanımlamalarına gerek yok
- **Tree-shaking**: Kullanılmayan kodlar bundle'a dahil edilmiyor

**Alternatif:** NgModules da kullanılabilirdi, ancak Standalone Components daha modern ve bakımı kolay.

---

## 3. Transaction yönetimini neden kullandınız? Hangi durumlarda transaction kullanıyorsunuz?

**Cevap:**
- **Veri bütünlüğü**: İşlemlerin ya tamamı başarılı olmalı ya da hiçbiri (ACID prensibi)
- **Rollback garantisi**: Hata durumunda tüm değişiklikler geri alınıyor
- **Audit log ile birlikte**: Teklif oluşturma/güncelleme ile audit log kaydı aynı transaction içinde

**Kullanım Yerleri:**
- Teklif oluşturma: Teklif + Kalemler + Audit Log
- Teklif güncelleme: Teklif + Kalemler + Audit Log
- Sepet → Teklif dönüştürme: Teklif + Kalemler + Sepet temizleme
- Müşteri onay/red: Durum güncelleme + Audit Log

**Örnek:**
```csharp
using var tr = await db.Database.BeginTransactionAsync();
// İşlemler...
await db.SaveChangesAsync();
Audit.Log(db, ...);
await db.SaveChangesAsync();
await tr.CommitAsync();
```

---

## 4. Audit Log sistemini neden kullandınız? Ne amaçla kullanılıyor?

**Cevap:**
- **Değişiklik geçmişi**: Kim, ne zaman, ne değiştirdi takibi
- **Güvenlik ve uyumluluk**: Veri değişikliklerinin kayıt altına alınması
- **Hata ayıklama**: Sorunlu işlemlerin geriye dönük analizi
- **İş kuralları**: Kritik işlemlerin (tekli oluşturma, onay, red) izlenmesi

**Ne Kaydediliyor:**
- Entity adı (örn: "Teklif")
- Entity ID
- Aksiyon (örn: "Oluşturuldu", "Güncellendi", "Silindi")
- Önceki değerler (JSON formatında)
- Yeni değerler (JSON formatında)
- Kullanıcı ID (kim yaptı)

**Kullanım:**
- Tüm kritik işlemlerde (Teklif CRUD, Müşteri onay/red)
- Transaction içinde kaydediliyor
- Veritabanında `AuditLogs` tablosunda saklanıyor

---

## 5. Neden JWT token kullanıyorsunuz? Session-based authentication yerine?

**Cevap:**
- **Stateless**: Sunucu tarafında session tutmaya gerek yok, token içinde tüm bilgi var
- **Scalability**: Birden fazla sunucu arasında session paylaşımına gerek yok
- **Güvenlik**: Token imzalanmış, değiştirilemez
- **Modern**: RESTful API'ler için standart yaklaşım
- **Mobil uygulama desteği**: Mobil uygulamalar için uygun

**Token İçeriği:**
- `sub`: UserId
- `role`: Kullanıcı rolü (Admin, Purchase, user)
- `exp`: Token geçerlilik süresi
- `iss`, `aud`: Token doğrulama için

**Refresh Token:**
- Access token kısa süreli (örn: 1 saat)
- Refresh token uzun süreli (örn: 30 gün)
- Kullanıcı deneyimi: Sürekli login olmaya gerek yok

---

## 6. Cari ve Tedarikci neden ayrı entity'ler? Tek bir "Partner" entity'si olamaz mıydı?

**Cevap:**
- **Farklı amaçlar**: 
  - Cari = Müşteri (satış yapılan, teklif verilen)
  - Tedarikci = Tedarikçi (malzeme alınan, maliyet fiyatı alınan)
- **Farklı ilişkiler**:
  - Cari → Teklif (1-N): Müşteriye teklif verilir
  - Tedarikci → TedarikciFiyat (1-N): Tedarikçiden fiyat alınır
- **Farklı iş kuralları**:
  - Cari: Adres yönetimi, teklif takibi
  - Tedarikci: Fiyat yönetimi, maliyet karşılaştırması
- **Gelecek planları**: Her biri için farklı özellikler eklenebilir

**Alternatif:** Tek bir "Partner" entity'si de kullanılabilirdi, ancak bu durumda:
- Daha karmaşık yapı (type field, conditional logic)
- Daha az net iş mantığı
- İlişkiler karışabilir

**Mevcut Yapı:**
- `Cari`: Müşteri bilgileri, adresler, teklifler
- `Tedarikci`: Tedarikçi bilgileri, fiyatlar

---

## 7. Müşteri onay sistemi nasıl çalışıyor? Güvenlik nasıl sağlanıyor?

**Cevap:**
- **Token tabanlı erişim**: Login olmaya gerek yok, token ile erişim
- **Zaman sınırlı token**: Token'ın geçerlilik süresi var (örn: 30 gün)
- **Benzersiz token**: Her teklif için benzersiz GUID token üretiliyor
- **Tek kullanımlık değil**: Müşteri tekrar görüntüleyebilir (onay/red öncesi)
- **Onay/red sonrası token silinir**: Güvenlik için

**Süreç:**
1. Teklif oluşturulur, durum otomatik olarak **"Taslak"** olur
2. Kullanıcı teklifi hazırlar (kalemler ekler, düzenler)
3. Teklif durumu manuel olarak **"Gonderildi"** yapılır (Admin veya kullanıcı kendi teklifi için)
4. **"Müşteri Linki"** butonuna tıklanır (sadece "Gonderildi" durumunda görünür)
5. Benzersiz token üretilir, geçerlilik süresi belirlenir (30 gün)
6. Token ve link veritabanına kaydedilir (`OnayToken`, `OnayTokenGecerlilik` alanları)
7. Link müşteriye gönderilir (email, WhatsApp vb.)
8. Müşteri linke tıklar, token ile teklif görüntülenir (anonim erişim)
9. Müşteri **"Onayla"** veya **"Reddet"** butonuna tıklar
10. Onay durumunda: Durum **"Kabul"** olur, `OnayZamani` kaydedilir, `OnaylayanAd` (opsiyonel) kaydedilir
11. Red durumunda: Durum **"Red"** olur, `RedZamani` kaydedilir, `RedNotu` (opsiyonel) kaydedilir
12. **Token silinir** (`OnayToken = null`, `OnayTokenGecerlilik = null`) - Güvenlik için
13. Audit log kaydedilir (kim, ne zaman, ne yaptı)

**Güvenlik:**
- **Token güvenliği**: Token Base64 ile encode edilmiş rastgele 32 byte (256 bit) veriden oluşur
- **Token veritabanında saklanıyor**: `Teklif.OnayToken` ve `Teklif.OnayTokenGecerlilik` alanlarında
- **Token zaman aşımı kontrolü**: Her istekte `OnayTokenGecerlilik > DateTime.UtcNow` kontrolü yapılıyor
- **Token ile teklif görüntüleme/onaylama anonim endpoint'ler**: Login gerektirmez, sadece token yeterli
- **Sadece "Gonderildi" durumunda token üretilebilir**: Backend ve frontend'de kontrol var
- **Onay/red sonrası token silinir**: `OnayToken = null`, `OnayTokenGecerlilik = null` - Güvenlik için
- **Transaction içinde işlemler**: Onay/red işlemleri transaction içinde yapılıyor, hata durumunda rollback
- **Zaten onaylanmış/reddedilmiş kontrolü**: Aynı teklif iki kez onaylanamaz veya reddedilemez
- **Audit log**: Tüm işlemler kayıt altında (kim, ne zaman, ne yaptı)

**API Endpoint'leri:**
- `POST /api/teklif/{id}/share`: Token üretme (yetkili kullanıcılar için)
- `GET /api/teklif/goruntule?token={token}`: Token ile teklif görüntüleme (anonim)
- `POST /api/teklif/onayla`: Teklif onaylama (anonim, token ile)
- `POST /api/teklif/reddet`: Teklif reddetme (anonim, token ile)

---

## 8. StokFiyat ve TedarikciFiyat neden ayrı tablolar? Farkları nelerdir?

**Cevap:**
- **Farklı amaçlar**:
  - `StokFiyat`: Kendi satış fiyatlarımız (liste bazlı, tarih bazlı)
  - `TedarikciFiyat`: Tedarikçilerden gelen maliyet fiyatları
- **Farklı kullanım senaryoları**:
  - `StokFiyat`: Müşteriye satış yaparken kullanılacak fiyat
  - `TedarikciFiyat`: Tedarikçiden alırken maliyet, karşılaştırma için
- **Farklı yönetim**:
  - `StokFiyat`: Kendi fiyat politikamız
  - `TedarikciFiyat`: Tedarikçi teklifleri, güncel fiyatlar

**Kullanım:**
- `StokFiyat`: `FiyatServisi.GetAktifFiyatAsync()` ile sorgulanır
- `TedarikciFiyat`: Teklif Karşılaştırma ekranında kullanılır
- `StokFiyat`: Sepete ürün eklerken varsayılan fiyat için
- `TedarikciFiyat`: Hangi tedarikçiden alınacağına karar vermek için

**Not**: UI'da şu anda sadece `TedarikciFiyat` gösteriliyor, `StokFiyat` backend'de mevcut ama kullanılmıyor (gelecekte eklenebilir).

---

## 9. Teklif Karşılaştırma ekranı neden sadece tedarikçi fiyatlarını gösteriyor? Müşteri teklifleri neden yok?

**Cevap:**
- **Amaç**: Tedarikçi maliyet karşılaştırması (müşteri karşılaştırması değil)
- **İş mantığı**: 
  - "Bu ürünü hangi tedarikçiden alırsam daha uygun?"
  - "Maliyetim ne olur, satış teklifimde ne yazayım?"
- **Kullanım senaryosu**:
  - Satış teklifi hazırlarken, arka planda tedarikçi maliyetlerini görmek
  - En uygun tedarikçiyi seçmek
  - Maliyet + kar marjı = satış fiyatı

**Neden Müşteri Teklifleri Yok?**
- Müşteri teklifleri farklı amaçla (satış)
- Tedarikçi fiyatları farklı amaçla (maliyet)
- Karıştırmamak için ayrı tutuluyor

**Özet:**
- Bu ekran **tedarikçiler arası maliyet/fiyat karşılaştırması** için
- **Müşteriler arası bir karşılaştırma değil**
- Tedarikçi fiyatlarını karşılaştırıp, en uygun tedarikçiyi seçmek için

---

## 10. User Defined Function (UDF) neden kullandınız? C# kodunda da yapılabilirdi?

**Cevap:**
- **Performans**: Veritabanı seviyesinde hızlı sorgulama
- **Tekrar kullanılabilirlik**: SQL sorgularında direkt kullanılabilir
- **Tarih bazlı filtreleme**: Aktif fiyat sorgulama için tarih kontrolü
- **Standart yaklaşım**: Fiyat sorgulama için yaygın bir pattern

**UDF Kullanımı:**
```sql
CREATE FUNCTION dbo.fn_GetAktifFiyat(
    @StokId INT, 
    @FiyatListeNo INT = NULL, 
    @Tarih DATETIME2 = NULL
)
RETURNS DECIMAL(18,2)
```

**Neden UDF?**
- Veritabanı seviyesinde hesaplama daha hızlı
- Birden fazla yerde kullanılabilir (stored procedure, view, C# kod)
- Tarih bazlı filtreleme için uygun
- `FiyatServisi.GetAktifFiyatAsync()` metodu bu UDF'yi kullanıyor

**Alternatif:** C# kodunda da yapılabilirdi, ancak:
- Her sorguda aynı mantığı tekrar yazmak gerekirdi
- Performans açısından veritabanı seviyesinde daha hızlı
- SQL sorgularında direkt kullanılamazdı

---

## Ekstra: Proje Hakkında Genel Sorular

### 11. Projeyi nasıl test ettiniz?

**Cevap:**
- **Manuel test**: Her özellik için manuel test yapıldı
- **Swagger**: API endpoint'leri Swagger üzerinden test edildi
- **Frontend test**: Angular uygulaması üzerinden end-to-end test
- **Veritabanı test**: Migration'lar test edildi, veri bütünlüğü kontrol edildi

### 12. Hangi özellikler gelecekte eklenebilir?

**Cevap:**
- **Email bildirimleri**: Teklif gönderildiğinde, onaylandığında email
- **PDF teklif oluşturma**: Teklifleri PDF olarak dışa aktarma
- **Gelişmiş raporlama**: Dashboard grafikleri, detaylı raporlar
- **Mobil uygulama**: React Native veya Flutter ile mobil uygulama
- **Gelişmiş arama**: Full-text search, filtreleme
- **Teklif versiyonlama**: Revizyon geçmişi, versiyon takibi
- **Çoklu dil desteği**: İngilizce, Türkçe dil desteği

### 13. Proje nasıl deploy edilebilir?

**Cevap:**
- **Backend**: .NET 8 uygulaması, IIS veya Docker container olarak deploy edilebilir
- **Frontend**: Angular uygulaması build edilip, statik dosyalar olarak serve edilebilir (nginx, IIS)
- **Veritabanı**: MS SQL Server, Azure SQL veya on-premise SQL Server
- **CI/CD**: GitHub Actions, Azure DevOps ile otomatik deploy

### 14. Güvenlik önlemleri nelerdir?

**Cevap:**
- **JWT Token**: Stateless authentication
- **BCrypt**: Şifre hash'leme
- **Role-based authorization**: Admin, Purchase, user rolleri
- **CORS**: Frontend-backend arası güvenli iletişim
- **SQL Injection koruması**: Entity Framework Core parametreli sorgular
- **XSS koruması**: Angular'ın built-in XSS koruması
- **Audit Log**: Tüm kritik işlemler kayıt altında

### 15. Proje performansı nasıl optimize edildi?

**Cevap:**
- **Lazy loading**: Angular modülleri lazy load ediliyor
- **Pagination**: Listeler sayfalanmış (20 kayıt/sayfa)
- **Index'ler**: Veritabanında sık sorgulanan alanlar index'li
- **AsNoTracking**: Read-only sorgularda Entity Framework tracking kapalı
- **UDF**: Veritabanı seviyesinde hızlı fiyat sorgulama
- **Caching**: Frontend'de lookup listeleri cache'lenebilir (gelecekte)

---

## Sunum İpuçları

### Hazırlık
1. **Demo hazırlığı**: Canlı demo için test verileri hazırlayın
2. **Backup**: Projenin yedeğini alın
3. **Test**: Tüm özellikleri test edin
4. **Dokümantasyon**: Sunum.md dosyasını gözden geçirin

### Sunum Sırasında
1. **Önce genel bakış**: Projenin amacını ve özelliklerini açıklayın
2. **Demo**: Canlı demo yapın, özellikleri gösterin
3. **Mimari**: Backend ve frontend mimarisini açıklayın
4. **Teknik detaylar**: Sorular geldikçe teknik detayları açıklayın
5. **Sorular**: Soruları açık ve net cevaplayın

### Olası Sorular İçin
1. **Hazırlık**: Yukarıdaki soruları gözden geçirin
2. **Dürüstlük**: Bilmediğiniz bir şey varsa "Bilmiyorum, araştıracağım" deyin
3. **Örnekler**: Kod örnekleriyle açıklama yapın
4. **Karşılaştırma**: Neden bu teknolojiyi seçtiğinizi açıklayın

---

**Başarılar! 🚀**

