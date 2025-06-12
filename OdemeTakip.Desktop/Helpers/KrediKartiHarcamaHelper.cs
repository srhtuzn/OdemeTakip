// OdemeTakip.Desktop.Helpers/KrediKartiHarcamaHelper.cs
using System;
using System.Linq;
using OdemeTakip.Data;
using OdemeTakip.Entities;
using Microsoft.EntityFrameworkCore; // Include için

namespace OdemeTakip.Desktop.Helpers
{
    public static class KrediKartiHarcamaHelper
    {
        public static void HarcamaEkle(AppDbContext db, int krediKartiId, decimal tutar, string aciklama, int taksitSayisi, DateTime harcamaTarihi)
        {
            var kart = db.KrediKartlari.Include(k => k.Company).FirstOrDefault(x => x.Id == krediKartiId); // Company'yi de include et
            if (kart == null)
                throw new Exception("Kart bulunamadı.");

            // Harcama için benzersiz bir kod üret (örn: HK0001, HK0002...)
            string harcamaKodu = GenerateHarcamaKodu(db); // Yeni metod çağrısı

            var harcama = new KrediKartiHarcama
            {
                KrediKartiId = krediKartiId,
                HarcamaKodu = harcamaKodu, // Oluşturulan kodu ata
                Aciklama = aciklama,
                Tutar = tutar,
                TaksitSayisi = taksitSayisi,
                HarcamaTarihi = harcamaTarihi,
                IsActive = true
            };
            db.KrediKartiHarcamalari.Add(harcama);
            db.SaveChanges(); // Harcamayı önce kaydet ki ID oluşsun

            // Harcama ID'sini taksitler için kullanacağız
            int harcamaId = harcama.Id;


            // İlk ödeme tarihini, harcama tarihi ve kartın ekstre kesim/son ödeme tarihlerine göre belirle
            DateTime ilkOdemeTarihi;
            // Eğer harcama, ekstre kesim tarihinden (DueDate) sonra yapıldıysa,
            // ilk taksit bir sonraki ayın son ödeme tarihine düşer.
            if (harcamaTarihi.Day > kart.DueDate.Day) // Harcama tarihi, ekstre kesim gününden sonra ise
            {
                ilkOdemeTarihi = new DateTime(harcamaTarihi.Year, harcamaTarihi.Month, kart.PaymentDueDate.Day).AddMonths(1);
            }
            else // Ekstre kesim gününde veya öncesinde yapıldıysa
            {
                ilkOdemeTarihi = new DateTime(harcamaTarihi.Year, harcamaTarihi.Month, kart.PaymentDueDate.Day);
            }

            // Eğer hesaplanan ilk ödeme tarihi, harcama tarihinden önce ise, bir ay ileri al.
            // Bu, özellikle ayın son günlerinde yapılan harcamalar için önemlidir.
            if (ilkOdemeTarihi < harcamaTarihi && harcamaTarihi.Day > kart.PaymentDueDate.Day) // Sadece ödeme günü geçmişse ileri al
            {
                ilkOdemeTarihi = ilkOdemeTarihi.AddMonths(1);
            }

            // Taksitleri oluştur
            for (int i = 0; i < taksitSayisi; i++)
            {
                var odemeTarihi = ilkOdemeTarihi.AddMonths(i);

                // Eğer ödeme tarihi, ilgili ayın son gününden sonraysa, ayın son gününe çek.
                // Örneğin, 31 Mart'a denk gelen ödeme, Nisan'da 30 Nisan'a çekilmeli.
                if (odemeTarihi.Day > DateTime.DaysInMonth(odemeTarihi.Year, odemeTarihi.Month))
                {
                    odemeTarihi = new DateTime(odemeTarihi.Year, odemeTarihi.Month, DateTime.DaysInMonth(odemeTarihi.Year, odemeTarihi.Month));
                }

                var odeme = new KrediKartiOdeme
                {
                    OdemeKodu = taksitSayisi == 1 ? harcamaKodu : $"{harcamaKodu}-T{i + 1:D2}", // Tek taksit ise sadece harcama kodu, değilse taksit no ile
                    KartAdi = kart.CardName,
                    Aciklama = aciklama, // Harcama açıklaması
                    Tutar = Math.Round(tutar / taksitSayisi, 2),
                    OdemeTarihi = odemeTarihi, // Hesaplanan taksit ödeme tarihi
                    Banka = kart.Banka,
                    CompanyId = kart.CompanyId,
                    IsActive = true,
                    OdenmeDurumu = false,
                    TaksitNo = i + 1,
                    ToplamTaksit = taksitSayisi,
                    IlkOdemeTarihi = ilkOdemeTarihi,
                    KrediKartiId = krediKartiId,
                    KrediKartiHarcamaId = harcamaId // Oluşturulan harcamanın ID'sini bağla
                };
                db.KrediKartiOdemeleri.Add(odeme);
            }
            db.SaveChanges(); // Taksitleri kaydet
        }

        // Harcama Kodu Üretici (KK0001, KK0002...)
        private static string GenerateHarcamaKodu(AppDbContext db)
        {
            var sonHarcama = db.KrediKartiHarcamalari
                                .OrderByDescending(h => h.Id)
                                .Select(h => h.HarcamaKodu)
                                .FirstOrDefault(); // Son harcama kodunu al

            int nextId = 1;
            if (!string.IsNullOrEmpty(sonHarcama) && sonHarcama.StartsWith("KK"))
            {
                if (int.TryParse(sonHarcama.Substring(2), out int lastId))
                {
                    nextId = lastId + 1;
                }
            }
            return $"KK{nextId:D4}";
        }
    }
}