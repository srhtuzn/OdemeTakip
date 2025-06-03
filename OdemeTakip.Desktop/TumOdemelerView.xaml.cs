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
        private List<OdemeViewModel> tumOdemelerSource = new    ();

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
            if (DgOdemeler == null || tumOdemelerSource == null) return;

            string filtreMetni = TxtArama.Text.Trim().ToLower();
            ICollectionView view = CollectionViewSource.GetDefaultView(DgOdemeler.ItemsSource);
            if (view == null && DgOdemeler.ItemsSource != null) // Eğer varsayılan view yoksa ama kaynak varsa
            {
                // Bu genellikle olmaz ama ItemsSource doğrudan List<T> ise olabilir.
                // Bu durumda, ItemsSource'u her filtrelemede yeniden atamak daha basit olabilir.
                // Ya da ItemsSource'a ObservableCollection atayıp onun üzerinden filtrelemek.
                // Şimdilik, FiltreleVeYukle'nin sonunda yapılan atamayı varsayıyoruz.
            }


            if (string.IsNullOrEmpty(filtreMetni))
            {
                if (view != null) view.Filter = null;
                // Eğer view null ise ve direkt ItemsSource'a atama yapıyorsak,
                // ana listeyi (filtrelenmiş ödeme durumuna göre) tekrar atamalıyız.
                // FiltreleVeYukle zaten doğru listeyi yüklüyor olmalı.
            }
            else
            {
                // Eğer ItemsSource bir ICollectionView ise:
                if (view != null)
                {
                    view.Filter = item =>
                    {
                        if (item is OdemeViewModel vm)
                        {
                            return (vm.Kod?.ToLower().Contains(filtreMetni) ?? false) ||
                                   (vm.Aciklama?.ToLower().Contains(filtreMetni) ?? false) ||
                                   (vm.KaynakModul?.ToLower().Contains(filtreMetni) ?? false) ||
                                   (vm.SirketAdi?.ToLower().Contains(filtreMetni) ?? false) ||
                                   (vm.OdeyenKullaniciAdi?.ToLower().Contains(filtreMetni) ?? false);
                        }
                        return false;
                    };
                }
                else
                {
                    // Eğer ItemsSource direkt List<T> ise, bu şekilde anlık filtreleme için
                    // Filtrelenmiş yeni bir liste oluşturup ItemsSource'a atamak gerekir.
                    // Bu, FiltreleVeYukle'nin bir parçası olarak ele alınabilir.
                    // Şimdilik, ana filtreleme FiltreleVeYukle'den gelecek.
                }
            }
        }


        public void FiltreleVeYukle()
        {
            var db = App.DbContext;
            if (db == null)
            {
                MessageBox.Show("Veritabanı bağlantısı (DbContext) bulunamadı.", "Kritik Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                tumOdemelerSource.Clear();
                if (DgOdemeler != null) DgOdemeler.ItemsSource = null;
                return;
            }
            tumOdemelerSource.Clear();

            DateTime simdi = DateTime.Today;
            int currentYear = simdi.Year;
            int currentMonth = simdi.Month;
            int currentDay = simdi.Day;

            bool? odemeDurumuFiltresi = null;
            if (CmbOdemeDurumu.SelectedItem is ComboBoxItem selectedDurumItem)
            {
                if (selectedDurumItem.Content.ToString() == "Ödenecekler")
                    odemeDurumuFiltresi = false;
                else if (selectedDurumItem.Content.ToString() == "Ödenmişler")
                    odemeDurumuFiltresi = true;
                // "Tümü" seçeneği null kalır, filtreleme yapılmaz
            }

            // ----- Sabit Giderler -----   
            var aktifSabitGiderSablonalari = db.SabitGiderler.AsNoTracking()
    .Include(x => x.Company)
    .Where(x => x.IsActive)
    .ToList();

            foreach (var giderSablonu in aktifSabitGiderSablonalari)
            {
                if (!giderSablonu.OtomatikMi)
                {
                    DateTime vadeTarihi = giderSablonu.BaslangicTarihi;
                    bool odendiMi = giderSablonu.OdendiMi;

                    // 🔥 Tarih filtresi sadece ödenecekler/ödenmişler filtresi seçiliyse çalışacak
                    bool tarihSarti = true;

                    if (odemeDurumuFiltresi.HasValue)
                    {
                        tarihSarti = (vadeTarihi.Year < currentYear) ||
                                     (vadeTarihi.Year == currentYear && vadeTarihi.Month < currentMonth) ||
                                     (vadeTarihi.Year == currentYear && vadeTarihi.Month == currentMonth && vadeTarihi.Day <= currentDay);
                    }

                    if ((!odemeDurumuFiltresi.HasValue || odemeDurumuFiltresi.Value == odendiMi) && tarihSarti)
                    {
                        // 🔥 Tümü seçiliyse veya uygun ödeme durumu ve tarih kontrolü sağlanıyorsa ekle
                        tumOdemelerSource.Add(new OdemeViewModel
                        {
                            Id = giderSablonu.Id,
                            KaynakId = giderSablonu.Id,
                            Kod = giderSablonu.OdemeKodu ?? string.Empty,
                            Aciklama = giderSablonu.GiderAdi ?? string.Empty,
                            Tarih = vadeTarihi,
                            Tutar = giderSablonu.Tutar,
                            ParaBirimi = giderSablonu.ParaBirimi ?? "TL",
                            KaynakModul = "Sabit Ödeme",
                            OdenmeDurumu = odendiMi,
                            SirketAdi = giderSablonu.Company?.Name ?? string.Empty,
                            FaturaNo = giderSablonu.FaturaNo,
                            OdeyenKullaniciAdi = giderSablonu.OdeyenKullaniciAdi
                        });
                    }
                }
                else // Otomatik sabit giderler
                {
                    DateTime iterTarih = giderSablonu.BaslangicTarihi;
                    // Ödenmişler için çok ileriye bakmaya gerek yok, "Tümü" veya "Ödenecekler" için 2 ay ileriye bakılır.
                    DateTime gelecekKontrolLimiti = odemeDurumuFiltresi == true ? simdi.AddMonths(1) : simdi.AddMonths(2);

                    while (iterTarih < gelecekKontrolLimiti)
                    {
                        // Sadece içinde bulunulan ay veya geçmiş aylardaki ödemeler dikkate alınır.
                        if ((iterTarih.Year < currentYear) || (iterTarih.Year == currentYear && iterTarih.Month <= currentMonth))
                        {
                            // İlgili periyoda ait ödeme kaydı veritabanında var mı kontrol edilir.
                            var instancePayment = db.SabitGiderler.AsNoTracking()
                                .FirstOrDefault(sg => sg.OdemeKodu == giderSablonu.OdemeKodu && sg.BaslangicTarihi == iterTarih && sg.OtomatikMi == true);
                            bool buPeriyotOdendi = instancePayment != null && instancePayment.OdendiMi;

                            // DÜZELTİLMİŞ KISIM:
                            // Eğer "Tümü" seçiliyse (odemeDurumuFiltresi.HasValue false ise) VEYA
                            // ödeme durumu filtreyle eşleşiyorsa (odemeDurumuFiltresi.Value == buPeriyotOdendi)
                            // listeye ekle.
                            if (!odemeDurumuFiltresi.HasValue || odemeDurumuFiltresi.Value == buPeriyotOdendi)
                            {
                                // "Tümü" filtresinin hatalı çalışmasına neden olan
                                // "if (!buPeriyotOdendi || odemeDurumuFiltresi == true)" koşulu buradan kaldırıldı.
                                tumOdemelerSource.Add(new OdemeViewModel
                                {
                                    Id = instancePayment?.Id ?? giderSablonu.Id, // Veritabanında kayıt varsa onun Id'si, yoksa şablon Id'si
                                    KaynakId = giderSablonu.Id, // Her zaman ana şablonun Id'si
                                    Kod = giderSablonu.OdemeKodu ?? string.Empty,
                                    // Açıklamadaki tarih formatı "MMMM yyyy" (örn: Haziran 2025) olarak düzeltildi.
                                    Aciklama = $"{giderSablonu.GiderAdi} ({iterTarih:MMMM yyyy})",
                                    Tarih = iterTarih,
                                    Tutar = giderSablonu.Tutar,
                                    ParaBirimi = giderSablonu.ParaBirimi ?? "TL",
                                    KaynakModul = "Sabit Ödeme",
                                    OdenmeDurumu = buPeriyotOdendi,
                                    SirketAdi = giderSablonu.Company != null ? (giderSablonu.Company.Name ?? string.Empty) : string.Empty,
                                    FaturaNo = giderSablonu.FaturaNo, // Şablondan gelen fatura no. Instance'a özel FaturaNo varsa farklı bir mantık gerekebilir.
                                    OdeyenKullaniciAdi = instancePayment?.OdeyenKullaniciAdi
                                });
                            }
                        }

                        // Bir sonraki periyodun tarihini hesapla
                        if (string.IsNullOrWhiteSpace(giderSablonu.Periyot) || (iterTarih.Year > gelecekKontrolLimiti.Year + 1)) break; // Güvenlik ve sonsuz döngü önlemi
                        iterTarih = giderSablonu.Periyot switch
                        {
                            "Aylık" => iterTarih.AddMonths(1),
                            "3 Aylık" => iterTarih.AddMonths(3),
                            "Yıllık" => iterTarih.AddYears(1),
                            _ => DateTime.MaxValue // Bilinmeyen periyot durumunda döngüden çık
                        };
                        if (iterTarih == DateTime.MaxValue) break; // Geçersiz tarih ise döngüden çık
                    }
                }
            }

            // ----- Genel Ödemeler -----
            tumOdemelerSource.AddRange(db.GenelOdemeler.AsNoTracking()
                .Include(x => x.Company)
                .Where(x => x.IsActive &&
                            (!odemeDurumuFiltresi.HasValue || x.IsOdedildiMi == odemeDurumuFiltresi.Value) &&
                            x.OdemeTarihi.HasValue &&
                            ((x.OdemeTarihi.Value.Year < currentYear) ||
                             (x.OdemeTarihi.Value.Year == currentYear && x.OdemeTarihi.Value.Month <= currentMonth)))
                .Select(x => new OdemeViewModel
                { /* ... alanlar ... OdenmeDurumu = x.IsOdedildiMi, OdeyenKullaniciAdi = x.OdeyenKullaniciAdi */
                    Id = x.Id,
                    KaynakId = x.Id,
                    Kod = x.OdemeKodu ?? string.Empty,
                    Aciklama = x.Aciklama ?? string.Empty,
                    Tarih = x.OdemeTarihi.Value,
                    Tutar = x.Tutar,
                    ParaBirimi = x.ParaBirimi ?? "TL",
                    KaynakModul = "Genel Ödeme",
                    OdenmeDurumu = x.IsOdedildiMi,
                    SirketAdi = x.Company != null ? (x.Company.Name ?? string.Empty) : string.Empty,
                    FaturaNo = x.FaturaNo,
                    OdeyenKullaniciAdi = x.OdeyenKullaniciAdi
                }));

            // ----- Kredi Taksitleri -----
            var tumAktifKrediTaksitleri = db.KrediTaksitler.AsNoTracking()
               .Include(x => x.Kredi)
               .Where(x => x.Kredi != null && x.Kredi.IsActive &&
                           (!odemeDurumuFiltresi.HasValue || x.OdenmeDurumu == odemeDurumuFiltresi.Value))
               .ToList();
            var gosterilecekKrediTaksitleri = tumAktifKrediTaksitleri
                .Where(t => (t.Tarih.Year < currentYear) || (t.Tarih.Year == currentYear && t.Tarih.Month <= currentMonth))
                // Ödenmemişler için sadece en yakınını, ödenmişler için tümünü (veya bir tarih aralığını) alabilirsiniz.
                // Şimdilik basitlik adına, ödenmemişlerin ilkini, ödenmişlerin hepsini (filtrelenmiş) alalım.
                .GroupBy(x => x.KrediId)
                .SelectMany(g => odemeDurumuFiltresi == false ?
                                 g.Where(t => !t.OdenmeDurumu).OrderBy(t => t.Tarih).Take(1) :
                                 g.Where(t => odemeDurumuFiltresi == true ? t.OdenmeDurumu : true) // Tümü veya ödenmişler
                )
                .Where(t => t != null)
                .ToList();
            foreach (var taksit in gosterilecekKrediTaksitleri)
            {
                tumOdemelerSource.Add(new OdemeViewModel
                { /* ... alanlar ... OdenmeDurumu = taksit.OdenmeDurumu, OdeyenKullaniciAdi = taksit.OdeyenKullaniciAdi */
                    Id = taksit.Id,
                    KaynakId = taksit.Id,
                    Kod = $"{taksit.KrediKodu}-T{taksit.TaksitNo:D2}",
                    Aciklama = $"{(taksit.Kredi != null ? taksit.Kredi.KrediKonusu : "")} (Taksit {taksit.TaksitNo})",
                    Tarih = taksit.Tarih,
                    Tutar = taksit.Tutar,
                    ParaBirimi = (taksit.Kredi != null ? taksit.Kredi.ParaBirimi : null) ?? "TL",
                    KaynakModul = "Kredi",
                    OdenmeDurumu = taksit.OdenmeDurumu,
                    OdemeTarihi = taksit.OdenmeTarihi,
                    OdemeBankasi = taksit.OdemeBankasi,
                    SirketAdi = (taksit.Kredi != null ? taksit.Kredi.SirketAdi : null) ?? string.Empty,
                    OdeyenKullaniciAdi = taksit.OdeyenKullaniciAdi
                });
            }

            // ----- Kredi Kartı Ödemeleri -----
            tumOdemelerSource.AddRange(
                db.KrediKartiOdemeleri
                    .AsNoTracking()
                    .Include(x => x.Company) // 🔥 FK Company çekiyoruz
                    .Where(x => x.IsActive &&
                                (!odemeDurumuFiltresi.HasValue || x.OdenmeDurumu == odemeDurumuFiltresi.Value) &&
                                x.OdemeTarihi.HasValue &&
                                ((x.OdemeTarihi.Value.Year < currentYear) ||
                                 (x.OdemeTarihi.Value.Year == currentYear && x.OdemeTarihi.Value.Month <= currentMonth)))
                    .Select(x => new OdemeViewModel
                    {
                        Id = x.Id,
                        KaynakId = x.Id,
                        Kod = x.OdemeKodu ?? string.Empty,
                        Aciklama = x.Aciklama ?? string.Empty,
                        Tarih = x.OdemeTarihi.Value,
                        Tutar = x.Tutar,
                        ParaBirimi = "TL",
                        KaynakModul = "Kredi Kartı",
                        OdenmeDurumu = x.OdenmeDurumu,
                        SirketAdi = x.Company != null ? (x.Company.Name ?? string.Empty) : string.Empty, // 🔥 Değişen burası
                        OdeyenKullaniciAdi = x.OdeyenKullaniciAdi
                    })
            );


            // ----- Çekler -----
            tumOdemelerSource.AddRange(
                db.Cekler.AsNoTracking()
                    .Include(x => x.CariFirma) // 🔥 CariFirma ilişkisini çekiyoruz
                    .Where(x => x.IsActive &&
                                (!odemeDurumuFiltresi.HasValue || x.OdenmeDurumu == odemeDurumuFiltresi.Value) &&
                                ((x.VadeTarihi.Year < currentYear) ||
                                 (x.VadeTarihi.Year == currentYear && x.VadeTarihi.Month <= currentMonth)))
                    .Select(x => new OdemeViewModel
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
                        SirketAdi = x.CariFirma != null ? (x.CariFirma.Name ?? string.Empty) : string.Empty, // 🔥 Burada SirketAdi = CariFirma.Name
                        OdeyenKullaniciAdi = x.OdeyenKullaniciAdi
                    })
            );


            // ----- Değişken Ödemeler -----
            tumOdemelerSource.AddRange(db.DegiskenOdemeler.AsNoTracking()
                .Include(x => x.Company)
                .Where(x => x.IsActive &&
                            (!odemeDurumuFiltresi.HasValue || x.OdenmeDurumu == odemeDurumuFiltresi.Value) &&
                            ((x.OdemeTarihi.Year < currentYear) ||
                             (x.OdemeTarihi.Year == currentYear && x.OdemeTarihi.Month <= currentMonth)))
                .Select(x => new OdemeViewModel
                { /* ... alanlar ... OdenmeDurumu = x.OdenmeDurumu, OdeyenKullaniciAdi = x.OdeyenKullaniciAdi */
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
                    SirketAdi = x.Company != null ? (x.Company.Name ?? string.Empty) : string.Empty,
                    FaturaNo = x.FaturaNo,
                    OdeyenKullaniciAdi = x.OdeyenKullaniciAdi
                }));

            if (DgOdemeler != null)
            {
                DgOdemeler.ItemsSource = tumOdemelerSource.OrderBy(x => x.Tarih).ToList();
            }
            // DataGrid yüklendikten sonra seçili bir öğe varsa "Sonraki Taksitler" expander'ını güncelle
            HandleDataGridSelectionChanged();
        }

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
                    // Seçilen taksitten sonrakileri ve ödenmemiş olanları al, veya tüm gelecek taksitleri
                    sonrakiTaksitlerListesi.AddRange(
                        kredi.Taksitler
                            .Where(t => t.Tarih > anaOdeme.Tarih) // Seçilenden sonraki taksitler
                            .OrderBy(t => t.Tarih)
                            .Take(12) // Örneğin sonraki 12 taksiti göster
                            .Select(t => new OdemeViewModel
                            {
                                Aciklama = $"Taksit {t.TaksitNo}",
                                Tarih = t.Tarih,
                                Tutar = t.Tutar,
                                OdenmeDurumu = t.OdenmeDurumu
                            })
                    );
                }
            }
            else if (anaOdeme.KaynakModul == "Sabit Ödeme")
            {
                var giderSablonu = db.SabitGiderler.AsNoTracking().FirstOrDefault(sg => sg.Id == anaOdeme.KaynakId); // KaynakId ana şablonun Id'si olmalı
                if (giderSablonu != null && giderSablonu.OtomatikMi)
                {
                    DateTime iterTarih = anaOdeme.Tarih; // Seçilen ödemenin tarihinden başla
                    if (string.IsNullOrWhiteSpace(giderSablonu.Periyot)) return;

                    for (int i = 0; i < 12; i++) // Örneğin sonraki 12 dönemi göster
                    {
                        // Bir sonraki dönemin tarihini hesapla
                        iterTarih = giderSablonu.Periyot switch
                        {
                            "Aylık" => iterTarih.AddMonths(1),
                            "3 Aylık" => iterTarih.AddMonths(3),
                            "Yıllık" => iterTarih.AddYears(1),
                            _ => DateTime.MaxValue
                        };
                        if (iterTarih == DateTime.MaxValue) break;

                        var instancePayment = db.SabitGiderler.AsNoTracking()
                            .FirstOrDefault(sg => sg.OdemeKodu == giderSablonu.OdemeKodu && sg.BaslangicTarihi == iterTarih && sg.OtomatikMi == true);
                        bool buPeriyotOdendi = instancePayment != null && instancePayment.OdendiMi;

                        sonrakiTaksitlerListesi.Add(new OdemeViewModel
                        {
                            Aciklama = $"{giderSablonu.GiderAdi} ({iterTarih:MMMM Gatsby})",
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