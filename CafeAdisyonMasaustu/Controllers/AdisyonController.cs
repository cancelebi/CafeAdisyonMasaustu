using CafeAdisyonMasaustu.Models;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;

namespace CafeAdisyonMasaustu.Controllers
{
    public class AdisyonController
    {
        private readonly string _sunucuBaglanti = "Server=localhost;Port=3306;Uid=root;Pwd=;";
        private readonly string _dbBaglanti = "Server=localhost;Port=3306;Database=cafe_adisyon_db;Uid=root;Pwd=;";

        private readonly string _gonderenMail = Environment.GetEnvironmentVariable("CAFE_SMTP_EMAIL") ?? "ornek@gmail.com";
        private readonly string _uygulamaSifresi = Environment.GetEnvironmentVariable("CAFE_SMTP_PASSWORD") ?? "UYGULAMA_SIFRENIZ";

        public static Kullanici? AktifKullanici { get; set; }

        public string UserImagesDir => Path.Combine(Application.StartupPath, "Images", "Users");
        public string ProductImagesDir => Path.Combine(Application.StartupPath, "Images", "Products");

        public AdisyonController()
        {
            Directory.CreateDirectory(UserImagesDir);
            Directory.CreateDirectory(ProductImagesDir);
            VeritabaniHazirla();
        }

        private void VeritabaniHazirla()
        {
            using (var conn = new MySqlConnection(_sunucuBaglanti))
            {
                conn.Open();
                new MySqlCommand("CREATE DATABASE IF NOT EXISTS cafe_adisyon_db CHARACTER SET utf8mb4 COLLATE utf8mb4_turkish_ci;", conn).ExecuteNonQuery();
            }

            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                conn.Open();

                string[] tablolar = {
                    @"CREATE TABLE IF NOT EXISTS Kullanicilar (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        KullaniciAdi VARCHAR(50) UNIQUE,
                        Sifre VARCHAR(50),
                        AdSoyad VARCHAR(100),
                        Email VARCHAR(100) DEFAULT '',
                        Rol VARCHAR(20),
                        FotoYolu VARCHAR(255) DEFAULT ''
                    );",
                    @"CREATE TABLE IF NOT EXISTS Urunler (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        UrunAdi VARCHAR(100),
                        Kategori VARCHAR(50),
                        Fiyat DECIMAL(10,2),
                        ResimYolu VARCHAR(255) DEFAULT ''
                    );",
                    @"CREATE TABLE IF NOT EXISTS Masalar (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        MasaAdi VARCHAR(50) UNIQUE
                    );",
                    @"CREATE TABLE IF NOT EXISTS Siparisler (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        MasaAdi VARCHAR(50),
                        UrunAdi VARCHAR(100),
                        Adet INT,
                        BirimFiyat DECIMAL(10,2)
                    );",
                    @"CREATE TABLE IF NOT EXISTS KasaHareketleri (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        MasaAdi VARCHAR(50),
                        Tutar DECIMAL(10,2),
                        Tarih DATETIME
                    );"
                };

                foreach (var sql in tablolar)
                    new MySqlCommand(sql, conn).ExecuteNonQuery();

                string[] eksikSutunlar = {
                    "ALTER TABLE Kullanicilar ADD COLUMN Email VARCHAR(100) DEFAULT '';",
                    "ALTER TABLE Kullanicilar ADD COLUMN FotoYolu VARCHAR(255) DEFAULT '';",
                    "ALTER TABLE Urunler ADD COLUMN ResimYolu VARCHAR(255) DEFAULT '';"
                };

                foreach (var alterSql in eksikSutunlar)
                {
                    try { new MySqlCommand(alterSql, conn).ExecuteNonQuery(); }
                    catch { }
                }

                if (Convert.ToInt32(new MySqlCommand("SELECT COUNT(*) FROM Kullanicilar", conn).ExecuteScalar()) == 0)
                {
                    new MySqlCommand(@"INSERT INTO Kullanicilar (KullaniciAdi, Sifre, AdSoyad, Email, Rol, FotoYolu) VALUES 
                        ('admin', '1234', 'İşletme Müdürü', 'admin@cafeadisyon.local', 'Admin', ''),
                        ('garson', '1234', 'Kasa Personeli', 'garson@cafeadisyon.local', 'Personel', '')", conn).ExecuteNonQuery();
                }

                if (Convert.ToInt32(new MySqlCommand("SELECT COUNT(*) FROM Masalar", conn).ExecuteScalar()) == 0)
                {
                    new MySqlCommand(@"INSERT INTO Masalar (MasaAdi) VALUES 
                        ('Masa 1'), ('Masa 2'), ('Masa 3'), ('Masa 4'), ('Masa 5'), ('Masa 6')", conn).ExecuteNonQuery();
                }

                if (Convert.ToInt32(new MySqlCommand("SELECT COUNT(*) FROM Urunler", conn).ExecuteScalar()) == 0)
                {
                    new MySqlCommand(@"INSERT INTO Urunler (UrunAdi, Kategori, Fiyat, ResimYolu) VALUES 
                        ('Kaşarlı Tost', 'Tost & Atıştırmalık', 95.00, ''),
                        ('Karışık Tost', 'Tost & Atıştırmalık', 115.00, ''),
                        ('Klasik Burger', 'Ana Yemek', 180.00, ''),
                        ('Çay', 'Sıcak İçecek', 20.00, ''),
                        ('Türk Kahvesi', 'Sıcak İçecek', 55.00, ''),
                        ('Ayran', 'İçecek', 30.00, '')", conn).ExecuteNonQuery();
                }
            }
        }

        public string ResmiProjeyeKopyala(string kaynakYol, bool isUser)
        {
            if (string.IsNullOrEmpty(kaynakYol) || !File.Exists(kaynakYol)) return "";
            string klasor = isUser ? UserImagesDir : ProductImagesDir;
            string dosyaAdi = Guid.NewGuid().ToString("N") + Path.GetExtension(kaynakYol);
            File.Copy(kaynakYol, Path.Combine(klasor, dosyaAdi), true);
            return Path.Combine("Images", isUser ? "Users" : "Products", dosyaAdi);
        }

        public Image? ResimYukle(string? goreliYol)
        {
            if (string.IsNullOrEmpty(goreliYol)) return null;
            string tamYol = Path.Combine(Application.StartupPath, goreliYol);
            if (!File.Exists(tamYol)) return null;
            using (var fs = new FileStream(tamYol, FileMode.Open, FileAccess.Read))
                return Image.FromStream(fs);
        }

        public bool GirisYap(string kadi, string sifre)
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT * FROM Kullanicilar WHERE KullaniciAdi=@k AND Sifre=@s LIMIT 1", conn))
                {
                    cmd.Parameters.AddWithValue("@k", kadi);
                    cmd.Parameters.AddWithValue("@s", sifre);
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            AktifKullanici = new Kullanici
                            {
                                Id = Convert.ToInt32(rdr["Id"]),
                                KullaniciAdi = rdr["KullaniciAdi"].ToString() ?? "",
                                Sifre = rdr["Sifre"].ToString() ?? "",
                                AdSoyad = rdr["AdSoyad"].ToString() ?? "",
                                Email = rdr["Email"].ToString() ?? "",
                                Rol = rdr["Rol"].ToString() ?? "",
                                FotoYolu = rdr["FotoYolu"].ToString() ?? ""
                            };
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool SifremiUnuttumMailGonder(string kullaniciAdi, string aliciMail)
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT Sifre, AdSoyad FROM Kullanicilar WHERE KullaniciAdi=@k LIMIT 1", conn))
                {
                    cmd.Parameters.AddWithValue("@k", kullaniciAdi);
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (!rdr.Read()) return false;

                        string sifre = rdr["Sifre"]?.ToString() ?? "";
                        string adSoyad = rdr["AdSoyad"]?.ToString() ?? kullaniciAdi;

                        var smtp = new SmtpClient("smtp.gmail.com", 587)
                        {
                            Credentials = new NetworkCredential(_gonderenMail, _uygulamaSifresi),
                            EnableSsl = true
                        };

                        var mail = new MailMessage(_gonderenMail, aliciMail)
                        {
                            Subject = "Cafe Adisyon - Şifre Hatırlatma",
                            Body = $"Merhaba {adSoyad},\n\n{kullaniciAdi} hesabı için giriş şifreniz: {sifre}"
                        };

                        smtp.Send(mail);
                        return true;
                    }
                }
            }
        }

        public void KendiProfiliniGuncelle(int id, string yeniSifre, string yeniFoto)
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("UPDATE Kullanicilar SET Sifre=@s, FotoYolu=@f WHERE Id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@s", yeniSifre);
                    cmd.Parameters.AddWithValue("@f", yeniFoto ?? "");
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }

            if (AktifKullanici != null && AktifKullanici.Id == id)
            {
                AktifKullanici.Sifre = yeniSifre;
                AktifKullanici.FotoYolu = yeniFoto ?? "";
            }
        }

        public DataTable PersonelleriGetir()
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                DataTable dt = new DataTable();
                new MySqlDataAdapter("SELECT Id, KullaniciAdi, Sifre, AdSoyad, Rol, FotoYolu FROM Kullanicilar", conn).Fill(dt);
                return dt;
            }
        }

        public void PersonelEkle(string kadi, string sifre, string ad, string rol, string foto)
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("INSERT INTO Kullanicilar (KullaniciAdi, Sifre, AdSoyad, Rol, FotoYolu) VALUES (@k,@s,@a,@r,@f)", conn))
                {
                    cmd.Parameters.AddWithValue("@k", kadi);
                    cmd.Parameters.AddWithValue("@s", sifre);
                    cmd.Parameters.AddWithValue("@a", ad);
                    cmd.Parameters.AddWithValue("@r", rol);
                    cmd.Parameters.AddWithValue("@f", foto ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void PersonelGuncelle(int id, string kadi, string sifre, string ad, string rol, string foto)
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("UPDATE Kullanicilar SET KullaniciAdi=@k, Sifre=@s, AdSoyad=@a, Rol=@r, FotoYolu=@f WHERE Id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@k", kadi);
                    cmd.Parameters.AddWithValue("@s", sifre);
                    cmd.Parameters.AddWithValue("@a", ad);
                    cmd.Parameters.AddWithValue("@r", rol);
                    cmd.Parameters.AddWithValue("@f", foto ?? "");
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void PersonelSil(int id)
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("DELETE FROM Kullanicilar WHERE Id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public DataTable MasalariGetir()
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                DataTable dt = new DataTable();
                new MySqlDataAdapter("SELECT Id, MasaAdi FROM Masalar ORDER BY Id ASC", conn).Fill(dt);
                return dt;
            }
        }

        public void MasaEkle(string masaAdi)
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("INSERT INTO Masalar (MasaAdi) VALUES (@a)", conn))
                {
                    cmd.Parameters.AddWithValue("@a", masaAdi);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MasaGuncelle(int id, string eskiAd, string yeniAd)
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("UPDATE Masalar SET MasaAdi=@y WHERE Id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@y", yeniAd);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd2 = new MySqlCommand("UPDATE Siparisler SET MasaAdi=@y WHERE MasaAdi=@e", conn))
                {
                    cmd2.Parameters.AddWithValue("@y", yeniAd);
                    cmd2.Parameters.AddWithValue("@e", eskiAd);
                    cmd2.ExecuteNonQuery();
                }
            }
        }

        public void MasaSil(int id)
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("DELETE FROM Masalar WHERE Id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public DataTable UrunleriGetir()
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                DataTable dt = new DataTable();
                new MySqlDataAdapter("SELECT Id, UrunAdi, Kategori, Fiyat, ResimYolu FROM Urunler ORDER BY Kategori, UrunAdi", conn).Fill(dt);
                return dt;
            }
        }

        public void UrunEkle(string ad, string kat, decimal fiyat, string resim)
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("INSERT INTO Urunler (UrunAdi, Kategori, Fiyat, ResimYolu) VALUES (@a,@k,@f,@r)", conn))
                {
                    cmd.Parameters.AddWithValue("@a", ad);
                    cmd.Parameters.AddWithValue("@k", kat);
                    cmd.Parameters.AddWithValue("@f", fiyat);
                    cmd.Parameters.AddWithValue("@r", resim ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UrunGuncelle(int id, string ad, string kat, decimal fiyat, string resim)
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("UPDATE Urunler SET UrunAdi=@a, Kategori=@k, Fiyat=@f, ResimYolu=@r WHERE Id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@a", ad);
                    cmd.Parameters.AddWithValue("@k", kat);
                    cmd.Parameters.AddWithValue("@f", fiyat);
                    cmd.Parameters.AddWithValue("@r", resim ?? "");
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UrunSil(int id)
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("DELETE FROM Urunler WHERE Id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool MasaDoluMu(string masaAdi)
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM Siparisler WHERE MasaAdi=@m", conn))
                {
                    cmd.Parameters.AddWithValue("@m", masaAdi);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public DataTable MasaSiparisleriniGetir(string masaAdi)
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                using (var cmd = new MySqlCommand("SELECT Id, UrunAdi, Adet, BirimFiyat, (Adet * BirimFiyat) AS Toplam FROM Siparisler WHERE MasaAdi=@m", conn))
                {
                    cmd.Parameters.AddWithValue("@m", masaAdi);
                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);
                    return dt;
                }
            }
        }

        public void SiparisEkle(string masaAdi, string urunAdi, decimal fiyat)
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                conn.Open();
                using (var cmdKontrol = new MySqlCommand("SELECT Id FROM Siparisler WHERE MasaAdi=@m AND UrunAdi=@u LIMIT 1", conn))
                {
                    cmdKontrol.Parameters.AddWithValue("@m", masaAdi);
                    cmdKontrol.Parameters.AddWithValue("@u", urunAdi);
                    object? mevcutId = cmdKontrol.ExecuteScalar();

                    if (mevcutId != null)
                    {
                        using (var cmdUpd = new MySqlCommand("UPDATE Siparisler SET Adet = Adet + 1 WHERE Id=@id", conn))
                        {
                            cmdUpd.Parameters.AddWithValue("@id", mevcutId);
                            cmdUpd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var cmdIns = new MySqlCommand("INSERT INTO Siparisler (MasaAdi, UrunAdi, Adet, BirimFiyat) VALUES (@m,@u,1,@f)", conn))
                        {
                            cmdIns.Parameters.AddWithValue("@m", masaAdi);
                            cmdIns.Parameters.AddWithValue("@u", urunAdi);
                            cmdIns.Parameters.AddWithValue("@f", fiyat);
                            cmdIns.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        // Seçili sipariş satırının adetini 1 azaltır; 1 ise kaydı tamamen siler
        public void SiparisSil(int siparisId)
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                conn.Open();
                int mevcutAdet = 0;
                using (var cmdAdet = new MySqlCommand("SELECT Adet FROM Siparisler WHERE Id=@id LIMIT 1", conn))
                {
                    cmdAdet.Parameters.AddWithValue("@id", siparisId);
                    object? adetObj = cmdAdet.ExecuteScalar();
                    if (adetObj == null) return;
                    mevcutAdet = Convert.ToInt32(adetObj);
                }

                if (mevcutAdet > 1)
                {
                    using (var cmdAzalt = new MySqlCommand("UPDATE Siparisler SET Adet = Adet - 1 WHERE Id=@id", conn))
                    {
                        cmdAzalt.Parameters.AddWithValue("@id", siparisId);
                        cmdAzalt.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (var cmdSil = new MySqlCommand("DELETE FROM Siparisler WHERE Id=@id", conn))
                    {
                        cmdSil.Parameters.AddWithValue("@id", siparisId);
                        cmdSil.ExecuteNonQuery();
                    }
                }
            }
        }

        public void HesapKapat(string masaAdi, decimal toplamTutar)
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                conn.Open();
                if (toplamTutar > 0)
                {
                    using (var cmdKasa = new MySqlCommand("INSERT INTO KasaHareketleri (MasaAdi, Tutar, Tarih) VALUES (@m,@t,NOW())", conn))
                    {
                        cmdKasa.Parameters.AddWithValue("@m", masaAdi);
                        cmdKasa.Parameters.AddWithValue("@t", toplamTutar);
                        cmdKasa.ExecuteNonQuery();
                    }
                }
                using (var cmdSil = new MySqlCommand("DELETE FROM Siparisler WHERE MasaAdi=@m", conn))
                {
                    cmdSil.Parameters.AddWithValue("@m", masaAdi);
                    cmdSil.ExecuteNonQuery();
                }
            }
        }

        public DataTable KasaHareketleriniGetir()
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                DataTable dt = new DataTable();
                new MySqlDataAdapter("SELECT Id, MasaAdi, Tutar, Tarih FROM KasaHareketleri ORDER BY Tarih DESC", conn).Fill(dt);
                return dt;
            }
        }

        public void KasayiSifirla()
        {
            using (var conn = new MySqlConnection(_dbBaglanti))
            {
                conn.Open();
                new MySqlCommand("DELETE FROM KasaHareketleri;", conn).ExecuteNonQuery();
            }
        }
    }
}