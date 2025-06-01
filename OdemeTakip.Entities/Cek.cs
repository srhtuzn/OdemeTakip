using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OdemeTakip.Entities
{
    public class Cek
    {
        public int Id { get; set; }
        public string? CekKodu { get; set; }

        public string? CekNumarasi { get; set; }
        public string? SirketAdi { get; set; }

        public string? FirmaAdi { get; set; }           // Çeki veren ya da alınan firma
        public string? CekTuru { get; set; }            // "Alınan" / "Verilen"

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tutar { get; set; }

        public DateTime VadeTarihi { get; set; }         // Ne zaman tahsil edilecek

        public DateTime? TahsilTarihi { get; set; }      // Tahsil edildiyse

        public string? Banka { get; set; }

        public string? Notlar { get; set; }
        public string? OdemeBankasi { get; set; }
        public DateTime? OdemeTarihi { get; set; }

        public bool TahsilEdildiMi { get; set; } = false;
        public bool OdenmeDurumu { get; set; } = false;

        public bool IsActive { get; set; } = true;
        public string? ParaBirimi { get; set; }
        public int? CariFirmaId { get; set; }
        public CariFirma? CariFirma { get; set; }
        public string? OdeyenKullaniciAdi { get; set; }

    }
}
