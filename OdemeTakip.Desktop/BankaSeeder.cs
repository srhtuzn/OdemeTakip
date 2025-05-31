using OdemeTakip.Data;
using OdemeTakip.Entities;
using System.Collections.Generic;
using System.Linq;

namespace OdemeTakip.Desktop
{
    public static class BankaSeeder
    {
        public static void Yukle(AppDbContext db)
        {
            if (!db.Bankalar.Any())
            {
                var bankaAdlari = new List<string>
                {
                    "Ziraat Bankası", "VakıfBank", "Halkbank", "İş Bankası", "Garanti BBVA",
                    "Yapı Kredi", "Akbank", "QNB Finansbank", "TEB", "DenizBank",
                    "Şekerbank", "AnadoluBank", "Alternatif Bank", "ICBC Turkey", "Odeabank",
                    "Türkiye Finans", "Kuveyt Türk", "Albaraka Türk", "ING Bank", "HSBC"
                };

                foreach (var ad in bankaAdlari)
                {
                    db.Bankalar.Add(new Banka { Adi = ad });
                }

                db.SaveChanges();
            }
        }
    }
}
