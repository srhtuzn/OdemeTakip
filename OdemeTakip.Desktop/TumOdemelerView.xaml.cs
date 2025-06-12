// OdemeTakip.Desktop/TumOdemelerView.xaml.cs
using System.Windows.Controls;
using OdemeTakip.Data; // AppDbContext için
using OdemeTakip.Desktop.ViewModels; // TumOdemelerListViewModel için
using System.Windows; // RoutedEventArgs ve MouseButtonEventArgs için

namespace OdemeTakip.Desktop
{
    /// <summary>
    /// TumOdemelerView.xaml için code-behind sınıfı.
    /// MVVM prensiplerine göre sadeleştirilmiştir.
    /// Temel sorumlulukları View'i başlatmak ve DataContext'i ilgili ViewModel'e bağlamaktır.
    /// Tüm iş mantığı ve veri işlemleri TumOdemelerListViewModel'de bulunur.
    /// </summary>
    public partial class TumOdemelerView : UserControl
    {
        /// <summary>
        /// TumOdemelerView sınıfının yeni bir örneğini başlatır.
        /// Kullanıcı kontrolünün DataContext'ini TumOdemelerListViewModel'e ayarlar.
        /// </summary>
        public TumOdemelerView()
        {
            InitializeComponent();
            // UserControl'ün DataContext'ini TumOdemelerListViewModel'e bağlıyoruz.
            // App.DbContext, uygulamanın yaşam döngüsü boyunca erişilebilir olmalıdır.
            this.DataContext = new TumOdemelerListViewModel(App.DbContext);
        }

        /// <summary>
        /// UserControl yüklendiğinde tetiklenen olay işleyicisi.
        /// ViewModel'deki veri yükleme komutunu tetikler.
        /// </summary>
        /// <param name="sender">Olayı tetikleyen nesne.</param>
        /// <param name="e">Olay argümanları.</param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // DataContext'in doğru ViewModel türünde olduğundan emin ol.
            if (this.DataContext is TumOdemelerListViewModel viewModel)
            {
                // ViewModel'deki LoadPaymentsCommand'ı tetikle.
                // Bu, sayfanın başlangıçta verileri yüklemesini sağlar.
                viewModel.LoadPaymentsCommand.Execute(null);
            }
        }

        /// <summary>
        /// DataGrid'deki bir satıra çift tıklandığında tetiklenen olay işleyicisi.
        /// ViewModel'deki DoubleClickCommand'ı tetikler.
        /// </summary>
        /// <param name="sender">Olayı tetikleyen DataGrid.</param>
        /// <param name="e">Fare butonu olay argümanları.</param>
        private void DgOdemeler_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // DataContext'in doğru ViewModel türünde olduğundan emin ol.
            if (this.DataContext is TumOdemelerListViewModel viewModel)
            {
                // ViewModel'deki DoubleClickCommand'ı tetikle.
                // ViewModel'in SelectedOdeme property'si zaten seçili öğeyi tuttuğu için
                // CommandParameter olarak null geçmek yeterlidir.
                // Alternatif olarak, CommandParameter olarak DataGrid'in SelectedItem'ını da gönderebilirsiniz:
                // viewModel.DoubleClickCommand.Execute(DgOdemeler.SelectedItem);
                viewModel.DoubleClickCommand.Execute(null);
            }
        }

        // --- Kaldırılan Metotlar ve Açıklamaları ---
        // Bu dosyadan kaldırılan ana metotlar ve property'ler şunlardır:
        // - INotifyPropertyChanged implementasyonu (ViewModel'e taşındı)
        // - _tumOdemelerSource (ViewModel'e taşındı)
        // - IsCurrentUserAdmin (ViewModel'e taşındı)
        // - SecilenAy, CurrentAyYil, ToplamOdeme, OdenmemisTutar, GecikmisTutar (ViewModel'e taşındı)
        // - AyBilgisiniGuncelle() (ViewModel'e taşındı)
        // - HesaplaRenk(), IkonBelirle() (ViewModel'e taşındı)
        // - BtnOncekiAy_Click(), BtnSonrakiAy_Click() (ViewModel'deki komutlara taşındı)
        // - FiltreleVeYukle() (ViewModel'deki ExecuteLoadPayments metoduna taşındı)
        // - BtnExcelExport_Click(), BtnPDFExport_Click() (ViewModel'deki komutlara taşındı)
        // - MockOdemeler(), LoadTaksitlerFor() (ViewModel'e taşındı)
        // - BtnFiltreleYenile_Click(), TxtArama_TextChanged(), CmbOdemeDurumu_SelectionChanged() (ViewModel'deki komutlara veya property binding'lerine taşındı)
        // - ApplyClientSideFilter() (ViewModel'e taşındı)
        // - Hızlı filtre butonlarının click olayları (ViewModel'deki komutlara taşındı)
        // - BtnOdenmeDurumuDegistir_Click(), GeriAlOdemeDurumu(), IsaretleOdendi() (ViewModel'e taşındı)
        // - SetPropertyValue(), UpdatePaymentStatus(), BulEntity() (ViewModel'e taşındı)
        // - BuildAciklama(), GetAyYil() (ViewModel'e taşındı)
    }
}