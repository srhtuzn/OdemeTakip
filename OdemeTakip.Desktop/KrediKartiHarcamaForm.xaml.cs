// OdemeTakip.Desktop/KrediKartiHarcamaForm.xaml.cs
using System;
using System.ComponentModel; // INotifyPropertyChanged için
using System.Windows;
using OdemeTakip.Data; // AppDbContext için
using OdemeTakip.Entities; // KrediKarti, KrediKartiHarcama için (sadece constructor'da tip olarak geçer)
using OdemeTakip.Desktop.ViewModels; // KrediKartiHarcamaFormViewModel için

namespace OdemeTakip.Desktop
{
    /// <summary>
    /// KrediKartiHarcamaForm.xaml için code-behind sınıfı.
    /// MVVM prensiplerine göre sadeleştirilmiştir. Tüm iş mantığı KrediKartiHarcamaFormViewModel'de bulunur.
    /// </summary>
    public partial class KrediKartiHarcamaForm : Window
    {
        private KrediKartiHarcamaFormViewModel _viewModel;

        /// <summary>
        /// KrediKartiHarcamaForm'un yeni bir örneğini başlatır.
        /// </summary>
        /// <param name="db">Veritabanı bağlamı (AppDbContext).</param>
        /// <param name="seciliKart">Harcamanın yapılacağı kredi kartı. Yeni harcama eklenirken gereklidir.</param>
        /// <param name="harcama">Düzenlenecek harcama nesnesi (null ise yeni kayıt).</param>
        public KrediKartiHarcamaForm(AppDbContext db, KrediKarti seciliKart, KrediKartiHarcama? harcama = null)
        {
            InitializeComponent();

            // ViewModel'i oluştur ve DataContext'e bağla.
            _viewModel = new KrediKartiHarcamaFormViewModel(db, seciliKart, harcama);
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
            if (e.PropertyName == nameof(KrediKartiHarcamaFormViewModel.DialogResult) && _viewModel.DialogResult.HasValue)
            {
                this.DialogResult = _viewModel.DialogResult;
                this.Close();
            }
        }

        // --- Kaldırılan Metotlar ve Alanlar ---
        // _db, _harcama, _isEdit alanları ViewModel'e taşındı.
        // TaksitleriOluştur(), YukleKartlar(), FormuDoldur(), BtnKaydet_Click(), BtnIptal_Click() gibi
        // tüm metotlar da ViewModel'e taşındı.
        // UI elementlerine doğrudan erişim (cmbKrediKartlari.SelectedValue gibi) XAML Binding'leri ile sağlanacak.
    }
}