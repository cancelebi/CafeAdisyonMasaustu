namespace CafeAdisyonMasaustu.Models
{
    public class KasaHareket
    {
        public int Id { get; set; }
        public string MasaAdi { get; set; } = "";
        public decimal Tutar { get; set; }
        public DateTime Tarih { get; set; }
    }
}