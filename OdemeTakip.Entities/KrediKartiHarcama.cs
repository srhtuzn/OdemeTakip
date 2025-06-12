// OdemeTakip.Entities/KrediKartiHarcama.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OdemeTakip.Entities
{
    public class KrediKartiHarcama
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(KrediKarti))]
        public int KrediKartiId { get; set; } // KrediKartı entity'de KrediKartiId int olduğu için int
        public virtual KrediKarti? KrediKarti { get; set; }

        [Required]
        public string Aciklama { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tutar { get; set; }

        public int TaksitSayisi { get; set; } = 1; // KrediKartiHarcama entity'de TaksitSayisi int olduğu için int

        [Column(TypeName = "decimal(18,2)")]
        public decimal AylikTutar => TaksitSayisi > 0 ? Tutar / TaksitSayisi : Tutar;

        public DateTime HarcamaTarihi { get; set; }
        public string ParaBirimi { get; set; } = "TL";
        public bool OdenmeDurumu { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public string? HarcamaKodu { get; set; }

    }
}