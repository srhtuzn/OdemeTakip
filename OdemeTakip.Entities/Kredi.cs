using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace OdemeTakip.Entities
{
    public class Kredi
    {
        public int Id { get; set; }
        public string? KrediKodu { get; set; }

        // public string? SirketAdi { get; set; } // Bu property ViewModel'de SirketAdi'nı Company'den alacağımız için gerek kalmayabilir.
        // Eğer doğrudan Kredi entity'sinde tutulması isteniyorsa kalabilir.
        // Ancak ilişki kurmak daha iyi.

        public string? KrediKonusu { get; set; } // Araç, yatırım, vs.
        [Column(TypeName = "decimal(18,2)")]
        public decimal ToplamTutar { get; set; } // Ana kredi tutarı
        public int TaksitSayisi { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AylikTaksitTutari { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OdenenTutar { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal KalanTutar { get; set; }

        public DateTime BaslangicTarihi { get; set; }

        public string? Notlar { get; set; }

        public bool IsActive { get; set; } = true;
        public string? ParaBirimi { get; set; }
        public bool OdenmeDurumu { get; set; } = false;
        public string Banka { get; set; } = string.Empty;

        public ICollection<KrediTaksit> Taksitler { get; set; } = new List<KrediTaksit>();

        // 👇 BURAYA EKLENECEK PROPERTY'LER 👇
        public int? CompanyId { get; set; } // İlişkili şirketin ForeignKey'i
        public Company? Company { get; set; } // İlişkili şirket navigasyon property'si

        public int? CariFirmaId { get; set; } // İlişkili cari firmanın ForeignKey'i
        public CariFirma? CariFirma { get; set; } // İlişkili cari firma navigasyon property'si
        // 👆 BURAYA EKLENECEK PROPERTY'LER 👆
    }
}