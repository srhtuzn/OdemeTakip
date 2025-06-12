// OdemeTakip.Desktop.ViewModels/KrediKartiAnaListViewModel.cs
using OdemeTakip.Data;
using OdemeTakip.Entities;
using OdemeTakip.Desktop.Helpers; // RelayCommand için
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Collections.Generic;

namespace OdemeTakip.Desktop.ViewModels
{
    public class KrediKartiAnaListViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _db;

        public ObservableCollection<KrediKartiViewModel> KrediKartlari { get; set; }

        private KrediKartiViewModel _selectedKrediKarti;
        public KrediKartiViewModel SelectedKrediKarti
        {
            get => _selectedKrediKarti;
            set
            {
                if (_selectedKrediKarti != value)
                {
                    _selectedKrediKarti = value;
                    OnPropertyChanged(nameof(SelectedKrediKarti));
                    ExecuteLoadHarcamalar(null);
                    ExecuteLoadTaksitler(null);
                    ((RelayCommand)EditCardCommand)?.RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteCardCommand)?.RaiseCanExecuteChanged();
                    ((RelayCommand)AddHarcamaCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<KrediKartiHarcamaViewModel> Harcamalar { get; set; }

        private KrediKartiHarcamaViewModel _selectedHarcama;
        public KrediKartiHarcamaViewModel SelectedHarcama
        {
            get => _selectedHarcama;
            set
            {
                if (_selectedHarcama != value)
                {
                    _selectedHarcama = value;
                    OnPropertyChanged(nameof(SelectedHarcama));
                    ((RelayCommand)EditHarcamaCommand)?.RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteHarcamaCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<KrediKartiOdemeViewModel> TaksitliOdemeler { get; set; }

        private string _kartSearchText = "";
        public string KartSearchText
        {
            get => _kartSearchText;
            set
            {
                if (_kartSearchText != value)
                {
                    _kartSearchText = value;
                    OnPropertyChanged(nameof(KartSearchText));
                    ExecuteLoadKrediKartlari(null);
                }
            }
        }

        private string _harcamaSearchText = "";
        public string HarcamaSearchText
        {
            get => _harcamaSearchText;
            set
            {
                if (_harcamaSearchText != value)
                {
                    _harcamaSearchText = value;
                    OnPropertyChanged(nameof(HarcamaSearchText));
                    ExecuteLoadHarcamalar(null);
                }
            }
        }

        public ICommand LoadKrediKartlariCommand { get; private set; }
        public ICommand AddCardCommand { get; private set; }
        public ICommand EditCardCommand { get; private set; }
        public ICommand DeleteCardCommand { get; private set; }

        public ICommand AddHarcamaCommand { get; private set; }
        public ICommand EditHarcamaCommand { get; private set; }
        public ICommand DeleteHarcamaCommand { get; private set; }

        public KrediKartiAnaListViewModel(AppDbContext db)
        {
            _db = db;
            KrediKartlari = new ObservableCollection<KrediKartiViewModel>();
            Harcamalar = new ObservableCollection<KrediKartiHarcamaViewModel>();
            TaksitliOdemeler = new ObservableCollection<KrediKartiOdemeViewModel>();

            InitializeCommands();
            ExecuteLoadKrediKartlari(null);
        }

        private void InitializeCommands()
        {
            LoadKrediKartlariCommand = new RelayCommand(ExecuteLoadKrediKartlari);
            AddCardCommand = new RelayCommand(ExecuteAddCard);
            EditCardCommand = new RelayCommand(ExecuteEditCard, CanExecuteCardOperation);
            DeleteCardCommand = new RelayCommand(ExecuteDeleteCard, CanExecuteCardOperation);

            AddHarcamaCommand = new RelayCommand(ExecuteAddHarcama, CanExecuteHarcamaAdd);
            EditHarcamaCommand = new RelayCommand(ExecuteEditHarcama, CanExecuteHarcamaOperation);
            DeleteHarcamaCommand = new RelayCommand(ExecuteDeleteHarcama, CanExecuteHarcamaOperation);
        }

        #region Command Execution Methods

       public void ExecuteLoadKrediKartlari(object parameter)
        {
            KrediKartlari.Clear();
            var kartlar = _db.KrediKartlari.AsNoTracking()
                .Include(k => k.Company)
                .Where(k => k.IsActive && k.CardName.ToLower().Contains(KartSearchText.ToLower()))
                .OrderBy(k => k.CardName)
                .Select(k => new KrediKartiViewModel
                {
                    Id = k.Id,
                    CardName = k.CardName,
                    OwnerType = k.OwnerType,
                    CompanyId = k.CompanyId,
                    CompanyName = k.Company != null ? k.Company.Name : "Bilinmiyor",
                    Banka = k.Banka,
                    CardNumberLast4 = k.CardNumberLast4,
                    Limit = k.Limit,
                    DueDate = k.DueDate,
                    PaymentDueDate = k.PaymentDueDate,
                    Notes = k.Notes,
                    IsActive = k.IsActive,
                    // 🔥 Yeni açıklama satırı formatı burada 🔥
                    Aciklama = $"{k.CardName} - {k.CardNumberLast4} - Limit: {k.Limit:C2} - Hesap Kesim: {k.DueDate:dd.MM}"
                })
                .ToList();

            foreach (var kart in kartlar)
            {
                KrediKartlari.Add(kart);
            }

            if (SelectedKrediKarti != null && !KrediKartlari.Any(k => k.Id == SelectedKrediKarti.Id))
            {
                SelectedKrediKarti = null;
            }
            else if (SelectedKrediKarti == null && KrediKartlari.Any())
            {
                SelectedKrediKarti = KrediKartlari.FirstOrDefault();
            }
        }

        public void ExecuteLoadHarcamalar(object parameter)
        {
            Harcamalar.Clear();
            if (SelectedKrediKarti == null) return;

            var harcamalar = _db.KrediKartiHarcamalari.AsNoTracking()
                .Where(h => h.KrediKartiId == SelectedKrediKarti.Id && h.IsActive && h.Aciklama.ToLower().Contains(HarcamaSearchText.ToLower()))
                .OrderByDescending(h => h.HarcamaTarihi)
                .Select(h => new KrediKartiHarcamaViewModel
                {
                    Id = h.Id,
                    KrediKartiId = h.KrediKartiId, // KrediKartiHarcamaViewModel'de int olduğu için direkt atanabilir
                    Aciklama = h.Aciklama,
                    Tutar = h.Tutar,
                    TaksitSayisi = h.TaksitSayisi, // KrediKartiHarcamaViewModel'de int olduğu için direkt atanabilir
                    HarcamaTarihi = h.HarcamaTarihi,
                    IsActive = h.IsActive,
                    KrediKartiAdi = SelectedKrediKarti.CardName
                })
                .ToList();

            foreach (var harcama in harcamalar)
            {
                Harcamalar.Add(harcama);
            }
        }

        public void ExecuteLoadTaksitler(object parameter)
        {
            TaksitliOdemeler.Clear();
            if (SelectedKrediKarti == null) return;

            var taksitler = _db.KrediKartiOdemeleri.AsNoTracking()
                .Include(o => o.KrediKartiHarcama)
                .Where(o => o.KrediKartiId == SelectedKrediKarti.Id && o.IsActive)
                .OrderBy(o => o.OdemeTarihi)
                .Select(o => new KrediKartiOdemeViewModel
                {
                    Id = o.Id,
                    KrediKartiId = o.KrediKartiId, // KrediKartiOdemeViewModel'de int? olduğu için direkt atanabilir
                    KrediKartiHarcamaId = o.KrediKartiHarcamaId, // KrediKartiOdemeViewModel'de int? olduğu için direkt atanabilir
                    OdemeTarihi = o.OdemeTarihi, // KrediKartiOdemeViewModel'de DateTime? olduğu için direkt atanabilir
                    Tutar = o.Tutar,
                    IsActive = o.IsActive,
                    OdenmeDurumu = o.OdenmeDurumu,
                    ToplamTaksit = o.ToplamTaksit, // KrediKartiOdemeViewModel'de int? olduğu için direkt atanabilir
                    TaksitNo = o.TaksitNo, // KrediKartiOdemeViewModel'de int? olduğu için direkt atanabilir
                    HarcamaAciklamasi = o.KrediKartiHarcama != null ? o.KrediKartiHarcama.Aciklama : "Harcama Bilgisi Yok",
                    HarcamaToplamTutar = o.KrediKartiHarcama != null ? o.KrediKartiHarcama.Tutar : 0,
                    HarcamaTaksitSayisi = o.KrediKartiHarcama != null ? o.KrediKartiHarcama.TaksitSayisi : 0, // KrediKartiHarcama entity'de int olduğu için int
                    KrediKartiAdi = SelectedKrediKarti.CardName
                })
                .ToList();

            foreach (var taksit in taksitler)
            {
                TaksitliOdemeler.Add(taksit);
            }
        }

        /// <summary>
        /// Yeni kart ekleme formunu açar.
        /// </summary>
        private void ExecuteAddCard(object parameter)
        {
            var form = new KrediKartiForm(_db); // Doğrudan View çağrısı (IDialogService ile iyileştirilebilir)
            if (form.ShowDialog() == true)
            {
                ExecuteLoadKrediKartlari(null); // Başarılı ise listeyi yenile
            }
        }

        /// <summary>
        /// Seçili kartı düzenleme formunu açar.
        /// </summary>
        private void ExecuteEditCard(object parameter)
        {
            if (SelectedKrediKarti == null) return;

            // Entity'yi takip modunda çekmek önemli, böylece KrediKartiForm'da değişiklikler direkt save edilebilir.
            var kartEntity = _db.KrediKartlari.Find(SelectedKrediKarti.Id);
            if (kartEntity == null)
            {
                MessageBox.Show("Kart bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var form = new KrediKartiForm(_db, kartEntity); // Doğrudan View çağrısı
            if (form.ShowDialog() == true)
            {
                ExecuteLoadKrediKartlari(null); // Başarılı ise listeyi yenile
            }
        }

        /// <summary>
        /// Seçili kartı pasif hale getirerek silme işlemini gerçekleştirir.
        /// </summary>
        private void ExecuteDeleteCard(object parameter)
        {
            if (SelectedKrediKarti == null) return;

            if (MessageBox.Show($"'{SelectedKrediKarti.CardName}' kartını silmek istiyor musunuz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var kartEntity = _db.KrediKartlari.Find(SelectedKrediKarti.Id);
                if (kartEntity != null)
                {
                    kartEntity.IsActive = false;
                    _db.SaveChanges();
                    ExecuteLoadKrediKartlari(null);
                }
            }
        }

        /// <summary>
        /// Kart ekleme/düzenleme butonlarının aktif olup olmayacağını belirler.
        /// </summary>
        private bool CanExecuteCardOperation(object parameter)
        {
            return SelectedKrediKarti != null;
        }

        /// <summary>
        /// Yeni harcama ekleme formunu açar.
        /// </summary>
        private void ExecuteAddHarcama(object parameter)
        {
            if (SelectedKrediKarti == null)
            {
                MessageBox.Show("Harcama eklemek için lütfen önce bir kredi kartı seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var kartEntity = _db.KrediKartlari.Find(SelectedKrediKarti.Id);
            if (kartEntity == null) return;

            var form = new KrediKartiHarcamaForm(_db, kartEntity);
            if (form.ShowDialog() == true)
            {
                ExecuteLoadHarcamalar(null);
                ExecuteLoadTaksitler(null);
            }
        }

        /// <summary>
        /// Seçili harcamayı düzenleme formunu açar.
        /// </summary>
        private void ExecuteEditHarcama(object parameter)
        {
            if (SelectedHarcama == null) return;

            var harcamaEntity = _db.KrediKartiHarcamalari.Find(SelectedHarcama.Id);
            if (harcamaEntity == null)
            {
                MessageBox.Show("Harcama bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _db.Entry(harcamaEntity).Reference(h => h.KrediKarti).Load();


            var form = new KrediKartiHarcamaForm(_db, harcamaEntity.KrediKarti!, harcamaEntity);
            if (form.ShowDialog() == true)
            {
                ExecuteLoadHarcamalar(null);
                ExecuteLoadTaksitler(null);
            }
        }

        private bool CanExecuteHarcamaAdd(object parameter)
        {
            return SelectedKrediKarti != null;
        }

        private bool CanExecuteHarcamaOperation(object parameter)
        {
            return SelectedHarcama != null;
        }

        /// <summary>
        /// Seçili harcamayı ve ilişkili taksitleri pasif hale getirerek silme işlemini gerçekleştirir.
        /// </summary>
        private void ExecuteDeleteHarcama(object parameter)
        {
            if (SelectedHarcama == null) return;

            if (MessageBox.Show($"'{SelectedHarcama.Aciklama}' harcamasını silmek istiyor musunuz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var harcamaEntity = _db.KrediKartiHarcamalari.Find(SelectedHarcama.Id);
                if (harcamaEntity != null)
                {
                    harcamaEntity.IsActive = false;

                    var taksitler = _db.KrediKartiOdemeleri
                        .Where(o => o.KrediKartiHarcamaId == harcamaEntity.Id && o.IsActive)
                        .ToList();

                    foreach (var taksit in taksitler)
                    {
                        taksit.IsActive = false;
                    }

                    _db.SaveChanges();
                    ExecuteLoadHarcamalar(null);
                    ExecuteLoadTaksitler(null);
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}