namespace MiniMarketCRM.Application.DTO
{
    public class DashboardSummaryDTO
    {
        public int AktifSepet { get; set; }          // seçili müşteri için beklemede sepet var mı? (0/1)
        public int BugunSiparisAdet { get; set; }    // bugün tamamlanan sipariş adedi
        public decimal BugunCiro { get; set; }       // bugün tamamlanan siparişlerin toplamı
        public int DusukStokAdet { get; set; }       // stok < eşik olan ürün sayısı
        public int DusukStokEsik { get; set; } = 3;  
        public int? SelectedMusteriId { get; set; }  
    }
}
