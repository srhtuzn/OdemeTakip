// OdemeTakip.Entities/KrediKartiOdeme.cs
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

        public int? CompanyId { get; set; }
        public Company? Company { get; set; }

        public string? KartAdi { get; set; }
        public string? Aciklama { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tutar { get; set; }
        public DateTime? OdemeTarihi { get; set; } // Nullable olduğu için DateTime?

        public string? Banka { get; set; }
        public bool OdenmeDurumu { get; set; } = false;

        public bool IsActive { get; set; } = true;
        public string? OdeyenKullaniciAdi { get; set; }

        // Yeni Alanlar (Taksit İçin)
        public int? TaksitNo { get; set; } // Nullable olduğu için int?
        public int? ToplamTaksit { get; set; } // Nullable olduğu için int?
        public DateTime? IlkOdemeTarihi { get; set; } // Nullable olduğu için DateTime?
        public int? KrediKartiId { get; set; } // Nullable olduğu için int?

        [ForeignKey(nameof(KrediKartiId))]
        public KrediKarti? KrediKarti { get; set; }
        public int? KrediKartiHarcamaId { get; set; } // Nullable olduğu için int?
        public KrediKartiHarcama? KrediKartiHarcama { get; set; } // Navigation Property
    }
}