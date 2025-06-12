// OdemeTakip.Desktop/KrediForm.xaml.cs
using System;
using System.ComponentModel; // INotifyPropertyChanged için
using System.Windows;
using OdemeTakip.Data; // AppDbContext için
using OdemeTakip.Entities; // Kredi entity için (sadece constructor'da tip olarak geçer)
using OdemeTakip.Desktop.ViewModels; // KrediFormViewModel için

namespace OdemeTakip.Desktop
{
    /// <summary>
    /// KrediForm.xaml için code-behind sınıfı.
    /// MVVM prensiplerine göre sadeleştirilmiştir. Tüm iş mantığı KrediFormViewModel'de bulunur.
    /// </summary>
    public partial class KrediForm : Window
    {
        private KrediFormViewModel _viewModel;

        /// <summary>
        /// KrediForm'un yeni bir örneğini başlatır.
        /// </summary>
        /// <param name="db">Veritabanı bağlamı (AppDbContext).</param>
        /// <param name="kredi">Düzenlenecek Kredi nesnesi (null ise yeni kayıt).</param>
        public KrediForm(AppDbContext db, Kredi? kredi = null)
        {
            InitializeComponent();

            // ViewModel'i oluştur ve DataContext'e bağla.
            _viewModel = new KrediFormViewModel(db, kredi);
            this.DataContext = _viewModel;

            // ViewModel'in DialogResult property'sindeki değişiklikleri dinle.
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
            if (e.PropertyName == nameof(KrediFormViewModel.DialogResult) && _viewModel.DialogResult.HasValue)
            {
                this.DialogResult = _viewModel.DialogResult;
                this.Close();
            }
        }

        // --- Kaldırılan Metotlar ve Alanlar ---
        // _db, _kredi, _isEdit alanları ViewModel'e taşındı.
        // BankalariYukle(), SirketleriYukle(), KrediKoduUret(), BtnKaydet_Click() gibi
        // tüm metotlar da ViewModel'e taşındı.
        // UI elementlerine doğrudan erişim (CmbSirketAdi.Text gibi) XAML Binding'leri ile sağlanacak.
    }
}