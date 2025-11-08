# Teklif Yönetim Sistemi - Sunum Dokümantasyonu

## 📋 İçindekiler
1. [Proje Genel Bakış](#proje-genel-bakış)
2. [Mimari Yapı](#mimari-yapı)
3. [Teknolojiler](#teknolojiler)
4. [Veritabanı Yapısı](#veritabanı-yapısı)
5. [Özellikler ve Modüller](#özellikler-ve-modüller)
6. [Güvenlik ve Yetkilendirme](#güvenlik-ve-yetkilendirme)
7. [API Endpoints](#api-endpoints)
8. [Kullanıcı Akışları](#kullanıcı-akışları)
9. [Önemli Tasarım Kararları](#önemli-tasarım-kararları)
10. [Teknik Detaylar](#teknik-detaylar)

---

## Proje Genel Bakış

### Ne Yaptık?
Teklif Yönetim Sistemi, işletmelerin müşterilerine teklif hazırlama, yönetme ve karşılaştırma süreçlerini dijitalleştiren kapsamlı bir web uygulamasıdır.

### Neden Yaptık?
- **Manuel süreçlerin otomasyonu**: Kağıt bazlı teklif hazırlama süreçlerini dijitalleştirerek hız ve verimlilik sağlamak
- **Maliyet optimizasyonu**: Tedarikçi fiyatlarını karşılaştırarak en uygun maliyetli teklifleri hazırlamak
- **İzlenebilirlik**: Tüm teklif süreçlerini audit log ile kayıt altına almak
- **Müşteri deneyimi**: Müşterilerin kendi tekliflerini görüntüleyip onay/red işlemlerini yapabilmesi

### Nasıl Yaptık?
- **Backend**: .NET 8 Minimal API ile RESTful servisler
- **Frontend**: Angular 17+ (Standalone Components) ile modern SPA
- **Veritabanı**: MS SQL Server ile ilişkisel veri modeli
- **Güvenlik**: JWT token tabanlı authentication ve role-based authorization

---

## Mimari Yapı

### Backend Katmanlı Mimari

#### 1. **Domain Katmanı** (`Domain/`)
**Ne?** İş mantığının temelini oluşturan entity'ler ve domain modelleri.

**Neden?** 
- Clean Architecture prensiplerine uygunluk
- İş mantığının veritabanı ve framework bağımlılıklarından bağımsız olması
- Test edilebilirlik ve bakım kolaylığı

**Nasıl?**
- `Entities/` klasöründe POCO (Plain Old CLR Objects) sınıfları
- Navigation properties ile ilişkiler tanımlanmış
- Örnek: `Cari`, `Stok`, `Teklif`, `TeklifKalem`, `User`, `Tedarikci`, `TedarikciFiyat`

#### 2. **Application Katmanı** (`Application/`)
**Ne?** İş mantığı servisleri ve hesaplama algoritmaları.

**Neden?**
- Domain katmanındaki entity'ler üzerinde iş mantığı işlemleri
- Tekrar kullanılabilir servisler

**Nasıl?**
- `TeklifHesap.cs`: Teklif toplamlarını hesaplayan statik metod
- `Stats.cs`: İstatistiksel hesaplamalar (medyan, ortalama vb.)

#### 3. **Infrastructure Katmanı** (`Infrastructure/`)
**Ne?** Veritabanı erişimi, migration'lar ve altyapı servisleri.

**Neden?**
- Entity Framework Core yapılandırması
- Veritabanı migration'ları
- Audit logging, numara üretimi gibi altyapı servisleri

**Nasıl?**
- `Data/AppDbContext.cs`: EF Core DbContext yapılandırması
- `Migrations/`: Veritabanı şema değişiklikleri
- `Services/Audit.cs`: Audit log kayıt servisi
- `Services/NoUretici.cs`: Otomatik numara üretimi (TLF-2025-00001 formatı)
- `Services/FiyatServisi.cs`: Stok fiyat sorgulama servisi

#### 4. **API Katmanı** (`Api/`)
**Ne?** HTTP endpoint'leri, DTO'lar ve request/response modelleri.

**Neden?**
- Minimal API yaklaşımı ile hızlı ve hafif API geliştirme
- Swagger entegrasyonu ile API dokümantasyonu
- JWT authentication ve authorization

**Nasıl?**
- `Endpoints/`: Her modül için ayrı endpoint dosyaları
- `Contracts/`: DTO (Data Transfer Object) tanımları
- `Services/`: JWT token üretimi, şifre hash'leme
- `Program.cs`: Uygulama yapılandırması ve middleware pipeline

### Frontend Yapısı

#### Angular Standalone Components
**Ne?** Her component'in kendi modülü olmadan bağımsız çalışabilmesi.

**Neden?**
- Daha hafif bundle boyutları
- Lazy loading kolaylığı
- Modern Angular yaklaşımı

**Nasıl?**
- `features/` klasöründe modül bazlı organizasyon
- Her feature kendi route'ları, component'leri ve service'leri ile
- `core/` klasöründe shared servisler (Auth, Token, Guards)

---

## Teknolojiler

### Backend
- **.NET 8**: En güncel .NET framework
- **Entity Framework Core 8**: ORM (Object-Relational Mapping)
- **MS SQL Server**: İlişkisel veritabanı
- **JWT Bearer Authentication**: Token tabanlı kimlik doğrulama
- **Swagger/OpenAPI**: API dokümantasyonu
- **Minimal API**: Endpoint tanımlama

### Frontend
- **Angular 17+**: Modern SPA framework
- **Devextreme (DevExtreme)**: DataGrid ve UI component'leri
- **RxJS**: Reactive programming (Observables)
- **TypeScript**: Tip güvenli JavaScript

### Veritabanı
- **MS SQL Server**: Production veritabanı
- **User Defined Functions (UDF)**: `fn_GetAktifFiyat` - Stok fiyat sorgulama

---

## Veritabanı Yapısı

### Ana Tablolar

#### 1. **Users** (Kullanıcılar)
```sql
- Id (PK, int)
- Email (unique, nvarchar)
- UserCode (unique, nvarchar) - Login için kullanılan kod
- UserName (unique, nvarchar)
- Password (nvarchar) - BCrypt hash'lenmiş
- Phone (unique, nvarchar, nullable)
- Role (nvarchar, default: 'user') - 'Admin', 'Purchase', 'user'
- Active (bit, default: true)
```

**Neden bu yapı?**
- Email ve UserCode ile esnek login seçenekleri
- Role-based access control için Role alanı
- Telefon numarası zorunlu değil (nullable)

#### 2. **Cariler** (Müşteriler)
```sql
- Id (PK, int)
- Kod (unique, nvarchar) - Benzersiz müşteri kodu
- Ad (required, nvarchar(200))
- VergiNo (nvarchar)
- VergiDairesi (nvarchar)
- Telefon (nvarchar)
- Eposta (nvarchar)
- CreatedAt (datetime2)
- CreatedByUserId (int, nullable, FK -> Users)
```

**İlişkiler:**
- `CariAdresler` (1-N): Bir müşterinin birden fazla adresi olabilir
- `Teklifler` (1-N): Bir müşteriye birden fazla teklif verilebilir

**Neden ayrı CariAdres tablosu?**
- Bir müşterinin farklı teslimat adresleri olabilir
- Adres bilgileri tekrar kullanılabilir
- Normalizasyon prensipleri

#### 3. **Stoklar** (Ürünler)
```sql
- Id (PK, int)
- Kod (unique, nvarchar) - Benzersiz stok kodu
- Ad (required, nvarchar(200))
- Birim (nvarchar, default: 'Adet')
- Cinsi (nvarchar) - Ürün kategorisi/tipi
- Aktif (bit, default: true)
```

**İlişkiler:**
- `StokFiyatlar` (1-N): Stok için farklı fiyat listeleri
- `TedarikciFiyatlar` (1-N): Tedarikçilerden gelen fiyatlar
- `TeklifKalemler` (1-N): Tekliflerde kullanılan stoklar

**Neden StokFiyat ve TedarikciFiyat ayrı?**
- `StokFiyat`: Kendi satış fiyatlarımız (liste bazlı, tarih bazlı)
- `TedarikciFiyat`: Tedarikçilerden gelen maliyet fiyatları
- İkisi farklı amaçlara hizmet eder (satış vs. maliyet)

#### 4. **Teklifler** (Teklifler)
```sql
- Id (PK, int)
- No (unique, nvarchar) - TLF-2025-00001 formatı
- Kod (nvarchar)
- CariId (FK -> Cariler)
- TeklfiTarihi (datetime2)
- Durum (nvarchar, default: 'Taslak') - 'Taslak', 'Revizyonda', 'Gonderildi', 'Kabul', 'Red'
- CreatedByUserId (int, nullable, FK -> Users)
- AraToplam (decimal(18,2))
- IskontoToplam (decimal(18,2))
- KdvToplam (decimal(18,2))
- GenelToplam (decimal(18,2))
- OnayToken (nvarchar, nullable) - Müşteri onay linki için
- OnayTokenGecerlilik (datetime2, nullable)
- OnayZamani (datetime2, nullable)
- OnaylayanAd (nvarchar, nullable)
- RedZamani (datetime2, nullable)
- RedNotu (nvarchar, nullable)
```

**İlişkiler:**
- `TeklifKalemler` (1-N): Teklifin satırları
- `Cariler` (N-1): Hangi müşteriye verildiği

**Neden OnayToken?**
- Müşterilerin login olmadan tekliflerini görüp onay/red edebilmesi
- Güvenli, zaman sınırlı token sistemi

#### 5. **TeklifKalemler** (Teklif Satırları)
```sql
- Id (PK, int)
- TeklifId (FK -> Teklifler)
- StokId (FK -> Stoklar)
- Miktar (decimal(18,2))
- BirimFiyat (decimal(18,2))
- IskontoOran (decimal(18,2)) - Yüzde
- KdvOran (decimal(18,2)) - Yüzde
- Tutar (decimal(18,2)) - Miktar * BirimFiyat
- IskontoTutar (decimal(18,2))
- KdvTutar (decimal(18,2))
- GenelTutar (decimal(18,2)) - AraToplam + KDV
```

**Neden hesaplanmış alanlar?**
- Performans: Her sorguda hesaplama yapmaya gerek yok
- Veri tutarlılığı: Hesaplamalar transaction içinde yapılıyor
- Audit: Değişiklikler kaydedilebiliyor

#### 6. **Tedarikciler** (Tedarikçiler)
```sql
- Id (PK, int)
- Ad (nvarchar, indexed)
- Telefon (nvarchar, nullable)
- Eposta (nvarchar, nullable)
- Aktif (bit, default: true)
```

**Neden ayrı Tedarikci tablosu?**
- Cari (müşteri) ve Tedarikci farklı kavramlar
- Tedarikçilerden gelen fiyatları yönetmek için
- Gelecekte tedarikçi bazlı raporlama için

#### 7. **TedarikciFiyatlar** (Tedarikçi Fiyatları)
```sql
- Id (PK, int)
- StokId (FK -> Stoklar)
- TedarikciId (FK -> Tedarikciler)
- FiyatListeNo (int)
- Fiyat (decimal(18,2))
- ParaBirimi (nvarchar)
- GuncellemeTarihi (datetime2)
```

**Neden bu yapı?**
- Aynı stok için farklı tedarikçilerden farklı fiyatlar
- Fiyat liste numarası ile farklı fiyat listeleri
- Tarih bazlı fiyat takibi

#### 8. **TeklifSepet** ve **TeklifSepetKalem**
**Ne?** Kullanıcıların teklif hazırlarken geçici olarak ürün ekledikleri sepet.

**Neden?**
- E-ticaret benzeri sepet deneyimi
- Teklif hazırlamadan önce ürünleri toplama
- Sepetten direkt teklife dönüştürme

**Yapı:**
```sql
TeklifSepet:
- Id (PK, int)
- UserId (FK -> Users, indexed)
- OlusturmaTarihi (datetime2)

TeklifSepetKalem:
- Id (PK, int)
- SepetId (FK -> TeklifSepet)
- StokId (FK -> Stoklar)
- Miktar (decimal(18,2))
- HedefFiyat (decimal(18,2), nullable) - Kullanıcının belirlediği fiyat
```

#### 9. **AuditLogs** (Denetim Kayıtları)
```sql
- Id (PK, int)
- Entity (nvarchar) - Hangi tablo (örn: 'Teklif')
- EntityId (int) - Hangi kayıt
- Aksiyon (nvarchar) - 'Oluşturuldu', 'Güncellendi', 'Silindi'
- UserId (int, nullable, FK -> Users)
- Onceki (nvarchar, nullable) - JSON formatında önceki değerler
- Sonraki (nvarchar, nullable) - JSON formatında yeni değerler
- CreatedAt (datetime2)
```

**Neden Audit Log?**
- Değişiklik geçmişi takibi
- Güvenlik ve uyumluluk gereksinimleri
- Hata ayıklama ve geri dönüş

### User Defined Function (UDF)

#### `fn_GetAktifFiyat`
**Ne?** Stok ID, fiyat liste numarası ve tarih verildiğinde aktif fiyatı döndüren SQL fonksiyonu.

**Neden?**
- Veritabanı seviyesinde hızlı fiyat sorgulama
- Tarih bazlı fiyat geçerliliği kontrolü
- Tekrar kullanılabilir mantık

**Nasıl?**
```sql
CREATE FUNCTION dbo.fn_GetAktifFiyat(
    @StokId INT, 
    @FiyatListeNo INT = NULL, 
    @Tarih DATETIME2 = NULL
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @ret DECIMAL(18,2);
    DECLARE @t DATETIME2 = ISNULL(@Tarih, SYSUTCDATETIME());
    SELECT TOP(1) @ret = Deger
    FROM StokFiyatlar WITH (NOLOCK)
    WHERE StokId = @StokId
      AND (@FiyatListeNo IS NULL OR FiyatListeNo = @FiyatListeNo)
      AND YururlukTarihi <= @t
    ORDER BY YururlukTarihi DESC;
    RETURN @ret;
END
```

**Kullanım:**
- Backend'de `FiyatServisi.GetAktifFiyatAsync()` metodu `StokFiyat` tablosundan aktif fiyatı sorgular
- **2 yerde kullanılıyor:**
  1. **`GET /api/stok/{id}/fiyat`** endpoint'inde: Belirli bir stok için aktif fiyatı döndürmek için
     - Fiyat liste numarası ve tarih parametreleri ile filtreleme yapılabilir
     - Stok detay sayfasında veya API üzerinden fiyat sorgulama için kullanılır
  2. **`POST /api/sepet/donustur`** endpoint'inde: Sepeti teklife dönüştürürken varsayılan fiyat bulmak için
     - Sepet kaleminde `HedefFiyat` yoksa, aktif fiyat sorgulanır
     - Eğer aktif fiyat da yoksa, varsayılan olarak `0m` kullanılır
     - Bu sayede kullanıcı fiyat girmemişse sistem otomatik fiyat bulur

---

## Özellikler ve Modüller

### 1. Kullanıcı Yönetimi ve Authentication

#### Kayıt Ol (Register)
**Ne?** Yeni kullanıcıların email, kullanıcı kodu, telefon ve şifre ile kayıt olması.

**Neden?**
- Sistemin kullanıcı tabanlı çalışması
- Her kullanıcının kendi tekliflerini yönetmesi
- Audit log için kullanıcı takibi

**Nasıl?**
- **Frontend**: `register.component.ts` - Form validasyonu ile
- **Backend**: `POST /api/auth/register`
- **Validasyonlar**:
  - Email format kontrolü
  - UserCode benzersizlik kontrolü
  - Telefon numarası Türkiye formatı kontrolü (11 hane)
  - Şifre güçlülük kontrolü (min 8 karakter, büyük/küçük harf, rakam, özel karakter)
- **Güvenlik**: Şifre BCrypt ile hash'leniyor

#### Giriş Yap (Login)
**Ne?** UserCode + Password ile kimlik doğrulama.

**Neden?**
- Kullanıcı dostu (email yerine kod)
- JWT token tabanlı stateless authentication

**Nasıl?**
- **Frontend**: `login.component.ts`
- **Backend**: `POST /api/auth/login`
- **Süreç**:
  1. UserCode ile kullanıcı bulunur
  2. Password BCrypt ile doğrulanır
  3. JWT token üretilir (Access Token + Refresh Token)
  4. Token localStorage'a kaydedilir
  5. Dashboard'a yönlendirilir

**JWT Token İçeriği:**
- `sub`: UserId
- `role`: Kullanıcı rolü (Admin, Purchase, user)
- `exp`: Token geçerlilik süresi
- `iss`, `aud`: Token doğrulama için

#### Refresh Token
**Ne?** Access token süresi dolduğunda yeni token almak.

**Neden?**
- Güvenlik: Access token kısa süreli (örn: 1 saat)
- Kullanıcı deneyimi: Sürekli login olmaya gerek yok

**Nasıl?**
- Refresh token veritabanında saklanıyor
- `POST /api/auth/refresh` endpoint'i ile yeni access token alınıyor
- Frontend'de interceptor ile otomatik refresh

---

### 2. Cari Kartları Yönetimi

#### Cari Listesi
**Ne?** Tüm müşterilerin listelendiği sayfa.

**Özellikler:**
- DevExtreme DataGrid ile sayfalama, sıralama, filtreleme
- "Yeni Cari Ekle" butonu ile inline ekleme
- "Details" butonu ile detay sayfasına gitme
- Inline düzenleme (Save/Cancel butonları)

**Neden DevExtreme?**
- Profesyonel data grid özellikleri
- Sayfalama, sıralama, filtreleme built-in
- Inline editing desteği

**Nasıl?**
- **Frontend**: `cari-list.component.ts`
- **Backend**: `GET /api/cari` (paged query)
- **Inline Editing**: 
  - Yeni satır eklenince "Save" ve "Cancel" butonları görünür
  - Kaydedilince "Details" butonu geri gelir
  - `onRowInserted`, `onRowUpdated` event'leri ile backend'e kayıt

#### Cari Detay Sayfası
**Ne?** Bir müşterinin tüm bilgilerinin görüntülendiği ve düzenlendiği sayfa.

**Özellikler:**
- Readonly kartlar: Kod, Ad, Vergi No, Vergi Dairesi, Telefon, E-posta
- Adres yönetimi: Birden fazla adres ekleme/düzenleme/silme
- "Bu Cariyi Sil" butonu: Modal onay ile silme

**Neden Readonly Kartlar?**
- Önemli bilgilerin görsel olarak vurgulanması
- Düzenleme formundan ayrı tutulması
- Modern UI/UX

**Nasıl?**
- **Frontend**: `cari-detail.component.ts`
- **Backend**: 
  - `GET /api/cari/{id}` - Cari ve adresler
  - `PUT /api/cari/{id}` - Cari güncelleme
  - `DELETE /api/cari/{id}` - Cari silme
- **Modal Onay**: Custom modal component (ekranın ortasında kart)

**Adres Yönetimi:**
- `CariAdres` entity'si ile 1-N ilişki
- Her adres: Adres satırı, şehir, posta kodu, ülke
- Adresler ayrı tabloda tutuluyor (normalizasyon)

---

### 3. Stok Kartları Yönetimi

#### Stok Listesi
**Ne?** Tüm ürünlerin listelendiği sayfa.

**Özellikler:**
- Cari listesi ile aynı yapı (inline editing, save/cancel)
- Admin rolü gerektiriyor

**Neden Admin Rolü?**
- Stok yönetimi kritik işlem
- Herkesin stok eklemesi/düzenlemesi istenmiyor

**Nasıl?**
- **Frontend**: `stok-list.component.ts`
- **Backend**: `GET /api/stok`, `POST /api/stok`, `PUT /api/stok/{id}`
- **Route Guard**: `data: { roles: ['Admin'] }` ile korunuyor

#### Stok Detay Sayfası
**Ne?** Bir ürünün detaylarının görüntülendiği ve düzenlendiği sayfa.

**Özellikler:**
- Readonly kartlar: Kod, Ad, Birim, Cinsi, Aktif
- **Tedarikçi Fiyatları**: Bu stok için farklı tedarikçilerden gelen fiyatlar
- "Bu Stoku Sil" butonu: Modal onay ile

**Neden Tedarikçi Fiyatları?**
- Aynı ürün için farklı tedarikçilerden farklı fiyatlar
- Maliyet karşılaştırması için
- Teklif hazırlarken en uygun fiyatı seçmek için

**Nasıl?**
- **Frontend**: `stok-detail.component.ts`
- **Backend**: 
  - `GET /api/stok/{id}/tedarikci-fiyatlar`
  - `POST /api/stok/{id}/tedarikci-fiyatlar`
  - `PUT /api/stok/{id}/tedarikci-fiyatlar/{fid}`
  - `DELETE /api/stok/{id}/tedarikci-fiyatlar/{fid}`
- **DataGrid**: Tedarikçi dropdown, fiyat, para birimi, güncelleme tarihi

**Not**: `StokFiyat` tablosu backend'de var ama UI'da kaldırıldı. Sadece `TedarikciFiyat` gösteriliyor.

---

### 4. Normal Teklif Fişi

#### Teklif Listesi
**Ne?** Tüm tekliflerin listelendiği sayfa.

**Özellikler:**
- Filtreleme: Cari, durum, tarih aralığı
- Durum renkleri: Taslak (gri), Gönderildi (mavi), Kabul (yeşil), Red (kırmızı)
- "Yeni Teklif" butonu

**Neden Durum Sistemi?**
- Teklif yaşam döngüsü takibi
- Workflow yönetimi
- Raporlama için

**Durumlar:**
- `Taslak`: Henüz gönderilmemiş
- `Revizyonda`: Gönderilmiş ama revize edilmiş
- `Gonderildi`: Müşteriye gönderilmiş
- `Kabul`: Müşteri onaylamış
- `Red`: Müşteri reddetmiş

#### Teklif Oluşturma
**Ne?** Yeni teklif oluşturma formu.

**Özellikler:**
- Cari seçimi (dropdown)
- Teklif tarihi
- Kalem ekleme: Stok seçimi, miktar, birim fiyat, iskonto oranı, KDV oranı
- Otomatik hesaplama: Tutar, iskonto tutarı, KDV tutarı, genel tutar
- "Müşteri Linki" butonu: Onay linki oluşturma

**Neden Otomatik Hesaplama?**
- Hata riskini azaltma
- Tutarlılık
- Kullanıcı deneyimi

**Hesaplama Mantığı:**
```csharp
Tutar = Miktar * BirimFiyat
IskontoTutar = Tutar * (IskontoOran / 100)
AraToplam = Tutar - IskontoTutar
KdvTutar = AraToplam * (KdvOran / 100)
GenelTutar = AraToplam + KdvTutar
```

**Nasıl?**
- **Frontend**: `teklif-create.component.ts`
- **Backend**: `POST /api/teklif`
- **Transaction**: Tüm işlemler transaction içinde
- **Audit Log**: "Oluşturuldu" aksiyonu kaydediliyor

#### Teklif Düzenleme
**Ne?** Mevcut teklifin düzenlenmesi.

**Kısıtlamalar:**
- Sadece `Taslak` ve `Revizyonda` durumundaki teklifler düzenlenebilir
- Sadece oluşturan kullanıcı veya Admin düzenleyebilir

**Neden Bu Kısıtlamalar?**
- Gönderilmiş tekliflerin değiştirilmemesi
- Veri bütünlüğü
- İş kuralları

**Nasıl?**
- **Frontend**: `teklif-edit.component.ts`
- **Backend**: `PUT /api/teklif/{id}`
- **Transaction + Audit Log**: "Güncellendi" aksiyonu

#### Müşteri Onay Sistemi
**Ne?** Müşterilerin login olmadan tekliflerini görüp onay/red edebilmesi.

**Neden?**
- Müşteri deneyimi: Login olmaya gerek yok
- Güvenlik: Token tabanlı, zaman sınırlı erişim
- Hız: Hızlı onay süreci

**Nasıl?**
1. **Token Oluşturma**: `POST /api/teklif/{id}/musteri-linki`
   - Benzersiz token üretilir
   - Geçerlilik süresi belirlenir (örn: 30 gün)
   - Token veritabanına kaydedilir

2. **Teklif Görüntüleme**: `GET /api/teklif/token/{token}` (anonim)
   - Token ile teklif bilgileri döner
   - Kalemler, toplamlar gösterilir

3. **Onay/Red**: 
   - `POST /api/teklif/onayla` (anonim)
   - `POST /api/teklif/reddet` (anonim)
   - Durum güncellenir, audit log kaydedilir

**Frontend**: `teklif-onay.component.ts` - Standalone component, route guard yok (anonim erişim)

---

### 5. Teklif Sepetim

#### Sepet Yönetimi
**Ne?** Kullanıcıların teklif hazırlamadan önce ürünleri topladığı geçici sepet.

**Neden?**
- E-ticaret benzeri deneyim
- Teklif hazırlamadan önce ürün seçimi
- Sepetten direkt teklife dönüştürme

**Özellikler:**
- Sepete ürün ekleme (stok seçimi, miktar, hedef fiyat)
- Sepet kalemlerini düzenleme/silme
- Sepeti teklife dönüştürme (cari seçimi ile)

**Nasıl?**
- **Frontend**: `sepet.component.ts`
- **Backend**: 
  - `GET /api/sepet` - Kullanıcının sepeti (yoksa oluşturulur)
  - `POST /api/sepet/kalemler` - Kalem ekleme
  - `PUT /api/sepet/kalem/{kid}` - Kalem güncelleme
  - `DELETE /api/sepet/kalem/{kid}` - Kalem silme
  - `POST /api/sepet/donustur?cariId={id}` - Sepeti teklife dönüştürme

**Sepet → Teklif Dönüşümü:**
1. Cari seçilir
2. Sepet kalemleri `TeklifKalem`'e dönüştürülür
3. **Fiyat belirleme mantığı:**
   - Önce `HedefFiyat` kontrol edilir (kullanıcı manuel girmişse)
   - `HedefFiyat` yoksa `FiyatServisi.GetAktifFiyatAsync()` ile aktif fiyat sorgulanır
   - Aktif fiyat da yoksa varsayılan olarak `0m` kullanılır
4. Teklif oluşturulur, hesaplamalar yapılır
5. Sepet temizlenir
6. Transaction commit edilir

**Neden Hedef Fiyat?**
- Kullanıcı manuel fiyat girebilir (karşılaştırma ekranından gelen fiyatlar)
- Aktif fiyat yoksa varsayılan değer kullanılır
- **FiyatServisi kullanımı**: `SepetEndpoints.cs` dosyasında `POST /api/sepet/donustur` endpoint'inde, her sepet kalemi için `FiyatServisi.GetAktifFiyatAsync(db, s.StokId, null, null)` çağrılır

---

### 6. Teklif Föyü

#### Föy Listesi
**Ne?** Tüm tekliflerin filtreleme ve export özellikleri ile görüntülendiği sayfa.

**Özellikler:**
- Filtreleme: Tarih aralığı, cari, durum, teklif no arama
- "Benim Föyüm": Sadece kullanıcının oluşturduğu teklifler
- Export: CSV ve Excel formatında dışa aktarma

**Neden Föy?**
- Raporlama ihtiyacı
- Dış sistemlere veri aktarımı
- Analiz için veri çıkarma

**Nasıl?**
- **Frontend**: `foy.component.ts`
- **Backend**: 
  - `GET /api/teklif/foy` - Tüm teklifler (filtreli)
  - `GET /api/teklif/foy/my` - Kullanıcının teklifleri
  - `GET /api/teklif/foy/export/csv` - CSV export
  - `GET /api/teklif/foy/export/xlsx` - Excel export

**Export Formatı:**
- CSV: `No,CariId,Tarih,Durum,GenelToplam`
- Excel: Tab-separated values (basit format)

---

### 7. Teklif Karşılaştırma

#### Karşılaştırma Ekranı
**Ne?** Aynı stok kalemi için farklı tedarikçilerin verdiği fiyatları karşılaştırma.

**Neden?**
- **Maliyet optimizasyonu**: En uygun tedarikçiyi bulmak
- **Satış fiyatı belirleme**: Maliyet + kar marjı = satış fiyatı
- **Tedarikçi performansı**: Hangi tedarikçi daha uygun fiyat veriyor

**Özellikler:**
- Stok seçimi (barkod okuyucu ile veya dropdown)
- Fiyat listesi: Tedarikçi adı, fiyat, para birimi, güncelleme tarihi
- Özet kutular: En düşük, medyan, en yüksek fiyat, son güncelleme
- Sepete ekleme: Seçilen satırları sepete ekleme
- Teklife ekleme: Seçilen satırları mevcut teklife kalem olarak ekleme

**Neden Sadece Tedarikçi Fiyatları?**
- Bu ekran **maliyet karşılaştırması** için
- Müşteri teklifleri değil, tedarikçi maliyetleri karşılaştırılıyor
- "Hangi tedarikçiden alırsam daha uygun?" sorusuna cevap

**Nasıl?**
- **Frontend**: `karsilastirma.component.ts`
- **Backend**: 
  - `GET /api/karsilastirma?stokId={id}` - Fiyat karşılaştırması
  - `POST /api/karsilastirma/sepet` - Sepete ekleme
  - `POST /api/karsilastirma/teklif` - Teklife ekleme

**Backend Mantığı:**
1. Stok ID ile `TedarikciFiyat` kayıtları sorgulanır
2. Fiyat liste numarası ve tarih filtreleri uygulanır
3. Tedarikçi bilgileri join edilir
4. Fiyata göre sıralanır (en düşükten en yükseğe)
5. Özet istatistikler hesaplanır (min, medyan, max)
6. Sayfalama uygulanır

**Özet Hesaplamaları:**
- `EnDusuk`: Minimum fiyat
- `Medyan`: Ortadaki fiyat (Stats.Median metodu)
- `EnYuksek`: Maximum fiyat
- `SonGuncelleme`: En güncel fiyat güncelleme tarihi
- `TedarikciSayisi`: Kaç farklı tedarikçi var
- `TeklifSayisi`: 0 (artık teklif gösterilmiyor)

**Sepete/Teklife Ekleme:**
- Seçilen satırların `StokId`'leri alınır
- Hedef fiyat olarak `Fiyat` kullanılır
- Sepet kalemi veya teklif kalemi oluşturulur

---

## Güvenlik ve Yetkilendirme

### Authentication (Kimlik Doğrulama)

#### JWT Token Sistemi
**Ne?** Stateless token tabanlı kimlik doğrulama.

**Neden?**
- Scalability: Sunucu tarafında session tutmaya gerek yok
- Security: Token imzalanmış, değiştirilemez
- Modern: RESTful API'ler için standart

**Nasıl?**
1. **Login**: UserCode + Password doğrulanır, JWT token üretilir
2. **Token İçeriği**: UserId, Role, Expiration
3. **Token Saklama**: Frontend'de localStorage
4. **Request Header**: `Authorization: Bearer {token}`
5. **Backend Doğrulama**: JWT Bearer middleware ile otomatik

**Token Yapılandırması:**
```csharp
ValidIssuer = "TeklifApp"
ValidAudience = "TeklifAppUsers"
IssuerSigningKey = SymmetricSecurityKey (appsettings'den)
ClockSkew = 2 dakika (saat farkı toleransı)
```

### Authorization (Yetkilendirme)

#### Role-Based Access Control (RBAC)
**Ne?** Kullanıcı rollerine göre yetki kontrolü.

**Roller:**
- `Admin`: Tüm yetkiler (stok yönetimi, kullanıcı yönetimi, karşılaştırma)
- `Purchase`: Satın alma yetkisi (stok görüntüleme, teklif oluşturma)
- `user`: Temel yetkiler (teklif oluşturma, kendi tekliflerini görüntüleme)

**Neden?**
- Güvenlik: Herkes her şeye erişemez
- İş kuralları: Rol bazlı iş akışları
- Audit: Kim ne yaptı takibi

**Nasıl?**
- **Backend**: `RequireAuthorization("Admin")` veya `RequireAuthorization("AdminOrPurchase")`
- **Frontend**: Route guard ile `data: { roles: ['Admin'] }`
- **JWT Claim**: Token içinde `role` claim'i var

#### Endpoint Yetkilendirmesi
**Örnekler:**
- `POST /api/stok`: Admin veya Purchase
- `DELETE /api/cari/{id}`: Admin veya Purchase
- `GET /api/karsilastirma`: Admin
- `POST /api/auth/make-admin`: Admin

#### Anonim Erişim
**Ne?** Login olmadan erişilebilen endpoint'ler.

**Hangi Endpoint'ler?**
- `GET /api/teklif/token/{token}`: Müşteri teklif görüntüleme
- `POST /api/teklif/onayla`: Müşteri onay
- `POST /api/teklif/reddet`: Müşteri red

**Neden?**
- Müşteri deneyimi: Login olmaya gerek yok
- Token güvenliği: Token ile yetkilendirme

### Şifre Güvenliği

#### BCrypt Hash
**Ne?** Şifrelerin hash'lenerek saklanması.

**Neden?**
- Güvenlik: Plain text şifre saklanmaz
- Salt: Her şifre için farklı salt
- Yavaş hash: Brute force saldırılarına karşı koruma

**Nasıl?**
- **Backend**: `Password.Hash()` ve `Password.Verify()` metodları
- BCrypt.NET kütüphanesi kullanılıyor
- Şifre kayıt sırasında hash'leniyor
- Login sırasında hash karşılaştırması yapılıyor

### CORS (Cross-Origin Resource Sharing)

**Ne?** Frontend'in farklı bir port'tan (4200) backend'e (5043) istek atabilmesi.

**Neden?**
- Development: Frontend ve backend ayrı portlarda çalışıyor
- Production: Farklı domain'lerden erişim

**Nasıl?**
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});
```

---

## API Endpoints

### Authentication Endpoints
- `POST /api/auth/register` - Kullanıcı kaydı
- `POST /api/auth/login` - Giriş yap
- `POST /api/auth/refresh` - Token yenile
- `POST /api/auth/logout` - Çıkış yap
- `POST /api/auth/make-admin` - Kullanıcıyı admin yap (Admin)
- `POST /api/auth/bootstrap-admin` - İlk admin atama (anonim, sadece admin yoksa)

### Cari Endpoints
- `GET /api/cari` - Cari listesi (paged, filtrelenebilir)
- `GET /api/cari/{id}` - Cari detay (adresler dahil)
- `POST /api/cari` - Yeni cari oluştur
- `PUT /api/cari/{id}` - Cari güncelle
- `DELETE /api/cari/{id}` - Cari sil

### Stok Endpoints
- `GET /api/stok` - Stok listesi (paged, filtrelenebilir)
- `GET /api/stok/{id}` - Stok detay
- `POST /api/stok` - Yeni stok oluştur (AdminOrPurchase)
- `PUT /api/stok/{id}` - Stok güncelle (AdminOrPurchase)
- `DELETE /api/stok/{id}` - Stok sil (AdminOrPurchase)
- `GET /api/stok/{id}/fiyat?liste={no}&tarih={date}` - **Aktif fiyat sorgulama** (FiyatServisi kullanır)
- `GET /api/stok/{id}/tedarikci-fiyatlar` - Tedarikçi fiyatları
- `POST /api/stok/{id}/tedarikci-fiyatlar` - Tedarikçi fiyatı ekle
- `PUT /api/stok/{id}/tedarikci-fiyatlar/{fid}` - Tedarikçi fiyatı güncelle
- `DELETE /api/stok/{id}/tedarikci-fiyatlar/{fid}` - Tedarikçi fiyatı sil

### Tedarikci Endpoints
- `GET /api/tedarikci` - Tedarikçi listesi (paged, filtrelenebilir)
- `GET /api/tedarikci/{id}` - Tedarikçi detay
- `POST /api/tedarikci` - Yeni tedarikçi oluştur (AdminOrPurchase)
- `PUT /api/tedarikci/{id}` - Tedarikçi güncelle (AdminOrPurchase)
- `DELETE /api/tedarikci/{id}` - Tedarikçi sil (AdminOrPurchase)

### Teklif Endpoints
- `GET /api/teklif` - Teklif listesi (paged, filtrelenebilir)
- `GET /api/teklif/{id}` - Teklif detay (kalemler dahil)
- `POST /api/teklif` - Yeni teklif oluştur
- `PUT /api/teklif/{id}` - Teklif güncelle
- `DELETE /api/teklif/{id}` - Teklif sil
- `POST /api/teklif/{id}/durum` - Teklif durumu değiştir
- `POST /api/teklif/{id}/musteri-linki` - Müşteri onay linki oluştur
- `GET /api/teklif/token/{token}` - Token ile teklif görüntüle (anonim)
- `POST /api/teklif/onayla` - Teklif onayla (anonim)
- `POST /api/teklif/reddet` - Teklif reddet (anonim)

### Sepet Endpoints
- `GET /api/sepet` - Kullanıcının sepeti
- `POST /api/sepet/kalemler` - Sepete kalem ekle
- `PUT /api/sepet/kalem/{kid}` - Sepet kalemi güncelle
- `DELETE /api/sepet/kalem/{kid}` - Sepet kalemi sil
- `POST /api/sepet/donustur?cariId={id}` - Sepeti teklife dönüştür

### Föy Endpoints
- `GET /api/teklif/foy` - Föy listesi (filtrelenebilir)
- `GET /api/teklif/foy/my` - Kullanıcının föyü
- `GET /api/teklif/foy/export/csv` - CSV export
- `GET /api/teklif/foy/export/xlsx` - Excel export

### Karşılaştırma Endpoints
- `GET /api/karsilastirma` - Fiyat karşılaştırması (stokId gerekli)
- `POST /api/karsilastirma/sepet` - Seçilen satırları sepete ekle
- `POST /api/karsilastirma/teklif` - Seçilen satırları teklife ekle

### Audit Endpoints
- `GET /api/audit` - Audit log listesi (filtrelenebilir)
- `GET /api/audit/{id}` - Audit log detay

### Users Endpoints
- `GET /api/users` - Kullanıcı listesi (Admin)
- `GET /api/users/me` - Kendi bilgilerim
- `PUT /api/users/me` - Kendi bilgilerimi güncelle
- `POST /api/users/me/password` - Şifre değiştir

---

## Kullanıcı Akışları

### 1. Yeni Kullanıcı Kaydı
1. `/login` sayfasından "Kayıt Ol" linkine tıklanır
2. `/register` sayfasında form doldurulur (email, userCode, telefon, şifre)
3. Form validasyonu yapılır
4. `POST /api/auth/register` ile kayıt oluşturulur
5. Başarılı olursa `/login` sayfasına yönlendirilir

### 2. Giriş ve Dashboard
1. `/login` sayfasında UserCode + Password girilir
2. `POST /api/auth/login` ile token alınır
3. Token localStorage'a kaydedilir
4. `/dashboard` sayfasına yönlendirilir
5. Dashboard'da KPI'lar, grafikler, son teklifler gösterilir

### 3. Teklif Oluşturma (Sepet Üzerinden)
1. `/karsilastirma` sayfasında stok seçilir
2. Tedarikçi fiyatları listelenir
3. Uygun fiyatlar seçilip "Sepete Ekle" butonuna tıklanır
4. `/sepet` sayfasında sepet görüntülenir
5. Gerekirse miktar/fiyat düzenlenir
6. "Teklife Dönüştür" butonuna tıklanır
7. Cari seçilir
8. Teklif oluşturulur, `/teklif/{id}` sayfasına yönlendirilir

### 4. Teklif Oluşturma (Direkt)
1. `/teklif` sayfasında "Yeni Teklif" butonuna tıklanır
2. `/teklif/create` sayfasında cari seçilir
3. Kalemler eklenir (stok, miktar, fiyat, iskonto, KDV)
4. Otomatik hesaplamalar yapılır
5. "Kaydet" butonuna tıklanır
6. Teklif oluşturulur, `/teklif/{id}` sayfasına yönlendirilir

### 5. Müşteri Onay Süreci
1. `/teklif/{id}` sayfasında "Müşteri Linki" butonuna tıklanır
2. Token oluşturulur, link gösterilir
3. Link müşteriye gönderilir (email, WhatsApp vb.)
4. Müşteri linke tıklar, `/teklif-onay?token=xxx` sayfası açılır
5. Teklif detayları görüntülenir
6. "Onayla" veya "Reddet" butonuna tıklanır
7. Durum güncellenir, audit log kaydedilir

### 6. Stok Yönetimi (Admin)
1. `/stok` sayfasında stoklar listelenir
2. "Yeni Stok Ekle" butonuna tıklanır
3. Inline editing ile stok bilgileri girilir
4. "Kaydet" butonuna tıklanır
5. Stok oluşturulur
6. Stok detay sayfasında tedarikçi fiyatları eklenir

---

## Önemli Tasarım Kararları

### 1. Minimal API vs Controller-Based API
**Karar**: Minimal API kullanıldı.

**Neden?**
- Daha az boilerplate kod
- Daha hızlı geliştirme
- .NET 8'in modern yaklaşımı
- Endpoint'ler daha okunabilir

**Alternatif**: MVC Controller'lar (daha geleneksel, daha fazla kod)

### 2. Standalone Components vs NgModules
**Karar**: Angular Standalone Components kullanıldı.

**Neden?**
- Daha hafif bundle boyutları
- Lazy loading kolaylığı
- Modern Angular yaklaşımı (Angular 17+)
- Her component bağımsız

**Alternatif**: NgModules (daha geleneksel, daha fazla yapılandırma)

### 3. Transaction Yönetimi
**Karar**: Her kritik işlem transaction içinde yapılıyor.

**Neden?**
- Veri bütünlüğü: Ya hepsi başarılı ya hepsi başarısız
- Hata durumunda rollback
- Audit log ile birlikte kaydedilmesi

**Örnekler:**
- Teklif oluşturma: Teklif + Kalemler + Audit Log
- Sepet → Teklif: Teklif + Kalemler + Sepet temizleme

### 4. Audit Log Sistemi
**Karar**: Tüm kritik işlemler audit log'a kaydediliyor.

**Neden?**
- Değişiklik geçmişi
- Güvenlik ve uyumluluk
- Hata ayıklama

**Ne Kaydediliyor?**
- Entity adı (örn: "Teklif")
- Entity ID
- Aksiyon (örn: "Oluşturuldu", "Güncellendi", "Silindi")
- Önceki değerler (JSON)
- Yeni değerler (JSON)
- Kullanıcı ID

### 5. Otomatik Numara Üretimi
**Karar**: Teklif numaraları otomatik üretiliyor (TLF-2025-00001 formatı).

**Neden?**
- Benzersizlik garantisi
- Sıralı numaralandırma
- Yıl bazlı organizasyon

**Nasıl?**
- `NoUretici.TeklifNo()` metodu
- Veritabanında aynı yıl için en yüksek numara bulunur
- +1 eklenir, 5 haneli format (00001, 00002, ...)

### 6. Cari ve Tedarikci Ayrımı
**Karar**: Cari (müşteri) ve Tedarikci ayrı entity'ler.

**Neden?**
- Farklı amaçlar: Cari = satış, Tedarikci = maliyet
- Farklı ilişkiler: Cari → Teklif, Tedarikci → Fiyat
- Gelecekte farklı özellikler eklenebilir

**Alternatif**: Tek bir "Partner" entity'si (daha karmaşık, daha az net)

### 7. StokFiyat vs TedarikciFiyat
**Karar**: İki ayrı fiyat tablosu.

**Neden?**
- `StokFiyat`: Kendi satış fiyatlarımız (liste bazlı, tarih bazlı)
- `TedarikciFiyat`: Tedarikçilerden gelen maliyet fiyatları
- Farklı amaçlar: Satış vs. Maliyet karşılaştırması

**Not**: UI'da sadece `TedarikciFiyat` gösteriliyor. `StokFiyat` backend'de var ama kullanılmıyor.

### 8. Müşteri Onay Token Sistemi
**Karar**: Login olmadan token ile teklif görüntüleme/onaylama.

**Neden?**
- Müşteri deneyimi: Login olmaya gerek yok
- Güvenlik: Token zaman sınırlı, benzersiz
- Hız: Hızlı onay süreci

**Güvenlik Önlemleri:**
- Token benzersiz (GUID)
- Geçerlilik süresi (örn: 30 gün)
- Tek kullanımlık değil (müşteri tekrar görüntüleyebilir)
- Onay/red sonrası token silinir

### 9. DevExtreme DataGrid Kullanımı
**Karar**: DevExtreme DataGrid component'i kullanıldı.

**Neden?**
- Profesyonel özellikler: Sayfalama, sıralama, filtreleme, inline editing
- Hızlı geliştirme
- Modern UI

**Alternatif**: Angular Material Table (daha basit, daha az özellik)

### 10. Inline Editing Pattern
**Karar**: List sayfalarında inline editing (satır içi düzenleme).

**Neden?**
- Kullanıcı deneyimi: Detay sayfasına gitmeye gerek yok
- Hız: Hızlı düzenleme
- Modern UI pattern

**Nasıl?**
- Yeni satır eklenince "Save" ve "Cancel" butonları görünür
- Kaydedilince "Details" butonu geri gelir
- DevExtreme'in built-in editing özellikleri kullanılıyor

---

## Teknik Detaylar

### Backend Teknik Detaylar

#### Entity Framework Core Yapılandırması
**AppDbContext.cs**:
- `OnModelCreating`: Index'ler, unique constraint'ler, ilişkiler
- `HasPrecision`: Decimal alanlar için hassasiyet (18,2)
- `OnDelete`: Cascade veya Restrict davranışları
- `HasDefaultValue`: Varsayılan değerler

**Örnekler:**
```csharp
modelBuilder.Entity<Cari>().HasIndex(x => x.Kod).IsUnique();
modelBuilder.Entity<TeklifKalem>().Property(x => x.Miktar).HasPrecision(18, 2);
modelBuilder.Entity<CariAdres>()
    .HasOne(x => x.Cari)
    .WithMany(x => x.Adresler)
    .OnDelete(DeleteBehavior.Cascade);
```

#### Migration Sistemi
**Ne?** Veritabanı şema değişikliklerinin versiyonlanması.

**Nasıl?**
```bash
dotnet ef migrations add MigrationAdi --project Infrastructure/Infrastructure.csproj --startup-project Api/Api.csproj
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project Api/Api.csproj
```

**Önemli Migration'lar:**
- `Init`: İlk tablo yapısı
- `Add_UDF_StokAktifFiyat`: UDF ekleme
- `AddCariStok`: Cari-Stok ilişkisi (sonra kaldırıldı)

#### FiyatServisi (Fiyat Sorgulama Servisi)
**Ne?** Stok için aktif fiyat sorgulama servisi.

**Neden?**
- Tekrar kullanılabilir fiyat sorgulama mantığı
- Tarih ve liste numarası bazlı filtreleme
- UDF yerine C# kodu ile daha esnek kontrol

**Nasıl?**
```csharp
public static async Task<decimal?> GetAktifFiyatAsync(
    AppDbContext db, 
    int stokId, 
    int? fiyatListeNo, 
    DateTime? tarih)
{
    var t = tarih ?? DateTime.UtcNow;
    var q = db.Set<StokFiyat>().AsNoTracking()
        .Where(x => x.StokId == stokId && x.YururlukTarihi <= t);
    if (fiyatListeNo.HasValue)
        q = q.Where(x => x.FiyatListeNo == fiyatListeNo.Value);
    var f = await q.OrderByDescending(x => x.YururlukTarihi)
        .Select(x => (decimal?)x.Deger)
        .FirstOrDefaultAsync();
    return f;
}
```

**Kullanım Yerleri:**
1. **`GET /api/stok/{id}/fiyat`** (StokEndpoints.cs):
   - Stok detay sayfasında aktif fiyat sorgulama
   - Fiyat liste numarası ve tarih parametreleri ile filtreleme
   
2. **`POST /api/sepet/donustur`** (SepetEndpoints.cs):
   - Sepeti teklife dönüştürürken varsayılan fiyat bulma
   - Sepet kaleminde `HedefFiyat` yoksa aktif fiyat sorgulanır
   - Kod: `var bf = s.HedefFiyat ?? (await FiyatServisi.GetAktifFiyatAsync(db, s.StokId, null, null)) ?? 0m;`

**Not**: Bu servis `StokFiyat` tablosunu sorgular. `TedarikciFiyat` tablosu farklı amaçla kullanılır (maliyet karşılaştırması).

#### DTO (Data Transfer Object) Pattern
**Ne?** Entity'lerin API'ye dönüştürülmüş versiyonları.

**Neden?**
- Güvenlik: Entity'lerin tüm alanları expose edilmez
- Performans: Sadece gerekli alanlar döner
- Versiyonlama: API değişiklikleri entity'leri etkilemez

**Örnek:**
```csharp
public record TeklifDto(int Id, string No, int CariId, DateTime Tarih, string Durum, decimal AraToplam, decimal IskontoToplam, decimal KdvToplam, decimal GenelToplam);
```

#### Paged Response Pattern
**Ne?** Sayfalanmış listeler için standart response formatı.

**Yapı:**
```csharp
public record Paged<T>(IEnumerable<T> Items, int TotalCount);
```

**Kullanım:**
```csharp
var items = await qry.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
var total = await qry.CountAsync();
return Results.Ok(new Paged<Dto>(items, total));
```

#### Error Handling
**Ne?** Hata durumlarında standart response formatı.

**Format:**
```csharp
Results.BadRequest(new { error = "Hata mesajı" });
Results.NotFound(new { error = "Kayıt bulunamadı" });
Results.Conflict(new { error = "Çakışma mesajı" });
```

### Frontend Teknik Detaylar

#### Service Pattern
**Ne?** Her feature için ayrı service sınıfı.

**Neden?**
- Separation of concerns
- Tekrar kullanılabilirlik
- Test edilebilirlik

**Örnek:**
```typescript
@Injectable({ providedIn: 'root' })
export class CariService {
    private base = environment.apiBase + '/cari';
    constructor(private http: HttpClient) {}
    list(p: any) { return this.http.get(`${this.base}`, { params }); }
    get(id: number) { return this.http.get(`${this.base}/${id}`); }
    // ...
}
```

#### RxJS Observable Pattern
**Ne?** Asenkron işlemler için Observable kullanımı.

**Kullanım:**
```typescript
this.cariService.list({ page: 1, pageSize: 20 }).subscribe({
    next: (res) => { /* başarılı */ },
    error: (err) => { /* hata */ }
});
```

**firstValueFrom**: Promise'e dönüştürme:
```typescript
const res = await firstValueFrom(this.cariService.list({ page: 1 }));
```

#### DevExtreme DataSource
**Ne?** DevExtreme DataGrid için veri kaynağı.

**Kullanım:**
```typescript
this.ds = new DataSource({
    load: async (loadOptions) => {
        const page = (loadOptions.skip ?? 0) / (loadOptions.take ?? 20) + 1;
        const res: any = await firstValueFrom(this.service.list({ page, pageSize: loadOptions.take }));
        return { data: res.items, totalCount: res.totalCount };
    }
});
```

#### Route Guards
**Ne?** Route erişim kontrolü.

**AuthGuard:**
```typescript
export const AuthGuard: CanActivateFn = (route, state) => {
    const token = localStorage.getItem('token');
    if (!token) return router.parseUrl('/login');
    const requiredRoles = route.data?.['roles'];
    if (requiredRoles && !requiredRoles.includes(userRole)) {
        return router.parseUrl('/login');
    }
    return true;
};
```

#### SSR (Server-Side Rendering) Desteği
**Ne?** Angular Universal ile server-side rendering.

**Neden?**
- SEO: Arama motorları içeriği görebilir
- İlk yükleme hızı: HTML server'dan gelir

**Nasıl?**
```typescript
isBrowser = isPlatformBrowser(this.platformId);
*ngIf="isBrowser" // Browser-only kodlar
```

#### Environment Configuration
**Ne?** Ortam bazlı yapılandırma.

**Kullanım:**
```typescript
// environment.ts
export const environment = {
    apiBase: 'http://localhost:5043/api'
};
```

### Veritabanı Teknik Detaylar

#### Index Stratejisi
**Ne?** Sorgu performansı için index'ler.

**Hangi Alanlar Index'li?**
- `Users.Email` (unique)
- `Users.UserCode` (unique)
- `Cariler.Kod` (unique)
- `Stoklar.Kod` (unique)
- `Teklifler.No` (unique)
- `TeklifSepet.UserId` (non-unique, sık sorgulanan)
- `TedarikciFiyatlar.StokId, FiyatListeNo, GuncellemeTarihi` (composite)

**Neden?**
- Unique constraint'ler için
- Sık sorgulanan alanlar için
- Join performansı için

#### Foreign Key Constraints
**Ne?** İlişkisel bütünlük için foreign key'ler.

**Örnekler:**
- `TeklifKalem.TeklifId` → `Teklif.Id` (Cascade: Teklif silinince kalemler de silinir)
- `Teklif.CariId` → `Cari.Id` (Restrict: Cari silinemez, teklif varsa)
- `CariAdres.CariId` → `Cari.Id` (Cascade: Cari silinince adresler de silinir)

#### Decimal Precision
**Ne?** Para tutarları için hassasiyet.

**Yapılandırma:**
```csharp
modelBuilder.Entity<TeklifKalem>()
    .Property(x => x.Miktar)
    .HasPrecision(18, 2); // 18 digit, 2 decimal
```

**Neden 18,2?**
- 18 digit: Çok büyük sayılar için yeterli
- 2 decimal: Para birimleri için yeterli (TL, USD, EUR)

---

## Sonuç

Bu proje, modern yazılım geliştirme prensipleri kullanılarak geliştirilmiş, ölçeklenebilir ve bakımı kolay bir teklif yönetim sistemidir. Katmanlı mimari, güvenlik önlemleri, audit logging ve kullanıcı dostu arayüz ile işletmelerin teklif süreçlerini dijitalleştirmesine olanak sağlar.

### Öne Çıkan Özellikler
- ✅ JWT token tabanlı güvenli authentication
- ✅ Role-based authorization
- ✅ Transaction yönetimi ile veri bütünlüğü
- ✅ Audit logging ile değişiklik takibi
- ✅ Müşteri self-service onay sistemi
- ✅ Tedarikçi fiyat karşılaştırması
- ✅ Modern Angular + DevExtreme UI
- ✅ RESTful API ile backend-frontend ayrımı

### Gelecek Geliştirmeler
- 📧 Email bildirimleri (teklif gönderildiğinde, onaylandığında)
- 📊 Gelişmiş raporlama ve dashboard grafikleri
- 📱 Mobil uygulama desteği
- 🔍 Gelişmiş arama ve filtreleme
- 📄 PDF teklif oluşturma
- 🔄 Teklif versiyonlama (revizyon geçmişi)

---

**Hazırlayan**: AI Assistant  
**Tarih**: 2025  
**Versiyon**: 1.0

