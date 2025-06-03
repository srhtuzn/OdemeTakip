using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdemeTakip.Entities
{
    public class KrediKartiHarcama
    {
        public int Id { get; set; }
        public int KrediKartiId { get; set; }
        public virtual KrediKarti? KrediKarti { get; set; }

        public string Aciklama { get; set; } = "";
        public decimal Tutar { get; set; }
        public int TaksitSayisi { get; set; } = 1; // Peşin alışverişlerde 1 olur
        public DateTime HarcamaTarihi { get; set; }
        public string ParaBirimi { get; set; } = "TL";

        public bool IsActive { get; set; } = true;
    }
}
