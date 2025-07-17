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

7. **Uygulamayı çalıştırın:**
   ```bash
   dotnet run --urls="http://localhost:5000"
   ```

8. **Uygulamaya erişin:**
   - Ana sayfa: `http://localhost:5000/`
   - Ziyaretçi kayıt: `http://localhost:5000/visitor.html`
   - Giriş sayfası: `http://localhost:5000/login.html`
   - Admin paneli: `http://localhost:5000/admin.html`

## Varsayılan Giriş Bilgileri

- **Kullanıcı Adı**: admin
- **Şifre**: admin123
- **Rol**: Admin

> **Not**: İlk çalıştırmada admin kullanıcısı otomatik olarak oluşturulur.

## Kullanım

### Ziyaretçi Kaydı
1. `visitor.html` sayfasından "Ziyaretçi Kaydı" sekmesini seçin
2. **Zorunlu bilgileri girin:**
   - Ana Blok/Alt Blok/Daire No
   - Daire Sahibi Adı Soyadı
   - Daire Sahibi Telefon Numarası
   - Ziyaretçi Adı Soyadı
3. **İsteğe bağlı bilgiler:**
   - Ziyaretçi Telefon Numarası
   - Araç Plakası
   - Ziyaret Nedeni
   - Fotoğraf
4. "Ziyaretçi Kaydı Yap" butonuna tıklayın

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
   - Raporlama ve Excel export
   - Filtreleme ve arama

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
- `PUT /api/user/{id}` - Kullanıcı güncelle (ADMIN)
- `DELETE /api/user/{id}` - Kullanıcı sil (ADMIN)

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
- **Bilgiler**: Id, FullName, ApartmentNumber, PhoneNumber, Email
- **Durum**: IsActive, CreatedAt

## Yeni Özellikler (v2.0)

### ✨ Güncellemeler
- **Mobil Interface**: Tab-based navigation (Ziyaretçi Kaydı / Aktif Ziyaretçiler)
- **Detaylı Bilgi Toplama**: Daire sahibi adı ve ziyaretçi telefonu eklendi
- **Gelişmiş UI**: Bootstrap 5 ile responsive tasarım
- **Foto Link**: Ziyaretçi fotoğraflarına direkt erişim
- **Real-time Updates**: Otomatik 30 saniye yenileme
- **Enhanced Admin Panel**: Daha detaylı ziyaretçi bilgileri

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
- **Admin**: Tüm işlemler (kullanıcı yönetimi, silme)
- **Manager**: Raporlama ve ziyaretçi yönetimi
- **Security**: Sadece ziyaretçi kaydı ve görüntüleme

## Güvenlik Notları

1. **Production ortamında:**
   - JWT anahtarını mutlaka değiştirin
   - HTTPS kullanın
   - Güçlü şifreler kullanın
   - SMS API anahtarlarını güvenli saklayın

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

**Versiyon**: 2.0  
**Son Güncelleme**: 2025-07-15  
**Geliştirici**: Claude Code Assistant