using OdemeTakip.Data;
using OdemeTakip.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace OdemeTakip.Desktop.Helpers
{
    /// <summary>
    /// Değişken ödeme şablonlarına göre otomatik ödeme kayıtları oluşturan yardımcı sınıf.
    /// Her ay için belirli şablonlara uygun (fatura numarası ve tutarı eksik) ödeme taslakları oluşturur.
    /// </summary>
    public static class DegiskenOdemeGenerator
    {
        /// <summary>
        /// Tanımlı değişken ödeme şablonlarını kontrol eder ve ilgili aylara ait eksik faturaları oluşturur.
        /// İşlem sadece mevcut ay ve bir sonraki ay için ödeme taslakları oluşturur.
        /// </summary>
        /// <param name="db">Veritabanı bağlamı (AppDbContext).</param>
        public static void Uygula(AppDbContext db)
        {
            var bugun = DateTime.Today;

            // Sadece mevcut ay ve bir sonraki ay için faturalar oluşturulacak.
            // Bu, veritabanında gereksiz yere çok ileriki aylara ait kayıtların birikmesini önler.
            var olusturulacakAylar = new[] { bugun.Month, bugun.Month + 1 };

            // Aktif olan değişken ödeme şablonlarını şirket bilgileriyle birlikte çekiyoruz.
            // .Include(x => x.Company) N+1 sorgu probleminden kaçınmak için önemlidir.
            var sablonlar = db.DegiskenOdemeSablonlari
                .Include(x => x.Company)
                .Include(x => x.CariFirma) // CariFirma bilgisi de gerekebilir, eklendi
                .Where(x => x.IsActive)
                .ToList();

            foreach (var sablon in sablonlar)
            {
                // Mevcut ve bir sonraki ay için döngü
                foreach (var ay in olusturulacakAylar)
                {
                    // Yılın 12 ayını geçmemesini kontrol et (örneğin Aralık ayında 13. ay olmasın)
                    if (ay > 12)
                        continue;

                    // Şablonun belirttiği gün, ilgili ay ve yıla göre hedef tarihi oluştur.
                    // Eğer şablon günü ayın son gününden büyükse, ayın son gününe yuvarla.
                    // Örneğin, Şubat ayında 30 veya 31. gün için şablon varsa, Şubat'ın son gününe (28 veya 29) ayarlanır.
                    var olusturulacakAyinYili = bugun.Year;
                    if (ay < bugun.Month) // Eğer ay mevcut aydan küçükse, sonraki yıla ait olabilir (örn: Ocak ayında bir sonraki yılın Ocak'ını oluştururken)
                    {
                        olusturulacakAyinYili++; // Bu, bir sonraki yıla ait ayları doğru işler (örn: Aralık ayında Ocak şablonu bir sonraki yıla ait olur)
                    }

                    // Hedef tarihin geçerliliğini kontrol et ve ayın son gününe yuvarla
                    DateTime hedefTarih;
                    try
                    {
                        hedefTarih = new DateTime(olusturulacakAyinYili, ay, Math.Min(sablon.Gun, DateTime.DaysInMonth(olusturulacakAyinYili, ay)));
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // Nadir de olsa hatalı gün değeri durumunda, loglama veya atlama yapılabilir.
                        // Şimdilik continue ile atlayalım.
                        continue;
                    }


                    // Eğer hedef tarih bugünden daha eski ise ve o ay için zaten ödeme varsa, oluşturma.
                    // Veya hedef tarih gelecekte ise ve o ay için zaten ödeme varsa, oluşturma.
                    // Bu, aynı ay içinde birden fazla çalışmayı ve geçmişe dönük gereksiz kayıtları engeller.
                    bool zatenVar = db.DegiskenOdemeler.Any(x =>
                        x.GiderTuru == sablon.GiderTuru &&
                        x.CompanyId == sablon.CompanyId &&
                        x.OdemeTarihi.Year == hedefTarih.Year &&
                        x.OdemeTarihi.Month == hedefTarih.Month &&
                        x.SablonId == sablon.Id); // ŞablonId ile kontrol, daha spesifik hale getirir.

                    if (zatenVar)
                        continue;

                    // Yalnızca gelecekteki veya mevcut aydaki (bugüne eşit veya sonraki) faturaları oluşturalım.
                    // Geçmiş aylara ait faturaların tekrar oluşturulmasını engeller.
                    if (hedefTarih.Date < bugun.Date && ay == bugun.Month) // Sadece mevcut ay içinde geçmiş tarihleri kontrol et
                    {
                        // Eğer mevcut ay içindeyiz ve hedef tarih bugünden eskiyse, bu faturayı oluşturmayız.
                        // Çünkü uygulama çalıştıysa ve geçmişteki bir faturayı oluşturmamışsa, muhtemelen manuel girilmiştir.
                        // Ya da zaten geçmiş aylar için ayrı bir generator çalışmıştır/çalışacaktır.
                        // Bu kısım projenizin iş mantığına göre değişebilir.
                        continue;
                    }

                    var odeme = new DegiskenOdeme
                    {
                        OdemeKodu = KodUret(db), // Mevcut KodUret metodunuzu kullanmaya devam edin
                        GiderTuru = sablon.GiderTuru,
                        Aciklama = sablon.Aciklama,
                        OdemeTarihi = hedefTarih, // Ayarlanmış tarih
                        Tutar = 0, // Kullanıcı sonra dolduracak
                        ParaBirimi = sablon.ParaBirimi,
                        CompanyId = sablon.CompanyId,
                        CariFirmaId = sablon.CariFirmaId,
                        IsActive = true,
                        OdenmeDurumu = false, // Henüz ödenmemiş
                        FaturaNo = null, // Fatura numarası başlangıçta boş
                        SablonId = sablon.Id // Hangi şablondan geldiğini belirtmek için (önemli)
                    };

                    db.DegiskenOdemeler.Add(odeme);
                }
            }

            // Tüm değişiklikleri tek bir işlemde veritabanına kaydet
            db.SaveChanges();
        }

        /// <summary>
        /// Yeni bir değişken ödeme için benzersiz bir kod üretir.
        /// "AO" ön eki ve dört haneli ardışık bir sayı kullanır (örn: AO0001, AO0002).
        /// </summary>
        /// <param name="db">Veritabanı bağlamı (AppDbContext).</param>
        /// <returns>Yeni ödeme kodu.</returns>
        private static string KodUret(AppDbContext db)
        {
            // "AO" ile başlayan en büyük kod numarasını bul.
            var sonKod = db.DegiskenOdemeler
                .Where(x => x.OdemeKodu != null && x.OdemeKodu.StartsWith("AO")) // Null kontrolü eklendi
                .OrderByDescending(x => x.OdemeKodu)
                .Select(x => x.OdemeKodu)
                .FirstOrDefault();

            int yeniNumara = 1;
            // Eğer daha önce kod üretilmişse, en son kodu ayrıştırıp bir sonraki numarayı al.
            if (!string.IsNullOrEmpty(sonKod) && sonKod.Length > 2 && int.TryParse(sonKod.Substring(2), out var mevcutNumara))
            {
                yeniNumara = mevcutNumara + 1;
            }

            // Yeni kodu formatla ve döndür.
            return $"AO{yeniNumara:D4}";
        }
    }
}