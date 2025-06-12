// OdemeTakip.Desktop.ViewModels/SablonListViewModel.cs
using OdemeTakip.Data;
using OdemeTakip.Entities;
using OdemeTakip.Desktop.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace OdemeTakip.Desktop.ViewModels
{
    public class SablonListViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _db;

        public ObservableCollection<SablonViewModel> Sablonlar { get; set; }

        private SablonViewModel _selectedSablon;
        public SablonViewModel SelectedSablon
        {
            get => _selectedSablon;
            set
            {
                if (_selectedSablon != value)
                {
                    _selectedSablon = value;
                    OnPropertyChanged(nameof(SelectedSablon));
                    ((RelayCommand)UpdateCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand LoadCommand { get; private set; }
        public ICommand AddCommand { get; private set; }
        public ICommand UpdateCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        public SablonListViewModel(AppDbContext db)
        {
            _db = db;
            Sablonlar = new ObservableCollection<SablonViewModel>();

            LoadCommand = new RelayCommand(ExecuteLoadSablonlar);
            AddCommand = new RelayCommand(ExecuteAddSablon);
            UpdateCommand = new RelayCommand(ExecuteUpdateSablon, CanExecuteUpdateOrDeleteSablon);
            DeleteCommand = new RelayCommand(ExecuteDeleteSablon, CanExecuteUpdateOrDeleteSablon);

            ExecuteLoadSablonlar(null);
        }

        #region Command Execution Methods

        private void ExecuteLoadSablonlar(object parameter)
        {
            Sablonlar.Clear();
            var liste = _db.DegiskenOdemeSablonlari.AsNoTracking() // AsNoTracking eklendi
                .Include(x => x.Company)
                .Include(x => x.CariFirma) // CariFirma da eklendi
                .Where(x => x.IsActive)
                .OrderBy(x => x.GiderTuru) // Sıralama eklendi
                .Select(x => new SablonViewModel
                {
                    Id = x.Id,
                    GiderTuru = x.GiderTuru,
                    Aciklama = x.Aciklama,
                    Gun = x.Gun,
                    SirketAdi = x.Company != null ? x.Company.Name : "",
                    CariFirmaAdi = x.CariFirma != null ? x.CariFirma.Name : "",
                    ParaBirimi = x.ParaBirimi
                })
                .ToList();

            foreach (var item in liste)
                Sablonlar.Add(item);
        }

        private void ExecuteAddSablon(object parameter)
        {
            var form = new SablonForm(_db);
            if (form.ShowDialog() == true)
            {
                ExecuteLoadSablonlar(null);
            }
        }

        private bool CanExecuteUpdateOrDeleteSablon(object parameter)
        {
            return SelectedSablon != null;
        }

        private void ExecuteUpdateSablon(object parameter)
        {
            if (SelectedSablon != null)
            {
                // Entity'yi takip modunda çek
                var sablonEntity = _db.DegiskenOdemeSablonlari.Find(SelectedSablon.Id);
                if (sablonEntity == null)
                {
                    MessageBox.Show("Şablon bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var form = new SablonForm(_db, SelectedSablon.Id); // ID ile formu aç
                if (form.ShowDialog() == true)
                {
                    ExecuteLoadSablonlar(null);
                }
            }
        }

        private void ExecuteDeleteSablon(object parameter)
        {
            if (SelectedSablon != null)
            {
                if (MessageBox.Show("Bu şablonla oluşturulmuş tüm ödemeler de silinecek. Emin misiniz?",
                                    "Şablon ve Ödeme Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    var sablon = _db.DegiskenOdemeSablonlari.Find(SelectedSablon.Id); // Takip modunda çek
                    if (sablon != null)
                    {
                        var bagliOdemeler = _db.DegiskenOdemeler
                            .Where(x => x.SablonId == sablon.Id) // SablonId üzerinden doğru ilişkiyi kullanıyoruz
                            .ToList();

                        if (bagliOdemeler.Any())
                            _db.DegiskenOdemeler.RemoveRange(bagliOdemeler);

                        _db.DegiskenOdemeSablonlari.Remove(sablon);

                        _db.SaveChanges();
                        ExecuteLoadSablonlar(null);

                        MessageBox.Show("Şablon ve ilişkili ödemeler başarıyla silindi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            ((RelayCommand)LoadCommand)?.RaiseCanExecuteChanged(); // Yenile butonu için
            ((RelayCommand)AddCommand)?.RaiseCanExecuteChanged();
            ((RelayCommand)UpdateCommand)?.RaiseCanExecuteChanged();
            ((RelayCommand)DeleteCommand)?.RaiseCanExecuteChanged();
        }
        #endregion
    }
}