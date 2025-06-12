// OdemeTakip.Desktop.ViewModels/TumOdemelerListViewModel.cs
using Microsoft.EntityFrameworkCore; // Include, AsNoTracking için
using OdemeTakip.Data;
using OdemeTakip.Desktop.Helpers; // RelayCommand ve belki StartOfWeek için
using OdemeTakip.Entities; // İlgili entity'ler için (Cek, GenelOdeme, DegiskenOdeme vb.)
using System;
using System.Collections.Generic; // List için
using System.Collections.ObjectModel; // UI'ya bağlanacak koleksiyon için
using System.ComponentModel; // INotifyPropertyChanged için
using System.Linq; // LINQ metotları için
using System.Windows; // MessageBox için (şimdilik)
using System.Windows.Input; // ICommand için
using System.Globalization;

namespace OdemeTakip.Desktop.ViewModels
{
    /// <summary>
    /// TumOdemelerView (Tüm Ödemeler Listesi Ekranı) için ViewModel.
    /// Farklı modüllerden gelen ödemeleri toplar, filtreler, sıralar ve UI'ya sunar.
    /// </summary>
    public class TumOdemelerListViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _db;

        // UI'ya bağlanacak ana ödeme koleksiyonu
        public ObservableCollection<OdemeViewModel> Odemeler { get; set; }

        // DataGrid'de seçili olan ödeme
        private OdemeViewModel _selectedOdeme;
        public OdemeViewModel SelectedOdeme
        {
            get => _selectedOdeme;
            set
            {
                if (_selectedOdeme != value)
                {
                    _selectedOdeme = value;
                    OnPropertyChanged(nameof(SelectedOdeme));
                    // Seçili öğe değiştikçe ilgili komutların CanExecute durumunu tetikle
                    ((RelayCommand)ChangePaymentStatusCommand)?.RaiseCanExecuteChanged();
                    ((RelayCommand)DoubleClickCommand)?.RaiseCanExecuteChanged(); // Eğer çift tıklama butona bağlıysa
                }
            }
        }

        // Ay ve Yıl Filtreleme Property'leri
        private DateTime _secilenAy;
        public DateTime SecilenAy
        {
            get => _secilenAy;
            set
            {
                if (_secilenAy != value)
                {
                    _secilenAy = value;
                    OnPropertyChanged(nameof(SecilenAy));
                    OnPropertyChanged(nameof(CurrentAyYil)); // Ay değişince yıl bilgisini de güncelle
                    ExecuteLoadPayments(null); // Ay değiştiğinde ödemeleri yeniden yükle
                }
            }
        }

        public string CurrentAyYil
        {
            get => SecilenAy.ToString("MMMM yyyy"); // Formatı doğrudan burada sağla
            set { /* Setter yok, sadece getter'dan türetiliyor */ }
        }

        // Arama ve Filtreleme Property'leri
        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    // Arama metni değiştikçe anlık filtreleme yapabiliriz (performans dikkate alınmalı)
                    // Veya bir "Ara" butonu/komutu ekleyebiliriz.
                    ExecuteLoadPayments(null); // Şimdilik her değişimde tüm veriyi yükle
                }
            }
        }

        private OdemeDurumuFilter _selectedOdemeDurumuFilter = OdemeDurumuFilter.Tumu;
        public OdemeDurumuFilter SelectedOdemeDurumuFilter
        {
            get => _selectedOdemeDurumuFilter;
            set
            {
                if (_selectedOdemeDurumuFilter != value)
                {
                    _selectedOdemeDurumuFilter = value;
                    OnPropertyChanged(nameof(SelectedOdemeDurumuFilter));
                    ExecuteLoadPayments(null); // Filtre değiştiğinde ödemeleri yeniden yükle
                }
            }
        }

        public ObservableCollection<OdemeDurumuFilter> OdemeDurumuFiltreSecenekleri { get; set; }

        // Hesaplanan Toplamlar
        private decimal _toplamOdeme;
        public decimal ToplamOdeme
        {
            get => _toplamOdeme;
            set
            {
                if (_toplamOdeme != value)
                {
                    _toplamOdeme = value;
                    OnPropertyChanged(nameof(ToplamOdeme));
                }
            }
        }

        private decimal _odenmemisTutar;
        public decimal OdenmemisTutar
        {
            get => _odenmemisTutar;
            set
            {
                if (_odenmemisTutar != value)
                {
                    _odenmemisTutar = value;
                    OnPropertyChanged(nameof(OdenmemisTutar));
                }
            }
        }

        private decimal _gecikmisTutar;
        public decimal GecikmisTutar
        {
            get => _gecikmisTutar;
            set
            {
                if (_gecikmisTutar != value)
                {
                    _gecikmisTutar = value;
                    OnPropertyChanged(nameof(GecikmisTutar));
                }
            }
        }

        // Komutlar
        public ICommand LoadPaymentsCommand { get; private set; }
        public ICommand PreviousMonthCommand { get; private set; }
        public ICommand NextMonthCommand { get; private set; }
        public ICommand FilterTodayCommand { get; private set; }
        public ICommand FilterThisWeekCommand { get; private set; }
        public ICommand FilterThisMonthCommand { get; private set; }
        public ICommand FilterUnpaidCommand { get; private set; }
        public ICommand ExportToExcelCommand { get; private set; }
        public ICommand ExportToPdfCommand { get; private set; }
        public ICommand ChangePaymentStatusCommand { get; private set; }
        public ICommand DoubleClickCommand { get; private set; } // DataGrid için

        // Constructor
        public TumOdemelerListViewModel(AppDbContext db)
        {
            _db = db;
            Odemeler = new ObservableCollection<OdemeViewModel>();
            _secilenAy = DateTime.Today; // Başlangıçta geçerli ay

            InitializeCommands();
            InitializeFilterOptions();

            ExecuteLoadPayments(null); // ViewModel başlatıldığında ödemeleri yükle
        }

        private void InitializeCommands()
        {
            LoadPaymentsCommand = new RelayCommand(ExecuteLoadPayments);
            PreviousMonthCommand = new RelayCommand(ExecutePreviousMonth);
            NextMonthCommand = new RelayCommand(ExecuteNextMonth);
            FilterTodayCommand = new RelayCommand(ExecuteFilterToday);
            FilterThisWeekCommand = new RelayCommand(ExecuteFilterThisWeek);
            FilterThisMonthCommand = new RelayCommand(ExecuteFilterThisMonth);
            FilterUnpaidCommand = new RelayCommand(ExecuteFilterUnpaid);
            ExportToExcelCommand = new RelayCommand(ExecuteExportToExcel);
            ExportToPdfCommand = new RelayCommand(ExecuteExportToPdf);
            ChangePaymentStatusCommand = new RelayCommand(ExecuteChangePaymentStatus, CanExecutePaymentStatusChange);
            DoubleClickCommand = new RelayCommand(ExecuteDoubleClick, CanExecuteDoubleClick);
        }

        private void InitializeFilterOptions()
        {
            OdemeDurumuFiltreSecenekleri = new ObservableCollection<OdemeDurumuFilter>(Enum.GetValues(typeof(OdemeDurumuFilter)).Cast<OdemeDurumuFilter>());
        }

        #region Command Execution Methods

        /// <summary>
        /// Tüm ödeme türlerini veritabanından çeker, filtrelenir ve listeyi günceller.
        /// </summary>
        /// <param name="parameter">Komut parametresi (kullanılmıyor).</param>
        public void ExecuteLoadPayments(object parameter)
        {
            Odemeler.Clear(); // Önceki verileri temizle

            DateTime filtreBaslangic = new DateTime(SecilenAy.Year, SecilenAy.Month, 1);
            DateTime filtreBitis = filtreBaslangic.AddMonths(1).AddDays(-1);

            // Tüm entity'leri topla ve OdemeViewModel'e map'le
            List<OdemeViewModel> tumVeri = new List<OdemeViewModel>();

            // Çekler
            tumVeri.AddRange(_db.Cekler.AsNoTracking()
                .Include(x => x.CariFirma)
                .Where(x => x.IsActive && x.VadeTarihi.Date >= filtreBaslangic.Date && x.VadeTarihi.Date <= filtreBitis.Date)
                .Select(MapCek));

            // Genel Ödemeler
            tumVeri.AddRange(_db.GenelOdemeler.AsNoTracking()
                .Include(x => x.Company)
                .Include(x => x.CariFirma)
                .Where(x => x.IsActive && x.OdemeTarihi.HasValue && x.OdemeTarihi.Value.Date >= filtreBaslangic.Date && x.OdemeTarihi.Value.Date <= filtreBitis.Date)
                .Select(MapGenelOdeme));

            // Değişken Abonelik Ödemeleri
            tumVeri.AddRange(_db.DegiskenOdemeler.AsNoTracking()
               .Include(x => x.Company)
               .Include(x => x.CariFirma)
               .Where(x => x.IsActive && x.OdemeTarihi.Date >= filtreBaslangic.Date && x.OdemeTarihi.Date <= filtreBitis.Date)
               .Select(MapDegiskenOdeme));

            // Kredi Taksitleri (Bu ayki taksitleri veya ilgili tarihteki taksitleri çek)
            // Kredi taksitleri genellikle aylık bazda olduğu için, ana Kredi'nin active olup olmadığını kontrol ederiz.
            tumVeri.AddRange(_db.KrediTaksitler.AsNoTracking()
                .Include(x => x.Kredi)
                    .ThenInclude(k => k.Company) // Kredinin şirketini de dahil et
                .Where(x => x.Kredi.IsActive && x.Tarih.Date >= filtreBaslangic.Date && x.Tarih.Date <= filtreBitis.Date)
                .Select(MapKrediTaksit));

            // Sabit Giderler (Bu ayki sabit giderleri çek)
            // Sabit giderler de aylık tekrarladığı için, ilgili ayın kaydını veya temsilini çekmeliyiz.
            // Bu kısım, SabitGider entity'sinin yapısına ve tekrarlama mantığına göre değişebilir.
            // Örneğin, eğer her ay yeni bir SabitGider kaydı oluşmuyorsa,
            // başlangıç tarihi bu ayda olan veya bu aydan önce başlayıp hala aktif olanları çekmeliyiz.
            // Mevcut kodunuzda GroupBy kullanılıyor, bu ayın ilk kaydını çekmek için:
            tumVeri.AddRange(_db.SabitGiderler.AsNoTracking()
             .Include(x => x.Company)
             .Include(x => x.CariFirma)
             .Where(x => x.IsActive && x.BaslangicTarihi.Month == SecilenAy.Month && x.BaslangicTarihi.Year == SecilenAy.Year) // Sadece bu ayın başlangıç tarihi olanları al
             .Select(g => MapSabitGider(g!, g!.BaslangicTarihi, g.OdendiMi))); // MapSabitGider metodunu kullan

            // Kredi Kartı Harcamaları (Bu ayın ekstresine yansıyan harcamaları grupla)
            // Kredi kartı mantığı biraz daha karmaşıktır, mevcut mantığı koruyalım.

            var krediKartiOdemeleriQuery = _db.KrediKartiOdemeleri.AsNoTracking()
                 .Include(o => o.KrediKarti)
                     .ThenInclude(kk => kk.Company)
                 .Include(o => o.KrediKartiHarcama)
                 .Where(o => o.IsActive && o.OdemeTarihi.HasValue &&
                             o.OdemeTarihi.Value.Date >= filtreBaslangic.Date &&
                             o.OdemeTarihi.Value.Date <= filtreBitis.Date);

            // Veriyi veritabanından belleğe çek
            var krediKartiOdemeleriList = krediKartiOdemeleriQuery.ToList();

            // Bellek üzerinde OdemeViewModel'e map etme
            tumVeri.AddRange(krediKartiOdemeleriList.Select(o => new OdemeViewModel
            {
                Id = o.Id,
                KaynakId = o.KrediKartiId.GetValueOrDefault(),
                // Harcama kodu ve taksit numarasına göre kod oluştur
                Kod = o.OdemeKodu, // KrediKartiHarcamaHelper'da belirlenen OdemeKodu'nu kullan
                Aciklama = o.KrediKarti != null ?
                           $"{o.KrediKartiHarcama?.Aciklama ?? "Harcama Bilgisi Yok"} " + // Harcama açıklaması
                           $"- {o.KrediKarti.CardName} " + // Kart Adı
                           $"- Son 4 Hane: {o.KrediKarti.CardNumberLast4} " + // Son 4 Hane
                           $"- Limit: {o.KrediKarti.Limit:C2} " + // Limit
                           $"- Hesap Kesim: {o.KrediKarti.DueDate:dd.MM}" + // Hesap Kesim Tarihi
                           (o.ToplamTaksit > 1 ? $" (Taksit {o.TaksitNo.GetValueOrDefault()}/{o.ToplamTaksit.GetValueOrDefault()})" : "") // Taksit bilgisi (sadece çok taksitli ise)
                           :
                           $"{o.KrediKartiHarcama?.Aciklama ?? "Harcama Bilgisi Yok"} " + // Kart bilgisi yoksa sadece harcama açıklaması
                           (o.ToplamTaksit > 1 ? $" (Taksit {o.TaksitNo.GetValueOrDefault()}/{o.ToplamTaksit.GetValueOrDefault()})" : ""),
                Tarih = o.OdemeTarihi.GetValueOrDefault(),
                VadeTarihi = o.OdemeTarihi.GetValueOrDefault(),
                Tutar = o.Tutar,
                ParaBirimi = "TL",
                KaynakModul = "Kredi Kartı Taksit", // Modül adını "Kredi Kartı Taksit" olarak bırakalım, daha açıklayıcı
                OdenmeDurumu = o.OdenmeDurumu,
                OdemeTarihi = o.OdemeTarihi,
                OdemeBankasi = o.OdemeBankasi,
                SirketAdi = o.KrediKarti?.Company?.Name ?? "",
                CariFirmaAdi = "", // Kredi kartı harcamaları genellikle cari firma ile doğrudan ilişkili değil, boş kalabilir.
                FaturaNo = null, // Harcama detayı için FaturaNo alanı yok, null kalabilir
                OdeyenKullaniciAdi = o.OdeyenKullaniciAdi,
                Durum = o.OdenmeDurumu,
                ModulTipi = "KrediKart",
                TaksitNo = o.TaksitNo.GetValueOrDefault(),
                HarcamaToplamTutar = o.KrediKartiHarcama != null ? o.KrediKartiHarcama.Tutar : 0,
                HarcamaTaksitSayisi = o.KrediKartiHarcama != null ? o.KrediKartiHarcama.TaksitSayisi : 0
            }));
            // ...



            // 🎯 Genel Filtreleme ve Sıralama 🎯
            var filtreliListe = tumVeri
               .Where(x =>
                   // Arama metni filtresi
                   (string.IsNullOrWhiteSpace(SearchText) ||
                    (x.Kod?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                    (x.Aciklama?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                    (x.KaynakModul?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                    (x.SirketAdi?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                    (x.CariFirmaAdi?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                    (x.OdeyenKullaniciAdi?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true)))
               .Where(x =>
                   // Ödeme durumu filtresi
                   SelectedOdemeDurumuFilter == OdemeDurumuFilter.Tumu ||
                   (SelectedOdemeDurumuFilter == OdemeDurumuFilter.Odenmisler && x.OdenmeDurumu) ||
                   (SelectedOdemeDurumuFilter == OdemeDurumuFilter.Odenmemisler && !x.OdenmeDurumu))
               .OrderBy(x => x.VadeTarihi)
               .ToList();

            // ObservableCollection'ı güncelle
            Odemeler.Clear();
            foreach (var odeme in filtreliListe)
            {
                // 🎨 Renk ve İkon Ata
                odeme.RenkKod = HesaplaRenk(odeme);
                odeme.ModulIkon = IkonBelirle(odeme.ModulTipi);
                odeme.DurumText = odeme.OdenmeDurumu ? "Ödendi" : (odeme.VadeTarihi.Date < DateTime.Today ? "Gecikmiş" : "Ödenecek");
                Odemeler.Add(odeme);
            }

            // 💰 Toplamları Hesapla
            ToplamOdeme = Odemeler.Sum(x => x.Tutar);
            OdenmemisTutar = Odemeler.Where(x => !x.Durum).Sum(x => x.Tutar);
            GecikmisTutar = Odemeler.Where(x => !x.Durum && x.VadeTarihi.Date < DateTime.Today).Sum(x => x.Tutar);
        }

        private void ExecutePreviousMonth(object parameter)
        {
            SecilenAy = SecilenAy.AddMonths(-1);
        }

        private void ExecuteNextMonth(object parameter)
        {
            SecilenAy = SecilenAy.AddMonths(1);
        }

        private void ExecuteFilterToday(object parameter)
        {
            DateTime today = DateTime.Today;
            SecilenAy = today; // Ayı bugüne ayarla
            ExecuteLoadPayments(new DateRangeFilter { StartDate = today, EndDate = today }); // Filtre ile yükle
        }

        private void ExecuteFilterThisWeek(object parameter)
        {
            DateTime today = DateTime.Today;
            // Hafta başlangıcını belirle (örneğin Pazartesi)
            DateTime startOfWeek = today.StartOfWeek(DayOfWeek.Monday); // Bu metodu Helpers'a eklemelisiniz.
            DateTime endOfWeek = startOfWeek.AddDays(6);
            SecilenAy = today; // Ayı bugüne ayarla
            ExecuteLoadPayments(new DateRangeFilter { StartDate = startOfWeek, EndDate = endOfWeek });
        }

        private void ExecuteFilterThisMonth(object parameter)
        {
            DateTime today = DateTime.Today;
            DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            SecilenAy = today; // Ayı bugüne ayarla
            ExecuteLoadPayments(new DateRangeFilter { StartDate = startOfMonth, EndDate = endOfMonth });
        }

        private void ExecuteFilterUnpaid(object parameter)
        {
            SelectedOdemeDurumuFilter = OdemeDurumuFilter.Odenmemisler;
            ExecuteLoadPayments(null); // Ödenmemişleri filtrele
        }

        private void ExecuteExportToExcel(object parameter)
        {
            MessageBox.Show("Excel export özelliği geliştirilecek.");
            // Burada Excel dışa aktarma mantığı gelecek
        }

        private void ExecuteExportToPdf(object parameter)
        {
            MessageBox.Show("PDF export özelliği geliştirilecek.");
            // Burada PDF dışa aktarma mantığı gelecek
        }

        private bool CanExecutePaymentStatusChange(object parameter)
        {
            return SelectedOdeme != null; // Yalnızca bir ödeme seçiliyse komut aktif
        }

        /// <summary>
        /// Seçili ödemenin durumunu değiştirir (ödendi/geri al).
        /// </summary>
        /// <param name="parameter">Komut parametresi (kullanılmıyor).</param>
        private void ExecuteChangePaymentStatus(object parameter)
        {
            if (SelectedOdeme == null) return;

            var db = _db; // ViewModel kendi DB Context'ini kullanıyor
            if (db == null)
            {
                MessageBox.Show("Veritabanı bağlantısı kurulamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SelectedOdeme.OdenmeDurumu) // Ödendiyse geri alma işlemi
            {
                // Admin kontrolü ve şifre popup mantığı burada.
                // İdealde bu popuplar bir DialogService üzerinden çağrılmalı.
                if (App.CurrentUser?.Role == UserRole.Admin)
                {
                    if (MessageBox.Show($"'{SelectedOdeme.Kod} - {SelectedOdeme.Aciklama}' ödemesi zaten 'Ödendi' olarak işaretli.\nÖdeyen: {SelectedOdeme.OdeyenKullaniciAdi}\nBu durumu geri almak istiyor musunuz?",
                                        "Geri Alma Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        if (GeriAlOdemeDurumu(db, SelectedOdeme))
                        {
                            MessageBox.Show("Ödeme durumu başarıyla geri alındı 🔄", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                            ExecuteLoadPayments(null); // Listeyi yenile
                        }
                    }
                }
                else // Admin değilse şifre onayı
                {
                    var sifrePopup = new SifreDogrulamaPopup(); // View'e doğrudan referans, ideal değil
                    if (sifrePopup.ShowDialog() == true && sifrePopup.SifreDogru)
                    {
                        if (GeriAlOdemeDurumu(db, SelectedOdeme))
                        {
                            MessageBox.Show("Ödeme durumu başarıyla geri alındı 🔄", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                            ExecuteLoadPayments(null); // Listeyi yenile
                        }
                    }
                }
            }
            else // Ödenmediyse "Ödendi" olarak işaretle
            {
                if (MessageBox.Show($"Bu ödeme '{SelectedOdeme.Kod} - {SelectedOdeme.Aciklama}' için 'ödendi' olarak işaretlensin mi?",
                                    "Ödeme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    // Banka hesaplarını çekme
                    List<BankaHesabi> bankaHesaplari;
                    try
                    {
                        bankaHesaplari = SelectedOdeme.KaynakModul == "Kredi Kartı" && !string.IsNullOrEmpty(SelectedOdeme.SirketAdi)
                            ? db.BankaHesaplari.Include(x => x.Company).Where(x => x.IsActive && x.Company != null && x.Company.Name == SelectedOdeme.SirketAdi).ToList()
                            : db.BankaHesaplari.Where(x => x.IsActive).ToList();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Banka hesapları yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var popup = new OdemeOnayPopup(bankaHesaplari); // View'e doğrudan referans, ideal değil
                    if (popup.ShowDialog() == true)
                    {
                        if (!popup.SecilenTarih.HasValue || string.IsNullOrEmpty(popup.SecilenHesapKodu))
                        {
                            MessageBox.Show("Lütfen geçerli bir ödeme tarihi ve hesap seçin.", "Eksik Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        if (IsaretleOdendi(db, SelectedOdeme, popup.SecilenTarih.Value, popup.SecilenHesapKodu))
                        {
                            MessageBox.Show("Ödeme başarıyla 'Ödendi' olarak işaretlendi ✅", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                            ExecuteLoadPayments(null); // Listeyi yenile
                        }
                    }
                }
            }
        }

        private bool CanExecuteDoubleClick(object parameter)
        {
            return SelectedOdeme != null; // Yalnızca bir ödeme seçiliyse çift tıklama aktif
        }

        /// <summary>
        /// DataGrid'deki bir ödemeye çift tıklandığında ilgili detay formunu açar.
        /// </summary>
        /// <param name="parameter">Komut parametresi (genellikle DataGrid'in SelectedItem'ı).</param>
        private void ExecuteDoubleClick(object parameter)
        {
            if (SelectedOdeme == null) return;

            if (SelectedOdeme.ModulTipi == "Degisken")
            {
                // Değişken Ödeme için Fatura Formu aç
                var faturaForm = new EksikFaturaForm(App.DbContext, SelectedOdeme.Id); // View'e doğrudan referans
                faturaForm.ShowDialog();
                ExecuteLoadPayments(null); // Güncelleme sonrası listeyi yenile
            }
            else // Diğer modüller için taksit/detay penceresi
            {
                // Kredi kartı harcamaları için ayrı bir detay penceresi açabiliriz
                // Yada kredi taksitleri için taksit detayları.
                // OdemeViewModel'de KaynakModul yerine ModulTipi daha uygun.
                List<OdemeViewModel> detaylar = new List<OdemeViewModel>();

                if (SelectedOdeme.ModulTipi == "Kredi")
                {
                    detaylar = LoadTaksitlerFor(SelectedOdeme);
                }
                // Diğer modüller için de detay yükleme mantığı buraya eklenebilir
                // Örneğin: if (SelectedOdeme.ModulTipi == "KrediKart") { LoadKrediKartiHarcamalari(SelectedOdeme); }

                if (detaylar.Any())
                {
                    var detayWindow = new TaksitDetayWindow($"{SelectedOdeme.Aciklama} Detayları", detaylar); // View'e doğrudan referans
                    detayWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Bu ödeme türü için detay görüntüleme mevcut değil veya detay bulunamadı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        #endregion

        #region Helper Methods (Business Logic - Map, BulEntity, UpdatePaymentStatus, Renk, İkon)

        // Mevcut code-behind'daki Map metotlarını buraya taşıyoruz
        // Her bir ödeme entity'sini OdemeViewModel'e dönüştürür.
        private static OdemeViewModel MapSabitGider(SabitGider gider, DateTime tarih, bool odendiMi) =>
            new()
            {
                Id = gider.Id,
                KaynakId = gider.Id,
                Kod = gider.OdemeKodu ?? "",
                Aciklama = BuildAciklama($"{gider.GiderAdi} ({tarih:MMMM yyyy})", gider.Aciklama, gider.FaturaNo),
                Tarih = tarih,
                Tutar = gider.Tutar,
                ParaBirimi = gider.ParaBirimi ?? "TL",
                KaynakModul = "Sabit Ödeme",
                OdenmeDurumu = odendiMi,
                OdemeTarihi = odendiMi ? gider.OdemeTarihi : null,
                OdemeBankasi = odendiMi ? gider.OdemeBankasi : null,
                SirketAdi = gider.Company?.Name ?? "",
                CariFirmaAdi = gider.CariFirma?.Name ?? "",
                FaturaNo = gider.FaturaNo,
                OdeyenKullaniciAdi = gider.OdeyenKullaniciAdi,
                Durum = odendiMi,
                ModulTipi = "Sabit",
                VadeTarihi = tarih,
                TaksitNo = 0
            };

        private static OdemeViewModel MapGenelOdeme(GenelOdeme odeme) => new()
        {
            Id = odeme.Id,
            KaynakId = odeme.Id,
            Kod = odeme.OdemeKodu ?? "",
            Aciklama = BuildAciklama(odeme.Aciklama, "", odeme.FaturaNo),
            Tarih = odeme.OdemeTarihi ?? DateTime.MinValue,
            Tutar = odeme.Tutar,
            ParaBirimi = odeme.ParaBirimi ?? "TL",
            KaynakModul = "Genel Ödeme",
            OdenmeDurumu = odeme.IsOdedildiMi,
            OdemeTarihi = odeme.IsOdedildiMi ? odeme.OdemeTarihi : null,
            OdemeBankasi = odeme.IsOdedildiMi ? odeme.OdemeBankasi : null,
            SirketAdi = odeme.Company?.Name ?? "",
            CariFirmaAdi = odeme.CariFirma?.Name ?? "",
            FaturaNo = odeme.FaturaNo,
            OdeyenKullaniciAdi = odeme.OdeyenKullaniciAdi,
            Durum = odeme.IsOdedildiMi,
            ModulTipi = "Genel",
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
            SirketAdi = taksit.Kredi?.Company?.Name ?? "", // Kredinin şirketi
            CariFirmaAdi = taksit.Kredi?.CariFirma?.Name ?? "", // Kredinin cari firması
            OdeyenKullaniciAdi = taksit.OdeyenKullaniciAdi,
            Durum = taksit.OdenmeDurumu,
            VadeTarihi = taksit.Tarih,
            ModulTipi = "Kredi",
            TaksitNo = taksit.TaksitNo
        };

        private static OdemeViewModel MapCek(Cek cek) => new()
        {
            Id = cek.Id,
            KaynakId = cek.Id,
            Kod = cek.CekKodu ?? "",
            Aciklama = $"Çek No: {cek.CekNumarasi} - {cek.Aciklama ?? ""}",
            Tarih = cek.VadeTarihi,
            Tutar = cek.Tutar,
            ParaBirimi = cek.ParaBirimi ?? "TL",
            KaynakModul = "Çek",
            OdenmeDurumu = cek.OdenmeDurumu,
            OdemeTarihi = cek.OdenmeDurumu ? cek.TahsilTarihi : null, // Çek tahsil tarihi
            OdemeBankasi = cek.OdenmeDurumu ? cek.OdemeBankasi : null,
            SirketAdi = cek.SirketAdi ?? "",
            CariFirmaAdi = cek.CariFirma?.Name ?? "",
            OdeyenKullaniciAdi = cek.OdeyenKullaniciAdi,
            Durum = cek.OdenmeDurumu,
            VadeTarihi = cek.VadeTarihi,
            ModulTipi = "Cek",
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
            ModulTipi = "Degisken",
            TaksitNo = 0
        };

        // Kredi Taksitleri için detay yükleme (tek bir ödeme için)
        private List<OdemeViewModel> LoadTaksitlerFor(OdemeViewModel anaOdeme)
        {
            var db = App.DbContext;
            if (db == null) return new List<OdemeViewModel>();

            List<OdemeViewModel> list = new();

            if (anaOdeme.ModulTipi == "Kredi")
            {
                // Kredi taksitlerini ana ödemenin ID'si üzerinden çek
                // Kredi ile ilişkili taksitleri ve Kredi'nin kendisini de yükle
                var kredi = db.Krediler.Include(k => k.Taksitler)
                                      .FirstOrDefault(k => k.Id == anaOdeme.KaynakId); // OdemeViewModel'deki KaynakId'yi kullan

                if (kredi != null)
                {
                    list.AddRange(kredi.Taksitler
                        .Where(t => t.Tarih.Year == SecilenAy.Year && t.Tarih.Month == SecilenAy.Month) // Seçilen aya göre filtrele
                        .OrderBy(t => t.Tarih)
                        .Select(MapKrediTaksit));
                }
            }
            return list;
        }

        // Genel Açıklama Oluşturucu
        private static string BuildAciklama(string? anaBaslik, string? aciklama, string? faturaNo)
        {
            var list = new List<string>();
            if (!string.IsNullOrWhiteSpace(anaBaslik)) list.Add(anaBaslik);
            if (!string.IsNullOrWhiteSpace(aciklama)) list.Add(aciklama);
            if (!string.IsNullOrWhiteSpace(faturaNo)) list.Add($"(Fatura: {faturaNo})");
            return string.Join(" - ", list);
        }

        // Renk Hesaplama
        private string HesaplaRenk(OdemeViewModel odeme)
        {
            if (odeme.Durum) return "Green"; // Ödendi
            if (odeme.VadeTarihi.Date < DateTime.Today.Date) return "Red"; // Gecikmiş
            if (odeme.VadeTarihi.Date == DateTime.Today.Date) return "Orange"; // Bugün
            return "Gray"; // Gelecek
        }

        // İkon Belirleme
        private string IkonBelirle(string modulTipi)
        {
            return modulTipi switch
            {
                "Kredi" => "🏦",
                "KrediKart" => "💳",
                "Sabit" => "🧾",
                "Degisken" => "🔁",
                "Cek" => "📜",
                "Genel" => "💸",
                _ => "❓",
            };
        }

        // Varlıkların ödeme durumunu güncelleyen yardımcı metod
        private void UpdatePaymentStatus(dynamic typedEntity, bool isPaid, DateTime? paymentDate, string? paymentBank, string? payingUser)
        {
            SetPropertyValue(typedEntity, "OdendiMi", isPaid);
            SetPropertyValue(typedEntity, "IsOdedildiMi", isPaid);
            SetPropertyValue(typedEntity, "OdenmeDurumu", isPaid);

            SetPropertyValue(typedEntity, "OdemeTarihi", isPaid ? paymentDate : null);
            SetPropertyValue(typedEntity, "TahsilTarihi", isPaid ? paymentDate : null); // Eğer bu property varsa

            SetPropertyValue(typedEntity, "OdemeBankasi", isPaid ? paymentBank : null);
            SetPropertyValue(typedEntity, "OdeyenKullaniciAdi", isPaid ? payingUser : null);
        }

        // Dinamik property setleme (Reflection)
        private void SetPropertyValue(object entity, string propertyName, object? value)
        {
            var propertyInfo = entity.GetType().GetProperty(propertyName);
            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                try
                {
                    // Eğer değer null ise, doğrudan null'ı ata
                    if (value == null)
                    {
                        propertyInfo.SetValue(entity, null);
                        return; // Metodu sonlandır
                    }

                    // Hedef property'nin tipi
                    Type targetType = propertyInfo.PropertyType;

                    // Eğer hedef tip nullable ise, underlying tipini al (örn: DateTime? için DateTime)
                    Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                    object convertedValue;

                    // Özel durumlar veya genel dönüşüm
                    if (underlyingType == typeof(DateTime))
                    {
                        // Eğer hedef DateTime veya DateTime? ise
                        if (value is DateTime dtValue)
                        {
                            convertedValue = dtValue;
                        }
                        else if (value is string strValue && DateTime.TryParse(strValue, out DateTime parsedDt))
                        {
                            convertedValue = parsedDt;
                        }
                        else
                        {
                            convertedValue = Convert.ChangeType(value, underlyingType);
                        }
                    }
                    else if (underlyingType == typeof(string))
                    {
                        convertedValue = value.ToString();
                    }
                    else if (underlyingType.IsEnum) // Eğer hedef tip bir Enum ise
                    {
                        convertedValue = Enum.Parse(underlyingType, value.ToString());
                    }
                    else
                    {
                        // Diğer tüm tipler için genel dönüşüm
                        convertedValue = Convert.ChangeType(value, underlyingType);
                    }

                    // Dönüştürülen değeri ata
                    propertyInfo.SetValue(entity, convertedValue);
                }
                catch (ArgumentException argEx)
                {
                    System.Diagnostics.Debug.WriteLine($"SetPropertyValue Hata (ArgumentException): Property '{propertyName}', Değer '{value}', Tip '{value?.GetType()}', Beklenen Tip '{propertyInfo.PropertyType}'. Hata: {argEx.Message}");
                }
                catch (InvalidCastException castEx)
                {
                    System.Diagnostics.Debug.WriteLine($"SetPropertyValue Hata (InvalidCastException): Property '{propertyName}', Değer '{value}'. Hata: {castEx.Message}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SetPropertyValue Genel Hata: Property '{propertyName}'. Hata: {ex.Message}");
                }
            }
        }

        // Entity'leri ID ve kaynak modülüne göre bulan yardımcı metod
        private object? BulEntity(AppDbContext db, string kaynakModul, int entityId, string kod, DateTime vadeTarihi)
        {
            DateTime vadeGun = vadeTarihi.Date; // Sadece tarihi karşılaştır

            try
            {
                switch (kaynakModul)
                {
                    case "Sabit Ödeme":
                        return db.SabitGiderler.FirstOrDefault(x => x.Id == entityId && x.BaslangicTarihi.Date == vadeGun);
                    case "Genel Ödeme":
                        return db.GenelOdemeler.FirstOrDefault(x => x.Id == entityId);
                    case "Kredi Kartı Taksit": // Yeni KaynakModul Adı: "Kredi Kartı Taksit"
                                               // OdemeViewModel'den gelen Id, KrediKartiOdeme'nin kendi Id'sidir.
                                               // Vade tarihi ile de kontrol edelim.
                        return db.KrediKartiOdemeleri.FirstOrDefault(x => x.Id == entityId &&
                                                                       x.OdemeTarihi.HasValue && x.OdemeTarihi.Value.Date == vadeGun);
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


        // Ödeme durumu geri alma işlemi
        private bool GeriAlOdemeDurumu(AppDbContext db, OdemeViewModel vm)
        {
            object? entity = BulEntity(db, vm.KaynakModul, vm.Id, vm.Kod, vm.VadeTarihi); // VadeTarihi kullan
            if (entity == null)
            {
                MessageBox.Show($"Geri alınacak kayıt bulunamadı (Debug Bilgisi):\nModül: {vm.KaynakModul}, ID: {vm.Id}, Kod: {vm.Kod}, Vade: {vm.VadeTarihi:dd.MM.yyyy}", "Kayıt Bulunamadı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            try
            {
                dynamic typedEntity = entity;
                UpdatePaymentStatus(typedEntity, false, null, null, null); // Ödenmemiş olarak işaretle
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Geri alma işleminde hata: {ex.Message}\n{ex.StackTrace}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Ödeme işaretleme işlemi
        private bool IsaretleOdendi(AppDbContext db, OdemeViewModel vm, DateTime odemeTarihi, string odemeBankasi)
        {
            object? entity = BulEntity(db, vm.KaynakModul, vm.Id, vm.Kod, vm.VadeTarihi); // VadeTarihi kullan
            if (entity == null)
            {
                MessageBox.Show($"Ödenecek kayıt bulunamadı (Debug Bilgisi):\nModül: {vm.KaynakModul}, ID: {vm.Id}, Kod: {vm.Kod}, Vade: {vm.VadeTarihi:dd.MM.yyyy}", "Kayıt Bulunamadı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            try
            {
                dynamic typedEntity = entity;
                string? odeyenKullanici = App.CurrentUser?.Username; // Uygulamadaki oturum açmış kullanıcıyı al
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

        #endregion

        // OdemeTakip.Desktop.ViewModels/TumOdemelerListViewModel.cs içinde, OnPropertyChanged metodu

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // 👇 BURADAKİ DÜZELTME 👇
            // Her komutun null olup olmadığını kontrol ederek NullReferenceException'ı önle
            if (LoadPaymentsCommand != null)
            {
                ((RelayCommand)LoadPaymentsCommand).RaiseCanExecuteChanged();
            }
            if (PreviousMonthCommand != null)
            {
                ((RelayCommand)PreviousMonthCommand).RaiseCanExecuteChanged();
            }
            if (NextMonthCommand != null)
            {
                ((RelayCommand)NextMonthCommand).RaiseCanExecuteChanged();
            }
            if (FilterTodayCommand != null)
            {
                ((RelayCommand)FilterTodayCommand).RaiseCanExecuteChanged();
            }
            if (FilterThisWeekCommand != null)
            {
                ((RelayCommand)FilterThisWeekCommand).RaiseCanExecuteChanged();
            }
            if (FilterThisMonthCommand != null)
            {
                ((RelayCommand)FilterThisMonthCommand).RaiseCanExecuteChanged();
            }
            if (FilterUnpaidCommand != null)
            {
                ((RelayCommand)FilterUnpaidCommand).RaiseCanExecuteChanged();
            }
            if (ExportToExcelCommand != null)
            {
                ((RelayCommand)ExportToExcelCommand).RaiseCanExecuteChanged();
            }
            if (ExportToPdfCommand != null)
            {
                ((RelayCommand)ExportToPdfCommand).RaiseCanExecuteChanged();
            }
            if (ChangePaymentStatusCommand != null)
            {
                ((RelayCommand)ChangePaymentStatusCommand).RaiseCanExecuteChanged();
            }
            if (DoubleClickCommand != null)
            {
                ((RelayCommand)DoubleClickCommand).RaiseCanExecuteChanged();
            }
        }
        #endregion
    }

    /// <summary>
    /// OdemeDurumu için filtre seçenekleri
    /// </summary>
    public enum OdemeDurumuFilter
    {
        [Description("Tümü")]
        Tumu,
        [Description("Ödenecekler")]
        Odenmemisler,
        [Description("Ödenmişler")]
        Odenmisler
    }

    // Tarih aralığı filtresi için basit bir sınıf
    public class DateRangeFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    // DateTime uzantı metodu (Helpers klasörüne eklenebilir)
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}