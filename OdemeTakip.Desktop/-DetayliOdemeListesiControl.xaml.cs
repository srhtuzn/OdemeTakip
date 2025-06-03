using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Desktop.ViewModels; // OdemeViewModel için
using OdemeTakip.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; // Brushes için

namespace OdemeTakip.Desktop
{
    public partial class DetayliOdemeListesiControl : UserControl
    {
        private List<OdemeViewModel> tumOdemelerSource = new List<OdemeViewModel>();

        public DetayliOdemeListesiControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            YenidenYukle();
        }

        public void YenidenYukle()
        {
            Yukle();
        }

        private void Yukle()
        {
            var db = App.DbContext;
            if (db == null)
            {
                MessageBox.Show("Veritabanı bağlantısı (DbContext) bulunamadı.", "Kritik Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var tumOdemelerSource = new List<OdemeViewModel>();

            DateTime simdi = DateTime.Today;
            int currentYear = simdi.Year;
            int currentMonth = simdi.Month;

            // ----- Sabit Giderler -----
            var sabitGiderler = db.SabitGiderler.AsNoTracking()
                .Include(x => x.Company)
                .Where(x => x.IsActive)
                .ToList();

            foreach (var gider in sabitGiderler)
            {
                // Manuel Giderler
                if (!gider.OtomatikMi)
                {
                    tumOdemelerSource.Add(new OdemeViewModel
                    {
                        Id = gider.Id,
                        KaynakId = gider.Id,
                        Kod = gider.OdemeKodu ?? string.Empty,
                        Aciklama = gider.GiderAdi ?? string.Empty,
                        Tarih = gider.BaslangicTarihi,
                        Tutar = gider.Tutar,
                        ParaBirimi = gider.ParaBirimi ?? "TL",
                        KaynakModul = "Sabit Ödeme",
                        OdenmeDurumu = gider.OdendiMi,
                        SirketAdi = gider.Company?.Name ?? string.Empty,
                        FaturaNo = gider.FaturaNo,
                        OdeyenKullaniciAdi = gider.OdeyenKullaniciAdi
                    });
                }
                else // Otomatik Giderler - Her ay ileri taşı
                {
                    DateTime iterTarih = gider.BaslangicTarihi;
                    DateTime limitTarih = simdi.AddMonths(2);

                    while (iterTarih < limitTarih)
                    {
                        var mevcutGider = db.SabitGiderler.AsNoTracking()
                            .FirstOrDefault(sg => sg.OdemeKodu == gider.OdemeKodu && sg.BaslangicTarihi == iterTarih);

                        bool odendiMi = mevcutGider != null && mevcutGider.OdendiMi;

                        tumOdemelerSource.Add(new OdemeViewModel
                        {
                            Id = mevcutGider?.Id ?? 0,
                            KaynakId = gider.Id,
                            Kod = gider.OdemeKodu ?? string.Empty,
                            Aciklama = gider.GiderAdi + $" ({iterTarih:MMMM yyyy})",
                            Tarih = iterTarih,
                            Tutar = gider.Tutar,
                            ParaBirimi = gider.ParaBirimi ?? "TL",
                            KaynakModul = "Sabit Ödeme",
                            OdenmeDurumu = odendiMi,
                            SirketAdi = gider.Company?.Name ?? string.Empty,
                            FaturaNo = gider.FaturaNo,
                            OdeyenKullaniciAdi = mevcutGider?.OdeyenKullaniciAdi
                        });

                        if (string.IsNullOrWhiteSpace(gider.Periyot) || (iterTarih.Year > limitTarih.Year + 1)) break;
                        iterTarih = gider.Periyot switch
                        {
                            "Aylık" => iterTarih.AddMonths(1),
                            "3 Aylık" => iterTarih.AddMonths(3),
                            "Yıllık" => iterTarih.AddYears(1),
                            _ => DateTime.MaxValue
                        };
                        if (iterTarih == DateTime.MaxValue) break;
                    }
                }
            }

            // ----- Genel Ödemeler -----
            var genelOdemeler = db.GenelOdemeler.AsNoTracking()
                .Include(x => x.Company)
                .Where(x => x.IsActive)
                .ToList();

            tumOdemelerSource.AddRange(genelOdemeler.Select(x => new OdemeViewModel
            {
                Id = x.Id,
                KaynakId = x.Id,
                Kod = x.OdemeKodu ?? string.Empty,
                Aciklama = x.Aciklama ?? string.Empty,
                Tarih = x.OdemeTarihi ?? DateTime.MinValue,
                Tutar = x.Tutar,
                ParaBirimi = x.ParaBirimi ?? "TL",
                KaynakModul = "Genel Ödeme",
                OdenmeDurumu = x.IsOdedildiMi,
                SirketAdi = x.Company?.Name ?? string.Empty,
                FaturaNo = x.FaturaNo,
                OdeyenKullaniciAdi = x.OdeyenKullaniciAdi
            }));

            // ----- Kredi Taksitleri -----
            var krediTaksitler = db.KrediTaksitler.AsNoTracking()
                .Include(x => x.Kredi)
                .Where(x => x.Kredi != null && x.Kredi.IsActive)
                .ToList();

            foreach (var taksit in krediTaksitler)
            {
                tumOdemelerSource.Add(new OdemeViewModel
                {
                    Id = taksit.Id,
                    KaynakId = taksit.Id,
                    Kod = $"{taksit.KrediKodu}-T{taksit.TaksitNo:D2}",
                    Aciklama = $"{taksit.Kredi?.KrediKonusu} (Taksit {taksit.TaksitNo})",
                    Tarih = taksit.Tarih,
                    Tutar = taksit.Tutar,
                    ParaBirimi = taksit.Kredi?.ParaBirimi ?? "TL",
                    KaynakModul = "Kredi",
                    OdenmeDurumu = taksit.OdenmeDurumu,
                    OdemeTarihi = taksit.OdenmeTarihi,
                    OdemeBankasi = taksit.OdemeBankasi,
                    SirketAdi = taksit.Kredi?.SirketAdi ?? string.Empty,
                    OdeyenKullaniciAdi = taksit.OdeyenKullaniciAdi
                });
            }

            // ----- Kredi Kartı Ödemeleri -----
            var krediKartiOdemeleri = db.KrediKartiOdemeleri.AsNoTracking()
                .Where(x => x.IsActive)
                .ToList();

            tumOdemelerSource.AddRange(krediKartiOdemeleri.Select(x => new OdemeViewModel
            {
                Id = x.Id,
                KaynakId = x.Id,
                Kod = x.OdemeKodu ?? string.Empty,
                Aciklama = x.Aciklama ?? string.Empty,
                Tarih = x.OdemeTarihi ?? DateTime.MinValue,
                Tutar = x.Tutar,
                ParaBirimi = "TL",
                KaynakModul = "Kredi Kartı",
                OdenmeDurumu = x.OdenmeDurumu,
                SirketAdi = x.Company != null ? (x.Company.Name ?? string.Empty) : string.Empty, // 🔥 Değişti
                OdeyenKullaniciAdi = x.OdeyenKullaniciAdi
            }));


            // ----- Çekler -----
            var cekler = db.Cekler.AsNoTracking()
                .Include(x => x.CariFirma)
                .Where(x => x.IsActive)
                .ToList();

            tumOdemelerSource.AddRange(cekler.Select(x => new OdemeViewModel
            {
                Id = x.Id,
                KaynakId = x.Id,
                Kod = x.CekKodu ?? string.Empty,
                Aciklama = $"Çek No: {x.CekNumarasi}",
                Tarih = x.VadeTarihi,
                Tutar = x.Tutar,
                ParaBirimi = x.ParaBirimi ?? "TL",
                KaynakModul = "Çek",
                OdenmeDurumu = x.OdenmeDurumu,
                SirketAdi = x.CariFirma?.Name ?? string.Empty,
                OdeyenKullaniciAdi = x.OdeyenKullaniciAdi
            }));

            // ----- Değişken Ödemeler -----
            var degiskenOdemeler = db.DegiskenOdemeler.AsNoTracking()
                .Include(x => x.Company)
                .Where(x => x.IsActive)
                .ToList();

            tumOdemelerSource.AddRange(degiskenOdemeler.Select(x => new OdemeViewModel
            {
                Id = x.Id,
                KaynakId = x.Id,
                Kod = x.OdemeKodu,
                Aciklama = x.GiderTuru + (string.IsNullOrWhiteSpace(x.Aciklama) ? "" : $" - {x.Aciklama}"),
                Tarih = x.OdemeTarihi,
                Tutar = x.Tutar,
                ParaBirimi = x.ParaBirimi ?? "TL",
                KaynakModul = "Değişken S. Ödeme",
                OdenmeDurumu = x.OdenmeDurumu,
                SirketAdi = x.Company?.Name ?? string.Empty,
                FaturaNo = x.FaturaNo,
                OdeyenKullaniciAdi = x.OdeyenKullaniciAdi
            }));

            // -- Bekleyen ve Geçmiş Odemeleri Ayır --
            var bekleyenOdemeler = tumOdemelerSource.Where(x => !x.OdenmeDurumu).OrderBy(x => x.Tarih).ToList();
            var gecmisOdemeler = tumOdemelerSource.Where(x => x.OdenmeDurumu).OrderByDescending(x => x.Tarih).ToList();

            dgBekleyenOdemeler.ItemsSource = bekleyenOdemeler;
            dgGecmisOdemeler.ItemsSource = gecmisOdemeler;
        }

        // GÜNCELLENMİŞ ARAMA METODU
        private void TxtArama_TextChanged(object sender, TextChangedEventArgs? e)
        {
            if (txtArama == null || dgBekleyenOdemeler == null || dgGecmisOdemeler == null) return;

            string filtre = txtArama.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(filtre))
            {
                // Arama kutusu boşsa tam listeyi ver
                dgBekleyenOdemeler.ItemsSource = tumOdemelerSource
                    .Where(x => !x.OdenmeDurumu)
                    .OrderBy(x => x.Tarih)
                    .ToList();

                dgGecmisOdemeler.ItemsSource = tumOdemelerSource
                    .Where(x => x.OdenmeDurumu)
                    .OrderByDescending(x => x.Tarih)
                    .ToList();
            }
            else
            {
                // Arama var, filtreli liste
                var filtrelenmis = tumOdemelerSource.Where(x =>
                    (x.Kod != null && x.Kod.ToLower().Contains(filtre)) ||
                    (x.Aciklama != null && x.Aciklama.ToLower().Contains(filtre)) ||
                    (x.KaynakModul != null && x.KaynakModul.ToLower().Contains(filtre)) ||
                    (x.SirketAdi != null && x.SirketAdi.ToLower().Contains(filtre)) ||
                    (x.OdeyenKullaniciAdi != null && x.OdeyenKullaniciAdi.ToLower().Contains(filtre))
                ).ToList();

                dgBekleyenOdemeler.ItemsSource = filtrelenmis
                    .Where(x => !x.OdenmeDurumu)
                    .OrderBy(x => x.Tarih)
                    .ToList();

                dgGecmisOdemeler.ItemsSource = filtrelenmis
                    .Where(x => x.OdenmeDurumu)
                    .OrderByDescending(x => x.Tarih)
                    .ToList();
            }
        }




        private bool IsaretleOdendi(AppDbContext db, OdemeViewModel seciliViewModel, DateTime? secilenTarih, string? secilenHesapKodu)
        {
            if (seciliViewModel == null)
                return false;

            object? entity = BulEntity(db, seciliViewModel.KaynakModul, seciliViewModel.Id, seciliViewModel.Kod, seciliViewModel.Tarih);

            if (entity == null)
            {
                MessageBox.Show($"Ödenecek kayıt bulunamadı (Debug Bilgisi):\nModül: {seciliViewModel.KaynakModul}, ID: {seciliViewModel.Id}, Kod: {seciliViewModel.Kod}, Vade: {seciliViewModel.Tarih:dd.MM.yyyy}", "Kayıt Bulunamadı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            try
            {
                dynamic typedEntity = entity;
                string? odeyenKullanici = App.CurrentUser?.Username;
                UpdatePaymentStatus(typedEntity, true, secilenTarih ?? DateTime.Now, secilenHesapKodu, odeyenKullanici);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ödeme işaretleme işleminde hata: {ex.Message}\n{ex.StackTrace}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        private void BtnOdenme_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var seciliViewModel = button.Tag as OdemeViewModel;
            if (seciliViewModel == null) return;

            var db = App.DbContext;

            if (seciliViewModel.OdenmeDurumu)
            {
                // GERİ ALMA (SADECE ADMIN)
                if (App.CurrentUser?.Role != UserRole.Admin)
                {
                    MessageBox.Show("Bu işlemi sadece Admin yapabilir.", "Yetki Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (GeriAlOdemeDurumu(db, seciliViewModel))
                {
                    MessageBox.Show("Ödeme geri alındı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                    YenidenYukle();
                }
            }
            else
            {
                // ÖDEME YAP
                List<BankaHesabi> bankaHesaplari;
                try
                {
                    bankaHesaplari = seciliViewModel.KaynakModul == "Kredi Kartı" && !string.IsNullOrEmpty(seciliViewModel.SirketAdi)
                       ? db.BankaHesaplari.Include(x => x.Company).Where(x => x.IsActive && x.Company != null && x.Company.Name == seciliViewModel.SirketAdi).ToList()
                       : db.BankaHesaplari.Where(x => x.IsActive).ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Banka hesapları yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var popup = new OdemeOnayPopup(bankaHesaplari);
                if (popup.ShowDialog() == true)
                {
                    if (popup.SecilenTarih == null || string.IsNullOrEmpty(popup.SecilenHesapKodu))
                    {
                        MessageBox.Show("Tarih ve banka seçimi yapılmadı.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (IsaretleOdendi(db, seciliViewModel, popup.SecilenTarih, popup.SecilenHesapKodu))
                    {
                        MessageBox.Show("Ödeme başarıyla 'Ödendi' olarak işaretlendi ✅", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        YenidenYukle();
                    }
                }
            }
        }

        private bool GeriAlOdemeDurumu(AppDbContext db, OdemeViewModel vm)
        {
            object? entity = BulEntity(db, vm.KaynakModul, vm.Id, vm.Kod, vm.Tarih);
            if (entity == null)
            {
                MessageBox.Show($"Geri alınacak kayıt bulunamadı (Debug Bilgisi):\nModül: {vm.KaynakModul}, ID: {vm.Id}, Kod: {vm.Kod}, Vade: {vm.Tarih:dd.MM.yyyy}", "Kayıt Bulunamadı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            try
            {
                dynamic typedEntity = entity;
                UpdatePaymentStatus(typedEntity, false, null, null, null);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Geri alma işleminde hata: {ex.Message}\n{ex.StackTrace}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private bool IsaretleOdendi(AppDbContext db, OdemeViewModel vm, DateTime odemeTarihi, string odemeBankasi)
        {
            object? entity = BulEntity(db, vm.KaynakModul, vm.Id, vm.Kod, vm.Tarih);
            if (entity == null)
            {
                MessageBox.Show($"Ödenecek kayıt bulunamadı (Debug Bilgisi):\nModül: {vm.KaynakModul}, ID: {vm.Id}, Kod: {vm.Kod}, Vade: {vm.Tarih:dd.MM.yyyy}", "Kayıt Bulunamadı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            try
            {
                dynamic typedEntity = entity;
                string? odeyenKullanici = App.CurrentUser?.Username;
                UpdatePaymentStatus(typedEntity, true, odemeTarihi, odemeBankasi, odeyenKullanici);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ödeme işaretleme işleminde hata: {ex.Message}\n{ex.StackTrace}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void SetPropertyValue(object entity, string propertyName, object? value)
        {
            var propertyInfo = entity.GetType().GetProperty(propertyName);
            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                try
                {
                    propertyInfo.SetValue(entity, value);
                }
                catch (ArgumentException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SetPropertyValue Hata: Property '{propertyName}', Değer '{value}', Tip '{value?.GetType()}', Beklenen Tip '{propertyInfo.PropertyType}'. Hata: {ex.Message}");
                }
            }
        }

        private void UpdatePaymentStatus(dynamic typedEntity, bool isPaid, DateTime? paymentDate, string? paymentBank, string? payingUser)
        {
            SetPropertyValue(typedEntity, "OdendiMi", isPaid);
            SetPropertyValue(typedEntity, "IsOdedildiMi", isPaid);
            SetPropertyValue(typedEntity, "OdenmeDurumu", isPaid);

            SetPropertyValue(typedEntity, "OdemeTarihi", isPaid ? paymentDate : null);
            SetPropertyValue(typedEntity, "TahsilTarihi", isPaid ? paymentDate : null);

            SetPropertyValue(typedEntity, "OdemeBankasi", isPaid ? paymentBank : null);
            SetPropertyValue(typedEntity, "OdeyenKullaniciAdi", isPaid ? payingUser : null);
        }

        private object? BulEntity(AppDbContext db, string kaynakModul, int entityId, string kod, DateTime vadeTarihi)
        {
            DateTime vadeGun = vadeTarihi.Date;
            try
            {
                switch (kaynakModul)
                {
                    case "Sabit Ödeme":
                        // SabitGider.BaslangicTarihi non-nullable DateTime
                        return db.SabitGiderler.FirstOrDefault(x => x.Id == entityId && x.OdemeKodu == kod && x.BaslangicTarihi.Date == vadeGun);
                    case "Genel Ödeme":
                        return db.GenelOdemeler.FirstOrDefault(x => x.Id == entityId);
                    case "Kredi Kartı":
                        return db.KrediKartiOdemeleri.FirstOrDefault(x => x.Id == entityId);
                    case "Çek":
                        return db.Cekler.FirstOrDefault(x => x.Id == entityId);
                    case "Kredi":
                        string krediKoduFromVm = kod.Contains("-T") ? kod.Split('-')[0] : kod;
                        // KrediTaksit.Tarih non-nullable DateTime
                        return db.KrediTaksitler.FirstOrDefault(x => x.Id == entityId && x.KrediKodu == krediKoduFromVm && x.Tarih.Date == vadeGun);
                    case "Değişken S. Ödeme":
                        // DegiskenOdeme.OdemeTarihi non-nullable DateTime
                        return db.DegiskenOdemeler.FirstOrDefault(x => x.Id == entityId && x.OdemeKodu == kod && x.OdemeTarihi.Date == vadeGun);
                    default:
                        MessageBox.Show($"Bilinmeyen kaynak modülü: {kaynakModul}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Entity bulunurken hata: {ex.Message}\nModül: {kaynakModul}, ID: {entityId}, Kod: {kod}, Vade: {vadeTarihi:dd.MM.yyyy}", "Veri Erişim Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}