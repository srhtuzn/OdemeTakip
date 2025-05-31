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
    }

}

