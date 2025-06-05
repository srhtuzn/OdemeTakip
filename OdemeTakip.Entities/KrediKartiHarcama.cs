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
        public int KrediKartiId { get; set; }
        public virtual KrediKarti? KrediKarti { get; set; }

        [Required]
        public string Aciklama { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tutar { get; set; }

        public int TaksitSayisi { get; set; } = 1; // 1: Peşin ödeme, >1: Taksitli

        public DateTime HarcamaTarihi { get; set; }

        public string ParaBirimi { get; set; } = "TL";

        public bool IsActive { get; set; } = true;

    }
}
