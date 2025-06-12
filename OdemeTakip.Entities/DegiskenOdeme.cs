using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdemeTakip.Entities
{
    public class DegiskenOdeme
    {
        public int Id { get; set; }

        public string OdemeKodu { get; set; } = "";     // Örn: DO0001
        public string GiderTuru { get; set; } = "";     // Örn: Elektrik, Su
        public string? FaturaNo { get; set; }

        public DateTime OdemeTarihi { get; set; }       // Bu fatura ne zaman ödendi/ödenecek
        public string? Aciklama { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tutar { get; set; }

        public string ParaBirimi { get; set; } = "TL";

        public bool OdenmeDurumu { get; set; } = false;
        public DateTime? OdenmeTarihi { get; set; }
        public string? OdemeBankasi { get; set; }

        public bool IsActive { get; set; } = true;

        public int? CompanyId { get; set; }
        public Company? Company { get; set; }
        public string? OdeyenKullaniciAdi { get; set; }
        public int? CariFirmaId { get; set; }   // Yeni ekleyeceğimiz alan
        public virtual CariFirma? CariFirma { get; set; } // Navigation Property
        public int? SablonId { get; set; } // Hangi şablondan geldiğini belirtir
        public virtual DegiskenOdemeSablonu? Sablon { get; set; } // Navigation Property

    }

}
