namespace CafeAdisyonMasaustu.Models
{
    public class Siparis
    {
        public int Id { get; set; }
        public string MasaAdi { get; set; } = "";
        public string UrunAdi { get; set; } = "";
        public int Adet { get; set; }
        public decimal BirimFiyat { get; set; }
    }
}