using System;
using System.Linq;
using OdemeTakip.Data;
using OdemeTakip.Entities;

namespace OdemeTakip.Desktop.Helpers
{
    public static class KrediKartiHarcamaHelper
    {
        public static void HarcamaEkle(AppDbContext db, int krediKartiId, decimal tutar, string aciklama, int taksitSayisi, DateTime harcamaTarihi)
        {
            var kart = db.KrediKartlari.FirstOrDefault(x => x.Id == krediKartiId);
            if (kart == null)
                throw new Exception("Kart bulunamadı.");

            var harcama = new KrediKartiHarcama
            {
                KrediKartiId = krediKartiId,
                Aciklama = aciklama,
                Tutar = tutar,
                TaksitSayisi = taksitSayisi,
                HarcamaTarihi = harcamaTarihi,
                IsActive = true
            };
            db.KrediKartiHarcamalari.Add(harcama);  // ✅ Düzeltilmiş

            var ilkOdemeTarihi = new DateTime(harcamaTarihi.Year, harcamaTarihi.Month, kart.PaymentDueDate.Day);
            if (ilkOdemeTarihi < harcamaTarihi)
                ilkOdemeTarihi = ilkOdemeTarihi.AddMonths(1);

            for (int i = 0; i < taksitSayisi; i++)
            {
                var odeme = new KrediKartiOdeme
                {
                    OdemeKodu = KodUret(db),
                    KartAdi = kart.CardName,
                    Banka = kart.Banka,
                    CompanyId = kart.CompanyId,
                    KrediKartiId = krediKartiId,
                    OdemeTarihi = ilkOdemeTarihi.AddMonths(i),
                    Tutar = Math.Round(tutar / taksitSayisi, 2),
                    IsActive = true,
                    OdenmeDurumu = false,
                    TaksitNo = i + 1,
                    ToplamTaksit = taksitSayisi,
                    IlkOdemeTarihi = ilkOdemeTarihi,
                    Aciklama = aciklama
                };
                db.KrediKartiOdemeleri.Add(odeme);
            }

            db.SaveChanges();
        }

        private static string KodUret(AppDbContext db)
        {
            int mevcut = db.KrediKartiOdemeleri.Count() + 1;
            return $"KKO{mevcut:D4}";
        }
    }
}
