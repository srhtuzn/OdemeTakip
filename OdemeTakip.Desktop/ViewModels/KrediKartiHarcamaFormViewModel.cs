// OdemeTakip.Desktop.ViewModels/KrediKartiHarcamaFormViewModel.cs
using OdemeTakip.Data;
using OdemeTakip.Entities; // KrediKarti, KrediKartiHarcama, KrediKartiOdeme için
using OdemeTakip.Desktop.Helpers; // RelayCommand için
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows; // MessageBox için (idealde IDialogService kullanılmalı)
using System.Globalization; // decimal.TryParse için
using Microsoft.EntityFrameworkCore; // Include için

namespace OdemeTakip.Desktop.ViewModels
{
    /// <summary>
    /// KrediKartiHarcamaForm (Kredi Kartı Harcaması Ekle/Güncelle Formu) için ViewModel.
    /// Harcama bilgilerinin girişini, doğrulamasını ve ilişkili taksit ödemelerinin oluşturulmasını yönetir.
    /// </summary>
    public class KrediKartiHarcamaFormViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _db;
        private KrediKartiHarcama _harcama; // Çalıştığımız harcama nesnesi
        private bool _isEdit;

        private KrediKarti _currentKrediKartiEntity; // Anlık seçili/ilgili KrediKarti nesnesi

        // Form Alanları için Property'ler
        private int _selectedKrediKartiId;
        public int SelectedKrediKartiId
        {
            get => _selectedKrediKartiId;
            set
            {
                if (_selectedKrediKartiId != value)
                {
                    _selectedKrediKartiId = value;
                    OnPropertyChanged(nameof(SelectedKrediKartiId));
                    // ComboBox'dan seçim değiştiğinde _currentKrediKartiEntity'yi güncelle
                    if (_db != null && value > 0)
                    {
                        // Company bilgisini de çekiyoruz, taksit oluştururken kullanılacak
                        _currentKrediKartiEntity = _db.KrediKartlari.Include(k => k.Company).FirstOrDefault(k => k.Id == value) ?? new KrediKarti();
                    }
                    else if (value == 0)
                    {
                        _currentKrediKartiEntity = new KrediKarti();
                    }
                    ((RelayCommand)SaveCommand)?.RaiseCanExecuteChanged();
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
                    ((RelayCommand)SaveCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _tutarText = "";
        public string TutarText
        {
            get => _tutarText;
            set
            {
                if (_tutarText != value)
                {
                    _tutarText = value;
                    OnPropertyChanged(nameof(TutarText));
                    ((RelayCommand)SaveCommand)?.RaiseCanExecuteChanged();
                }
            }
        }
        public decimal Tutar => decimal.TryParse(TutarText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0;

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
                    ((RelayCommand)SaveCommand)?.RaiseCanExecuteChanged();
                }
            }
        }
        public int TaksitSayisi => int.TryParse(TaksitSayisiText, out var val) ? val : 0;

        private DateTime? _harcamaTarihi;
        public DateTime? HarcamaTarihi
        {
            get => _harcamaTarihi;
            set
            {
                if (_harcamaTarihi != value)
                {
                    _harcamaTarihi = value;
                    OnPropertyChanged(nameof(HarcamaTarihi));
                    ((RelayCommand)SaveCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        // ComboBox Kaynağı
        public ObservableCollection<KrediKarti> KrediKartlari { get; set; }

        // Komutlar
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

        /// <summary>
        /// KrediKartiHarcamaFormViewModel sınıfının yeni bir örneğini başlatır.
        /// </summary>
        /// <param name="db">Veritabanı bağlamı.</param>
        /// <param name="seciliKart">Harcamanın yapılacağı kredi kartı. Yeni harcama eklenirken gereklidir.</param>
        /// <param name="harcama">Düzenlenecek harcama nesnesi (null ise yeni kayıt).</param>
        public KrediKartiHarcamaFormViewModel(AppDbContext db, KrediKarti seciliKart, KrediKartiHarcama? harcama = null)
        {
            _db = db;
            // Constructor'da gelen seciliKart'ı burada atıyoruz.
            // Bu kartın Company bilgisi de yüklü olmalı, bu yüzden Find yerine FirstOrDefault ile Include kullanıldı.
            _currentKrediKartiEntity = _db.KrediKartlari.Include(k => k.Company).FirstOrDefault(k => k.Id == seciliKart.Id) ?? seciliKart;


            _harcama = harcama ?? new KrediKartiHarcama { IsActive = true, OdenmeDurumu = false };
            _isEdit = harcama != null;

            InitializeComboBoxSources();
            LoadHarcamaData();

            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        private void InitializeComboBoxSources()
        {
            if (_db != null && _db.KrediKartlari != null)
            {
                KrediKartlari = new ObservableCollection<KrediKarti>(_db.KrediKartlari.Where(k => k.IsActive).OrderBy(k => k.CardName).ToList());
            }
            else
            {
                KrediKartlari = new ObservableCollection<KrediKarti>();
            }
        }

        private void LoadHarcamaData()
        {
            if (_isEdit)
            {
                SelectedKrediKartiId = _harcama.KrediKartiId;
                Aciklama = _harcama.Aciklama;
                TutarText = _harcama.Tutar.ToString("N2", CultureInfo.InvariantCulture);
                TaksitSayisiText = _harcama.TaksitSayisi.ToString();
                HarcamaTarihi = _harcama.HarcamaTarihi;
            }
            else
            {
                // Yeni harcama için başlangıç kartını set et (constructor'dan gelen seciliKart'ın Id'si)
                SelectedKrediKartiId = _currentKrediKartiEntity.Id;
                HarcamaTarihi = DateTime.Today;
            }
        }

        private bool CanExecuteSave(object parameter)
        {
            bool isValid =
                SelectedKrediKartiId > 0 &&
                !string.IsNullOrWhiteSpace(Aciklama) &&
                Tutar > 0 &&
                TaksitSayisi >= 1 &&
                HarcamaTarihi.HasValue;

            return isValid;
        }

        private void ExecuteSave(object parameter)
        {
            if (!CanExecuteSave(null))
            {
                MessageBox.Show("Lütfen tüm zorunlu alanları doğru doldurun.", "Doğrulama Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _harcama.KrediKartiId = SelectedKrediKartiId;
            _harcama.Aciklama = Aciklama;
            _harcama.Tutar = Tutar;
            _harcama.TaksitSayisi = TaksitSayisi;
            _harcama.HarcamaTarihi = HarcamaTarihi.Value;
            _harcama.IsActive = true;
            _harcama.OdenmeDurumu = false; // Harcama başlangıçta ödenmemiş olur, taksitler ödenir.

            try
            {
                if (!_isEdit)
                {
                    _harcama.HarcamaKodu = GenerateHarcamaKodu(_db); // Yeni harcama için kod oluştur
                    _db.KrediKartiHarcamalari.Add(_harcama);
                }
                else
                {
                    _db.KrediKartiHarcamalari.Update(_harcama);
                    // Düzenleme modunda eski taksitleri pasif hale getirme
                    // (TaksitleriOlusturVeGuncelle metodu içinde zaten yapılıyor)
                }
                _db.SaveChanges(); // Harcamanın ID'si burada atanır.

                // Taksitleri oluştur/güncelle
                TaksitleriOlusturVeGuncelle();
                _db.SaveChanges(); // Taksitleri kaydet

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Harcama kaydedilirken bir hata oluştu: {ex.Message}\n{ex.StackTrace}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
            }
        }

        /// <summary>
        /// Harcamaya ait taksit ödemelerini oluşturur veya günceller.
        /// Hem tek taksitli hem de çok taksitli harcamalar için KrediKartiOdeme kaydı oluşturur.
        /// </summary>
        private void TaksitleriOlusturVeGuncelle()
        {
            // Mevcut taksitleri soft-delete yap
            var mevcutTaksitler = _db.KrediKartiOdemeleri
                .Where(o => o.KrediKartiHarcamaId == _harcama.Id && o.IsActive)
                .ToList();

            foreach (var taksit in mevcutTaksitler)
            {
                taksit.IsActive = false; // Eski taksitleri pasifleştir
            }

            var krediKarti = _currentKrediKartiEntity; // Null kontrolü yapıldı
            if (krediKarti == null || krediKarti.Id == 0)
            {
                throw new Exception("İlgili kredi kartı bilgisi eksik. Taksitler oluşturulamadı.");
            }

            // Yeni taksitleri oluştur
            List<KrediKartiOdeme> yeniTaksitler = new List<KrediKartiOdeme>();
            decimal taksitTutariRounded = Math.Round(_harcama.Tutar / _harcama.TaksitSayisi, 2);

            // İlk ödeme tarihini hesapla (Ekstre Kesim Tarihi mantığı)
            DateTime ilkOdemeTarihiBase = new DateTime(_harcama.HarcamaTarihi.Year, _harcama.HarcamaTarihi.Month, krediKarti.PaymentDueDate.Day);

            // Eğer harcama, kartın ekstre kesim tarihinden (DueDate) sonra yapıldıysa,
            // ilk taksit bir sonraki ayın son ödeme tarihine düşer.
            DateTime harcamaEkstreKesimTarihi = new DateTime(_harcama.HarcamaTarihi.Year, _harcama.HarcamaTarihi.Month, krediKarti.DueDate.Day);
            if (_harcama.HarcamaTarihi.Date > harcamaEkstreKesimTarihi.Date)
            {
                ilkOdemeTarihiBase = ilkOdemeTarihiBase.AddMonths(1); // Bir sonraki ayın son ödeme tarihine at
            }
            // Aksi halde, aynı ayın son ödeme tarihine düşer (ilkOdemeTarihiBase zaten doğru ayda).


            for (int i = 0; i < _harcama.TaksitSayisi; i++)
            {
                DateTime taksitOdemeTarihi = ilkOdemeTarihiBase.AddMonths(i);

                // Ayın son günü kontrolü (31 çeken ay -> 30 çeken ay gibi)
                int maxDayInMonth = DateTime.DaysInMonth(taksitOdemeTarihi.Year, taksitOdemeTarihi.Month);
                if (taksitOdemeTarihi.Day > maxDayInMonth)
                {
                    taksitOdemeTarihi = new DateTime(taksitOdemeTarihi.Year, taksitOdemeTarihi.Month, maxDayInMonth);
                }

                decimal currentTaksitTutari = taksitTutariRounded;
                // Son taksit küsuratını düzeltme
                if (i == _harcama.TaksitSayisi - 1)
                {
                    currentTaksitTutari = _harcama.Tutar - (taksitTutariRounded * (_harcama.TaksitSayisi - 1));
                }

                var taksit = new KrediKartiOdeme
                {
                    KrediKartiId = _harcama.KrediKartiId,
                    KrediKartiHarcamaId = _harcama.Id,
                    OdemeTarihi = taksitOdemeTarihi, // Hesaplanan vade tarihi
                    Tutar = currentTaksitTutari,
                    IsActive = true,
                    OdenmeDurumu = false, // Başlangıçta ödenmemiş
                    ToplamTaksit = _harcama.TaksitSayisi,
                    TaksitNo = i + 1,
                    // Kod ve Açıklama formatı güncellendi
                    OdemeKodu = _harcama.TaksitSayisi == 1 ? _harcama.HarcamaKodu : $"{_harcama.HarcamaKodu}-T{(i + 1):D2}",
                    Aciklama = _harcama.Aciklama, // Harcama açıklaması (HarcamaHelper'daki gibi)

                    KartAdi = krediKarti.CardName,
                    Banka = krediKarti.Banka,
                    CompanyId = krediKarti.CompanyId,
                    IlkOdemeTarihi = ilkOdemeTarihiBase, // İlk ödeme tarihi bilgisini sakla
                    OdeyenKullaniciAdi = null // İlk oluşturulduğunda boş bırak
                };
                yeniTaksitler.Add(taksit);
            }
            _db.KrediKartiOdemeleri.AddRange(yeniTaksitler);
        }

        private void ExecuteCancel(object parameter)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Yeni bir KrediKartiHarcama kodu üretir (KK0001, KK0002...).
        /// </summary>
        /// <param name="db">Veritabanı bağlamı.</param>
        /// <returns>Yeni Harcama Kodu.</returns>
        private string GenerateHarcamaKodu(AppDbContext db)
        {
            // En yüksek mevcut HarcamaKodu'nu bul
            var lastHarcamaCode = db.KrediKartiHarcamalari
                                    .Where(h => h.HarcamaKodu != null && h.HarcamaKodu.StartsWith("KK"))
                                    .Select(h => h.HarcamaKodu)
                                    .OrderByDescending(h => h) // String olarak sıralama yapıyoruz, bu yüzden KK0010 > KK0009 olacaktır
                                    .FirstOrDefault();

            int nextId = 1;
            if (!string.IsNullOrEmpty(lastHarcamaCode) && lastHarcamaCode.Length > 2 && int.TryParse(lastHarcamaCode.Substring(2), out int lastId))
            {
                nextId = lastId + 1;
            }
            return $"KK{nextId:D4}";
        }


        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (SaveCommand is RelayCommand saveRelayCommand)
            {
                saveRelayCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion
    }
}