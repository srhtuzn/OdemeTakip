using System;
using System.Linq;
using OdemeTakip.Data;

namespace OdemeTakip.Desktop.Helpers
{
    public static class PaymentHelper
    {
        public static object? BulEntity(AppDbContext db, string kaynakModul, int entityId, string kod, DateTime vadeTarihi)
        {
            DateTime vadeGun = vadeTarihi.Date;
            try
            {
                switch (kaynakModul)
                {
                    case "Sabit Ödeme":
                        return db.SabitGiderler.FirstOrDefault(x => x.Id == entityId && x.OdemeKodu == kod && x.BaslangicTarihi.Date == vadeGun);
                    case "Genel Ödeme":
                        return db.GenelOdemeler.FirstOrDefault(x => x.Id == entityId);
                    case "Kredi Kartı":
                        return db.KrediKartiOdemeleri.FirstOrDefault(x => x.Id == entityId);
                    case "Çek":
                        return db.Cekler.FirstOrDefault(x => x.Id == entityId);
                    case "Kredi":
                        string krediKoduFromVm = kod.Contains("-T") ? kod.Split('-')[0] : kod;
                        return db.KrediTaksitler.FirstOrDefault(x => x.Id == entityId && x.KrediKodu == krediKoduFromVm && x.Tarih.Date == vadeGun);
                    case "Değişken S. Ödeme":
                        return db.DegiskenOdemeler.FirstOrDefault(x => x.Id == entityId && x.OdemeKodu == kod && x.OdemeTarihi.Date == vadeGun);
                    default:
                        throw new ArgumentException($"Bilinmeyen kaynak modülü: {kaynakModul}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Entity bulunurken hata: {ex.Message}");
            }
        }

        public static void UpdatePaymentStatus(dynamic typedEntity, bool isPaid, DateTime? paymentDate, string? paymentBank, string? payingUser)
        {
            SetPropertyValue(typedEntity, "OdendiMi", isPaid);
            SetPropertyValue(typedEntity, "IsOdedildiMi", isPaid);
            SetPropertyValue(typedEntity, "OdenmeDurumu", isPaid);

            SetPropertyValue(typedEntity, "OdemeTarihi", isPaid ? paymentDate : null);
            SetPropertyValue(typedEntity, "TahsilTarihi", isPaid ? paymentDate : null);

            SetPropertyValue(typedEntity, "OdemeBankasi", isPaid ? paymentBank : null);
            SetPropertyValue(typedEntity, "OdeyenKullaniciAdi", isPaid ? payingUser : null);
        }

        private static void SetPropertyValue(object entity, string propertyName, object? value)
        {
            var propertyInfo = entity.GetType().GetProperty(propertyName);
            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(entity, value);
            }
        }
    }
}
