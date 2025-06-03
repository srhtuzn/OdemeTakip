using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OdemeTakip.Entities
{
    public class GenelOdeme
    {
        public int Id { get; set; }
        public string? OdemeKodu { get; set; }
        public string? OdemeAdi { get; set; }
        public string? Aciklama { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tutar { get; set; }
        public string? ParaBirimi { get; set; }
        public DateTime? OdemeTarihi { get; set; }
        public string? OdemeBankasi { get; set; }

        // FK + Navigation
        public int? CompanyId { get; set; }
        public Company? Company { get; set; }

        public bool IsOdedildiMi { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public string? OdeyenKullaniciAdi { get; set; }
        public string? FaturaNo { get; set; } // Opsiyonel fatura no (fatura aramalarına uygun)
        public int? CariFirmaId { get; set; }   // Yeni ekleyeceğimiz alan
        public virtual CariFirma CariFirma { get; set; } // Navigation Property
    }
}