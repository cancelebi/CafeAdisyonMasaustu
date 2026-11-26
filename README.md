# Cafe Adisyon Masaüstü

C# Windows Forms (.NET 10) ve MySQL kullanılarak geliştirilmiş masaüstü cafe adisyon, sipariş, ürün, personel ve kasa takip uygulaması.

## Özellikler

- **Rol Bazlı Giriş Sistemi (`Admin` / `Personel`):**
  - **Admin:** Tüm sekmelere erişebilir (Masalar & Sipariş, Masa Yönetimi, Ürün Yönetimi, Garson Yönetimi, Profilim, Kasa Raporu).
  - **Personel:** Sipariş alma, masa yönetimi, ürün listeleme/ekleme/silme ve kendi profilini güncelleme işlemlerini yapabilir.
- **Masalar & Sipariş Takibi:** Dolu/boş masa durumlarını renklendirme (kırmızı/yeşil), adisyona ürün ekleme/azaltma ve hesap kapatma.
- **Kasa Raporu:** Kapatılan masaların tahsilat geçmişini listeleme, toplam ciro hesaplama ve gün sonu kasa sıfırlama.
- **Görsel Desteği:** Ürünler ve personel profilleri için yerel dosya sisteminden fotoğraf seçme ve önizleme.
- **Otomatik Veritabanı Kurulumu:** Uygulama ilk açılışta `cafe_adisyon_db` veritabanını, tabloları ve örnek verileri otomatik oluşturur.

## Gereksinimler

- [.NET 10 SDK](https://dotnet.microsoft.com/) (Windows)
- MySQL / MariaDB (örneğin **XAMPP** üzerinde `localhost:3306`)

## Kurulum ve Çalıştırma

1. **XAMPP Control Panel** üzerinden **MySQL** servisini başlatın (`localhost:3306`, kullanıcı: `root`, şifre: boş).
2. `CafeAdisyonMasaustu.slnx` dosyasını Visual Studio ile açıp çalıştırın veya terminalden şu komutu kullanın:
   ```bash
   dotnet run --project CafeAdisyonMasaustu/CafeAdisyonMasaustu.csproj
   ```
3. *(Opsiyonel)* "Şifremi Unuttum" e-posta gönderimi için ortam değişkenlerini tanımlayabilirsiniz:
   - `CAFE_SMTP_EMAIL`
   - `CAFE_SMTP_PASSWORD` (Gmail Uygulama Şifresi)

## Varsayılan Giriş Bilgileri

| Rol | Kullanıcı Adı | Şifre |
| --- | --- | --- |
| Admin | `admin` | `1234` |
| Personel | `garson` | `1234` |
