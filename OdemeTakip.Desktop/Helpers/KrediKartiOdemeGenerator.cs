using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Entities;

namespace OdemeTakip.Desktop.Helpers
{
    public static class KrediKartiOdemeGenerator
    {
        public static void Uygula(AppDbContext db)
        {
            var bugun = DateTime.Today;
            var ayBaslangic = new DateTime(bugun.Year, bugun.Month, 1);
            var aySonu = ayBaslangic.AddMonths(1).AddDays(-1);

            // Aktif tüm kredi kartlarını çekiyoruz
            var kartlar = db.KrediKartlari
                .Where(x => x.IsActive)
                .ToList();

            foreach (var kart in kartlar)
            {
                // Ödeme günü gelmiş/geçmiş/gelsin, hiç önemli değil — bu ay için kayıt aç
                var hedefTarih = new DateTime(bugun.Year, bugun.Month, kart.PaymentDueDate.Day);

                // Eğer PaymentDueDate günü, ayın max gününden büyükse (örn: 31 Haziran yok) son günü al
                if (hedefTarih > aySonu)
                    hedefTarih = aySonu;

                // Bu ayda bu kart için ödeme kaydı var mı?
                bool zatenVar = db.KrediKartiOdemeleri
                    .Any(x => x.KartAdi == kart.CardName &&
                              x.OdemeTarihi.HasValue &&
                              x.OdemeTarihi.Value.Month == bugun.Month &&
                              x.OdemeTarihi.Value.Year == bugun.Year);

                if (zatenVar)
                    continue; // Var, ekleme.

                // 0 TL'lik otomatik ödeme kaydı oluştur
                var odeme = new KrediKartiOdeme
                {
                    OdemeKodu = KodUret(db),
                    KartAdi = kart.CardName,
                    Banka = kart.Banka,
                    CompanyId = kart.CompanyId,
                    IsActive = true,
                    OdemeTarihi = hedefTarih,
                    Tutar = 0,
                    OdenmeDurumu = false
                };

                db.KrediKartiOdemeleri.Add(odeme);
            }

            db.SaveChanges();
        }

        private static string KodUret(AppDbContext db)
        {
            int adet = db.KrediKartiOdemeleri.Count() + 1;
            return $"KKO{adet:D4}";
        }
    }
}
