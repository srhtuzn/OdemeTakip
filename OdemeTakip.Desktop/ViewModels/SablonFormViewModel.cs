// OdemeTakip.Desktop.ViewModels/SablonFormViewModel.cs
using OdemeTakip.Data;
using OdemeTakip.Entities;
using OdemeTakip.Desktop.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using System.Globalization;

namespace OdemeTakip.Desktop.ViewModels
{
    public class SablonFormViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _db;
        private int? _sablonId;
        private bool _isEditMode;

        private string _giderTuru = "";
        public string GiderTuru
        {
            get => _giderTuru;
            set
            {
                if (_giderTuru != value)
                {
                    _giderTuru = value;
                    OnPropertyChanged(nameof(GiderTuru));
                }
            }
        }

        private string _aciklama = "";
        public string Aciklama
        {
            get => _aciklama;
            set
            {
                if (_aciklama != value)
                {
                    _aciklama = value;
                    OnPropertyChanged(nameof(Aciklama));
                }
            }
        }

        private int _gun;
        public int Gun
        {
            get => _gun;
            set
            {
                if (_gun != value)
                {
                    _gun = value;
                    OnPropertyChanged(nameof(Gun));
                }
            }
        }

        private int? _selectedSirketId;
        public int? SelectedSirketId
        {
            get => _selectedSirketId;
            set
            {
                if (_selectedSirketId != value)
                {
                    _selectedSirketId = value;
                    OnPropertyChanged(nameof(SelectedSirketId));
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
                }
            }
        }

        private string _paraBirimi = "TL";
        public string ParaBirimi
        {
            get => _paraBirimi;
            set
            {
                if (_paraBirimi != value)
                {
                    _paraBirimi = value;
                    OnPropertyChanged(nameof(ParaBirimi));
                }
            }
        }

        public List<string> GiderTurleri { get; set; }
        public ObservableCollection<Company> Sirketler { get; set; }
        public ObservableCollection<CariFirma> CariFirmalar { get; set; }
        public List<string> ParaBirimleri { get; set; }

        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private bool? _dialogResult;
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

        public SablonFormViewModel(AppDbContext db, int? sablonId = null)
        {
            _db = db;
            _sablonId = sablonId;
            _isEditMode = sablonId.HasValue;

            InitializeComboBoxSources();
            LoadSablonData();

            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        private void InitializeComboBoxSources()
        {
            GiderTurleri = new List<string> { "Elektrik", "Su", "Doğalgaz", "Telefon", "İnternet", "Diğer" };
            Sirketler = new ObservableCollection<Company>(_db.Companies.Where(x => x.IsActive).OrderBy(x => x.Name).ToList());
            CariFirmalar = new ObservableCollection<CariFirma>(_db.CariFirmalar.Where(x => x.IsActive).OrderBy(x => x.Name).ToList());
            ParaBirimleri = new List<string> { "TL", "USD", "EUR" };

            ParaBirimi = ParaBirimleri.FirstOrDefault() ?? "TL";
        }

        private void LoadSablonData()
        {
            if (_isEditMode && _sablonId.HasValue)
            {
                var sablon = _db.DegiskenOdemeSablonlari.FirstOrDefault(x => x.Id == _sablonId.Value);
                if (sablon != null)
                {
                    GiderTuru = sablon.GiderTuru;
                    Aciklama = sablon.Aciklama;
                    Gun = sablon.Gun;
                    SelectedSirketId = sablon.CompanyId;
                    SelectedCariFirmaId = sablon.CariFirmaId;
                    ParaBirimi = sablon.ParaBirimi;
                }
            }
        }

        private bool CanExecuteSave(object parameter)
        {
            bool isGiderTuruValid = !string.IsNullOrWhiteSpace(GiderTuru);
            bool isGunValid = Gun >= 1 && Gun <= 31;
            bool isSirketValid = SelectedSirketId.HasValue;
            bool isCariFirmaValid = SelectedCariFirmaId.HasValue;
            bool isParaBirimiValid = !string.IsNullOrWhiteSpace(ParaBirimi);

            return isGiderTuruValid && isGunValid && isSirketValid && isCariFirmaValid && isParaBirimiValid;
        }

        private void ExecuteSave(object parameter)
        {
            if (!CanExecuteSave(null))
            {
                MessageBox.Show("Lütfen tüm zorunlu alanları doğru doldurun.", "Doğrulama Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_isEditMode && _sablonId.HasValue)
                {
                    var sablon = _db.DegiskenOdemeSablonlari.FirstOrDefault(x => x.Id == _sablonId.Value);
                    if (sablon != null)
                    {
                        sablon.GiderTuru = GiderTuru;
                        sablon.Aciklama = Aciklama;
                        sablon.Gun = Gun;
                        sablon.CompanyId = SelectedSirketId;
                        sablon.CariFirmaId = SelectedCariFirmaId;
                        sablon.ParaBirimi = ParaBirimi;
                    }
                }
                else
                {
                    var yeniSablon = new DegiskenOdemeSablonu
                    {
                        GiderTuru = GiderTuru,
                        Aciklama = Aciklama,
                        Gun = Gun,
                        CompanyId = SelectedSirketId,
                        CariFirmaId = SelectedCariFirmaId,
                        ParaBirimi = ParaBirimi,
                        IsActive = true
                    };
                    _db.DegiskenOdemeSablonlari.Add(yeniSablon);
                }

                _db.SaveChanges();
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kaydetme sırasında bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
            }
        }

        private void ExecuteCancel(object parameter)
        {
            DialogResult = false;
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
        #endregion
    }
}