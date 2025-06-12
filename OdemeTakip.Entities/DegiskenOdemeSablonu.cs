using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdemeTakip.Entities
{
    public class DegiskenOdemeSablonu
    {
        public int Id { get; set; }
        public string GiderTuru { get; set; } = "";   // Elektrik, Su
        public int? CompanyId { get; set; }
        public Company? Company { get; set; }

        public string? Aciklama { get; set; }
        public string ParaBirimi { get; set; } = "TL";

        public int Gun { get; set; } = 1;             // Her ayın kaçında oluşturulsun?
        public bool IsActive { get; set; } = true;
        public string? OdeyenKullaniciAdi { get; set; }
        public int? CariFirmaId { get; set; } // 🔥 Burayı ekle!
        public virtual CariFirma? CariFirma { get; set; } // 🔥 Navigation Property
    }

}
