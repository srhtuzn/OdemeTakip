using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdemeTakip.Entities
{
    public class KrediKartiOdeme
    {
        public int Id { get; set; }
        public string? OdemeKodu { get; set; }
        public string? OdemeBankasi { get; set; }

        public int? CompanyId { get; set; }  // 🔥 Şirket FK
        public Company? Company { get; set; }  // 🔥 Şirket Navigation

        public string? KartAdi { get; set; }
        public string? Aciklama { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tutar { get; set; }
        public DateTime? OdemeTarihi { get; set; }

        public string? Banka { get; set; }
        public bool OdenmeDurumu { get; set; } = false;

        public bool IsActive { get; set; } = true;
        public string? OdeyenKullaniciAdi { get; set; }
    }


}
