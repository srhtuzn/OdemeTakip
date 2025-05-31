using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdemeTakip.Entities
{
    public class BankaHesabi
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }         // 🔗 FK
        public string BankaAdi { get; set; } = string.Empty;
        public string HesapKodu { get; set; } = ""; // örn: BH001
        public string Iban { get; set; } = string.Empty;
        public string HesapSahibi { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public Company Company { get; set; } = null!;
    }

}
