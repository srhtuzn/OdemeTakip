// OdemeTakip.Desktop.ViewModels/KrediFormViewModel.cs
using OdemeTakip.Data;
using OdemeTakip.Entities;
using OdemeTakip.Desktop.Helpers; // RelayCommand için
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows; // MessageBox için
using System.Globalization; // CultureInfo için
using Microsoft.EntityFrameworkCore; // Include, Load için

namespace OdemeTakip.Desktop.ViewModels
{
    /// <summary>
    /// Kredi Ekle/Güncelle Formu (KrediForm) için ViewModel.
    /// Kredi bilgilerinin girişini, doğrulamasını ve ilişkili taksit ödemelerinin yönetilmesini sağlar.
    /// </summary>
    public class KrediFormViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _db;
        private Kredi _kredi;
        private bool _isEdit;

        /// <summary>
        /// Formun düzenleme modunda olup olmadığını belirtir.
        /// UI'dan Binding için kullanılır (sadece okunur).
        /// </summary>
        public bool IsEdit
        {
            get => _isEdit;
        }

        private string _krediKodu = "";
        public string KrediKodu
        {
            get => _krediKodu;
            set
            {
                if (_krediKodu != value)
                {
                    _krediKodu = value;
                    OnPropertyChanged(nameof(KrediKodu));
                    RaiseSaveCanExecuteChanged(); // Kaydet butonunun CanExecute durumunu tetikle
                }
            }
        }

        private int? _selectedCompanyId;
        public int? SelectedCompanyId
        {
            get => _selectedCompanyId;
            set
            {
                if (_selectedCompanyId != value)
                {
                    _selectedCompanyId = value;
                    OnPropertyChanged(nameof(SelectedCompanyId));
                    RaiseSaveCanExecuteChanged(); // Kaydet butonunun CanExecute durumunu tetikle
                }
            }
        }

        private int? _selectedCariFirmaId;
        public int? SelectedCariFirmaId
        {
            get => _selectedCariFirmaId;
            set
            {
                if (_selectedCariFirmaId != value)
                {
                    _selectedCariFirmaId = value;
                    OnPropertyChanged(nameof(SelectedCariFirmaId));
                    RaiseSaveCanExecuteChanged(); // Kaydet butonunun CanExecute durumunu tetikle
                }
            }
        }

        private string _krediKonusu = "";
        public string KrediKonusu
        {
            get => _krediKonusu;
            set
            {
                if (_krediKonusu != value)
                {
                    _krediKonusu = value;
                    OnPropertyChanged(nameof(KrediKonusu));
                    RaiseSaveCanExecuteChanged(); // Kaydet butonunun CanExecute durumunu tetikle
                }
            }
        }

        private string _toplamTutarText = "";
        public string ToplamTutarText
        {
            get => _toplamTutarText;
            set
            {
                if (_toplamTutarText != value)
                {
                    _toplamTutarText = value;
                    OnPropertyChanged(nameof(ToplamTutarText));
                    OnPropertyChanged(nameof(AylikTaksitTutariText)); // Aylık taksit tutarı değişebilir
                    RaiseSaveCanExecuteChanged(); // Kaydet butonunun CanExecute durumunu tetikle
                }
            }
        }
        /// <summary>
        /// Toplam tutarı decimal olarak döner. Geçersiz girişlerde 0 döner.
        /// </summary>
        public decimal ToplamTutar => decimal.TryParse(ToplamTutarText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0;

        private string _taksitSayisiText = "";
        public string TaksitSayisiText
        {
            get => _taksitSayisiText;
            set
            {
                if (_taksitSayisiText != value)
                {
                    _taksitSayisiText = value;
                    OnPropertyChanged(nameof(TaksitSayisiText));
                    OnPropertyChanged(nameof(AylikTaksitTutariText)); // Aylık taksit tutarı değişebilir
                    RaiseSaveCanExecuteChanged(); // Kaydet butonunun CanExecute durumunu tetikle
                }
            }
        }
        /// <summary>
        /// Taksit sayısını int olarak döner. Geçersiz girişlerde 0 döner.
        /// </summary>
        public int TaksitSayisi => int.TryParse(TaksitSayisiText, out var val) ? val : 0;

        private string _aylikTaksitTutariText = "";
        public string AylikTaksitTutariText
        {
            get
            {
                // Aylık taksit tutarını ToplamTutar ve TaksitSayisi'na göre hesapla
                if (TaksitSayisi > 0 && ToplamTutar > 0)
                {
                    // Yuvarlama hassasiyetini koruyarak hesaplama
                    return (ToplamTutar / TaksitSayisi).ToString("N2", CultureInfo.InvariantCulture);
                }
                return "0.00";
            }
            set
            {
                // Aylık taksit tutarı genellikle manuel olarak set edilmez, hesaplanır.
                // Eğer UI'dan manuel girişine izin verilirse bu setter kullanılabilir.
                if (_aylikTaksitTutariText != value)
                {
                    _aylikTaksitTutariText = value;
                    OnPropertyChanged(nameof(AylikTaksitTutariText));
                }
            }
        }
        /// <summary>
        /// Hesaplanan aylık taksit tutarını decimal olarak döner.
        /// </summary>
        public decimal AylikTaksitTutari => decimal.TryParse(AylikTaksitTutariText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0;


        private string _odenenTutarText = "";
        public string OdenenTutarText
        {
            get => _odenenTutarText;
            set
            {
                if (_odenenTutarText != value)
                {
                    _odenenTutarText = value;
                    OnPropertyChanged(nameof(OdenenTutarText));
                    RaiseSaveCanExecuteChanged(); // Kaydet butonunun CanExecute durumunu tetikle
                }
            }
        }
        public decimal OdenenTutar => decimal.TryParse(OdenenTutarText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0;

        private DateTime? _baslangicTarihi;
        public DateTime? BaslangicTarihi
        {
            get => _baslangicTarihi;
            set
            {
                if (_baslangicTarihi != value)
                {
                    _baslangicTarihi = value;
                    OnPropertyChanged(nameof(BaslangicTarihi));
                    RaiseSaveCanExecuteChanged(); // Kaydet butonunun CanExecute durumunu tetikle
                }
            }
        }

        private string _notlar = "";
        public string Notlar
        {
            get => _notlar;
            set
            {
                if (_notlar != value)
                {
                    _notlar = value;
                    OnPropertyChanged(nameof(Notlar));
                }
            }
        }

        private string _selectedBanka = "";
        public string SelectedBanka
        {
            get => _selectedBanka;
            set
            {
                if (_selectedBanka != value)
                {
                    _selectedBanka = value;
                    OnPropertyChanged(nameof(SelectedBanka));
                    RaiseSaveCanExecuteChanged(); // Kaydet butonunun CanExecute durumunu tetikle
                }
            }
        }

        private string _selectedParaBirimi = "TL";
        public string SelectedParaBirimi
        {
            get => _selectedParaBirimi;
            set
            {
                if (_selectedParaBirimi != value)
                {
                    _selectedParaBirimi = value;
                    OnPropertyChanged(nameof(SelectedParaBirimi));
                    RaiseSaveCanExecuteChanged(); // Kaydet butonunun CanExecute durumunu tetikle
                }
            }
        }

        // ComboBox ve ListBox'lar için veri kaynakları
        public ObservableCollection<Company> Sirketler { get; set; }
        public ObservableCollection<CariFirma> CariFirmalar { get; set; }
        public ObservableCollection<string> Bankalar { get; set; }
        public List<string> ParaBirimleri { get; set; }

        // Komutlar
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private bool? _dialogResult;
        /// <summary>
        /// Formun kapanış sonucunu (true: kaydet, false: iptal) tutar.
        /// View'den pencereyi kapatmak için kullanılır.
        /// </summary>
        public bool? DialogResult
        {
            get => _dialogResult;
            private set
            {
                if (_dialogResult != value)
                {
                    _dialogResult = value;
                    OnPropertyChanged(nameof(DialogResult));
                }
            }
        }

        /// <summary>
        /// KrediFormViewModel sınıfının yeni bir örneğini başlatır.
        /// </summary>
        /// <param name="db">Veritabanı bağlamı (AppDbContext).</param>
        /// <param name="kredi">Düzenlenecek Kredi nesnesi (null ise yeni kayıt).</param>
        public KrediFormViewModel(AppDbContext db, Kredi? kredi = null)
        {
            _db = db;
            _kredi = kredi ?? new Kredi();
            _isEdit = kredi != null;

            // Komutları property'lere ilk değer atamalarından ÖNCE başlatmak önemlidir.
            // Bu, property'lerin setter'ları OnPropertyChanged'ı tetiklediğinde komutların null olmasını engeller.
            InitializeCommands();

            InitializeComboBoxSources(); // ComboBox/Listbox veri kaynaklarını yükle
            LoadKrediData(); // Formdaki verileri yükle (yeni veya düzenleme için)
        }

        /// <summary>
        /// Komutları (Save, Cancel) başlatır ve CanExecute koşullarını belirler.
        /// </summary>
        private void InitializeCommands()
        {
            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        /// <summary>
        /// ComboBox ve ListBox'lar için gerekli veri kaynaklarını (Şirketler, Cari Firmalar vb.) yükler.
        /// </summary>
        private void InitializeComboBoxSources()
        {
            Sirketler = new ObservableCollection<Company>(_db.Companies.Where(x => x.IsActive).OrderBy(x => x.Name).ToList());
            CariFirmalar = new ObservableCollection<CariFirma>(_db.CariFirmalar.Where(x => x.IsActive).OrderBy(x => x.Name).ToList());

            Bankalar = new ObservableCollection<string> { "Ziraat Bankası", "Garanti BBVA", "İş Bankası", "Yapı Kredi", "Akbank", "Diğer" };
            ParaBirimleri = new List<string> { "TL", "USD", "EUR" };

            SelectedParaBirimi = ParaBirimleri.FirstOrDefault() ?? "TL";
            SelectedBanka = Bankalar.FirstOrDefault() ?? "Ziraat Bankası";
        }

        /// <summary>
        /// Düzenleme modunda ise mevcut kredi verilerini form alanlarına yükler,
        /// yeni kayıt modunda ise varsayılan değerleri atar.
        /// </summary>
        private void LoadKrediData()
        {
            if (_isEdit)
            {
                // İlişkili taksitleri yükle (KrediForm'da gösteriliyorsa veya taksitler üzerinde işlem yapılıyorsa gerekli)
                _db.Entry(_kredi).Collection(k => k.Taksitler).Load();

                KrediKodu = _kredi.KrediKodu ?? "";
                SelectedCompanyId = _kredi.CompanyId;
                SelectedCariFirmaId = _kredi.CariFirmaId;
                KrediKonusu = _kredi.KrediKonusu ?? "";
                ToplamTutarText = _kredi.ToplamTutar.ToString("N2", CultureInfo.InvariantCulture);
                TaksitSayisiText = _kredi.TaksitSayisi.ToString();
                // AylikTaksitTutariText manuel set edilmez, computed property olduğu için Otomatik hesaplanır.
                OdenenTutarText = _kredi.OdenenTutar.ToString("N2", CultureInfo.InvariantCulture);
                BaslangicTarihi = _kredi.BaslangicTarihi;
                Notlar = _kredi.Notlar ?? "";
                SelectedBanka = _kredi.Banka ?? "Ziraat Bankası";
                SelectedParaBirimi = _kredi.ParaBirimi ?? "TL";
            }
            else
            {
                KrediKodu = KodUret(); // Yeni kayıt için otomatik kod üret
                BaslangicTarihi = DateTime.Today;
                // Yeni kayıtta varsayılan taksit sayısı ve tutar değerleri atayabiliriz
                TaksitSayisiText = "1";
                ToplamTutarText = "0.00";
            }
            // AylikTaksitTutariText'in UI'da doğru görünmesini sağlamak için PropertyChanged'ı tetikle.
            OnPropertyChanged(nameof(AylikTaksitTutariText));
        }

        /// <summary>
        /// Yeni bir Kredi Kodu üretir (K0001, K0002...).
        /// </summary>
        /// <returns>Yeni Kredi Kodu.</returns>
        private string KodUret()
        {
            // Veritabanındaki en son Kredi kodunu bul ve bir artır
            var lastKredi = _db.Krediler
                               .OrderByDescending(k => k.Id) // ID'ye göre sıralama daha güvenilirdir (string sıralamasından kaçınır)
                               .Select(k => k.KrediKodu)
                               .FirstOrDefault();

            int nextId = 1;
            if (!string.IsNullOrEmpty(lastKredi) && lastKredi.StartsWith("K") && lastKredi.Length > 1)
            {
                if (int.TryParse(lastKredi.Substring(1), out int lastId))
                {
                    nextId = lastId + 1;
                }
            }
            return $"K{nextId:D4}";
        }

        /// <summary>
        /// Kaydet komutunun çalışıp çalışamayacağını belirler (UI'daki Kaydet butonunu etkinleştirir/devre dışı bırakır).
        /// </summary>
        /// <param name="parameter">Komut parametresi (kullanılmıyor).</param>
        /// <returns>Kaydetme işlemi için form verileri geçerliyse true, aksi takdirde false.</returns>
        private bool CanExecuteSave(object parameter)
        {
            // Form verilerinin geçerliliğini kontrol et
            bool isValid =
                !string.IsNullOrWhiteSpace(KrediKodu) &&
                SelectedCompanyId.HasValue && SelectedCompanyId.Value > 0 && // CompanyId'nin geçerli bir değeri olduğundan emin olun
                SelectedCariFirmaId.HasValue && SelectedCariFirmaId.Value > 0 && // CariFirmaId'nin geçerli bir değeri olduğundan emin olun
                !string.IsNullOrWhiteSpace(KrediKonusu) &&
                ToplamTutar > 0 && // Toplam tutar pozitif olmalı
                TaksitSayisi >= 1 && // Taksit sayısı en az 1 olmalı
                                     // AylikTaksitTutari > 0 && // 🔥 Bu kontrol kaldırıldı/yorum satırı yapıldı (ToplamTutar ve TaksitSayisi zaten valid ise bu da valid olmalı)
                BaslangicTarihi.HasValue &&
                !string.IsNullOrWhiteSpace(SelectedBanka) &&
                !string.IsNullOrWhiteSpace(SelectedParaBirimi);

            return isValid;
        }

        /// <summary>
        /// Kredi verilerini kaydeder veya günceller, ilişkili taksitleri oluşturur/günceller.
        /// </summary>
        /// <param name="parameter">Komut parametresi (kullanılmıyor).</param>
        private void ExecuteSave(object parameter)
        {
            if (!CanExecuteSave(null))
            {
                MessageBox.Show("Lütfen tüm zorunlu alanları doğru doldurun.", "Doğrulama Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _kredi.KrediKodu = KrediKodu;
                _kredi.CompanyId = SelectedCompanyId;
                _kredi.CariFirmaId = SelectedCariFirmaId;
                _kredi.KrediKonusu = KrediKonusu;
                _kredi.ToplamTutar = ToplamTutar;
                _kredi.TaksitSayisi = TaksitSayisi;
                _kredi.AylikTaksitTutari = AylikTaksitTutari; // Hesaplanan aylık taksit tutarını ata
                _kredi.OdenenTutar = OdenenTutar;
                _kredi.BaslangicTarihi = BaslangicTarihi.Value;
                _kredi.Notlar = Notlar;
                _kredi.Banka = SelectedBanka;
                _kredi.ParaBirimi = SelectedParaBirimi;
                _kredi.IsActive = true; // Yeni/güncellenen kredi aktif olsun
                _kredi.OdenmeDurumu = (OdenenTutar >= ToplamTutar && ToplamTutar > 0); // Kredi tamamen ödendi mi?

                if (!_isEdit)
                {
                    _db.Krediler.Add(_kredi);
                }

                _db.SaveChanges(); // Kredi nesnesini kaydet ve ID'sini al (Taksitleri oluşturmak için ID gerekli)

                TaksitleriOlusturVeGuncelle(); // Taksitleri oluştur/güncelle

                _db.SaveChanges(); // Taksit değişikliklerini kaydet

                DialogResult = true; // Formu başarıyla kapat
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kaydetme sırasında bir hata oluştu: {ex.Message}\n{ex.StackTrace}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false; // Formu hata ile kapat
            }
        }

        /// <summary>
        /// Kredi taksitlerini oluşturur veya günceller.
        /// Düzenleme modunda eski taksitleri silip yeniden oluşturur.
        /// </summary>
        private void TaksitleriOlusturVeGuncelle()
        {
            // Düzenleme modunda eski taksitleri sil (tamamen sıfırdan oluşturmak için)
            // Not: Taksitlerin durumlarını (ödendi mi vs.) korumak istiyorsanız bu mantığı değiştirmelisiniz.
            // Genellikle taksitler bir kez oluşturulduktan sonra sadece durumları güncellenir, silinmez.
            var existingTaksitler = _db.KrediTaksitler.Where(t => t.KrediId == _kredi.Id).ToList();
            if (existingTaksitler.Any())
            {
                _db.KrediTaksitler.RemoveRange(existingTaksitler);
                // DbContext'in yerel önbelleğini de temizlemek için _kredi.Taksitler.Clear() yapılabilir
                // _kredi.Taksitler?.Clear();
            }

            // Kredi nesnesi kaydedilmeden taksitler oluşturulamaz, _kredi.Id'nin geçerli olması gerekir.
            if (_kredi.Id == 0)
                throw new InvalidOperationException("Kredi nesnesi kaydedilmeden taksitler oluşturulamaz. Kredi ID'si 0.");


            List<KrediTaksit> yeniTaksitler = new List<KrediTaksit>();

            for (int i = 0; i < TaksitSayisi; i++)
            {
                DateTime taksitTarihi = BaslangicTarihi.Value.AddMonths(i);

                // Son taksitin tutarını yuvarlama hatalarını dengelemek için ayarla
                decimal currentTaksitTutari = AylikTaksitTutari;
                if (i == TaksitSayisi - 1)
                {
                    // Toplam tutardan önceki taksitlerin toplamını çıkar
                    currentTaksitTutari = ToplamTutar - (AylikTaksitTutari * (TaksitSayisi - 1));
                    // Hassasiyet problemi yaşamamak için yuvarla
                    currentTaksitTutari = Math.Round(currentTaksitTutari, 2);
                }

                var taksit = new KrediTaksit
                {
                    KrediId = _kredi.Id,
                    KrediKodu = _kredi.KrediKodu,
                    TaksitNo = i + 1,
                    Tutar = currentTaksitTutari, // Güncellenmiş tutarı kullan
                    Tarih = taksitTarihi,
                    OdenmeDurumu = false, // Yeni taksitler başlangıçta ödenmemiş
                    OdeyenKullaniciAdi = null, // Henüz ödenmediği için boş
                    OdemeBankasi = null,
                    OdenmeTarihi = null
                };
                yeniTaksitler.Add(taksit);
            }

            _db.KrediTaksitler.AddRange(yeniTaksitler);
            // Kredi nesnesinin Taksitler koleksiyonunu güncelleyin, böylece EF Core tarafından takip edilir.
            // Bu, koleksiyon otomatik yükleme (lazy loading) kapalıysa veya viewmodel'de hemen güncel taksitlere ihtiyacınız varsa faydalıdır.
            _kredi.Taksitler = yeniTaksitler;
        }

        /// <summary>
        /// İptal komutunu yürütür, formu iptal durumuyla kapatır.
        /// </summary>
        /// <param name="parameter">Komut parametresi (kullanılmıyor).</param>
        private void ExecuteCancel(object parameter)
        {
            DialogResult = false;
        }

        /// <summary>
        /// SaveCommand'ın CanExecute durumunu güvenli bir şekilde tetiklemek için yardımcı metod.
        /// </summary>
        private void RaiseSaveCanExecuteChanged()
        {
            // SaveCommand'ın null olup olmadığını kontrol ederek NullReferenceException'ı önle
            if (SaveCommand is RelayCommand saveCmd)
            {
                saveCmd.RaiseCanExecuteChanged();
            }
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            // Property değiştiğinde diğer komutların CanExecute durumunu da tetiklemek isterseniz buraya ekleyebilirsiniz.
            // Ancak genellikle sadece SaveCommand'ın durumu değişir.
            // Bu metodun kendisi artık doğrudan RaiseSaveCanExecuteChanged() çağırmamalıdır,
            // çünkü her property setter'ı zaten bunu yapıyor.
            // Eğer diğer komutların durumu değişiyorsa, onları da buraya ekleyebilirsiniz:
            if (CancelCommand is RelayCommand cancelCmd)
            {
                cancelCmd.RaiseCanExecuteChanged(); // CancelCommand'ın CanExecute durumu değişmiyorsa gerek yok
            }
        }
        #endregion
    }
}