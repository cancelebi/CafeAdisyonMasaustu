using CafeAdisyonMasaustu.Controllers;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CafeAdisyonMasaustu.Views
{
    public partial class AnaForm : Form
    {
        private readonly AdisyonController _controller = new AdisyonController();
        private string _seciliMasa = "";

        private PictureBox pbUstProfil = null!;
        private Label lblUstBilgi = null!;
        private TabControl tabControl = null!;

        // Sekme 1: Masalar & Sipariş
        private FlowLayoutPanel flpMasalar = null!;
        private DataGridView dgvMenuSecim = null!, dgvAdisyon = null!;
        private Label lblSeciliMasa = null!, lblToplamTutar = null!;

        // Sekme 2: Masa Yönetimi
        private DataGridView dgvMasalarYonetim = null!;
        private TextBox txtMasaAdi = null!;
        private int _seciliMasaId = 0;
        private string _eskiMasaAdi = "";

        // Sekme 3: Ürün Yönetimi
        private DataGridView dgvUrunYonetim = null!;
        private TextBox txtUrunAdi = null!, txtUrunFiyat = null!;
        private ComboBox cmbKategori = null!;
        private PictureBox pbUrunOnizleme = null!;
        private string _secilenUrunResimYolu = "";
        private int _seciliUrunId = 0;

        // Sekme 4: Garson Yönetimi (Sadece Admin)
        private DataGridView dgvPersonelYonetim = null!;
        private TextBox txtPerKadi = null!, txtPerSifre = null!, txtPerAdSoyad = null!;
        private ComboBox cmbPerRol = null!;
        private PictureBox pbPerOnizleme = null!;
        private string _secilenPerFotoYolu = "";
        private int _seciliPerId = 0;

        // Sekme 5: Kendi Profilim
        private PictureBox pbProfilFoto = null!;
        private TextBox txtProfilYeniSifre = null!;
        private string _secilenProfilYeniFoto = "";

        // Sekme 6: Kasa
        private DataGridView dgvKasa = null!;
        private Label lblToplamCiro = null!;

        public AnaForm()
        {
            ArayuzuKur();
            TumVerileriYenile();
        }

        private void ArayuzuKur()
        {
            var user = AdisyonController.AktifKullanici!;
            this.Text = $"Cafe Adisyon Masaüstü - [{user.AdSoyad} | {user.Rol}]";
            this.Size = new Size(1180, 720);
            this.StartPosition = FormStartPosition.CenterScreen;

            Panel pnlUst = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Color.FromArgb(30, 34, 43) };
            pbUstProfil = new PictureBox
            {
                Location = new Point(12, 6),
                Size = new Size(46, 46),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.DimGray,
                Image = _controller.ResimYukle(user.FotoYolu)
            };
            lblUstBilgi = new Label
            {
                Text = $"Hoş Geldiniz, {user.AdSoyad}  ({user.Rol})",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(68, 17),
                AutoSize = true
            };
            Button btnCikis = new Button
            {
                Text = "Oturumu Kapat",
                Width = 130,
                Height = 34,
                Location = new Point(1020, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.Crimson,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCikis.Click += (s, e) => this.Close();
            pnlUst.Controls.AddRange(new Control[] { pbUstProfil, lblUstBilgi, btnCikis });

            tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };

            TabPage tabSiparis = new TabPage("Masalar & Sipariş");
            TabPage tabMasaYonetim = new TabPage("Masa Ekle / Sil / Güncelle");
            TabPage tabUrunYonetim = new TabPage("Ürün Ekle / Sil / Güncelle");
            TabPage tabPersonel = new TabPage("Garson Yönetimi");
            TabPage tabProfil = new TabPage("Profilim (Foto & Şifre)");
            TabPage tabKasa = new TabPage("Kasa Raporu");

            SekmeSiparisKur(tabSiparis);
            SekmeMasaYonetimKur(tabMasaYonetim);
            SekmeUrunYonetimKur(tabUrunYonetim, user.Rol);
            SekmePersonelKur(tabPersonel);
            SekmeProfilKur(tabProfil);
            SekmeKasaKur(tabKasa);

            tabControl.TabPages.Add(tabSiparis);
            tabControl.TabPages.Add(tabMasaYonetim);
            tabControl.TabPages.Add(tabUrunYonetim);

            if (user.Rol == "Admin")
            {
                tabControl.TabPages.Add(tabPersonel);
                tabControl.TabPages.Add(tabProfil);
                tabControl.TabPages.Add(tabKasa);
            }
            else
            {
                tabControl.TabPages.Add(tabProfil);
            }

            this.Controls.Add(tabControl);
            this.Controls.Add(pnlUst);
        }

        private void MasayaSeciliUrunuEkle()
        {
            if (string.IsNullOrEmpty(_seciliMasa))
            {
                MessageBox.Show("Önce soldan bir masa seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (dgvMenuSecim.CurrentRow == null)
            {
                MessageBox.Show("Menüden bir ürün seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string urun = dgvMenuSecim.CurrentRow.Cells["UrunAdi"].Value?.ToString() ?? "";
            decimal fiyat = Convert.ToDecimal(dgvMenuSecim.CurrentRow.Cells["Fiyat"].Value);
            _controller.SiparisEkle(_seciliMasa, urun, fiyat);
            AdisyonYenile();
            MasalariCiz();
        }

        // 1. MASALAR & SİPARİŞ
        private void SekmeSiparisKur(TabPage tab)
        {
            flpMasalar = new FlowLayoutPanel
            {
                Location = new Point(10, 10),
                Size = new Size(320, 590),
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblMenu = new Label { Text = "Menü (Seçip Butona Bas veya Çift Tıkla)", Location = new Point(342, 10), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            dgvMenuSecim = new DataGridView
            {
                Location = new Point(342, 35),
                Size = new Size(415, 512),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 48 }
            };
            dgvMenuSecim.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                MasayaSeciliUrunuEkle();
            };

            Button btnMasayaEkle = new Button
            {
                Text = "MASAYA ÜRÜN EKLE",
                Location = new Point(342, 554),
                Size = new Size(415, 46),
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnMasayaEkle.Click += (s, e) => MasayaSeciliUrunuEkle();

            lblSeciliMasa = new Label { Text = "Seçili Masa: YOK", Location = new Point(770, 10), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.DarkBlue };
            dgvAdisyon = new DataGridView
            {
                Location = new Point(770, 35),
                Size = new Size(375, 410),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            Button btnSiparisSil = new Button
            {
                Text = "SİPARİŞİ SİL (-1 Adet)",
                Location = new Point(770, 452),
                Size = new Size(375, 38),
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSiparisSil.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(_seciliMasa))
                {
                    MessageBox.Show("Önce bir masa seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (dgvAdisyon.CurrentRow == null)
                {
                    MessageBox.Show("Adisyondan silinecek ürünü seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int siparisId = Convert.ToInt32(dgvAdisyon.CurrentRow.Cells["Id"].Value);
                _controller.SiparisSil(siparisId);
                AdisyonYenile();
                MasalariCiz();
            };

            lblToplamTutar = new Label { Text = "Toplam: 0,00 ₺", Location = new Point(770, 498), AutoSize = true, Font = new Font("Segoe UI", 13, FontStyle.Bold) };
            Button btnHesapKapat = new Button
            {
                Text = "HESABI KAPAT (ÖDEME AL)",
                Location = new Point(770, 535),
                Size = new Size(375, 65),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnHesapKapat.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(_seciliMasa)) return;
                decimal toplam = 0;
                foreach (DataGridViewRow r in dgvAdisyon.Rows) toplam += Convert.ToDecimal(r.Cells["Toplam"].Value);
                _controller.HesapKapat(_seciliMasa, toplam);
                MessageBox.Show($"{_seciliMasa} hesabı kapatıldı. Tahsilat: {toplam:C2}");
                AdisyonYenile();
                MasalariCiz();
                KasaYenile();
            };

            tab.Controls.AddRange(new Control[]
            {
                flpMasalar, lblMenu, dgvMenuSecim, btnMasayaEkle,
                lblSeciliMasa, dgvAdisyon, btnSiparisSil, lblToplamTutar, btnHesapKapat
            });
        }

        // 2. MASA YÖNETİMİ
        private void SekmeMasaYonetimKur(TabPage tab)
        {
            dgvMasalarYonetim = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(450, 550),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvMasalarYonetim.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                _seciliMasaId = Convert.ToInt32(dgvMasalarYonetim.Rows[e.RowIndex].Cells["Id"].Value);
                _eskiMasaAdi = dgvMasalarYonetim.Rows[e.RowIndex].Cells["MasaAdi"].Value?.ToString() ?? "";
                txtMasaAdi.Text = _eskiMasaAdi;
            };

            Label lbl = new Label { Text = "Masa Adı (Örn: Masa 7, Bahçe 1):", Location = new Point(500, 30), AutoSize = true };
            txtMasaAdi = new TextBox { Location = new Point(500, 55), Width = 260 };

            Button btnEkle = new Button { Text = "Masa Ekle", Location = new Point(500, 95), Width = 125, Height = 36, BackColor = Color.SeaGreen, ForeColor = Color.White };
            btnEkle.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtMasaAdi.Text)) return;
                try
                {
                    _controller.MasaEkle(txtMasaAdi.Text.Trim());
                    txtMasaAdi.Clear();
                    TumVerileriYenile();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.Contains("Duplicate") ? "Bu masa adı zaten mevcut!" : "Hata: " + ex.Message);
                }
            };

            Button btnGuncelle = new Button { Text = "Masa Güncelle", Location = new Point(635, 95), Width = 125, Height = 36, BackColor = Color.SteelBlue, ForeColor = Color.White };
            btnGuncelle.Click += (s, e) =>
            {
                if (_seciliMasaId == 0 || string.IsNullOrWhiteSpace(txtMasaAdi.Text)) return;
                try
                {
                    _controller.MasaGuncelle(_seciliMasaId, _eskiMasaAdi, txtMasaAdi.Text.Trim());
                    _seciliMasaId = 0;
                    txtMasaAdi.Clear();
                    TumVerileriYenile();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.Contains("Duplicate") ? "Bu masa adı zaten mevcut!" : "Hata: " + ex.Message);
                }
            };

            Button btnSil = new Button { Text = "Seçili Masayı Sil", Location = new Point(500, 142), Width = 260, Height = 36, BackColor = Color.IndianRed, ForeColor = Color.White };
            btnSil.Click += (s, e) =>
            {
                if (_seciliMasaId == 0) return;
                _controller.MasaSil(_seciliMasaId);
                _seciliMasaId = 0;
                txtMasaAdi.Clear();
                TumVerileriYenile();
            };

            tab.Controls.AddRange(new Control[] { dgvMasalarYonetim, lbl, txtMasaAdi, btnEkle, btnGuncelle, btnSil });
        }

        // 3. ÜRÜN YÖNETİMİ + RESİM
        private void SekmeUrunYonetimKur(TabPage tab, string rol)
        {
            dgvUrunYonetim = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(680, 560),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 52 }
            };
            dgvUrunYonetim.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var row = dgvUrunYonetim.Rows[e.RowIndex];
                _seciliUrunId = Convert.ToInt32(row.Cells["Id"].Value);
                txtUrunAdi.Text = row.Cells["UrunAdi"].Value?.ToString() ?? "";
                cmbKategori.Text = row.Cells["Kategori"].Value?.ToString() ?? "";
                txtUrunFiyat.Text = row.Cells["Fiyat"].Value?.ToString() ?? "";
                _secilenUrunResimYolu = row.Cells["ResimYolu"].Value?.ToString() ?? "";
                pbUrunOnizleme.Image = _controller.ResimYukle(_secilenUrunResimYolu);
            };

            Label l1 = new Label { Text = "Ürün Adı:", Location = new Point(730, 25), AutoSize = true };
            txtUrunAdi = new TextBox { Location = new Point(730, 48), Width = 250 };

            Label l2 = new Label { Text = "Kategori:", Location = new Point(730, 85), AutoSize = true };
            cmbKategori = new ComboBox { Location = new Point(730, 108), Width = 250 };
            cmbKategori.Items.AddRange(new string[] { "Ana Yemek", "Tost & Atıştırmalık", "Çorba", "Sıcak İçecek", "İçecek", "Tatlı" });

            Label l3 = new Label { Text = "Fiyat (₺):", Location = new Point(730, 145), AutoSize = true };
            txtUrunFiyat = new TextBox { Location = new Point(730, 168), Width = 250 };

            pbUrunOnizleme = new PictureBox { Location = new Point(730, 210), Size = new Size(120, 120), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
            Button btnResimSec = new Button { Text = "Ürün Resmi Seç", Location = new Point(860, 250), Width = 120, Height = 36 };
            btnResimSec.Click += (s, e) =>
            {
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Resimler|*.jpg;*.jpeg;*.png" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        _secilenUrunResimYolu = _controller.ResmiProjeyeKopyala(ofd.FileName, false);
                        pbUrunOnizleme.Image = _controller.ResimYukle(_secilenUrunResimYolu);
                    }
                }
            };

            Button btnEkle = new Button { Text = "Ürün Ekle", Location = new Point(730, 350), Width = 120, Height = 38, BackColor = Color.SeaGreen, ForeColor = Color.White };
            btnEkle.Click += (s, e) =>
            {
                if (!decimal.TryParse(txtUrunFiyat.Text, out decimal fiyat) || string.IsNullOrWhiteSpace(txtUrunAdi.Text)) return;
                _controller.UrunEkle(txtUrunAdi.Text.Trim(), cmbKategori.Text, fiyat, _secilenUrunResimYolu);
                UrunFormTemizle();
                TumVerileriYenile();
            };

            Button btnGuncelle = new Button { Text = "Güncelle", Location = new Point(860, 350), Width = 120, Height = 38, BackColor = Color.SteelBlue, ForeColor = Color.White };
            btnGuncelle.Click += (s, e) =>
            {
                if (_seciliUrunId == 0 || !decimal.TryParse(txtUrunFiyat.Text, out decimal fiyat)) return;
                _controller.UrunGuncelle(_seciliUrunId, txtUrunAdi.Text.Trim(), cmbKategori.Text, fiyat, _secilenUrunResimYolu);
                UrunFormTemizle();
                TumVerileriYenile();
            };

            Button btnSil = new Button { Text = "Seçili Ürünü Sil", Location = new Point(730, 398), Width = 250, Height = 38, BackColor = Color.IndianRed, ForeColor = Color.White };
            btnSil.Click += (s, e) =>
            {
                if (_seciliUrunId == 0) return;
                _controller.UrunSil(_seciliUrunId);
                UrunFormTemizle();
                TumVerileriYenile();
            };

            if (rol != "Admin") btnGuncelle.Visible = false;

            tab.Controls.AddRange(new Control[] { dgvUrunYonetim, l1, txtUrunAdi, l2, cmbKategori, l3, txtUrunFiyat, pbUrunOnizleme, btnResimSec, btnEkle, btnGuncelle, btnSil });
        }

        private void UrunFormTemizle()
        {
            _seciliUrunId = 0;
            txtUrunAdi.Clear();
            txtUrunFiyat.Clear();
            _secilenUrunResimYolu = "";
            pbUrunOnizleme.Image = null;
        }

        // 4. GARSON YÖNETİMİ + FOTO
        private void SekmePersonelKur(TabPage tab)
        {
            dgvPersonelYonetim = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(680, 560),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 52 }
            };
            dgvPersonelYonetim.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var row = dgvPersonelYonetim.Rows[e.RowIndex];
                _seciliPerId = Convert.ToInt32(row.Cells["Id"].Value);
                txtPerKadi.Text = row.Cells["KullaniciAdi"].Value?.ToString() ?? "";
                txtPerSifre.Text = row.Cells["Sifre"].Value?.ToString() ?? "";
                txtPerAdSoyad.Text = row.Cells["AdSoyad"].Value?.ToString() ?? "";
                cmbPerRol.Text = row.Cells["Rol"].Value?.ToString() ?? "";
                _secilenPerFotoYolu = row.Cells["FotoYolu"].Value?.ToString() ?? "";
                pbPerOnizleme.Image = _controller.ResimYukle(_secilenPerFotoYolu);
            };

            Label l1 = new Label { Text = "Ad Soyad:", Location = new Point(730, 20), AutoSize = true };
            txtPerAdSoyad = new TextBox { Location = new Point(730, 42), Width = 250 };

            Label l2 = new Label { Text = "Kullanıcı Adı:", Location = new Point(730, 75), AutoSize = true };
            txtPerKadi = new TextBox { Location = new Point(730, 97), Width = 250 };

            Label l3 = new Label { Text = "Şifre:", Location = new Point(730, 130), AutoSize = true };
            txtPerSifre = new TextBox { Location = new Point(730, 152), Width = 250 };

            Label l4 = new Label { Text = "Yetki Rolü:", Location = new Point(730, 185), AutoSize = true };
            cmbPerRol = new ComboBox { Location = new Point(730, 207), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPerRol.Items.AddRange(new string[] { "Personel", "Admin" });
            cmbPerRol.SelectedIndex = 0;

            pbPerOnizleme = new PictureBox { Location = new Point(730, 245), Size = new Size(110, 110), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
            Button btnFoto = new Button { Text = "Garson Foto Seç", Location = new Point(850, 280), Width = 130, Height = 36 };
            btnFoto.Click += (s, e) =>
            {
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Resimler|*.jpg;*.jpeg;*.png" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        _secilenPerFotoYolu = _controller.ResmiProjeyeKopyala(ofd.FileName, true);
                        pbPerOnizleme.Image = _controller.ResimYukle(_secilenPerFotoYolu);
                    }
                }
            };

            Button btnEkle = new Button { Text = "Garson Ekle", Location = new Point(730, 375), Width = 120, Height = 38, BackColor = Color.SeaGreen, ForeColor = Color.White };
            btnEkle.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtPerKadi.Text) || string.IsNullOrWhiteSpace(txtPerSifre.Text)) return;
                try
                {
                    _controller.PersonelEkle(txtPerKadi.Text.Trim(), txtPerSifre.Text.Trim(), txtPerAdSoyad.Text.Trim(), cmbPerRol.Text, _secilenPerFotoYolu);
                    PersonelFormTemizle();
                    TumVerileriYenile();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.Contains("Duplicate") ? "Bu kullanıcı adı zaten kayıtlı! Farklı bir kullanıcı adı girin." : "Hata: " + ex.Message);
                }
            };

            Button btnGuncelle = new Button { Text = "Güncelle", Location = new Point(860, 375), Width = 120, Height = 38, BackColor = Color.SteelBlue, ForeColor = Color.White };
            btnGuncelle.Click += (s, e) =>
            {
                if (_seciliPerId == 0) return;
                try
                {
                    _controller.PersonelGuncelle(_seciliPerId, txtPerKadi.Text.Trim(), txtPerSifre.Text.Trim(), txtPerAdSoyad.Text.Trim(), cmbPerRol.Text, _secilenPerFotoYolu);
                    PersonelFormTemizle();
                    TumVerileriYenile();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.Contains("Duplicate") ? "Bu kullanıcı adı başka bir personelde kullanılıyor!" : "Hata: " + ex.Message);
                }
            };

            Button btnSil = new Button { Text = "Seçili Garsonu Sil", Location = new Point(730, 423), Width = 250, Height = 38, BackColor = Color.IndianRed, ForeColor = Color.White };
            btnSil.Click += (s, e) =>
            {
                if (_seciliPerId == 0 || _seciliPerId == AdisyonController.AktifKullanici!.Id) { MessageBox.Show("Aktif hesabı silemezsiniz!"); return; }
                _controller.PersonelSil(_seciliPerId);
                PersonelFormTemizle();
                TumVerileriYenile();
            };

            tab.Controls.AddRange(new Control[] { dgvPersonelYonetim, l1, txtPerAdSoyad, l2, txtPerKadi, l3, txtPerSifre, l4, cmbPerRol, pbPerOnizleme, btnFoto, btnEkle, btnGuncelle, btnSil });
        }

        private void PersonelFormTemizle()
        {
            _seciliPerId = 0;
            txtPerAdSoyad.Clear();
            txtPerKadi.Clear();
            txtPerSifre.Clear();
            _secilenPerFotoYolu = "";
            pbPerOnizleme.Image = null;
        }

        // 5. KENDİ PROFİLİM (GARSON FOTO + ŞİFRE)
        private void SekmeProfilKur(TabPage tab)
        {
            var user = AdisyonController.AktifKullanici!;
            Label lblBaslik = new Label { Text = $"Kişisel Profil Ayarları - {user.AdSoyad}", Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(40, 30), AutoSize = true };

            pbProfilFoto = new PictureBox
            {
                Location = new Point(40, 80),
                Size = new Size(160, 160),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = _controller.ResimYukle(user.FotoYolu)
            };

            Button btnYeniFoto = new Button { Text = "Profil Fotoğrafını Değiştir", Location = new Point(40, 250), Width = 160, Height = 36 };
            btnYeniFoto.Click += (s, e) =>
            {
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Resimler|*.jpg;*.jpeg;*.png" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        _secilenProfilYeniFoto = _controller.ResmiProjeyeKopyala(ofd.FileName, true);
                        pbProfilFoto.Image = _controller.ResimYukle(_secilenProfilYeniFoto);
                    }
                }
            };

            Label lblSifre = new Label { Text = "Yeni Şifreniz:", Location = new Point(240, 90), AutoSize = true };
            txtProfilYeniSifre = new TextBox { Location = new Point(240, 115), Width = 240, PasswordChar = '●' };

            Button btnKaydet = new Button
            {
                Text = "Profilimi ve Şifremi Güncelle",
                Location = new Point(240, 160),
                Width = 240,
                Height = 42,
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnKaydet.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtProfilYeniSifre.Text))
                {
                    MessageBox.Show("Lütfen yeni şifrenizi girin!");
                    return;
                }
                string guncelFoto = string.IsNullOrEmpty(_secilenProfilYeniFoto) ? user.FotoYolu : _secilenProfilYeniFoto;
                _controller.KendiProfiliniGuncelle(user.Id, txtProfilYeniSifre.Text.Trim(), guncelFoto);
                pbUstProfil.Image = _controller.ResimYukle(guncelFoto);
                MessageBox.Show("Profil fotoğrafınız ve şifreniz başarıyla güncellendi!");
                TumVerileriYenile();
            };

            tab.Controls.AddRange(new Control[] { lblBaslik, pbProfilFoto, btnYeniFoto, lblSifre, txtProfilYeniSifre, btnKaydet });
        }

        // 6. KASA RAPORU + SIFIRLAMA BUTONU
        private void SekmeKasaKur(TabPage tab)
        {
            dgvKasa = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(1100, 510),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            lblToplamCiro = new Label
            {
                Text = "Toplam Kasa Cirosu: 0,00 ₺",
                Location = new Point(20, 548),
                AutoSize = true,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.DarkGreen
            };

            Button btnKasaSifirla = new Button
            {
                Text = "Kasayı Sıfırla (Gün Sonu Al)",
                Location = new Point(880, 542),
                Size = new Size(240, 40),
                BackColor = Color.Crimson,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnKasaSifirla.Click += (s, e) =>
            {
                DialogResult onay = MessageBox.Show(
                    "Tüm kasa geçmişi silinecek ve toplam ciro 0,00 ₺ olarak sıfırlanacak.\nOnaylıyor musunuz?",
                    "Gün Sonu / Kasa Sıfırlama",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (onay == DialogResult.Yes)
                {
                    _controller.KasayiSifirla();
                    KasaYenile();
                    MessageBox.Show("Kasa başarıyla sıfırlandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            tab.Controls.AddRange(new Control[] { dgvKasa, lblToplamCiro, btnKasaSifirla });
        }

        // VERİ YENİLEME
        private void TumVerileriYenile()
        {
            MasalariCiz();
            dgvMasalarYonetim.DataSource = _controller.MasalariGetir();
            UrunTablolariniYenile();
            PersonelTablosunuYenile();
            KasaYenile();
        }

        private void MasalariCiz()
        {
            flpMasalar.Controls.Clear();
            DataTable dt = _controller.MasalariGetir();
            foreach (DataRow row in dt.Rows)
            {
                string masaAdi = row["MasaAdi"].ToString() ?? "";
                bool dolu = _controller.MasaDoluMu(masaAdi);
                Button btn = new Button
                {
                    Text = dolu ? $"{masaAdi}\n(DOLU)" : $"{masaAdi}\n(BOŞ)",
                    Width = 100,
                    Height = 80,
                    BackColor = dolu ? Color.Firebrick : Color.SeaGreen,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Margin = new Padding(5)
                };
                btn.Click += (s, e) =>
                {
                    _seciliMasa = masaAdi;
                    lblSeciliMasa.Text = $"Seçili Masa: {_seciliMasa}";
                    AdisyonYenile();
                };
                flpMasalar.Controls.Add(btn);
            }
        }

        private void AdisyonYenile()
        {
            if (string.IsNullOrEmpty(_seciliMasa)) return;
            DataTable dt = _controller.MasaSiparisleriniGetir(_seciliMasa);
            dgvAdisyon.DataSource = dt;
            if (dgvAdisyon.Columns["Id"] != null) dgvAdisyon.Columns["Id"].Visible = false;
            decimal toplam = 0;
            foreach (DataRow r in dt.Rows) toplam += Convert.ToDecimal(r["Toplam"]);
            lblToplamTutar.Text = $"Toplam: {toplam:C2}";
        }

        private void UrunTablolariniYenile()
        {
            DataTable dt1 = ResimliTabloHazirla(_controller.UrunleriGetir(), "ResimYolu", "Ürün Resmi");
            dgvMenuSecim.DataSource = dt1;
            if (dgvMenuSecim.Columns["ResimYolu"] != null) dgvMenuSecim.Columns["ResimYolu"].Visible = false;
            if (dgvMenuSecim.Columns["Id"] != null) dgvMenuSecim.Columns["Id"].Visible = false;

            DataTable dt2 = ResimliTabloHazirla(_controller.UrunleriGetir(), "ResimYolu", "Ürün Resmi");
            dgvUrunYonetim.DataSource = dt2;
            if (dgvUrunYonetim.Columns["ResimYolu"] != null) dgvUrunYonetim.Columns["ResimYolu"].Visible = false;
        }

        private void PersonelTablosunuYenile()
        {
            DataTable dt = ResimliTabloHazirla(_controller.PersonelleriGetir(), "FotoYolu", "Profil Foto");
            dgvPersonelYonetim.DataSource = dt;
            if (dgvPersonelYonetim.Columns["FotoYolu"] != null) dgvPersonelYonetim.Columns["FotoYolu"].Visible = false;
        }

        private DataTable ResimliTabloHazirla(DataTable kaynak, string yolSutunu, string gorselBaslik)
        {
            kaynak.Columns.Add(gorselBaslik, typeof(Image));
            foreach (DataRow row in kaynak.Rows)
            {
                Image? img = _controller.ResimYukle(row[yolSutunu].ToString());
                row[gorselBaslik] = img != null ? new Bitmap(img, new Size(45, 45)) : new Bitmap(45, 45);
            }
            return kaynak;
        }

        private void KasaYenile()
        {
            DataTable dt = _controller.KasaHareketleriniGetir();
            dgvKasa.DataSource = dt;
            decimal ciro = 0;
            foreach (DataRow r in dt.Rows) ciro += Convert.ToDecimal(r["Tutar"]);
            lblToplamCiro.Text = $"Toplam Kasa Cirosu: {ciro:C2}";
        }
    }
}