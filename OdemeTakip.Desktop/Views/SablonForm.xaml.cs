using System;
using System.ComponentModel; // INotifyPropertyChanged için
using System.Windows;
using OdemeTakip.Data; // AppDbContext için hala gerekli
using OdemeTakip.Desktop.ViewModels; // SablonFormViewModel'i kullanmak için

namespace OdemeTakip.Desktop
{
    /// <summary>
    /// SablonForm.xaml için code-behind sınıfı.
    /// Bu sınıfın temel sorumluluğu, View'i başlatmak ve DataContext'ini uygun ViewModel'e bağlamaktır.
    /// Formun veri toplama, kaydetme ve doğrulama mantığı SablonFormViewModel'de yürütülür.
    /// </summary>
    public partial class SablonForm : Window
    {
        private SablonFormViewModel _viewModel;

        /// <summary>
        /// SablonForm'un yeni bir örneğini başlatır.
        /// </summary>
        /// <param name="db">Veritabanı bağlamı (AppDbContext).</param>
        /// <param name="sablonId">Düzenlenecek şablonun ID'si; yeni kayıt için null.</param>
        public SablonForm(AppDbContext db, int? sablonId = null)
        {
            InitializeComponent();

            // ViewModel'i oluştur ve DataContext'e bağla.
            _viewModel = new SablonFormViewModel(db, sablonId);
            this.DataContext = _viewModel;

            // ViewModel'in DialogResult property'sindeki değişiklikleri dinle.
            // Bu, ViewModel'in pencereyi kapatma isteğini View'e bildirmesini sağlar.
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        /// <summary>
        /// ViewModel'deki property değişikliklerini dinler.
        /// Özellikle DialogResult property'si değiştiğinde pencereyi kapatır.
        /// </summary>
        /// <param name="sender">Olayı tetikleyen nesne (ViewModel).</param>
        /// <param name="e">Property değişimi olay argümanları.</param>
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Eğer değişen property DialogResult ise ve bir değeri varsa, pencereyi kapat.
            if (e.PropertyName == nameof(SablonFormViewModel.DialogResult) && _viewModel.DialogResult.HasValue)
            {
                this.DialogResult = _viewModel.DialogResult;
                this.Close();
            }
        }

        // --- Kaldırılan Metotlar ve Açıklamaları ---
        // Daha önceki sürümde bulunan aşağıdaki metotlar ve alanlar artık burada yer almayacak.
        // Çünkü bu işlemlerin tümü SablonFormViewModel.cs dosyasına taşınmıştır.

        // private readonly AppDbContext _db;         // ViewModel'e taşındı
        // private readonly int? _sablonId;           // ViewModel'e taşındı
        // private readonly bool _isEdit;             // ViewModel'e taşındı

        // private void YukleComboBoxlar() { }        // ViewModel'deki InitializeComboBoxSources metoduna taşındı
        // private void SablonYukle(int id) { }       // ViewModel'deki LoadSablonData metoduna taşındı
        // private void BtnKaydet_Click(...) { }      // ViewModel'deki SaveCommand'a taşındı
        // private bool ValidateForm() { }            // ViewModel'deki CanExecuteSave metoduna taşındı
        // private void BtnIptal_Click(...) { }       // ViewModel'deki CancelCommand'a taşındı

        // Tüm public property'ler (Kod, GiderTuru, Aciklama, Tarih, Tutar vb.) de kaldırıldı.
        // Bu property'ler artık SablonFormViewModel'de yönetilmektedir ve XAML üzerinden ona bağlanır.
        // public string Kod { get; private set; }
        // ... (diğer public property'ler) ...
    }
}