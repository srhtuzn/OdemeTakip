using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Desktop.ViewModels; // OdemeViewModel için
using OdemeTakip.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input; // MouseButtonEventArgs için
using System.Windows.Media; // Brushes için

namespace OdemeTakip.Desktop
{
    public partial class TumOdemelerView : UserControl
    {
        private readonly List<OdemeViewModel> _tumOdemelerSource = new();


        public TumOdemelerView()
        {
            InitializeComponent();
            CmbOdemeDurumu.SelectedIndex = 0; // Varsayılan "Ödenecekler"
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            FiltreleVeYukle();
        }

        private void BtnFiltreleYenile_Click(object sender, RoutedEventArgs e)
        {
            FiltreleVeYukle();
        }

        private void TxtArama_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Anlık arama için basit bir filtreleme, daha iyisi FiltreleVeYukle'yi çağırmak olabilir
            // veya ayrı bir filtreleme metodu yazmak. Şimdilik basit bırakalım.
            // FiltreleVeYukle(); // Her harf girişinde yükleme yapmak yavaş olabilir.
            ApplyClientSideFilter();
        }

        private void CmbOdemeDurumu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) // UserControl yüklendikten sonra çalışsın
            {
                FiltreleVeYukle();
            }
        }

        private void ApplyClientSideFilter()
        {
            if (DgOdemeler == null) return;

            string filtreMetni = TxtArama.Text.Trim();
            ICollectionView view = CollectionViewSource.GetDefaultView(DgOdemeler.ItemsSource);

            if (string.IsNullOrEmpty(filtreMetni))
            {
                if (view != null) view.Filter = null;
            }
            else
            {
                if (view != null)
                {
                    view.Filter = item => item is OdemeViewModel vm &&
                        (vm.Kod?.Contains(filtreMetni, StringComparison.OrdinalIgnoreCase) == true ||
                         vm.Aciklama?.Contains(filtreMetni, StringComparison.OrdinalIgnoreCase) == true ||
                         vm.KaynakModul?.Contains(filtreMetni, StringComparison.OrdinalIgnoreCase) == true ||
                         vm.SirketAdi?.Contains(filtreMetni, StringComparison.OrdinalIgnoreCase) == true ||
                         vm.OdeyenKullaniciAdi?.Contains(filtreMetni, StringComparison.OrdinalIgnoreCase) == true);
                }
            }
        }



        public void FiltreleVeYukle()
        {
            var db = App.DbContext;
            if (db == null)
            {
                MessageBox.Show("Veritabanı bağlantısı bulunamadı.", "Kritik Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                _tumOdemelerSource.Clear();
                DgOdemeler.ItemsSource = null;
                return;
            }

            _tumOdemelerSource.Clear();
            DateTime simdi = DateTime.Today;

            bool? odemeDurumuFiltresi = CmbOdemeDurumu.SelectedItem switch
            {
                ComboBoxItem item when item.Content.ToString() == "Ödenecekler" => false,
                ComboBoxItem item when item.Content.ToString() == "Ödenmişler" => true,
                _ => null
            };

            // Çekler
            _tumOdemelerSource.AddRange(db.Cekler.AsNoTracking()
                .Include(x => x.CariFirma)
                .Where(x => x.IsActive && (!odemeDurumuFiltresi.HasValue || x.OdenmeDurumu == odemeDurumuFiltresi))
                .Select(MapCek));

            // Genel Ödemeler
            _tumOdemelerSource.AddRange(db.GenelOdemeler.AsNoTracking()
                .Include(x => x.Company)
                .Include(x => x.CariFirma) // ✅ EKLENDİ
                .Where(x => x.IsActive && (!odemeDurumuFiltresi.HasValue || x.IsOdedildiMi == odemeDurumuFiltresi))
                .Select(MapGenelOdeme));

            // Değişken Ödemeler
            _tumOdemelerSource.AddRange(db.DegiskenOdemeler.AsNoTracking()
                .Include(x => x.Company)
                .Include(x => x.CariFirma) // ✅ EKLENDİ
                .Where(x => x.IsActive && (!odemeDurumuFiltresi.HasValue || x.OdenmeDurumu == odemeDurumuFiltresi))
                .Select(MapDegiskenOdeme));

            // Kredi — Her Kredi'den sadece ilk ödenmemiş
            var krediTaksitler = db.KrediTaksitler.AsNoTracking()
                .Include(x => x.Kredi)
                .Where(x => x.Kredi.IsActive && (!odemeDurumuFiltresi.HasValue || x.OdenmeDurumu == odemeDurumuFiltresi))
                .AsEnumerable()
                .GroupBy(x => x.KrediId)
                .Select(g => g.OrderBy(t => t.Tarih).FirstOrDefault())
                .Where(t => t != null)
                .Select(t => MapKrediTaksit(t!));

            _tumOdemelerSource.AddRange(krediTaksitler);

            // Sabit Ödeme — Her şablondan sadece ilk ödenmemiş
            var sabitGiderler = db.SabitGiderler.AsNoTracking()
                .Include(x => x.Company)
                .Include(x => x.CariFirma) // ✅ EKLENDİ
                .Where(x => x.IsActive && (!odemeDurumuFiltresi.HasValue || x.OdendiMi == odemeDurumuFiltresi))
                .AsEnumerable()
                .GroupBy(x => x.OdemeKodu)
                .Select(g => g.OrderBy(t => t.BaslangicTarihi).FirstOrDefault())
                .Where(t => t != null)
                .Select(g => MapSabitGider(g!, g!.BaslangicTarihi, g.OdendiMi));

            _tumOdemelerSource.AddRange(sabitGiderler);

            // Kredi Kartı — Şirket ve Kart bazlı bu ayın kaydı
            var ayBaslangic = new DateTime(simdi.Year, simdi.Month, 1);
            var aySonu = ayBaslangic.AddMonths(1).AddDays(-1);

            var krediKartiOdemeler = db.KrediKartiOdemeleri.AsNoTracking()
                .Include(x => x.Company)
                .Where(x => x.IsActive &&
                            (!odemeDurumuFiltresi.HasValue || x.OdenmeDurumu == odemeDurumuFiltresi) &&
                            x.OdemeTarihi >= ayBaslangic && x.OdemeTarihi <= aySonu)
                .Select(MapKrediKartiOdeme);

            _tumOdemelerSource.AddRange(krediKartiOdemeler);

            if (DgOdemeler != null)
                DgOdemeler.ItemsSource = _tumOdemelerSource.OrderBy(x => x.Tarih).ToList();
        }


        private static OdemeViewModel MapSabitGider(SabitGider gider, DateTime tarih, bool odendiMi) =>
            new()
            {
                Id = gider.Id,
                KaynakId = gider.Id,
                Kod = gider.OdemeKodu ?? "",
                Aciklama = $"{gider.GiderAdi} ({tarih:MMMM yyyy})",
                Tarih = tarih,
                Tutar = gider.Tutar,
                ParaBirimi = gider.ParaBirimi ?? "TL",
                KaynakModul = "Sabit Ödeme",
                OdenmeDurumu = odendiMi,
                SirketAdi = gider.Company?.Name ?? "",
                CariFirmaAdi = gider.CariFirma?.Name ?? "",
                FaturaNo = gider.FaturaNo,
                OdeyenKullaniciAdi = gider.OdeyenKullaniciAdi,
                Durum = odendiMi,
                VadeTarihi = tarih,
                TaksitNo = 0
            };

        private static OdemeViewModel MapGenelOdeme(GenelOdeme odeme) => new()
        {
            Id = odeme.Id,
            KaynakId = odeme.Id,
            Kod = odeme.OdemeKodu ?? "",
            Aciklama = odeme.Aciklama ?? "",
            Tarih = odeme.OdemeTarihi ?? DateTime.MinValue,
            Tutar = odeme.Tutar,
            ParaBirimi = odeme.ParaBirimi ?? "TL",
            KaynakModul = "Genel Ödeme",
            OdenmeDurumu = odeme.IsOdedildiMi,
            SirketAdi = odeme.Company?.Name ?? "",
            CariFirmaAdi = odeme.CariFirma?.Name ?? "",
            FaturaNo = odeme.FaturaNo,
            OdeyenKullaniciAdi = odeme.OdeyenKullaniciAdi,
            Durum = odeme.IsOdedildiMi,
            VadeTarihi = odeme.OdemeTarihi ?? DateTime.MinValue,
            TaksitNo = 0
        };

        private static OdemeViewModel MapKrediTaksit(KrediTaksit taksit) => new()
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
            SirketAdi = taksit.Kredi?.SirketAdi ?? "",
            OdeyenKullaniciAdi = taksit.OdeyenKullaniciAdi,
            Durum = taksit.OdenmeDurumu,
            VadeTarihi = taksit.Tarih,
            TaksitNo = taksit.TaksitNo
        };

        private static OdemeViewModel MapKrediKartiOdeme(KrediKartiOdeme odeme) => new()
        {
            Id = odeme.Id,
            KaynakId = odeme.Id,
            Kod = odeme.OdemeKodu ?? "",
            Aciklama = odeme.Aciklama ?? "",
            Tarih = odeme.OdemeTarihi ?? DateTime.MinValue,
            Tutar = odeme.Tutar,
            ParaBirimi = "TL",
            KaynakModul = "Kredi Kartı",
            OdenmeDurumu = odeme.OdenmeDurumu,
            SirketAdi = odeme.Company?.Name ?? "",        // 🔥 Şirket Adı burası
            CariFirmaAdi = "",                            // 🔥 Burada cari firma yok zaten kredi kartı ödemelerinde
            OdeyenKullaniciAdi = odeme.OdeyenKullaniciAdi,
            Durum = odeme.OdenmeDurumu,
            VadeTarihi = odeme.OdemeTarihi ?? DateTime.MinValue,
            TaksitNo = 0
        };

        private static OdemeViewModel MapCek(Cek cek) => new()
        {
            Id = cek.Id,
            KaynakId = cek.Id,
            Kod = cek.CekKodu ?? "",
            Aciklama = $"Çek No: {cek.CekNumarasi}",
            Tarih = cek.VadeTarihi,
            Tutar = cek.Tutar,
            ParaBirimi = cek.ParaBirimi ?? "TL",
            KaynakModul = "Çek",
            OdenmeDurumu = cek.OdenmeDurumu,
            SirketAdi = cek.SirketAdi ?? "",            
            CariFirmaAdi = cek.CariFirma?.Name ?? "",
            OdeyenKullaniciAdi = cek.OdeyenKullaniciAdi,
            Durum = cek.OdenmeDurumu,
            VadeTarihi = cek.VadeTarihi,
            TaksitNo = 0
        };

        private static OdemeViewModel MapDegiskenOdeme(DegiskenOdeme odeme) => new()
        {
            Id = odeme.Id,
            KaynakId = odeme.Id,
            Kod = odeme.OdemeKodu,
            Aciklama = $"{odeme.GiderTuru}{(string.IsNullOrWhiteSpace(odeme.Aciklama) ? "" : $" - {odeme.Aciklama}")}",
            Tarih = odeme.OdemeTarihi,
            Tutar = odeme.Tutar,
            ParaBirimi = odeme.ParaBirimi ?? "TL",
            KaynakModul = "Değişken S. Ödeme",
            OdenmeDurumu = odeme.OdenmeDurumu,
            OdemeTarihi = odeme.OdemeTarihi,
            OdemeBankasi = odeme.OdemeBankasi,
            SirketAdi = odeme.Company?.Name ?? "",
            CariFirmaAdi = odeme.CariFirma?.Name ?? "",
            FaturaNo = odeme.FaturaNo,
            OdeyenKullaniciAdi = odeme.OdeyenKullaniciAdi,
            Durum = odeme.OdenmeDurumu,
            VadeTarihi = odeme.OdemeTarihi,
            TaksitNo = 0
        };
    


        private void DgOdemeler_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DgOdemeler.SelectedItem is OdemeViewModel selectedPayment)
            {
                // Sadece periyodik ödemeler için sonraki taksitleri göster
                if (selectedPayment.KaynakModul == "Kredi" ||
                    (selectedPayment.KaynakModul == "Sabit Ödeme" && IsSabitOdemeOtomatik(selectedPayment.KaynakId)))
                {
                    LoadSonrakiTaksitler(selectedPayment);
                    ExpanderSonrakiTaksitler.IsExpanded = true;
                    ExpanderSonrakiTaksitler.Visibility = Visibility.Visible;
                }
                else
                {
                    ExpanderSonrakiTaksitler.Visibility = Visibility.Collapsed;
                    ExpanderSonrakiTaksitler.IsExpanded = false;
                }
            }
        }

        private void HandleDataGridSelectionChanged() // DgOdemeler.SelectionChanged'a bağlanabilir veya başka yerden çağrılabilir
        {
            if (DgOdemeler.SelectedItem is OdemeViewModel selectedPayment)
            {
                if (selectedPayment.KaynakModul == "Kredi" ||
                   (selectedPayment.KaynakModul == "Sabit Ödeme" && IsSabitOdemeOtomatik(selectedPayment.KaynakId)))
                {
                    // Expander'ı görünür yap ama hemen açma, kullanıcı çift tıklasın veya bir butonla açsın.
                    // Veya direkt LoadSonrakiTaksitler çağrılabilir.
                    // Şimdilik sadece expander'ı görünür yapıp, içeriğini çift tıklama ile dolduracağız.
                    ExpanderSonrakiTaksitler.DataContext = selectedPayment; // Çift tıklamada kullanmak için
                    ExpanderSonrakiTaksitler.Visibility = Visibility.Visible;
                    ExpanderSonrakiTaksitler.IsExpanded = false; // Kullanıcı açsın
                    DgSonrakiTaksitler.ItemsSource = null; // Önceki içeriği temizle
                }
                else
                {
                    ExpanderSonrakiTaksitler.Visibility = Visibility.Collapsed;
                    ExpanderSonrakiTaksitler.IsExpanded = false;
                }
            }
            else
            {
                ExpanderSonrakiTaksitler.Visibility = Visibility.Collapsed;
                ExpanderSonrakiTaksitler.IsExpanded = false;
            }
        }


        private bool IsSabitOdemeOtomatik(int sabitOdemeSablonId)
        {
            // Bu metot, verilen Id'ye sahip SabitGider şablonunun OtomatikMi olduğunu kontrol eder.
            // KaynakId, ana şablonun Id'si olmalı.
            var db = App.DbContext;
            if (db == null) return false;
            var sablon = db.SabitGiderler.AsNoTracking().FirstOrDefault(sg => sg.Id == sabitOdemeSablonId);
            return sablon?.OtomatikMi ?? false;
        }

        private void LoadSonrakiTaksitler(OdemeViewModel anaOdeme)
        {
            var db = App.DbContext;
            if (db == null) return;

            List<OdemeViewModel> sonrakiTaksitlerListesi = new();
            DateTime simdi = DateTime.Today;

            if (anaOdeme.KaynakModul == "Kredi")
            {
                var kredi = db.Krediler.Include(k => k.Taksitler)
                              .FirstOrDefault(k => k.Taksitler.Any(t => t.Id == anaOdeme.Id));
                if (kredi != null)
                {
                    sonrakiTaksitlerListesi.AddRange(
                        kredi.Taksitler
                            .Where(t => t.Tarih > anaOdeme.Tarih)
                            .OrderBy(t => t.Tarih)
                            .Take(12)
                            .Select(t => new OdemeViewModel
                            {
                                Aciklama = $"Taksit {t.TaksitNo}",
                                Tarih = t.Tarih,
                                Tutar = t.Tutar,
                                OdenmeDurumu = t.OdenmeDurumu
                            }));
                }
            }
            else if (anaOdeme.KaynakModul == "Sabit Ödeme")
            {
                var giderSablonu = db.SabitGiderler.AsNoTracking().FirstOrDefault(sg => sg.Id == anaOdeme.KaynakId);
                if (giderSablonu != null && giderSablonu.OtomatikMi)
                {
                    DateTime iterTarih = anaOdeme.Tarih;
                    for (int i = 0; i < 12; i++)
                    {
                        iterTarih = giderSablonu.Periyot switch
                        {
                            "Aylık" => iterTarih.AddMonths(1),
                            "3 Aylık" => iterTarih.AddMonths(3),
                            "Yıllık" => iterTarih.AddYears(1),
                            _ => DateTime.MaxValue
                        };
                        if (iterTarih == DateTime.MaxValue) break;

                        var instancePayment = db.SabitGiderler.AsNoTracking()
                            .FirstOrDefault(sg => sg.OdemeKodu == giderSablonu.OdemeKodu &&
                                                  sg.BaslangicTarihi == iterTarih && sg.OtomatikMi);
                        bool buPeriyotOdendi = instancePayment != null && instancePayment.OdendiMi;

                        sonrakiTaksitlerListesi.Add(new OdemeViewModel
                        {
                            Aciklama = $"{giderSablonu.GiderAdi} ({iterTarih:MMMM yyyy})",
                            Tarih = iterTarih,
                            Tutar = giderSablonu.Tutar,
                            OdenmeDurumu = buPeriyotOdendi
                        });
                    }
                }
            }

            DgSonrakiTaksitler.ItemsSource = sonrakiTaksitlerListesi;
        }



        // BtnOdenme_Click, GeriAlOdemeDurumu, IsaretleOdendi, UpdatePaymentStatus, BulEntity metotları
        // bir önceki cevaptaki gibi kalacak, sadece namespace ve OdemeViewModel alan adları kontrol edilmeli.
        // Onları tekrar ekliyorum:

        private void BtnOdenmeDurumuDegistir_Click(object sender, RoutedEventArgs e) // Click olayının adı XAML ile eşleşmeli
        {
            var btn = sender as Button;
            if (btn == null) return;
            var seciliViewModel = btn.Tag as OdemeViewModel;
            if (seciliViewModel == null) return;

            var db = App.DbContext;
            if (db == null)
            {
                MessageBox.Show("Veritabanı bağlantısı kurulamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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
                        FiltreleVeYukle();
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
                // ✨ GÜVENLİ KONTROL VE DÖNÜŞÜM ✨
                if (popup.SecilenTarih.HasValue && !string.IsNullOrEmpty(popup.SecilenHesapKodu))
                {
                    DateTime secilenTarihKesin = popup.SecilenTarih.Value;

                    bool basarili = IsaretleOdendi(db, seciliViewModel, secilenTarihKesin, popup.SecilenHesapKodu);

                    if (basarili)
                    {
                        MessageBox.Show("Ödeme başarıyla 'Ödendi' olarak işaretlendi ✅", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        FiltreleVeYukle();
                    }
                }
                else
                {
                    MessageBox.Show("Lütfen geçerli bir ödeme tarihi ve hesap seçin.", "Eksik Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                try { propertyInfo.SetValue(entity, value); }
                catch (ArgumentException argEx)
                { System.Diagnostics.Debug.WriteLine($"SetPropertyValue Hata (ArgumentException): Property '{propertyName}', Değer '{value}', Tip '{value?.GetType()}', Beklenen Tip '{propertyInfo.PropertyType}'. Hata: {argEx.Message}"); }
                catch (Exception ex)
                { System.Diagnostics.Debug.WriteLine($"SetPropertyValue Genel Hata: Property '{propertyName}'. Hata: {ex.Message}"); }
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