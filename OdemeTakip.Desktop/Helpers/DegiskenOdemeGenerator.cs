using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Entities;
using System;
using System.Linq;

namespace OdemeTakip.Desktop.Helpers
{
    public static class DegiskenOdemeGenerator
    {
        public static void Uygula(AppDbContext db)
        {
            var bugun = DateTime.Today;

            var sablonlar = db.DegiskenOdemeSablonlari
                .Include(x => x.Company)
                .Where(x => x.IsActive)
                .ToList();

            foreach (var sablon in sablonlar)
            {
                var hedefTarih = new DateTime(bugun.Year, bugun.Month, sablon.Gun);

                bool zatenVar = db.DegiskenOdemeler.Any(x =>
                    x.GiderTuru == sablon.GiderTuru &&
                    x.CompanyId == sablon.CompanyId &&
                    x.OdemeTarihi.Year == hedefTarih.Year &&
                    x.OdemeTarihi.Month == hedefTarih.Month);

                if (zatenVar)
                    continue;

                var odeme = new DegiskenOdeme
                {
                    OdemeKodu = KodUret(db),
                    GiderTuru = sablon.GiderTuru,
                    Aciklama = sablon.Aciklama,
                    OdemeTarihi = hedefTarih,
                    Tutar = 0,
                    ParaBirimi = sablon.ParaBirimi,
                    CompanyId = sablon.CompanyId,
                    IsActive = true,
                    OdenmeDurumu = false
                };

                db.DegiskenOdemeler.Add(odeme);
            }

            db.SaveChanges();
        }

        private static string KodUret(AppDbContext db)
        {
            int adet = db.DegiskenOdemeler.Count() + 1;
            return $"DSO{adet.ToString("D4")}";
        }
    }
}
