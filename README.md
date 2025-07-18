# Ziyaretçi Yönetim Sistemi

Site içi güvenliği artırmak ve giriş-çıkışları dijital olarak kayıt altına almak amacıyla geliştirilmiş kapsamlı bir web uygulamasıdır.

## Özellikler

### Mobil/Tablet Arayüzü
- ✅ **Tab-based Interface**: Mobil dostu sekmeli arayüz
- ✅ **Ziyaretçi Kaydı**: Ana/Alt blok ve daire no'ya göre kayıt
- ✅ **Detaylı Bilgi Toplama**: Daire sahibi adı, telefon numarası
- ✅ **Ziyaretçi Telefonu**: Ziyaretçi iletişim bilgileri
- ✅ **Araç Takibi**: Plaka kayıt sistemi
- ✅ **Mobil Fotoğraf**: Kamera ile ziyaretçi fotoğrafı
- ✅ **SMS Bildirimi**: Otomatik daire sahibi bilgilendirme
- ✅ **Responsive Design**: Bootstrap 5 tabanlı
- ✅ **Aktif Ziyaretçiler**: Gerçek zamanlı takip ve çıkış işlemi

### Web Tabanlı Yönetici Paneli
- ✅ **Dashboard**: İstatistiksel özet ve KPI'lar
- ✅ **Ziyaretçi Yönetimi**: Geçmiş kayıt takibi ve filtreleme
- ✅ **Raporlama**: Excel/CSV formatında detaylı raporlar
- ✅ **Kullanıcı Yönetimi**: Rol bazlı yetkilendirme sistemi
- ✅ **Şifre Yönetimi**: Admin tarafından merkezi şifre değiştirme
- ✅ **Daire Sahipleri Modülü**: Kapsamlı sakin yönetimi
- ✅ **Mail Sistemi**: SMTP entegrasyonu ve test maili
- ✅ **Fotoğraf Görüntüleme**: Ziyaretçi fotoğraflarına erişim
- ✅ **Audit Log**: Tüm işlemlerin kayıt altına alınması

### Güvenlik
- ✅ **JWT Authentication**: Token tabanlı kimlik doğrulama
- ✅ **Role-based Authorization**: Admin/Manager/Security rolleri
- ✅ **BCrypt Encryption**: Güvenli şifre saklama
- ✅ **API Security**: CORS ve endpoint koruması
- ✅ **Input Validation**: Veri doğrulama ve sanitizasyon

## Teknoloji Stack

- **Frontend**: Bootstrap 5, Vanilla JavaScript ES6
- **Backend**: C# ASP.NET Core 9.0
- **Database**: SQLite (Entity Framework Core)
- **Authentication**: JWT Bearer Token
- **SMS API**: Entegrasyon hazır (konfigürasyon gerekli)
- **Mail System**: SMTP protokolü ile e-posta gönderimi
- **Platform**: Cross-platform (Windows, Linux, macOS)

## Kurulum

### Gereksinimler
- .NET 9.0 SDK
- Git (proje klonlama için)
- Modern web tarayıcı

### Adımlar

1. **Projeyi klonlayın:**
   ```bash
   git clone <repository-url>
   cd VisitorManagementSystem
   ```

2. **Bağımlılıkları yükleyin:**
   ```bash
   dotnet restore
   ```

3. **Entity Framework araçlarını yükleyin:**
   ```bash
   dotnet tool install --global dotnet-ef
   ```

4. **Veritabanını oluşturun:**
   ```bash
   dotnet ef database update
   ```

5. **JWT ayarlarını yapılandırın:**
   `appsettings.json` dosyasında JWT anahtarını değiştirin:
   ```json
   "Jwt": {
     "Key": "YourSuperSecretKeyThatIsLongEnoughForHS256Encryption",
     "Issuer": "VisitorManagementSystem",
     "Audience": "VisitorManagementUsers"
   }
   ```

6. **SMS ayarlarını yapılandırın (isteğe bağlı):**
   ```json
   "SmsSettings": {
     "ApiKey": "your-sms-api-key",
     "ApiSecret": "your-sms-api-secret",
     "SenderName": "ZiyaretciSys"
   }
   ```

7. **Mail ayarlarını yapılandırın (isteğe bağlı):**
   Mail sistemi admin panelinden ayarlanabilir, ancak appsettings.json'da da tanımlanabilir:
   ```json
   "MailSettings": {
     "SenderName": "Ziyaretçi Yönetim Sistemi",
     "SenderEmail": "sistem@example.com",
     "SmtpServer": "smtp.gmail.com",
     "Port": 587,
     "Username": "kullanici@gmail.com",
     "Password": "uygulama-sifresi",
     "SecurityType": "TLS",
     "IsActive": false
   }
   ```

8. **Uygulamayı çalıştırın:**
   ```bash
   dotnet run --launch-profile http
   ```

8. **Uygulamaya erişin:**
   - Ana sayfa: `http://localhost:5002/`
   - Ziyaretçi kayıt: `http://localhost:5002/visitor.html`
   - Giriş sayfası: `http://localhost:5002/login.html`
   - Admin paneli: `http://localhost:5002/admin.html`

## Varsayılan Giriş Bilgileri

- **Kullanıcı Adı**: admin
- **Şifre**: admin123
- **Rol**: Admin

> **Not**: İlk çalıştırmada admin kullanıcısı otomatik olarak oluşturulur.

## Kullanım

### Ziyaretçi Kaydı
1. `visitor.html` sayfasından "Ziyaretçi Kaydı" sekmesini seçin
2. **Akıllı daire seçimi (YENİ!):**
   - Ana Blok (dropdown): A, B, C, D, E, F
   - Alt Blok (dropdown): 1, 2, 3, 4, 5, 6, 7, 8
   - Daire No: Rakam girin
   - **Otomatik doldurma**: Daire bilgileri girilince daire sahibi adı ve telefonu otomatik gelir
3. **Alternatif: Ad soyad arama (YENİ!):**
   - Daire Sahibi Adı alanına yazmaya başlayın (2+ karakter)
   - Canlı öneriler listesinden seçim yapın
   - **Otomatik doldurma**: Seçim yapılınca daire bilgileri ve telefon otomatik gelir
4. **Telefon arama (YENİ!):**
   - Telefon numarası gelince 📞 "Ara" butonu aktif olur
   - Tıklayarak doğrudan arama yapabilirsiniz
5. **Diğer bilgiler:**
   - Ziyaretçi Adı Soyadı (zorunlu)
   - Ziyaretçi Telefon Numarası (isteğe bağlı)
   - Araç Plakası (isteğe bağlı)
   - Ziyaret Nedeni (isteğe bağlı)
   - Fotoğraf (isteğe bağlı)
6. "Ziyaretçi Kaydı Yap" butonuna tıklayın

### Aktif Ziyaretçi Takibi
1. "Aktif Ziyaretçiler" sekmesinde tüm aktif ziyaretçileri görün
2. Her ziyaretçinin detaylı bilgilerini inceleyin
3. Yetkili kullanıcılar çıkış işlemi yapabilir

### Yönetici İşlemleri
1. `/login.html` sayfasından sisteme giriş yapın
2. Admin paneline otomatik yönlendirileceksiniz
3. **Mevcut özellikler:**
   - Ziyaretçi listesi ve yönetimi
   - İstatistiksel dashboard
   - Kullanıcı yönetimi (Admin)
   - Daire sahipleri yönetimi
   - Mail sistemi ayarları (Admin)
   - Raporlama ve Excel export
   - Filtreleme ve arama

### Şifre Yönetimi
1. **Admin panelinde** "Kullanıcılar" sekmesine gidin
2. Değiştirmek istediğiniz kullanıcının yanındaki **düzenle** butonuna tıklayın
3. "Şifre değiştir" checkbox'ını işaretleyin
4. Yeni şifreyi girin ve kaydedin
5. Kullanıcı yeni şifresi ile giriş yapabilir

### Mail Sistemi Ayarları
1. **Admin panelinde** "Ayarlar" sekmesine gidin
2. **Mail Ayarları** bölümünde SMTP bilgilerini girin:
   - Gönderen adı ve e-posta
   - SMTP sunucusu ve port
   - Kullanıcı adı ve şifre
   - Güvenlik türü (TLS/SSL)
3. **Bağlantıyı test edin**
4. **Test maili gönderin**
5. Ayarları kaydedin ve sistemi aktif edin

## API Endpoints

### Kimlik Doğrulama
- `POST /api/auth/login` - Giriş yap
- `POST /api/auth/change-password` - Şifre değiştir

### Ziyaretçi İşlemleri
- `GET /api/visitor` - Tüm ziyaretçileri listele (AUTH)
- `GET /api/visitor/active` - Aktif ziyaretçileri listele (PUBLIC)
- `POST /api/visitor` - Yeni ziyaretçi kaydet (PUBLIC)
- `POST /api/visitor/{id}/checkout` - Ziyaretçi çıkışı (AUTH)
- `POST /api/visitor/upload-photo` - Fotoğraf yükle (PUBLIC)
- `DELETE /api/visitor/{id}` - Ziyaretçi sil (ADMIN)

### Kullanıcı İşlemleri
- `GET /api/user` - Kullanıcıları listele (ADMIN)
- `POST /api/user` - Yeni kullanıcı oluştur (ADMIN)
- `PUT /api/user/{id}` - Kullanıcı güncelle/şifre değiştir (ADMIN)
- `DELETE /api/user/{id}` - Kullanıcı sil (ADMIN)

### Daire Sahipleri İşlemleri
- `GET /api/resident` - Daire sahiplerini listele (AUTH)
- `GET /api/resident/{id}` - Daire sahibi detayı (AUTH)
- `GET /api/resident/apartment/{apartmentNumber}` - **Daire ile sakin arama (PUBLIC)** 🆕
- `POST /api/resident/search` - **Daire sahibi arama/autocomplete (PUBLIC)** 🆕
- `POST /api/resident` - Yeni daire sahibi ekle (ADMIN)
- `PUT /api/resident/{id}` - Daire sahibi güncelle (ADMIN)
- `DELETE /api/resident/{id}` - Daire sahibi sil (ADMIN)
- `GET /api/resident/search/license/{plate}` - Plaka ile arama (AUTH)
- `POST /api/resident/import` - Excel içe aktarma (ADMIN)
- `GET /api/resident/export` - Excel dışa aktarma (ADMIN)

### Mail Sistemi İşlemleri
- `GET /api/mailsettings` - Mail ayarlarını getir (ADMIN)
- `POST /api/mailsettings` - Mail ayarlarını kaydet (ADMIN)
- `POST /api/mailsettings/test-connection` - SMTP bağlantı testi (ADMIN)
- `POST /api/mailsettings/send-test` - Test maili gönder (ADMIN)
- `POST /api/mailsettings/deactivate` - Mail sistemini devre dışı bırak (ADMIN)
- `GET /api/mailsettings/presets` - SMTP ön ayarları getir (ADMIN)

## Veritabanı Şeması

### Visitors
- **Temel Bilgiler**: Id, FullName, ApartmentNumber, LicensePlate, IdNumber
- **Zaman Bilgileri**: CheckInTime, CheckOutTime, CreatedAt
- **İletişim**: ResidentPhone, **ResidentName**, **VisitorPhone**
- **Medya**: PhotoPath
- **Durum**: IsActive, Notes, CreatedBy

### Users
- **Kimlik**: Id, Username, FullName, PasswordHash
- **Yetkilendirme**: Role (Admin/Manager/Security)
- **Durum**: IsActive, LastLogin, CreatedAt

### VisitorLogs
- **Audit**: Id, VisitorId, Action, Timestamp, Details, PerformedBy

### Residents
- **Bilgiler**: Id, FullName, ApartmentNumber, Block, SubBlock, DoorNumber
- **İletişim**: ResidentContacts (ayrı tablo)
- **Araçlar**: ResidentVehicles (ayrı tablo)
- **Durum**: IsActive, CreatedAt, Notes

### ResidentContacts
- **İletişim**: Id, ResidentId, ContactType, ContactValue, Label, Priority

### ResidentVehicles
- **Araç**: Id, ResidentId, LicensePlate, Brand, Model, Color, Year, VehicleType

### MailSettings
- **SMTP**: Id, SenderName, SenderEmail, SmtpServer, Port, Username, Password
- **Güvenlik**: SecurityType, IsActive, CreatedAt, UpdatedAt

## Yeni Özellikler (v2.2)

### 🚀 Son Güncellemeler (v2.2)
- **🏢 Akıllı Daire Sistemi**: Daire seçimi ile otomatik daire sahibi bilgisi getirme
- **🔍 Canlı Arama**: Ad soyad ile autocomplete özelliği (2+ karakter ile aktif)
- **📞 Telefon Entegrasyonu**: Daire sahibini doğrudan arama butonu (tel: protokolü)
- **↔️ İki Yönlü Bağlama**: Daire no veya ad soyad girilince diğerleri otomatik doldurulur
- **🎯 Hızlı Erişim**: Admin dropdown menü (Ziyaretçi sayfası + Çıkış)
- **🔐 API Güvenlik**: Ziyaretçi kaydı için anonymous endpoint'ler

### ✨ Önceki Güncellemeler (v2.0-2.1)
- **Mobil Interface**: Tab-based navigation (Ziyaretçi Kaydı / Aktif Ziyaretçiler)
- **Detaylı Bilgi Toplama**: Daire sahibi adı ve ziyaretçi telefonu eklendi
- **Gelişmiş UI**: Bootstrap 5 ile responsive tasarım
- **Foto Link**: Ziyaretçi fotoğraflarına direkt erişim
- **Real-time Updates**: Otomatik 30 saniye yenileme
- **Enhanced Admin Panel**: Daha detaylı ziyaretçi bilgileri
- **Daire Sahipleri Modülü**: Kapsamlı sakin yönetimi ve araç takibi
- **Mail Sistemi**: SMTP entegrasyonu, test maili ve ön ayarlar
- **Merkezi Şifre Yönetimi**: Admin tarafından kullanıcı şifre değiştirme
- **Rol Bazlı Erişim**: Manager rolü için sekme gizleme

### 🔧 Teknik İyileştirmeler
- **SQLite Database**: Hafif ve taşınabilir veritabanı
- **Entity Framework Core**: Code-first yaklaşım
- **JWT Authentication**: Güvenli token sistemi
- **CORS Support**: API erişim kontrolü
- **Input Validation**: Veri doğrulama ve sanitizasyon

## Özelleştirme

### SMS Sağlayıcı Entegrasyonu
`Services/SmsService.cs` dosyasında `SendSmsAsync` metodunu SMS sağlayıcınıza göre düzenleyin.

### Stil Değişiklikleri
`wwwroot/css/style.css` dosyasını düzenleyerek görünümü özelleştirebilirsiniz.

### Rol Bazlı Erişim
- **Admin**: Tüm işlemler (kullanıcı yönetimi, şifre değiştirme, mail ayarları, silme)
- **Manager**: Raporlama, ziyaretçi yönetimi ve daire sahipleri (kullanıcılar ve ayarlar gizli)
- **Security**: Sadece ziyaretçi kaydı ve görüntüleme

## Güvenlik Notları

1. **Production ortamında:**
   - JWT anahtarını mutlaka değiştirin
   - HTTPS kullanın
   - Güçlü şifreler kullanın
   - SMS API anahtarlarını güvenli saklayın
   - Mail şifrelerini (özellikle Gmail uygulama şifreleri) güvenli saklayın

2. **Yedekleme:**
   - `VisitorManagementDB.db` dosyasını düzenli yedekleyin
   - Fotoğraf klasörünü (`wwwroot/uploads/`) yedekleyin

3. **Güvenlik:**
   - Varsayılan admin şifresini değiştirin
   - Gereksiz kullanıcı hesaplarını silin
   - Audit log'ları düzenli kontrol edin

## Sorun Giderme

### Yaygın Sorunlar
1. **Database Error**: `dotnet ef database update` çalıştırın
2. **JWT Error**: `appsettings.json` JWT ayarlarını kontrol edin
3. **Photo Upload Error**: `wwwroot/uploads/photos/` klasörünün yazılabilir olduğundan emin olun
4. **CORS Error**: Browser cache'ini temizleyin

### Debug Modu
```bash
dotnet run --environment=Development
```

## Katkıda Bulunma

1. Projeyi fork edin
2. Yeni feature branch oluşturun
3. Değişikliklerinizi commit edin
4. Pull request gönderin

## Lisans

Bu proje özel kullanım için geliştirilmiştir.

---

**Versiyon**: 2.2  
**Son Güncelleme**: 2025-07-18  
**Geliştirici**: Claude Code Assistant

## Sistem Durumu

✅ **Aktif Port**: 5002  
✅ **Veritabanı**: SQLite - Hazır  
✅ **Admin Kullanıcı**: Otomatik oluşturuldu  
✅ **Static Files**: Aktif  
✅ **JWT Authentication**: Çalışıyor  
✅ **Mail Sistemi**: Yapılandırılabilir
✅ **Daire Sahipleri Modülü**: Aktif
✅ **Merkezi Şifre Yönetimi**: Aktif
✅ **🆕 Akıllı Daire Sistemi**: Aktif
✅ **🆕 Autocomplete Arama**: Aktif  
✅ **🆕 Telefon Entegrasyonu**: Aktif
✅ **🆕 Admin Dropdown**: Aktif

### Çalıştırma Komutu
```bash
dotnet run --launch-profile http
```

### Hızlı Erişim
- **Giriş**: http://localhost:5002/login.html
- **Ziyaretçi Kaydı**: http://localhost:5002/visitor.html
- **Admin Panel**: http://localhost:5002/admin.html

### 🎯 Yeni Özellikler Test Rehberi

#### Akıllı Daire Sistemi Test:
1. **http://localhost:5002/visitor.html** adresini açın
2. Ana Blok: **B** seçin
3. Alt Blok: **4** seçin  
4. Daire No: **10** yazın
5. **Otomatik**: Daire sahibi "Özlem Erdem" ve telefon "5057073802" gelecek
6. **📞 Ara butonu** aktif hale gelecek

#### Autocomplete Arama Test:
1. **Daire Sahibi Adı** alanını temizleyin
2. **"Özlem"** yazmaya başlayın
3. **Canlı öneriler** görünecek
4. Listeden seçim yapın
5. **Otomatik**: Tüm bilgiler (daire no, telefon) dolacak

#### Admin Dropdown Test:
1. **http://localhost:5002/login.html** - admin/admin123 ile giriş yapın
2. Admin panelinde kullanıcı adının yanında **dropdown oku** görünecek
3. Dropdown açılınca **Ziyaretçi** ve **Çıkış** seçenekleri olacak