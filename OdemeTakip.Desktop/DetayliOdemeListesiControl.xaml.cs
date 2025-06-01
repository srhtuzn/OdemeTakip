using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Desktop.ViewModels; // OdemeViewModel için (Namespace'i kontrol edin)
using OdemeTakip.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; // Brushes için

namespace OdemeTakip.Desktop // XAML'deki x:Class ile aynı namespace olmalı
{
    public partial class DetayliOdemeListesiControl : UserControl
    {
        private List<OdemeViewModel> tumOdemelerSource = new();

        public DetayliOdemeListesiControl()
        {
            InitializeComponent();
            // DataGrid için gerekli Converter'ları Resources'a ekleyebilirsiniz (eğer henüz eklenmediyse)
            // Örneğin:
            // if (this.Resources["DurumYaziConverter"] == null)
            //     this.Resources.Add("DurumYaziConverter", new DurumYaziConverter());
            // if (this.Resources["DurumRenkConverter"] == null)
            //     this.Resources.Add("DurumRenkConverter", new DurumRenkConverter());
        }

        // XAML'de Loaded="UserControl_Loaded" olarak tanımlı
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            YenidenYukle();
        }

        public void YenidenYukle()
        {
            Yukle();
            // Arama kutusu boşsa tüm listeyi göster, doluysa filtrelemeyi tekrar uygula
            TxtArama_TextChanged(txtArama, null); // XAML'deki TextChanged olay adıyla eşleşiyor
        }

        private void Yukle()
        {
            var db = App.DbContext;
            if (db == null)
            {
                MessageBox.Show("Veritabanı bağlantısı (DbContext) bulunamadı.", "Kritik Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            tumOdemelerSource.Clear();

            DateTime simdi = DateTime.Today;

            // ----- Sabit Giderler -----
            var aktifSabitGiderler = db.SabitGiderler
                .Include(x => x.Company)
                .Where(x => x.IsActive)
                .ToList();

            foreach (var gider in aktifSabitGiderler)
            {
                if (!gider.OtomatikMi)
                {
                    DateTime vadeTarihi = gider.BaslangicTarihi;
                    DateTime vadeAyininBiri = new(vadeTarihi.Year, vadeTarihi.Month, 1);
                    if (!gider.OdendiMi && simdi >= vadeAyininBiri)
                    {
                        tumOdemelerSource.Add(new OdemeViewModel
                        {
                            Id = gider.Id,
                            KaynakId = gider.Id,
                            Kod = gider.OdemeKodu ?? "",
                            Aciklama = gider.GiderAdi ?? "",
                            Tarih = vadeTarihi,
                            Tutar = gider.Tutar,
                            ParaBirimi = gider.ParaBirimi ?? "TL",
                            KaynakModul = "Sabit Ödeme",
                            OdenmeDurumu = gider.OdendiMi,
                            SirketAdi = gider.Company?.Name ?? "",
                            FaturaNo = gider.FaturaNo,
                            OdeyenKullaniciAdi = gider.OdeyenKullaniciAdi
                        });
                    }
                }
                else // Otomatik sabit giderler
                {
                    DateTime iterTarih = gider.BaslangicTarihi;
                    DateTime gelecekKontrolLimiti = simdi.AddMonths(2); // Örn: 2 ay sonrasına kadar bak

                    while (iterTarih < gelecekKontrolLimiti)
                    {
                        DateTime iterAyininBiri = new(iterTarih.Year, iterTarih.Month, 1);
                        if (simdi >= iterAyininBiri) // Ödeme ayı başladıysa veya geçmişteyse
                        {
                            // Bu periyoda ait ödeme yapılıp yapılmadığını SabitGiderler tablosundan kontrol et
                            // Bu, her otomatik ödeme örneği için SabitGiderler tablosunda ayrı bir kayıt olduğunu varsayar.
                            var instancePayment = db.SabitGiderler
                                .AsNoTracking() // Performans için, zaten değişiklik yapmayacağız
                                .FirstOrDefault(sg => sg.OdemeKodu == gider.OdemeKodu && sg.BaslangicTarihi == iterTarih);

                            bool buPeriyotOdendi = instancePayment != null && instancePayment.OdendiMi;

                            if (!buPeriyotOdendi)
                            {
                                tumOdemelerSource.Add(new OdemeViewModel
                                {
                                    Id = instancePayment?.Id ?? gider.Id, // Eğer instance varsa onun Id'si, yoksa şablon Id'si
                                    KaynakId = gider.Id, // Ana şablonun Id'si
                                    Kod = gider.OdemeKodu ?? "",
                                    Aciklama = $"{gider.GiderAdi} ({iterTarih:MMMM yyyy})",
                                    Tarih = iterTarih,
                                    Tutar = gider.Tutar,
                                    ParaBirimi = gider.ParaBirimi ?? "TL",
                                    KaynakModul = "Sabit Ödeme",
                                    OdenmeDurumu = false,
                                    SirketAdi = gider.Company?.Name ?? "",
                                    FaturaNo = gider.FaturaNo,
                                    OdeyenKullaniciAdi = instancePayment?.OdeyenKullaniciAdi // Instance'tan al
                                });
                            }
                        }
                        if (iterTarih.Year > gelecekKontrolLimiti.Year && iterTarih.Month > gelecekKontrolLimiti.Month && iterTarih.Day > gelecekKontrolLimiti.Day) break;

                        iterTarih = gider.Periyot switch
                        {
                            "Aylık" => iterTarih.AddMonths(1),
                            "3 Aylık" => iterTarih.AddMonths(3),
                            "Yıllık" => iterTarih.AddYears(1),
                            _ => iterTarih.AddYears(100)
                        };
                    }
                }
            }

            // ----- Genel Ödemeler -----
            tumOdemelerSource.AddRange(db.GenelOdemeler
                .Include(x => x.Company)
                .Where(x => x.IsActive && !x.IsOdedildiMi && x.OdemeTarihi != null &&
                            simdi >= new DateTime(x.OdemeTarihi.Value.Year, x.OdemeTarihi.Value.Month, 1))
                .Select(x => new OdemeViewModel
                {
                    Id = x.Id,
                    KaynakId = x.Id,
                    Kod = x.OdemeKodu ?? "",
                    Aciklama = x.Aciklama ?? "",
                    Tarih = x.OdemeTarihi ?? DateTime.MinValue,
                    Tutar = x.Tutar,
                    ParaBirimi = x.ParaBirimi ?? "TL",
                    KaynakModul = "Genel Ödeme",
                    OdenmeDurumu = x.IsOdedildiMi,
                    SirketAdi = x.Company != null ? x.Company.Name : "",
                    FaturaNo = x.FaturaNo,
                    OdeyenKullaniciAdi = x.OdeyenKullaniciAdi
                }));

            // ----- Kredi Taksitleri -----
            var tumKrediTaksitleri = db.KrediTaksitler
                .Include(x => x.Kredi).ThenInclude(k => k.SirketAdi)
                .Where(x => x.Kredi.IsActive && !x.OdenmeDurumu)
                .ToList();
            var gosterilecekKrediTaksitleri = tumKrediTaksitleri
                .Where(t => simdi >= new DateTime(t.Tarih.Year, t.Tarih.Month, 1))
                .GroupBy(x => x.KrediKodu)
                .Select(g => g.OrderBy(x => x.Tarih).First())
                .ToList();
            foreach (var taksit in gosterilecekKrediTaksitleri)
            {
                tumOdemelerSource.Add(new OdemeViewModel
                {
                    Id = taksit.Id,
                    KaynakId = taksit.Id,
                    Kod = $"{taksit.KrediKodu}-T{taksit.TaksitNo:D2}",
                    Aciklama = $"{taksit.Kredi.KrediKonusu} (Taksit {taksit.TaksitNo})",
                    Tarih = taksit.Tarih,
                    Tutar = taksit.Tutar,
                    ParaBirimi = taksit.Kredi.ParaBirimi ?? "TL",
                    KaynakModul = "Kredi",
                    OdenmeDurumu = taksit.OdenmeDurumu,
                    OdemeTarihi = taksit.OdenmeTarihi,
                    OdemeBankasi = taksit.OdemeBankasi,
                    SirketAdi = taksit.Kredi.SirketAdi ?? "",
                    OdeyenKullaniciAdi = taksit.OdeyenKullaniciAdi
                });
            }

            // ----- Kredi Kartı Ödemeleri -----
            tumOdemelerSource.AddRange(db.KrediKartiOdemeleri
                .Where(x => x.IsActive && !x.OdenmeDurumu && x.OdemeTarihi != null &&
                            simdi >= new DateTime(x.OdemeTarihi.Value.Year, x.OdemeTarihi.Value.Month, 1))
                .Select(x => new OdemeViewModel
                {
                    Id = x.Id,
                    KaynakId = x.Id,
                    Kod = x.OdemeKodu ?? "",
                    Aciklama = x.Aciklama ?? "",
                    Tarih = x.OdemeTarihi ?? DateTime.MinValue,
                    Tutar = x.Tutar,
                    ParaBirimi = "TL",
                    KaynakModul = "Kredi Kartı",
                    OdenmeDurumu = x.OdenmeDurumu,
                    SirketAdi = x.OwnerName,
                    OdeyenKullaniciAdi = x.OdeyenKullaniciAdi
                }));

            // ----- Çekler -----
            tumOdemelerSource.AddRange(db.Cekler
                .Include(x => x.CariFirma)
                .Where(x => x.IsActive && !x.OdenmeDurumu &&
                            simdi >= new DateTime(x.VadeTarihi.Year, x.VadeTarihi.Month, 1))
                .Select(x => new OdemeViewModel
                {
                    Id = x.Id,
                    KaynakId = x.Id,
                    Kod = x.CekKodu ?? "",
                    Aciklama = $"Çek No: {x.CekNumarasi}",
                    Tarih = x.VadeTarihi,
                    Tutar = x.Tutar,
                    ParaBirimi = x.ParaBirimi ?? "TL",
                    KaynakModul = "Çek",
                    OdenmeDurumu = x.OdenmeDurumu,
                    SirketAdi = x.CariFirma != null ? x.CariFirma.Name : "",
                    OdeyenKullaniciAdi = x.OdeyenKullaniciAdi
                }));

            // ----- Değişken Ödemeler -----
            tumOdemelerSource.AddRange(db.DegiskenOdemeler
                .Include(x => x.Company)
                .Where(x => x.IsActive && !x.OdenmeDurumu &&
                            simdi >= new DateTime(x.OdemeTarihi.Year, x.OdemeTarihi.Month, 1))
                .Select(x => new OdemeViewModel
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
                    OdemeTarihi = x.OdemeTarihi,
                    OdemeBankasi = x.OdemeBankasi,
                    SirketAdi = x.Company != null ? x.Company.Name : "",
                    FaturaNo = x.FaturaNo,
                    OdeyenKullaniciAdi = x.OdeyenKullaniciAdi
                }));

            dgOdemeler.ItemsSource = tumOdemelerSource.OrderBy(x => x.Tarih).ToList();
        }

        // XAML'deki TextChanged olay adıyla eşleşiyor
        private void TxtArama_TextChanged(object sender, TextChangedEventArgs? e) // e parametresi nullable olabilir
        {
            string filtre = txtArama.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(filtre))
            {
                dgOdemeler.ItemsSource = tumOdemelerSource.OrderBy(x => x.Tarih).ToList();
            }
            else
            {
                var filtrelenmis = tumOdemelerSource.Where(x =>
                    (x.Kod?.ToLower().Contains(filtre) ?? false) ||
                    (x.Aciklama?.ToLower().Contains(filtre) ?? false) ||
                    (x.KaynakModul?.ToLower().Contains(filtre) ?? false) ||
                    (x.SirketAdi?.ToLower().Contains(filtre) ?? false) ||
                    (x.OdeyenKullaniciAdi?.ToLower().Contains(filtre) ?? false)
                ).OrderBy(x => x.Tarih).ToList();
                dgOdemeler.ItemsSource = filtrelenmis;
            }
        }

        private void BtnOdenme_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var seciliViewModel = btn?.Tag as OdemeViewModel;
            if (seciliViewModel == null) return;

            var db = App.DbContext;
            if (db == null) return;


            if (seciliViewModel.OdenmeDurumu) // GERİ ALMA İŞLEMİ
            {
                if (App.CurrentUser == null || App.CurrentUser.Role != UserRole.Admin)
                {
                    MessageBox.Show("Bu işlemi gerçekleştirmek için Admin yetkisine sahip olmanız gerekmektedir.", "Yetki Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var geriAlOnay = MessageBox.Show(
                    $"'{seciliViewModel.Kod} - {seciliViewModel.Aciklama}' ödemesi zaten 'Ödendi' olarak işaretli.\nÖdeyen: {seciliViewModel.OdeyenKullaniciAdi}\nBu durumu geri almak istiyor musunuz?",
                    "Geri Alma Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (geriAlOnay != MessageBoxResult.Yes) return;

                var sifrePopup = new SifreDogrulamaPopup();
                if (sifrePopup.ShowDialog() == true && sifrePopup.SifreDogru)
                {
                    bool basarili = GeriAlOdemeDurumu(db, seciliViewModel);
                    if (basarili)
                    {
                        MessageBox.Show("Ödeme durumu başarıyla geri alındı 🔄", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                        YenidenYukle();
                    }
                    else
                    {
                        MessageBox.Show("Ödeme durumu geri alınırken bir hata oluştu veya ilgili kayıt bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                return;
            }

            // ÖDEME İŞARETLEME İŞLEMİ
            var odemeOnay = MessageBox.Show(
                $"Bu ödeme '{seciliViewModel.Kod} - {seciliViewModel.Aciklama}' için 'ödendi' olarak işaretlensin mi?",
                "Ödeme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (odemeOnay != MessageBoxResult.Yes) return;

            List<BankaHesabi> bankaHesaplari;
            try
            {
                bankaHesaplari = seciliViewModel.KaynakModul == "Kredi Kartı" && !string.IsNullOrEmpty(seciliViewModel.SirketAdi)
                   ? db.BankaHesaplari.Include(x => x.Company).Where(x => x.IsActive && x.Company.Name == seciliViewModel.SirketAdi).ToList()
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
                bool basarili = IsaretleOdendi(db, seciliViewModel, popup.SecilenTarih, popup.SecilenHesapKodu);
                if (basarili)
                {
                    MessageBox.Show("Ödeme başarıyla 'Ödendi' olarak işaretlendi ✅", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    YenidenYukle();
                }
                else
                {
                    MessageBox.Show("Ödeme işaretlenirken bir hata oluştu veya ilgili kayıt bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool GeriAlOdemeDurumu(AppDbContext db, OdemeViewModel vm)
        {
            object? entity = BulEntity(db, vm.KaynakModul, vm.Id, vm.Kod, vm.Tarih);
            if (entity == null) return false;
            try
            {
                dynamic typedEntity = entity;

                UpdatePaymentStatus(typedEntity, false, null, null, null); // OdeyenKullaniciAdi null olacak

                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Hata loglama veya kullanıcıya bildirme
                MessageBox.Show($"Geri alma işleminde hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private bool IsaretleOdendi(AppDbContext db, OdemeViewModel vm, DateTime odemeTarihi, string odemeBankasi)
        {
            object? entity = BulEntity(db, vm.KaynakModul, vm.Id, vm.Kod, vm.Tarih);
            if (entity == null) return false;
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
                MessageBox.Show($"Ödeme işaretleme işleminde hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Ortak özellik güncelleme metodu
        private void UpdatePaymentStatus(dynamic typedEntity, bool isPaid, DateTime? paymentDate, string? paymentBank, string? payingUser)
        {
            if (typedEntity.GetType().GetProperty("OdendiMi") != null) typedEntity.OdendiMi = isPaid; // SabitGider için
            if (typedEntity.GetType().GetProperty("IsOdedildiMi") != null) typedEntity.IsOdedildiMi = isPaid; // GenelOdeme için
            if (typedEntity.GetType().GetProperty("OdenmeDurumu") != null) typedEntity.OdenmeDurumu = isPaid; // Diğerleri için

            if (typedEntity.GetType().GetProperty("OdemeTarihi") != null) typedEntity.OdemeTarihi = isPaid ? paymentDate : null;
            if (typedEntity.GetType().GetProperty("TahsilTarihi") != null) typedEntity.TahsilTarihi = isPaid ? paymentDate : null; // Çek için

            if (typedEntity.GetType().GetProperty("OdemeBankasi") != null) typedEntity.OdemeBankasi = isPaid ? paymentBank : null;
            if (typedEntity.GetType().GetProperty("OdeyenKullaniciAdi") != null) typedEntity.OdeyenKullaniciAdi = isPaid ? payingUser : null;
        }


        private object? BulEntity(AppDbContext db, string kaynakModul, int entityId, string kod, DateTime vadeTarihi)
        {
            // ViewModel'deki Id, orijinal entity'nin PK'sı olmalı.
            // Bu Id'yi OdemeViewModel'e eklediğimizi varsayıyorum.
            switch (kaynakModul)
            {
                case "Sabit Ödeme":
                    // Otomatik sabit giderler için OdemeKodu ve vadeTarihi önemli.
                    // Eğer her periyot için ayrı kayıt tutulmuyorsa (şablon üzerinden gidiyorsa),
                    // o zaman Id (şablon Id'si) yeterli olabilir.
                    // Şu anki Yukle() mantığı, otomatikler için ayrı kayıt arıyor gibi.
                    return db.SabitGiderler.FirstOrDefault(x => x.Id == entityId && x.OdemeKodu == kod && x.BaslangicTarihi.Date == vadeTarihi.Date);
                case "Genel Ödeme":
                    return db.GenelOdemeler.FirstOrDefault(x => x.Id == entityId);
                case "Kredi Kartı":
                    return db.KrediKartiOdemeleri.FirstOrDefault(x => x.Id == entityId);
                case "Çek":
                    return db.Cekler.FirstOrDefault(x => x.Id == entityId);
                case "Kredi":
                    string krediKoduFromVm = kod.Contains("-T") ? kod.Split('-')[0] : kod;
                    return db.KrediTaksitler.FirstOrDefault(x => x.Id == entityId && x.KrediKodu == krediKoduFromVm && x.Tarih.Date == vadeTarihi.Date);
                case "Değişken S. Ödeme":
                    return db.DegiskenOdemeler.FirstOrDefault(x => x.Id == entityId);
                default:
                    MessageBox.Show($"Bilinmeyen kaynak modülü: {kaynakModul}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
            }
        }
    }
}