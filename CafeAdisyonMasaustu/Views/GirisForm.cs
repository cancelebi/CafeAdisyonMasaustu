using CafeAdisyonMasaustu.Controllers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CafeAdisyonMasaustu.Views
{
    public partial class GirisForm : Form
    {
        private readonly AdisyonController _controller = new AdisyonController();
        private TextBox txtKadi = null!;
        private TextBox txtSifre = null!;

        public GirisForm()
        {
            ArayuzuOlustur();
        }

        private void ArayuzuOlustur()
        {
            this.Text = "Cafe Adisyon Masaüstü - Personel Girişi";
            this.Size = new Size(400, 340);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 34, 43);

            Label lblBaslik = new Label
            {
                Text = "CAFE ADİSYON SİSTEMİ",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60
            };

            Label lblKadi = new Label { Text = "Kullanıcı Adı:", ForeColor = Color.LightGray, Location = new Point(50, 75), AutoSize = true };
            txtKadi = new TextBox { Location = new Point(50, 98), Width = 285, Font = new Font("Segoe UI", 11), Text = "admin" };

            Label lblSifre = new Label { Text = "Şifre:", ForeColor = Color.LightGray, Location = new Point(50, 138), AutoSize = true };
            txtSifre = new TextBox { Location = new Point(50, 161), Width = 285, Font = new Font("Segoe UI", 11), PasswordChar = '●', Text = "1234" };

            Button btnGiris = new Button
            {
                Text = "GİRİŞ YAP",
                Location = new Point(50, 208),
                Width = 285,
                Height = 40,
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGiris.Click += BtnGiris_Click;

            LinkLabel lnkUnuttum = new LinkLabel
            {
                Text = "Şifremi Unuttum (Mail Gönder)",
                LinkColor = Color.LightSkyBlue,
                Location = new Point(105, 260),
                AutoSize = true
            };
            lnkUnuttum.Click += LnkUnuttum_Click;

            this.Controls.AddRange(new Control[] { lblBaslik, lblKadi, txtKadi, lblSifre, txtSifre, btnGiris, lnkUnuttum });
            this.AcceptButton = btnGiris;
        }

        private void BtnGiris_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_controller.GirisYap(txtKadi.Text.Trim(), txtSifre.Text.Trim()))
                {
                    AnaForm anaForm = new AnaForm();
                    anaForm.FormClosed += (s, args) => this.Show();
                    this.Hide();
                    anaForm.Show();
                }
                else
                {
                    MessageBox.Show("Kullanıcı adı veya şifre hatalı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı bağlantı hatası (XAMPP MySQL açık mı?):\n" + ex.Message);
            }
        }

        private void LnkUnuttum_Click(object? sender, EventArgs e)
        {
            string mail = Microsoft.VisualBasic.Interaction.InputBox("Şifrenizin gönderileceği e-posta adresini girin:", "Şifremi Unuttum", "");
            if (string.IsNullOrWhiteSpace(mail)) return;

            try
            {
                if (_controller.SifremiUnuttumMailGonder(txtKadi.Text.Trim(), mail.Trim()))
                    MessageBox.Show("Şifreniz e-posta adresinize gönderildi.");
                else
                    MessageBox.Show("Bu kullanıcı adına ait kayıt bulunamadı.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Mail gönderilemedi: " + ex.Message);
            }
        }
    }
}