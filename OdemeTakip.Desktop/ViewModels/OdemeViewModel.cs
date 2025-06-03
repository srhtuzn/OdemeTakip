using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdemeTakip.Desktop.ViewModels
{
    public class OdemeViewModel
    {
        public string Kod { get; set; } = "";
        public string Aciklama { get; set; } = "";
        public DateTime Tarih { get; set; }
        public decimal Tutar { get; set; }
        public string ParaBirimi { get; set; } = "TL";
        public string KaynakModul { get; set; } = "";
        public bool OdenmeDurumu { get; set; }
        public DateTime? OdemeTarihi { get; set; }
        public string? OdemeBankasi { get; set; }
        public string? FaturaNo { get; set; }

        // 👇 Her modülde ortaklaştırılmış, şirket veya şahıs olabilir
        public string? SirketAdi { get; set; }
        public string? OdeyenKullaniciAdi { get; set; } = ""; // Ödeme yapan kullanıcının adı
        public int Id { get; set; } // Entity'nin asıl Primary Key'i için
        public int KaynakId { get; set; } // Hangi entity'den geldiğini belirtmek için (genellikle Id ile aynı olabilir)
        public bool Durum { get; set; } // Ödeme yapıldı mı?
        public DateTime VadeTarihi { get; set; } // Taksit veya Ödeme Vade Tarihi
        public int TaksitNo { get; set; } // Ödeme taksit numarası
        public string CariFirmaAdi { get; set; } = "";



    }

}

